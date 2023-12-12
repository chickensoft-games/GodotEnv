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
}

public class EnvironmentVariableClient :
  IEnvironmentVariableClient {
  public IProcessRunner ProcessRunner { get; }
  public IFileClient FileClient { get; }
  public IComputer Computer { get; }

  public EnvironmentVariableClient(
    IProcessRunner processRunner, IFileClient fileClient, IComputer computer
  ) {
    ProcessRunner = processRunner;
    FileClient = fileClient;
    Computer = computer;
  }

  public async Task SetUserEnv(string name, string value) {
    switch (FileClient.OS) {
      case OSType.Windows:
        var currentValue = GetUserEnv(name);
        Environment.SetEnvironmentVariable(
          name, value, EnvironmentVariableTarget.User
        );
        currentValue = GetUserEnv(name);
        break;
      case OSType.MacOS:
        var zshRcPath = FileClient.Combine(FileClient.UserDirectory, ".zshrc");
        FileClient.AddLinesToFileIfNotPresent(
          zshRcPath, $"export {name}=\"{value}\""
        );
        break;
      case OSType.Linux:
        var bashRcPath = FileClient.Combine(FileClient.UserDirectory, ".bashrc");
        FileClient.AddLinesToFileIfNotPresent(
          bashRcPath, $"export {name}=\"{value}\""
        );
        break;
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
        var currentValue = Environment.GetEnvironmentVariable(
          name, EnvironmentVariableTarget.User
        ) ?? "";

        Func<List<string>, string> filter = tokens => tokens.FindAll(t => t.Length > 0).Aggregate((a, b) => a + ';' + b);

        var tokens = currentValue.Split(';').ToList();
        tokens = tokens.Where(t => !t.Contains(value, StringComparison.OrdinalIgnoreCase)).ToList();

        tokens.Add(value);
        Environment.SetEnvironmentVariable(
          name, filter(tokens), EnvironmentVariableTarget.User
        );

        currentValue = Environment.GetEnvironmentVariable(
          name, EnvironmentVariableTarget.User
        ) ?? "";
        break;
      // TODO: Treat case where exported var is a composite value
      case OSType.MacOS:
        var zshRcPath = FileClient.Combine(FileClient.UserDirectory, ".zshrc");
        FileClient.AddLinesToFileIfNotPresent(
          zshRcPath, $"export {name}=\"{value}\""
        );
        break;
      case OSType.Linux:
        var bashRcPath =
          FileClient.Combine(FileClient.UserDirectory, ".bashrc");
        FileClient.AddLinesToFileIfNotPresent(
          bashRcPath, $"export {name}=\"{value}\""
        );
        break;
      case OSType.Unknown:
      default:
        break;
    }
  }

  public string GetUserEnv(string name) {
    switch (FileClient.OS) {
      case OSType.Windows:
        return Environment.GetEnvironmentVariable(
          name, EnvironmentVariableTarget.User
        ) ?? "";
      case OSType.MacOS:
        var zshRcPath = FileClient.Combine(FileClient.UserDirectory, ".zshrc");
        return ExtractBashEnvVarFromLine(
          FileClient.FindLineBeginningWithPrefix(
            zshRcPath, $"export {name}="
          ),
          name
        );
      case OSType.Linux:
        var bashRcPath =
          FileClient.Combine(FileClient.UserDirectory, ".bashrc");
        return ExtractBashEnvVarFromLine(
          FileClient.FindLineBeginningWithPrefix(
            bashRcPath, $"export {name}="
          ),
          name
        );
      case OSType.Unknown:
      default:
        return "";
    }
  }

  private static string ExtractBashEnvVarFromLine(string line, string name) =>
    line.Replace($"export {name}=", "").Replace("\"", "").Trim();
}
