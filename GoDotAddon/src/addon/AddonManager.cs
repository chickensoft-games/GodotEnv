namespace Chickensoft.GoDotAddon {
  using System.Collections.Generic;
  using System.IO;
  using System.Threading.Tasks;

  public class AddonManager {
    private readonly IAddonRepo _addonRepo;
    private readonly IReporter _reporter;
    private readonly IConfigFileRepo _configFileRepo;
    private readonly IDependencyGraph _dependencyGraph;

    public AddonManager(
      IAddonRepo addonRepo,
      IConfigFileRepo configFileRepo,
      IReporter reporter,
      IDependencyGraph dependencyGraph
    ) {
      _addonRepo = addonRepo;
      _configFileRepo = configFileRepo;
      _reporter = reporter;
      _dependencyGraph = dependencyGraph;
    }

    public async Task InstallAddons(string projectPath) {
      var searchPaths = new Queue<string>();
      searchPaths.Enqueue(projectPath);

      var projConfigFilePath
        = Path.Combine(projectPath, IApp.ADDONS_CONFIG_FILE);
      var projConfigFile
        = _configFileRepo.LoadOrCreateConfigFile(projConfigFilePath);
      var config = projConfigFile.ToConfig(projectPath);

      var addonsByUrl = new Dictionary<string, RequiredAddon>();

      do {
        var path = searchPaths.Dequeue();
        var configFilePath = Path.Combine(path, IApp.ADDONS_CONFIG_FILE);
        var configFile = _configFileRepo.LoadOrCreateConfigFile(configFilePath);
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

          var depEvent = _dependencyGraph.Add(addon, config);
          if (depEvent is ReportableDependencyEvent reportableDepEvent) {
            _reporter.DependencyEvent(reportableDepEvent);
          }

          if (depEvent is IDependencyNotInstalledEvent) {
            shouldInstall = false;
          }

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
