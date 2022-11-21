namespace Chickensoft.Chicken;

using System.IO.Abstractions;

public interface IConfigFileLoader : IJsonFileLoader<ConfigFile> {
  ConfigFile Load(string projectPath);
}

public class ConfigFileLoader : JsonFileLoader<ConfigFile>, IConfigFileLoader {
  public ConfigFileLoader(IApp app, IFileSystem fs) : base(app, fs) { }

  public ConfigFile Load(string projectPath) {
    var configFile = base.Load(
      projectPath: projectPath,
      possibleFilenames: App.ADDONS_CONFIG_FILES,
      defaultValue: new ConfigFile(
        addons: new(),
        cachePath: App.DEFAULT_CACHE_PATH,
        addonsPath: App.DEFAULT_ADDONS_PATH
      )
    );
    // Make sure addons folder and addons cache folder exist.
    if (!_fs.Directory.Exists(configFile.AddonsPath)) {
      _fs.Directory.CreateDirectory(configFile.AddonsPath);
    }
    if (!_fs.Directory.Exists(configFile.CachePath)) {
      _fs.Directory.CreateDirectory(configFile.CachePath);
    }
    return configFile;
  }
}
