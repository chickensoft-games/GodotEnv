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

      var projConfigFile
        = _configFileRepo.LoadOrCreateConfigFile(projectPath);
      var projectConfig = projConfigFile.ToConfig(projectPath);

      var cache = await _addonRepo.LoadCache(projectConfig);

      do {
        var path = searchPaths.Dequeue();
        var configFile = _configFileRepo.LoadOrCreateConfigFile(path);
        var configFilePath = Path.Combine(path, IApp.ADDONS_CONFIG_FILE);
        var addonConfigs = configFile.Addons;

        foreach ((var addonName, var addonConfig) in addonConfigs) {
          var name = addonName;

          var addon = new RequiredAddon(
            name: name,
            configFilePath: configFilePath,
            url: addonConfig.Url,
            checkout: addonConfig.Checkout,
            subfolder: addonConfig.Subfolder
          );

          var depEvent = _dependencyGraph.Add(addon);

          if (depEvent is IReportableDependencyEvent reportableDepEvent) {
            _reporter.DependencyEvent(reportableDepEvent);
          }

          if (depEvent is not IDependencyCannotBeInstalledEvent) {
            // Clone the addon from the git url, if needed.
            await _addonRepo.CacheAddon(addon, projectConfig);
            // Delete any previously installed addon.
            await _addonRepo.DeleteAddon(addon, projectConfig);
            // Copy the addon files from the cache to the installation folder.
            await _addonRepo.CopyAddonFromCache(addon, projectConfig);
          }

          var installedAddonPath = Path.Combine(projectConfig.AddonsPath, name);
          searchPaths.Enqueue(installedAddonPath);
        }
      } while (searchPaths.Count > 0);
    }
  }
}
