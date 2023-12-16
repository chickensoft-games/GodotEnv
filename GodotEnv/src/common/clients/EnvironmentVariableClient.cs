namespace Chickensoft.GodotEnv.Common.Clients;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Common.Utilities;

public interface IEnvironmentVariableClient
{
  string GetUserEnv(string name);
  Task SetUserEnv(string name, string value);
  Task AppendToUserEnv(string name, string value);
  string GetDefaultShell();
  bool IsShellSupported(string shellName);
}

public class EnvironmentVariableClient :
  IEnvironmentVariableClient
{
  public IProcessRunner ProcessRunner { get; }
  public IFileClient FileClient { get; }
  public IComputer Computer { get; }

  public EnvironmentVariableClient(
    IProcessRunner processRunner, IFileClient fileClient, IComputer computer
  )
  {
    ProcessRunner = processRunner;
    FileClient = fileClient;
    Computer = computer;
  }

  public async Task SetUserEnv(string name, string value)
  {
    var defaultShellName = GetDefaultShell();

    switch (FileClient.OS)
    {
      case OSType.Windows:
        var currentValue = GetUserEnv(name);
        Environment.SetEnvironmentVariable(
          name, value, EnvironmentVariableTarget.User
        );
        currentValue = GetUserEnv(name);
        break;
      case OSType.MacOS:
      case OSType.Linux:
        {
          var rcPath = FileClient.Combine(FileClient.UserDirectory, $".{defaultShellName}rc");
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

  public async Task AppendToUserEnv(string name, string value)
  {
    // Set a user environment variable.

    var shell = Computer.CreateShell(FileClient.AppDataDirectory);
    var defaultShellName = GetDefaultShell();

    switch (FileClient.OS)
    {
      case OSType.Windows:
        var currentValue = Environment.GetEnvironmentVariable(
          name, EnvironmentVariableTarget.User
        ) ?? "";

        Func<List<string>, string> filter = tokens => tokens.FindAll(t => t.Length > 0).Aggregate((a, b) => a + ';' + b);

        var tokens = currentValue.Split(';').ToList();
        tokens = tokens.Where(t => !t.Contains(value, StringComparison.OrdinalIgnoreCase)).ToList();

        tokens.Insert(0, value);
        Environment.SetEnvironmentVariable(
          name, filter(tokens), EnvironmentVariableTarget.User
        );

        currentValue = Environment.GetEnvironmentVariable(
          name, EnvironmentVariableTarget.User
        ) ?? "";
        break;
      // In case the path assigned to variable changes, the previous one will remain in the file but with lower priority.
      case OSType.MacOS:
      case OSType.Linux:
        var rcPath = FileClient.Combine(FileClient.UserDirectory, $".{defaultShellName}rc");
        FileClient.AddLinesToFileIfNotPresent(
          rcPath, $"export {name}=\"{value}:${name}\""
        );
        break;
      case OSType.Unknown:
      default:
        break;
    }
  }

  public string GetUserEnv(string name)
  {
    var shell = Computer.CreateShell(FileClient.AppDataDirectory);
    var defaultShellName = GetDefaultShell();

    switch (FileClient.OS)
    {
      case OSType.Windows:
        return Environment.GetEnvironmentVariable(
          name, EnvironmentVariableTarget.User
        ) ?? "";
      // It's important to use the user's default shell to get the env-var value here.
      // Note the use of the '-i' flag to initialize an interactive shell. Properly loading '<shell>'rc file.
      case OSType.MacOS:
      case OSType.Linux:
        {
          var task = shell.Run(
            $"{defaultShellName}", ["-ic", $"echo ${name}"]
          );
          task.Wait();
          return task.Result.StandardOutput;
        }
      case OSType.Unknown:
      default:
        return "";
    }
  }

  public string GetDefaultShell()
  {
    var shell = Computer.CreateShell(FileClient.AppDataDirectory);

    switch (FileClient.OS)
    {
      case OSType.MacOS:
        {
          var task = shell.Run(
            "sh", ["-c", "dscl . -read /Users/$USER UserShell | awk -F/ '{ print $NF }'"]
          );
          task.Wait();
          return task.Result.StandardOutput.TrimEnd(Environment.NewLine.ToCharArray());
        }
      case OSType.Linux:
        {
          var task = shell.Run(
            "sh", ["-c", "getent passwd $USER | awk -F/ '{ print $NF }'"]
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

  // Should be called on CLI initialization when making environment validation.
  // Can be used to guarantee that the user's default shell is supported.
  public bool IsShellSupported(string shellName) => new[] { "bash", "zsh" }.Contains(shellName.ToLower());
}
