namespace GoDotAddon {
  using System.IO.Abstractions;

  public interface IConfigFileRepo {
    ConfigFile LoadOrCreateConfigFile(string configFilePath);
  }

  public class ConfigFileRepo : IConfigFileRepo {
    private readonly FileSystem _fs;
    private readonly IApp _app;

    public ConfigFileRepo(IApp app) {
      _app = app;
      _fs = app.FS;
    }

    public ConfigFile LoadOrCreateConfigFile(string configFilePath) {
      if (_fs.File.Exists(configFilePath)) {
        return LoadConfigFile(configFilePath);
      }
      else {
        return CreateConfigFile();
      }
    }

    private ConfigFile LoadConfigFile(string configFilePath)
      => _app.LoadFile<ConfigFile>(configFilePath);

    private static ConfigFile CreateConfigFile() {
      var configFile = new ConfigFile(
        addons: new(), cachePath: IApp.DEFAULT_CACHE_DIR, IApp.DEFAULT_PATH_DIR
      );
      return configFile;
    }
  }
}
