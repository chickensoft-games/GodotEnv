namespace Chickensoft.GodotEnv.Common.Clients;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models;
using Utilities;

public interface IEnvironmentVariableClient
{
  ISystemInfo SystemInfo { get; }

  /// <summary>
  /// Retrieves the environment variable for the current user.
  /// </summary>
  /// <param name="name">Environment variable name.</param>
  /// <returns></returns>
  Task<string> GetUserEnv(string name);

  /// <summary>
  /// Adjusts the user-env variables to contain the Godot version managed GodotEnv.
  ///
  /// Updates (or creates) the user-wide GODOT environment containing the symlink path, which points to the active
  /// version of Godot. Also, PATH is updated prepending the Godot symlink path. This integration in UNIX shells is done
  /// via ~/.config/godotenv/env file, which is sourced in the user's shell initialization files.
  /// </summary>
  /// <param name="godotSymlinkPath">Path to the Godot bin symlink to be updated/created.</param>
  /// <param name="godotBinPath">Path to the Godot binary.</param>
  /// <returns></returns>
  Task UpdateGodotEnvEnvironment(string godotSymlinkPath, string godotBinPath);
}

public class EnvironmentVariableClient : IEnvironmentVariableClient
{
  public ISystemInfo SystemInfo { get; }
  public IProcessRunner ProcessRunner { get; }
  public IFileClient FileClient { get; }
  public IComputer Computer { get; }

  public EnvironmentVariableClient(
    ISystemInfo systemInfo, IProcessRunner processRunner, IFileClient fileClient, IComputer computer
  )
  {
    SystemInfo = systemInfo;
    ProcessRunner = processRunner;
    FileClient = fileClient;
    Computer = computer;
  }

  public async Task<string> GetUserEnv(string name)
  {
    var shell = Computer.CreateShell(FileClient.AppDataDirectory);

    switch (SystemInfo.OS)
    {
      case OSType.Windows:
        return Task.FromResult(GetEnvironmentVariableOnWindows(
          name, EnvironmentVariableTarget.User
        ) ?? "").Result;
      case OSType.MacOS:
      case OSType.Linux:
        {
          var envFilePath = FileClient.Combine(FileClient.AppDataDirectory, "env");
          if (!FileClient.FileExists(envFilePath))
          {
            return "";
          }
          // NOTE: 'sh' shell for maximum portability. 'envFilePath' is sourced
          // to apply the GodotEnv's shell patch.
          var processResult = await shell.Run(
            "sh", ["-c", $". \"{envFilePath}\"; echo ${name}"]
          );
          return processResult.StandardOutput;
        }
      case OSType.Unknown:
      default:
        return "";
    }
  }

  private void SetUserEnvOnWindows(string name, string value)
  {
    SetEnvironmentVariableOnWindows(
      name, value, EnvironmentVariableTarget.User
    );
  }

  // Using the .NET API seems to work only on Windows.
  private string? GetEnvironmentVariableOnWindows(string name, EnvironmentVariableTarget target) =>
    GetEnvironmentVariableOnWindowsProxy(name, target);
  // Shim for testing.
  public Func<string, EnvironmentVariableTarget, string?> GetEnvironmentVariableOnWindowsProxy { get; set; } = Environment.GetEnvironmentVariable;

  // Using the .NET API seems to work only on Windows.
  private void SetEnvironmentVariableOnWindows(string name, string value, EnvironmentVariableTarget target) =>
    SetEnvironmentVariableOnWindowsProxy(name, value, target);
  public Action<string, string, EnvironmentVariableTarget> SetEnvironmentVariableOnWindowsProxy { get; set; } = Environment.SetEnvironmentVariable;

