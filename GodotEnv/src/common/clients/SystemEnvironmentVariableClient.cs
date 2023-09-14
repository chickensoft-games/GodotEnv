namespace Chickensoft.GodotEnv.Common.Clients;

using System;
using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Common.Utilities;

public interface ISystemEnvironmentVariableClient {
  string GetEnv(string name);
  Task SetEnv(string name, string value);
}

public class SystemEnvironmentVariableClient :
  ISystemEnvironmentVariableClient {
  public IProcessRunner ProcessRunner { get; }
  public IFileClient FileClient { get; }

  public SystemEnvironmentVariableClient(
    IProcessRunner processRunner, IFileClient fileClient
  ) {
    ProcessRunner = processRunner;
    FileClient = fileClient;
  }

  public async Task SetEnv(string name, string value) {
    // Set a system-wide environment variable.
    // On Windows, this requires elevated privileges so we ask for permission.
    //
    // On macOS, we have to add the value to the .zshrc file.
    // On Linux, we have to add the value to the .bashrc file.
    //
    // Both macOS and Linux users must open a new shell or run `source` on the
    // rc file for the environment variable to become available afterwards.
    switch (FileClient.OS) {
      case OSType.Windows:
        await ProcessRunner.RunElevatedOnWindows(
          "cmd.exe", $"/c setx {name} \"{value}\" /M"
        );
        return;
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

  public string GetEnv(string name) {
    switch (FileClient.OS) {
      case OSType.Windows:
        return Environment.GetEnvironmentVariable(
          name, EnvironmentVariableTarget.Machine
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
