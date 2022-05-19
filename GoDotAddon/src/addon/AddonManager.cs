namespace GoDotAddon {
  public class AddonManager {
    private readonly IAddonRepo _addonRepo;

    public AddonManager(IAddonRepo addonRepo) => _addonRepo = addonRepo;

    public async Task InstallAddons(
      string projectPath, ConfigFileRepo configFileRepo
    ) {
      var searchPaths = new Queue<string>();
      searchPaths.Enqueue(projectPath);

      var projConfigFilePath
        = Path.Combine(projectPath, IApp.ADDONS_CONFIG_FILE);
      var projConfigFile
        = configFileRepo.LoadOrCreateConfigFile(projConfigFilePath);
      var config = projConfigFile.ToConfig(projectPath);

      var addonsByUrl = new Dictionary<string, RequiredAddon>();

      do {
        var path = searchPaths.Dequeue();
        var configFilePath = Path.Combine(path, IApp.ADDONS_CONFIG_FILE);
        var configFile = configFileRepo.LoadOrCreateConfigFile(configFilePath);
        var addonConfigs = configFile.Addons;

        foreach ((var addonName, var addonConfig) in addonConfigs) {
          var name = addonName;
          var shouldInstall = true;

          if (addonsByUrl.ContainsKey(addonConfig.Url)) {
            // We've already cached this addon, potentially under a different
            // name from a prior dependent's addons.json file.
            //
            // The first to depend on an addon with the same url wins.
            var existingAddon = addonsByUrl[addonConfig.Url];
            name = existingAddon.Name;
            shouldInstall = false;
          }

          var addon = new RequiredAddon(
            name: name,
            configFilePath: configFilePath,
            url: addonConfig.Url,
            checkout: addonConfig.Checkout,
            subfolder: addonConfig.Subfolder
          );

          if (shouldInstall) {
            // Clone the addon from the git url, if needed.
            await _addonRepo.CacheAddon(addon, config);
            // Delete any previously installed addon.
            await _addonRepo.DeleteAddon(addon, config);
            // Copy the addon files from the cache to the installation folder.
            await _addonRepo.CopyAddonFromCache(addon, config);
          }

          addonsByUrl[addonConfig.Url] = addon;

          var installedAddonPath = Path.Combine(config.AddonsPath, name);
          searchPaths.Enqueue(installedAddonPath);
        }
      } while (searchPaths.Count > 0);
    }
  }
}
