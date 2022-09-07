namespace Chickensoft.Chicken {
  using System.Collections.Generic;

  public interface IDependencyGraph {
    IDependencyGraphEvent Add(RequiredAddon addon);
  }

  public class DependencyGraph : IDependencyGraph {
    // dependencies keyed by url
    private readonly Dictionary<string, HashSet<RequiredAddon>> _dependencies
      = new();
    private readonly Dictionary<string, RequiredAddon> _dependenciesByName
      = new();

    public DependencyGraph() { }

    public IDependencyGraphEvent Add(RequiredAddon addon) {
      // First, check to make sure another addon isn't installed to the same
      // path as the new one.
      if (_dependenciesByName.TryGetValue(addon.Name, out var installedAddon)) {
        if (
          installedAddon.Url == addon.Url &&
          installedAddon.Subfolder == addon.Subfolder &&
          installedAddon.Checkout == addon.Checkout
        ) {
          // Dependency is already installed under the same name.
          return new DependencyAlreadyInstalledEvent(
            requested: addon,
            alreadyInstalled: installedAddon
          );
        }
        return new ConflictingDestinationPathEvent(
          conflict: addon,
          addon: installedAddon
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
            return new DependencyAlreadyInstalledEvent(
              requested: addon,
              alreadyInstalled: existingAddon
            );
          }
          else {
            // name, subfolder, or branch are different, but url is the same
            conflicts.Add(existingAddon);
          }
        }

        // Similar dependency warnings don't prevent an installation from
        // occurring, as they are probably desired. We do our best to warn
        // the user regardless in case it is a mistake.
        MarkInstalled(addon);

        return new SimilarDependencyWarning(
          conflict: addon,
          addons: conflicts
        );
      }

      MarkInstalled(addon);
      return new DependencyCanBeInstalledEvent(addon);
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
