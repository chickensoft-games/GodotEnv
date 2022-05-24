namespace Chickensoft.GoDotAddon {
  using System.IO.Abstractions;
  using Newtonsoft.Json;

  public interface IConfigFileRepo {
    ConfigFile LoadOrCreateConfigFile(string configFilePath);
  }

  public class ConfigFileRepo : IConfigFileRepo {
    private readonly IFileSystem _fs;
    private readonly IApp _app;

    public ConfigFileRepo(IApp app) {
      _app = app;
      _fs = app.FS;
    }

    public ConfigFile LoadOrCreateConfigFile(string configFilePath) {
      var configFile = _fs.File.Exists(configFilePath)
        ? Load(configFilePath)
        : Create(configFilePath);
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

    private ConfigFile Create(string configFilePath) {
      var configFile = new ConfigFile(
        addons: new(), cachePath: IApp.DEFAULT_CACHE_PATH, IApp.DEFAULT_ADDONS_PATH
      );
      var data = JsonConvert.SerializeObject(configFile);
      _app.SaveFile(configFilePath, data);
      return configFile;
    }
  }
}
