namespace Chickensoft.Chicken {
  using System.IO;
  using System.IO.Abstractions;

  public interface IConfigFileRepo {
    ConfigFile LoadOrCreateConfigFile(string projectPath);
  }

  public class ConfigFileRepo : IConfigFileRepo {
    private readonly IFileSystem _fs;
    private readonly IApp _app;

    public ConfigFileRepo(IApp app) {
      _app = app;
      _fs = app.FS;
    }

    public ConfigFile LoadOrCreateConfigFile(string projectPath) {
      var configFilePath = Path.Combine(projectPath, IApp.ADDONS_CONFIG_FILE);
      var configFile = _fs.File.Exists(configFilePath)
        ? Load(configFilePath)
        : Create();
      // Make sure addons folder and addons cache folder exist.
      if (!_fs.Directory.Exists(configFile.AddonsPath)) {
        _fs.Directory.CreateDirectory(configFile.AddonsPath);
      }
      if (!_fs.Directory.Exists(configFile.CachePath)) {
        _fs.Directory.CreateDirectory(configFile.CachePath);
      }
      return configFile;
    }

    private ConfigFile Load(string configFilePath)
      => _app.LoadFile<ConfigFile>(configFilePath);

    private static ConfigFile Create() {
      var configFile = new ConfigFile(
        addons: new(),
        cachePath: IApp.DEFAULT_CACHE_PATH,
        addonsPath: IApp.DEFAULT_ADDONS_PATH
      );
      return configFile;
    }
  }
}
