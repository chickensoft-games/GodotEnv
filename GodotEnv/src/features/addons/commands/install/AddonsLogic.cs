namespace Chickensoft.GodotEnv.Features.Addons.Commands;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Utilities;
using Chickensoft.GodotEnv.Features.Addons.Domain;
using Chickensoft.GodotEnv.Features.Addons.Models;
using Chickensoft.LogicBlocks;
using Chickensoft.LogicBlocks.Generator;

[StateMachine]
public partial class AddonsLogic : LogicBlockAsync<AddonsLogic.State> {
  public abstract record Input {
    public readonly record struct Install(string ProjectPath, int? MaxDepth);
  }

  public abstract record State : StateLogic {
    public record Unresolved : State, IGet<Input.Install> {
      public async Task<State> On(Input.Install input) {
        var addonsRepo = Context.Get<IAddonsRepository>();
        var addonsFileRepo = Context.Get<IAddonsFileRepository>();
        var addonGraph = Context.Get<IAddonGraph>();

        var searchPaths = new Queue<string>();
        searchPaths.Enqueue(input.ProjectPath);

        addonsRepo.EnsureCacheAndAddonsDirectoriesExists();

        var addonsToInstall = new List<ResolvedAddon>();

        var depth = 0;
        var fatalResolutionError = false;

        // Resolve addons using a flat dependency graph.

        do {
          var path = searchPaths.Dequeue();
          var addonsFile = addonsFileRepo.LoadAddonsFile(
            path, out var addonsFilePath
          );

          foreach ((var addonName, var addonEntry) in addonsFile.Addons) {
            // Resolve addon's url. For remote addons, the url is unchanged.
            // For local symlink addons, the actual path is resolved.
            // For normal local addons, the path is fully qualified.
            var url = addonsRepo.ResolveUrl(addonEntry, addonsFilePath);

            // Converts the addon entry in an addons.json file to an addon model
            // that contains additional information about the addon, like its
            // resolved url, its key name, and what addons.json file it came from.
            var addon = addonEntry.ToAddon(
              name: addonName, resolvedUrl: url, addonsFilePath: addonsFilePath
            );

            // Add the addon to the addon graph. If the addon conflicts with
            // anything in the graph, we will end up reporting a warning or an
            // error.
            var result = addonGraph.Add(addon);

            Context.Output(new Output.Report(result));

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
              var pathToCachedAddon = await addonsRepo.CacheAddon(
                addon, cacheName
              );

              // Checkout correct branch.
              await addonsRepo.PrepareCache(addon, cacheName);
              // Ensure the branch and submodules are up-to-date.
              await addonsRepo.UpdateCache(addon, cacheName);

              searchPaths.Enqueue(pathToCachedAddon);
            }
          }

          depth++;
        } while (
          CanGoOn(
            fatalResolutionError, searchPaths.Count, depth, input.MaxDepth
          )
        );

        if (fatalResolutionError) {
          return new CannotBeResolved();
        }

        if (addonsToInstall.Count == 0) {
          return new NothingToInstall();
        }

        // Install resolved addons.

        foreach (var resolvedAddon in addonsToInstall) {
          var addon = resolvedAddon.Addon;
          var cacheName = resolvedAddon.CacheName;
          if (addon.IsSymlink) {
            await addonsRepo.DeleteAddon(addon);
            addonsRepo.InstallAddonWithSymlink(addon);
            continue;
          }
          // Checkout the correct branch from the correct cache.
          await addonsRepo.PrepareCache(addon, cacheName);
          // Delete any previously installation in the addons directory.
          await addonsRepo.DeleteAddon(addon);
          // Copy the addon files from the cache to the addons directory.
          await addonsRepo.InstallAddonFromCache(addon, cacheName);
        }

        return new InstallationSucceeded();
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

    public record CannotBeResolved : State {
      public CannotBeResolved() {
        OnEnter<CannotBeResolved>(
          (state) => {
            Context.Output(
              new Output.Report(
                new ReportableEvent(
                  (log) => log.Err("Could not resolve addons.")
                )
              )
            );
            return Task.CompletedTask;
          }
        );
      }
    }

    public record NothingToInstall : State {
      public NothingToInstall() {
        OnEnter<NothingToInstall>(
          (state) => {
            Context.Output(
              new Output.Report(
                new ReportableEvent(
                  (log) => log.Info("No addons to install!")
                )
              )
            );
            return Task.CompletedTask;
          }
        );
      }
    }

    public record InstallationSucceeded : State {
      public InstallationSucceeded() {
        OnEnter<InstallationSucceeded>(
          (state) => {
            Context.Output(
              new Output.Report(
                new ReportableEvent(
                  (log) => log.Success("Addons installed successfully.")
                )
              )
            );
            return Task.CompletedTask;
          }
        );
      }
    }
  }

  public abstract record Output {
    public readonly record struct Report(IReportableEvent Event);
  }
}

public partial class AddonsLogic : LogicBlockAsync<AddonsLogic.State> {
  public AddonsLogic(
    IAddonsFileRepository addonsFileRepo,
    IAddonsRepository addonsRepo,
    IAddonGraph addonGraph
  ) {
    Set(addonsFileRepo);
    Set(addonsRepo);
    Set(addonGraph);
  }

  public override State GetInitialState() => new State.Unresolved();

  protected override void HandleError(Exception e) => Context.Output(
    new Output.Report(
      new ReportableEvent(
        (log) => {
          log.Err("An error occurred while installing addons:");
          log.Err(e.Message);
        }
      )
    )
  );
}
