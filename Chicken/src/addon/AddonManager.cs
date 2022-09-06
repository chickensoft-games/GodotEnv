namespace Chickensoft.Chicken {
  using System.Collections.Generic;
  using System.IO;
  using System.Threading.Tasks;

  public interface IAddonManager {
    Task InstallAddons(string projectPath);
  }

  public class AddonManager : IAddonManager {
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
          var url = ResolveUrl(addonConfig, path);

          var addon = new RequiredAddon(
            name: name,
            configFilePath: configFilePath,
            url: url,
            checkout: addonConfig.Checkout,
            subfolder: addonConfig.Subfolder,
            source: addonConfig.Source
          );

          var depEvent = _dependencyGraph.Add(addon);

          if (depEvent is IReportableDependencyEvent reportableDepEvent) {
            _reporter.DependencyEvent(reportableDepEvent);
          }

          if (depEvent is not IDependencyCannotBeInstalledEvent) {
            await InstallAddon(addon, projectConfig);
          }

          var installedAddonPath = Path.Combine(projectConfig.AddonsPath, name);
          searchPaths.Enqueue(installedAddonPath);
        }
      } while (searchPaths.Count > 0);
    }

    internal async Task InstallAddon(
      RequiredAddon addon, Config projectConfig
    ) {
      if (addon.IsSymlink) {
        await _addonRepo.DeleteAddon(addon, projectConfig);
        _addonRepo.InstallAddonWithSymlink(addon, projectConfig);
        return;
      }
      // Clone the addon from the git url, if needed.
      await _addonRepo.CacheAddon(addon, projectConfig);
      // Delete any previously installed addon.
      await _addonRepo.DeleteAddon(addon, projectConfig);
      // Copy the addon files from the cache to the installation folder.
      await _addonRepo.CopyAddonFromCache(addon, projectConfig);
    }

    /// <summary>
    /// Given an addon config and the path where the addon config resides,
    /// compute the actual addon's source.
    /// <br />
    /// For addons sourced on the local machine, this will convert relative
    // paths into absolute paths.
    /// </summary>
    /// <param name="addonConfig">Addon config.</param>
    /// <param name="path">Path containing the addons.json the addon was
    /// required from.</param>
    /// <returns>Resolved addon source.</returns>
    public static string ResolveUrl(AddonConfig addonConfig, string path) {
      var url = addonConfig.Url;
      if (addonConfig.IsSymlink && !Path.IsPathRooted(url)) {
        // Symlink addons with relative paths are relative to the
        // addons.json file that defines them.
        // Why we use GetFullPath: https://stackoverflow.com/a/1299356
        url = Path.GetFullPath(
          Path.TrimEndingDirectorySeparator(path) +
          Path.DirectorySeparatorChar +
          addonConfig.Url
        );
      }
      return url;
    }
  }
}
