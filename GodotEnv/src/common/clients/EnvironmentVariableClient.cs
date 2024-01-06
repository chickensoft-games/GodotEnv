namespace Chickensoft.GodotEnv.Common.Clients;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Common.Utilities;

public interface IEnvironmentVariableClient {
  Task<string> GetUserEnv(string name);
  void SetUserEnv(string name, string value);
  Task AppendToUserEnv(string name, string value);
  Task<string> GetUserDefaultShell();
  bool IsShellSupported(string shellName);
  /// <summary>
  /// Retrieves user's default shell and checks if it's supported.
  /// </summary>
  bool IsDefaultShellSupported { get; }
  /// <summary>
  /// The user's default shell.
  /// Defaulted to 'bash' if not supported on UNIX systems and to empty string on Windows.
  /// </summary>
  string UserShell { get; }
}

public class EnvironmentVariableClient :
  IEnvironmentVariableClient {

  public const string USER_SHELL_COMMAND_MAC = "dscl . -read /Users/$USER UserShell";
  public const string USER_SHELL_COMMAND_LINUX = "getent passwd $USER";
  public static readonly string[] SUPPORTED_UNIX_SHELLS = ["bash", "zsh"];

  public IProcessRunner ProcessRunner { get; }
  public IFileClient FileClient { get; }
  public IComputer Computer { get; }
  public IEnvironmentClient EnvironmentClient { get; }

  public bool IsDefaultShellSupported {
    get {
      var task = GetUserDefaultShell();
      task.Wait();
      var userShell = task.Result;
      return IsShellSupported(userShell);
    }
  }

  private string? _userShell;
  public string UserShell {
    get {
      if (!string.IsNullOrEmpty(_userShell)) {
        return _userShell;
      }
      var task = GetUserDefaultShell();
      task.Wait();
      _userShell = task.Result;
      _userShell = IsShellSupported(_userShell) ? _userShell : "bash";
      return _userShell;
    }
  }

  public EnvironmentVariableClient(
    IProcessRunner processRunner, IFileClient fileClient, IComputer computer, IEnvironmentClient environmentClient
  ) {
    ProcessRunner = processRunner;
    FileClient = fileClient;
    Computer = computer;
    EnvironmentClient = environmentClient;
  }

  public void SetUserEnv(string name, string value) {
    switch (FileClient.OS) {
      case OSType.Windows:
        EnvironmentClient.SetEnvironmentVariable(
          name, value, EnvironmentVariableTarget.User
        );
        break;
      case OSType.MacOS:
      case OSType.Linux: {
          var rcPath = FileClient.Combine(FileClient.UserDirectory, $".{UserShell}rc");
          FileClient.AddLinesToFileIfNotPresent(
            rcPath, $"export {name}=\"{value}\""
          );
          break;
        }
      case OSType.Unknown:
      default:
        break;
    }
  }

  public async Task AppendToUserEnv(string name, string value) {
    // Set a user environment variable.

    var shell = Computer.CreateShell(FileClient.AppDataDirectory);

    switch (FileClient.OS) {
      case OSType.Windows:
        var currentValue = await GetUserEnv(name);

        Func<List<string>, string> filter = tokens => tokens.FindAll(t => t.Length > 0).Aggregate((a, b) => a + ';' + b);

        var tokens = currentValue.Split(';').ToList();
        tokens = tokens.Where(t => !t.Contains(value, StringComparison.OrdinalIgnoreCase)).ToList();

        tokens.Insert(0, value);
        SetUserEnv(name, filter(tokens));
        break;
      // In case the path assigned to variable changes, the previous one will remain in the file but with lower priority.
      case OSType.MacOS:
      case OSType.Linux:
        var rcPath = FileClient.Combine(FileClient.UserDirectory, $".{UserShell}rc");
        FileClient.AddLinesToFileIfNotPresent(
          rcPath, $"export {name}=\"{value}:${name}\""
        );
        break;
      case OSType.Unknown:
      default:
        break;
    }
  }

  public async Task<string> GetUserEnv(string name) {
    var shell = Computer.CreateShell(FileClient.AppDataDirectory);

    switch (FileClient.OS) {
      case OSType.Windows:
        return Task.FromResult(EnvironmentClient.GetEnvironmentVariable(
          name, EnvironmentVariableTarget.User
        ) ?? "").Result;
      // It's important to use the user's default shell to get the env-var value here.
      // Note the use of the '-i' flag to initialize an interactive shell. Properly loading '<shell>'rc file.
      case OSType.MacOS:
      case OSType.Linux: {
          var processResult = await shell.Run(
            $"{UserShell}", ["-ic", $"echo ${name}"]
          );
          return processResult.StandardOutput;
        }
      case OSType.Unknown:
      default:
        return "";
    }
  }

  public async Task<string> GetUserDefaultShell() {
    var shell = Computer.CreateShell(FileClient.AppDataDirectory);

    switch (FileClient.OS) {
      case OSType.MacOS: {
          var processResult = await shell.Run(
            "sh", ["-c", USER_SHELL_COMMAND_MAC]
          );
          var shellName = processResult.StandardOutput.TrimEnd(Environment.NewLine.ToCharArray());
          return shellName.Split('/').Last();
        }
      case OSType.Linux: {
          var processResult = await shell.Run(
            "sh", ["-c", USER_SHELL_COMMAND_LINUX]
          );
          var shellName = processResult.StandardOutput.TrimEnd(Environment.NewLine.ToCharArray());
          return shellName.Split('/').Last();
        }
      case OSType.Windows:
      case OSType.Unknown:
      default:
        return string.Empty;
    }
  }

  public bool IsShellSupported(string shellName) => FileClient.OS switch {
    OSType.MacOS or OSType.Linux => SUPPORTED_UNIX_SHELLS.Contains(shellName.ToLower()),
    OSType.Windows => true,
    OSType.Unknown => false,
    _ => false,
  };
}
