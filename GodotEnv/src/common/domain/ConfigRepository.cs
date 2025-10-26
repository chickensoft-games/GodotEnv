namespace Chickensoft.GodotEnv.Common.Domain;

using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Common.Models;
using Microsoft.Extensions.Configuration;

public interface IConfigRepository
{
  IFileClient FileClient { get; }

  /// <summary>
  /// Absolute path to the application config file.
  /// </summary>
  string ConfigFilePath { get; }

  /// <summary>
  /// Loads the application config file, or returns one with the default values.
  /// </summary>
  /// <returns>An application config file.</returns>
  Config LoadConfig();
  void EnsureAppDataDirectoryExists();
  void SaveConfig(Config config);
}

public class ConfigRepository : IConfigRepository
{
  public IFileClient FileClient { get; }
  public string ConfigFilePath { get; }

  public ConfigRepository(IFileClient fileClient)
  {
    FileClient = fileClient;
    ConfigFilePath = FileClient.Combine(
      FileClient.AppDataDirectory, Defaults.CONFIG_FILE_NAME
    );
  }

  public Config LoadConfig()
  {
    var config = new Config(new ConfigValues());
    if (FileClient.FileExists(ConfigFilePath))
    {
      var configRoot = new ConfigurationBuilder()
        .AddJsonFile(ConfigFilePath)
        .Build();
      config = new Config(configRoot);
    }
    config.Upgrade();
    return config;
  }

  public void EnsureAppDataDirectoryExists() =>
    FileClient.CreateDirectory(FileClient.AppDataDirectory);

  public void SaveConfig(Config config) =>
    FileClient.WriteJsonFile(ConfigFilePath, config.ConfigValues);
}
