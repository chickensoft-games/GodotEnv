namespace Chickensoft.GodotEnv.Features.Addons.Commands;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Common.Utilities;
using Chickensoft.GodotEnv.Features.Addons.Domain;
using Chickensoft.GodotEnv.Features.Addons.Models;

public interface IAddonsInstaller {
  Task<AddonsInstaller.Result> Install(
    string projectPath,
    int? maxDepth,
    Action<IReportableEvent> onReport,
    Action<Addon, DownloadProgress> onDownload,
    Action<Addon, double> onExtract,
    CancellationToken token,
    string? addonsFileName = null
  );
}

public class AddonsInstaller : IAddonsInstaller {
  public enum Result {
    Succeeded,
    CannotBeResolved,
    NotAttempted,
    NothingToInstall,
  }

  public IAddonsFileRepository AddonsFileRepo { get; }
  public IAddonsRepository AddonsRepo { get; }
  public IAddonGraph AddonGraph { get; }

  public AddonsInstaller(
    IAddonsFileRepository addonsFileRepo,
    IAddonsRepository addonsRepo,
    IAddonGraph addonGraph
  ) {
    AddonsFileRepo = addonsFileRepo;
    AddonsRepo = addonsRepo;
    AddonGraph = addonGraph;
  }

  public async Task<Result> Install(
    string projectPath,
    int? maxDepth,
    Action<IReportableEvent> onReport,
    Action<Addon, DownloadProgress> onDownload,
    Action<Addon, double> onExtract,
    CancellationToken token,
    string? addonsFileName = null
  ) {

    var searchPaths = new Queue<string>();
    searchPaths.Enqueue(projectPath);

    AddonsRepo.EnsureCacheAndAddonsDirectoriesExists();

    var addonsToInstall = new List<ResolvedAddon>();

    var depth = 0;
    var fatalResolutionError = false;

    // Resolve addons using a flat dependency graph.

    do {
      var path = searchPaths.Dequeue();
      var addonsFile = AddonsFileRepo.LoadAddonsFile(
        path, out var addonsFilePath, addonsFileName
      );

      foreach ((var addonName, var addonEntry) in addonsFile.Addons) {
        // Resolve addon's url. For remote addons, the url is unchanged.
        // For local symlink addons, the actual path is resolved.
        // For normal local addons, the path is fully qualified.
        var url = AddonsRepo.ResolveUrl(addonEntry, path);

        // Converts the addon entry in an addons.json file to an addon model
        // that contains additional information about the addon, like its
        // resolved url, its key name, and what addons.json file it came from.
        var addon = addonEntry.ToAddon(
          name: addonName, resolvedUrl: url, addonsFilePath: addonsFilePath
        );

        // Add the addon to the addon graph. If the addon conflicts with
        // anything in the graph, we will end up reporting a warning or an
        // error.
        var result = AddonGraph.Add(addon);

        onReport(result);

        if (result is IAddonGraphFailureResult failure) {
          fatalResolutionError = true;
        }
        else if (result is not AddonAlreadyResolved) {
          IAddon? canonicalAddon = null;
          if (result is AddonResolvedButMightConflict mightConflict) {
            canonicalAddon = mightConflict.CanonicalAddon;
          }

          // When copying addons to the cache from their source, the cache
          // name is determined by the name of the first encountered addon
          // to use that url.
          //
          // If another addon is encountered  later with the same url, it
          // will share the cache of the first addon. We just need to make
          // sure that each branch required by the addons is cached in each
          // clone that resides in the cache.
          //
          // Letting addons that share the same url use the same clone of
          // their source assets prevents multiple copies of the same
          // repository from being cloned.

          var resolvedAddon = new ResolvedAddon(addon, canonicalAddon);
          var cacheName = resolvedAddon.CacheName;

          // Mark our addon to be installed later.
          addonsToInstall.Add(resolvedAddon);

          // Cache addon and add the cached addon to the search paths.

          // This ensure that any addons that addon itself declares will be
          // resolved, enabling us to resolve a flat dependency graph (like
          // bower did back in the day).
          // For symlink'd addons, the path to the cached addon is just the
          // path to whatever the symlink points to.
          var pathToCachedAddon = await AddonsRepo.CacheAddon(
            addon,
            cacheName,
            new Progress<DownloadProgress>(
              (progress) => onDownload(addon, progress)
            ),
            new Progress<double>(
              (progress) => onExtract(addon, progress)
            ),
            token
          );

          // Checkout correct branch.
          await AddonsRepo.PrepareCache(addon, cacheName);
          // Ensure the branch and submodules are up-to-date.
          await AddonsRepo.UpdateCache(addon, cacheName);

          searchPaths.Enqueue(pathToCachedAddon);
        }
      }

      depth++;
    } while (
      CanGoOn(
        fatalResolutionError, searchPaths.Count, depth, maxDepth
      )
    );

    if (fatalResolutionError) {
      return Result.CannotBeResolved;
    }

    if (addonsToInstall.Count == 0) {
      return Result.NothingToInstall;
    }

    // Install resolved addons.

    foreach (var resolvedAddon in addonsToInstall) {
      var addon = resolvedAddon.Addon;
      var cacheName = resolvedAddon.CacheName;

      if (addon.IsSymlink) {
        await AddonsRepo.DeleteAddon(addon);
        AddonsRepo.InstallAddonWithSymlink(addon);
        continue;
      }

      // Checkout the correct branch from the correct cache.
      await AddonsRepo.PrepareCache(addon, cacheName);
      // Delete any previously installation in the addons directory.
      await AddonsRepo.DeleteAddon(addon);
      // Copy the addon files from the cache to the addons directory.
      await AddonsRepo.InstallAddonFromCache(addon, cacheName);
    }

    return Result.Succeeded;
  }

  /// <summary>
  /// Returns true if we addons can continue to be resolved.
  /// </summary>
  /// <param name="fatalResolutionError">Whether or not a fatal resolution
  /// has occurred.</param>
  /// <param name="numPaths">Number of paths remaining to be searched.</param>
  /// <param name="depth">Current addon resolution depth (i.e., how many addons
  /// deep we are).</param>
  /// <param name="maxDepth">Maximum number of addons that can be searched
  /// for addons.</param>
  /// <returns>True if addons can continue to be resolved.</returns>
  internal static bool CanGoOn(
    bool fatalResolutionError, int numPaths, int depth, int? maxDepth
  ) => !fatalResolutionError && numPaths > 0 && (
    maxDepth is null || depth < maxDepth
  );
}