  public async Task UpdateGodotEnvEnvironment(string godotSymlinkPath, string godotBinPath)
  {
    switch (SystemInfo.OS)
    {
      case OSType.Windows:
        SetUserEnvOnWindows(Defaults.GODOT_ENV_VAR_NAME, godotSymlinkPath);
        await AppendToUserEnvOnWindows(Defaults.PATH_ENV_VAR_NAME, godotBinPath);
        break;
      case OSType.MacOS:
      case OSType.Linux:
        UpdateEnvFileOnUnix(godotSymlinkPath, godotBinPath);
        break;
      case OSType.Unknown:
      default:
        break;
    }
  }

  private void UpdateEnvFileOnUnix(string godotSymlinkPath, string godotBinPath)
  {
    // Replace home full path by '$HOME' to make it more portable.
    var godotBinPathDynamic = godotBinPath.Replace(FileClient.UserDirectory, "$HOME");
    var godotSymlinkPathDynamic = godotSymlinkPath.Replace(FileClient.UserDirectory, "$HOME");

    // Create file '~/.config/godotenv/env' (AppDataDirectory/env)
    var envFilePath = FileClient.Combine(FileClient.AppDataDirectory, "env");
    // Console.WriteLine($"{nameof(EnvironmentVariableClient)} envFilePath: {envFilePath}");
    if (FileClient.FileExists(envFilePath))
    {
      FileClient.DeleteFile(envFilePath);
    }

    FileClient.CreateFile(envFilePath,
      $$"""
        #!/bin/sh
        # godotenv shell setup (Updates PATH, and defines {{Defaults.GODOT_ENV_VAR_NAME}} environment variable)

        # affix colons on either side of $PATH to simplify matching
        case ":${PATH}:" in
            *:"{{godotBinPathDynamic}}":*)
                ;;
            *)
                # Prepending path making it the highest in priority.
                export PATH="{{godotBinPathDynamic}}:$PATH"
                ;;
        esac

        if [ -z "${{{Defaults.GODOT_ENV_VAR_NAME}}:-}" ]; then  # If variable not defined or empty.
            export {{Defaults.GODOT_ENV_VAR_NAME}}="{{godotSymlinkPathDynamic}}"
        fi

        """);
    // Console.WriteLine($"{nameof(EnvironmentVariableClient)} envFile content:\n{File.ReadAllText(envFilePath)}");

    // Update shell initialization files to source the godotenv's env file.
    var envFilePathDynamic = envFilePath.Replace(FileClient.UserDirectory, "$HOME");
    var cmd = $". \"{envFilePathDynamic}\" # Added by GodotEnv\n";

    // We expect the shell initialization files to exist, so, this being the case, we just patch them.
    var shellFilesToUpdate = new[] {
      $"{FileClient.UserDirectory}/.profile", $"{FileClient.UserDirectory}/.bashrc",
      $"{FileClient.UserDirectory}/.zshenv",
    };
    foreach (var filePath in shellFilesToUpdate)
    {
      // Console.WriteLine($"shellFile: {filePath}");
      if (!FileClient.FileExists(filePath))
      { continue; }

      // Console.WriteLine($"Updating shell file '{filePath}'...");
      FileClient.AddLinesToFileIfNotPresent(filePath, cmd);
    }
  }

  private async Task AppendToUserEnvOnWindows(string name, string value)
  {
    var currentValue = await GetUserEnv(name);
    // On Windows Path, each segment is separated by ';'. We use this to split the string into tokens.
    var tokens = currentValue.Split(';').ToList();
    // Filter tokens keeping the ones that are different from the 'value' that we are trying to add.
    tokens = [.. tokens.Where(t => !t.Contains(value, StringComparison.OrdinalIgnoreCase))];

    // Lambda function that receive a List<string> of tokens.
    // For each string of length > 0, concatenate each one with ';' between then.
    static string concatenateWindowsPaths(List<string> tokens)
      => tokens.FindAll(t => t.Length > 0).Aggregate((a, b) => a + ';' + b);

    // Insert at the beginning.
    tokens.Insert(0, value);
    SetUserEnvOnWindows(name, concatenateWindowsPaths(tokens));
  }
}
