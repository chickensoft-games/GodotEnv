namespace Chickensoft.GodotEnv.Common.Domain;

using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Common.Models;
using Microsoft.Extensions.Configuration;

public interface IConfigFileRepository {
  public IFileClient FileClient { get; }

  /// <summary>
  /// Absolute path to the application config file.
  /// </summary>
  public string ConfigFilePath { get; }

  /// <summary>
  /// Loads the application config file, or returns one with the default values.
  /// </summary>
  /// <returns>An application config file.</returns>
  public Config LoadConfig();
  public void EnsureAppDataDirectoryExists();
  public void SaveConfig(Config config);
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

  public Config LoadConfig() {
    var configRoot = new ConfigurationBuilder()
      .AddJsonFile(ConfigFilePath)
      .Build();
    var config = new Config();
    configRoot.Bind(config);
    // re-saving the config upgrades it
    SaveConfig(config);
    return config;
  }

  public void EnsureAppDataDirectoryExists() =>
    FileClient.CreateDirectory(FileClient.AppDataDirectory);

  public void SaveConfig(Config config) {
    UpgradeConfig(config);
    FileClient.WriteJsonFile(ConfigFilePath, config);
  }

  // We need the deprecated properties to update the values of the new properties
#pragma warning disable CS0618
  public void UpgradeConfig(Config config) {
    if (config.GodotInstallationsPath is not null) {
      if (
        config.GodotInstallationsPath != Defaults.CONFIG_GODOT_INSTALLATIONS_PATH
        && config.GodotInstallationsPath != config.Godot.InstallationsPath
      ) {
        config.Godot.InstallationsPath = config.GodotInstallationsPath;
      }
      config.GodotInstallationsPath = null;
      SaveConfig(config);
    }
  }
#pragma warning restore CS0618

}
