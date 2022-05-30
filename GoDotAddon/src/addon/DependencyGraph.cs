namespace Chickensoft.GoDotAddon {
  using System.Collections.Generic;

  public interface IDependencyGraph {
    DependencyEvent? Add(RequiredAddon addon, Config config);
  }

  public class DependencyGraph : IDependencyGraph {
    // dependencies keyed by url
    private readonly Dictionary<string, HashSet<RequiredAddon>> _dependencies
      = new();
    private readonly Dictionary<string, RequiredAddon> _dependenciesByName
      = new();

    public DependencyGraph() { }

    public DependencyEvent Add(RequiredAddon addon, Config config) {
      // First, check to make sure another addon isn't installed to the same
      // path as the new one.
      if (_dependenciesByName.TryGetValue(addon.Name, out var sameNameAddon)) {
        if (
          sameNameAddon.Url == addon.Url &&
          sameNameAddon.Subfolder == addon.Subfolder &&
          sameNameAddon.Checkout == addon.Checkout
        ) {
          // Dependency is already installed.
          return new DependencyAlreadyInstalledEvent();
        }
        return new ConflictingDestinationPathEvent(
          conflict: addon,
          addon: sameNameAddon,
          config: config
        );
      }

      // Check for similar dependencies in the flat dependency graph to
      // alert the user to potential conflicts.
      if (_dependencies.TryGetValue(addon.Url, out var addonsWithSameUrl)) {
        var conflicts = new List<RequiredAddon>();
        foreach (var existingAddon in addonsWithSameUrl) {
          if (
            existingAddon.Subfolder == addon.Subfolder
            && existingAddon.Checkout == addon.Checkout
          ) {
            // Dependency is already installed under a different name.
            return new DependencyAlreadyInstalledEvent();
          }
          else if (existingAddon.Subfolder == addon.Subfolder) {
            // only branch is different
            conflicts.Add(existingAddon);
          }
          else if (existingAddon.Checkout == addon.Checkout) {
            // only subfolder is different
            conflicts.Add(existingAddon);
          }
        }

        // Similar dependency warnings don't prevent an installation from
        // occurring, as they are probably desired. We do our best to warn
        // the user regardless in case it is a mistake.
        MarkInstalled(addon);

        if (conflicts.Count > 0) {
          return new SimilarDependencyWarning(
            conflict: addon,
            addons: conflicts
          );
        }
      }
      else {
        MarkInstalled(addon);
      }
      return new DependencyInstalledEvent();
    }

    private void MarkInstalled(RequiredAddon addon) {
      if (_dependencies.TryGetValue(addon.Url, out var addonsWithSameUrl)) {
        addonsWithSameUrl.Add(addon);
      }
      else {
        _dependencies[addon.Url] = new HashSet<RequiredAddon>() { addon };
      }
      _dependenciesByName.Add(addon.Name, addon);
    }
  }
}
