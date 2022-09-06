namespace Chickensoft.Chicken {
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Threading.Tasks;

  public interface IAddonManager {
    Task InstallAddons(string projectPath);
  }

  public class AddonManager : IAddonManager {
    public IAddonRepo AddonRepo { get; init; }
    public IReporter Reporter { get; init; }
    public IConfigFileRepo ConfigFileRepo { get; init; }
    public IDependencyGraph DependencyGraph { get; init; }

    public AddonManager(
      IAddonRepo addonRepo,
      IConfigFileRepo configFileRepo,
      IReporter reporter,
      IDependencyGraph dependencyGraph
    ) {
      AddonRepo = addonRepo;
      ConfigFileRepo = configFileRepo;
      Reporter = reporter;
      DependencyGraph = dependencyGraph;
    }

    public async Task InstallAddons(string projectPath) {
      var searchPaths = new Queue<string>();
      searchPaths.Enqueue(projectPath);

      var projConfigFile
        = ConfigFileRepo.LoadOrCreateConfigFile(projectPath);
      var projectConfig = projConfigFile.ToConfig(projectPath);

      var cache = await AddonRepo.LoadCache(projectConfig);

      do {
        var path = searchPaths.Dequeue();
        var configFile = ConfigFileRepo.LoadOrCreateConfigFile(path);
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

          var depEvent = DependencyGraph.Add(addon);
          Reporter.Handle(depEvent);

          if (depEvent is not IDependencyCannotBeInstalledEvent) {
            try {
              await InstallAddon(addon, projectConfig);
              Reporter.Handle(new AddonInstalledEvent(addon));
            }
            catch (Exception e) {
              var failedEvent = new AddonFailedToInstallEvent(addon, e);
              Reporter.Handle(failedEvent);
            }
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
        await AddonRepo.DeleteAddon(addon, projectConfig);
        AddonRepo.InstallAddonWithSymlink(addon, projectConfig);
        return;
      }
      // Clone the addon from the git url, if needed.
      await AddonRepo.CacheAddon(addon, projectConfig);
      // Delete any previously installed addon.
      await AddonRepo.DeleteAddon(addon, projectConfig);
      // Copy the addon files from the cache to the installation folder.
      await AddonRepo.CopyAddonFromCache(addon, projectConfig);
    }

    /// <summary>
    /// Given an addon config and the path where the addon config resides,
    /// compute the actual addon's source url.
    /// <br />
    /// For addons sourced on the local machine, this will convert relative
    // paths into absolute paths.
    /// </summary>
    /// <param name="addonConfig">Addon config.</param>
    /// <param name="path">Path containing the addons.json the addon was
    /// required from.</param>
    /// <returns>Resolved addon source.</returns>
    public string ResolveUrl(AddonConfig addonConfig, string path) {
      var url = addonConfig.Url;
      if (addonConfig.IsRemote) { return url; }
      // If the path containing the addons.json is a symlink, determine the
      // actual path containing the addons.json file. This allows addons
      // that have their own addons with relative paths to be relative to
      // where the addon is actually stored, which is more intuitive.
      if (AddonRepo.IsDirectorySymlink(path)) {
        path = AddonRepo.DirectorySymlinkTarget(path);
      }
      if (!Path.IsPathRooted(url)) {
        // Locally sourced addons with relative paths are relative to the
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
