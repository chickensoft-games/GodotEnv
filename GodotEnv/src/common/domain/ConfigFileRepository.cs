namespace Chickensoft.GodotEnv.Common.Domain;
using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Common.Models;

public interface IConfigFileRepository {
  IFileClient FileClient { get; }

  /// <summary>
  /// Absolute path to the application config file.
  /// </summary>
  public string ConfigFilePath { get; }

  /// <summary>
  /// Loads the application config file, or returns one with the default values.
  /// </summary>
  /// <param name="filename">Path to the config file, or the default storage
  /// location if it does not exist.</param>
  /// <returns>An application config file.</returns>
  ConfigFile LoadConfigFile(out string filename);
  void EnsureAppDataDirectoryExists();
  void SaveConfig(ConfigFile config);
}

public class ConfigFileRepository : IConfigFileRepository {
  public IFileClient FileClient { get; }
  public string ConfigFilePath { get; }

  public ConfigFileRepository(IFileClient fileClient) {
    FileClient = fileClient;
    ConfigFilePath = FileClient.Combine(
      FileClient.AppDataDirectory, Defaults.CONFIG_FILE_NAME
    );
  }

  public ConfigFile LoadConfigFile(out string filename) =>
    FileClient.ReadJsonFile(
      projectPath: FileClient.AppDataDirectory,
      possibleFilenames: [Defaults.CONFIG_FILE_NAME],
      filename: out filename,
      defaultValue: new ConfigFile()
    );

  public void EnsureAppDataDirectoryExists() =>
    FileClient.CreateDirectory(FileClient.AppDataDirectory);

  public void SaveConfig(ConfigFile config) =>
    FileClient.WriteJsonFile(ConfigFilePath, config);
}
