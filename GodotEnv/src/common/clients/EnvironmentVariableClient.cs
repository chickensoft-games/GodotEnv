namespace Chickensoft.GodotEnv.Common.Clients;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Common.Utilities;

public interface IEnvironmentVariableClient {
  string GetUserEnv(string name);
  Task SetUserEnv(string name, string value);
  Task AppendToUserEnv(string name, string value);
  string GetUserDefaultShell();
  bool IsShellSupported(string shellName);
  bool IsDefaultShellSupported { get; }

  /// <summary>
  /// The user's default shell. Defaulted to 'bash' if not supported.
  /// </summary>
  string UserShell { get; }
}

public class EnvironmentVariableClient :
  IEnvironmentVariableClient {

  public const string USER_SHELL_COMMAND_MAC = "dscl . -read /Users/$USER UserShell | awk -F/ '{ print $NF }'";
  public const string USER_SHELL_COMMAND_LINUX = "getent passwd $USER | awk -F/ '{ print $NF }'";
  public static readonly string[] SUPPORTED_SHELLS = ["bash", "zsh"];

  public IProcessRunner ProcessRunner { get; }
  public IFileClient FileClient { get; }
  public IComputer Computer { get; }
  public IEnvironmentClient EnvironmentClient { get; }

  public bool IsDefaultShellSupported => IsShellSupported(GetUserDefaultShell());

  public string UserShell {
    get {
      var userShell = GetUserDefaultShell();
      userShell = IsShellSupported(userShell) ? userShell : "bash";
      return userShell;
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

  public async Task SetUserEnv(string name, string value) {
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
        var currentValue = GetUserEnv(name);

        Func<List<string>, string> filter = tokens => tokens.FindAll(t => t.Length > 0).Aggregate((a, b) => a + ';' + b);

        var tokens = currentValue.Split(';').ToList();
        tokens = tokens.Where(t => !t.Contains(value, StringComparison.OrdinalIgnoreCase)).ToList();

        tokens.Insert(0, value);
        await SetUserEnv(name, filter(tokens));
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

  public string GetUserEnv(string name) {
    var shell = Computer.CreateShell(FileClient.AppDataDirectory);

    switch (FileClient.OS) {
      case OSType.Windows:
        return EnvironmentClient.GetEnvironmentVariable(
          name, EnvironmentVariableTarget.User
        ) ?? "";
      // It's important to use the user's default shell to get the env-var value here.
      // Note the use of the '-i' flag to initialize an interactive shell. Properly loading '<shell>'rc file.
      case OSType.MacOS:
      case OSType.Linux: {
          var task = shell.Run(
            $"{UserShell}", ["-ic", $"echo ${name}"]
          );
          task.Wait();
          return task.Result.StandardOutput;
        }
      case OSType.Unknown:
      default:
        return "";
    }
  }

  public string GetUserDefaultShell() {
    var shell = Computer.CreateShell(FileClient.AppDataDirectory);

    switch (FileClient.OS) {
      case OSType.MacOS: {
          var task = shell.Run(
            "sh", ["-c", USER_SHELL_COMMAND_MAC]
          );
          task.Wait();
          return task.Result.StandardOutput.TrimEnd(Environment.NewLine.ToCharArray());
        }
      case OSType.Linux: {
          var task = shell.Run(
            "sh", ["-c", USER_SHELL_COMMAND_LINUX]
          );
          task.Wait();
          return task.Result.StandardOutput.TrimEnd(Environment.NewLine.ToCharArray());
        }
      case OSType.Windows:
      case OSType.Unknown:
      default:
        return string.Empty;
    }
  }

  public bool IsShellSupported(string shellName) {
    bool ans = false;

    switch (FileClient.OS) {
      case OSType.MacOS:
      case OSType.Linux:
        ans = SUPPORTED_SHELLS.Contains(shellName.ToLower());
        break;
      case OSType.Windows:
        ans = true;
        break;
      case OSType.Unknown:
      default:
        ans = false;
        break;
    }

    return ans;
  }
}
