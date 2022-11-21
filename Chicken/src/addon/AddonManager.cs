namespace Chickensoft.Chicken;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;

public interface IAddonManager {
  Task InstallAddons(
    IApp app, string projectPath, IFileCopier copier, int? maxDepth = null
  );
}

public class AddonManager : IAddonManager {
  public IAddonRepo AddonRepo { get; init; }
  public ILog Log { get; init; }
  public IConfigFileLoader ConfigFileRepo { get; init; }
  public IDependencyGraph DependencyGraph { get; init; }

  private readonly IFileSystem _fs;

  public AddonManager(
    IFileSystem fs,
    IAddonRepo addonRepo,
    IConfigFileLoader configFileLoader,
    ILog log,
    IDependencyGraph dependencyGraph
  ) {
    _fs = fs;
    AddonRepo = addonRepo;
    ConfigFileRepo = configFileLoader;
    Log = log;
    DependencyGraph = dependencyGraph;
  }

  public async Task InstallAddons(
    IApp app, string projectPath, IFileCopier copier, int? maxDepth = null
  ) {
    var searchPaths = new Queue<string>();
    searchPaths.Enqueue(projectPath);

    var projConfigFile
      = ConfigFileRepo.Load(projectPath);
    var projectConfig = projConfigFile.ToConfig(projectPath);

    var cache = await AddonRepo.LoadCache(projectConfig);

    var depth = 0;

    do {
      var path = searchPaths.Dequeue();
      var configFile = ConfigFileRepo.Load(path);
      var configFilePath
        = app.FileThatExists(_fs, path, App.ADDONS_CONFIG_FILES);
      var addonConfigs = configFile.Addons;

      foreach ((var addonName, var addonConfig) in addonConfigs) {
        var name = addonName;
        var url = app.ResolveUrl(_fs, addonConfig, path);

        var addon = new RequiredAddon(
          name: name,
          configFilePath: configFilePath,
          url: url,
          checkout: addonConfig.Checkout,
          subfolder: addonConfig.Subfolder,
          source: addonConfig.Source
        );

        var depEvent = DependencyGraph.Add(addon);
        depEvent.Log(Log);

        if (depEvent is not IDependencyCannotBeInstalledEvent) {
          try {
            await InstallAddon(addon, projectConfig, copier);
            new AddonInstalledEvent(addon).Log(Log);
          }
          catch (Exception e) {
            var failedEvent = new AddonFailedToInstallEvent(addon, e);
            failedEvent.Log(Log);
          }
        }

        var installedAddonPath = Path.Combine(projectConfig.AddonsPath, name);
        searchPaths.Enqueue(installedAddonPath);
        depth++;
      }
    } while (CanGoOn(searchPaths.Count, depth, maxDepth));
  }

  internal async Task InstallAddon(
    RequiredAddon addon, Config projectConfig, IFileCopier copier
  ) {
    if ((addon as ISourceRepository).IsSymlink) {
      await AddonRepo.DeleteAddon(addon, projectConfig);
      AddonRepo.InstallAddonWithSymlink(addon, projectConfig);
      return;
    }
    // Clone the addon from the git url, if needed.
    await AddonRepo.CacheAddon(addon, projectConfig);
    // Delete any previously installed addon.
    await AddonRepo.DeleteAddon(addon, projectConfig);
    // Copy the addon files from the cache to the installation folder.
    await AddonRepo.CopyAddonFromCache(addon, projectConfig, copier);
  }

  internal static bool CanGoOn(int numPaths, int depth, int? maxDepth)
    => numPaths > 0 && (maxDepth is null || depth < maxDepth);
}
