namespace Chickensoft.GodotEnv;
using System.Collections.Generic;
using System.Linq;
using Chickensoft.GodotEnv.Features.Addons.Models;

public interface IAddonGraph {
  /// <summary>
  /// All addons added to the graph. This list is valid as long as each result
  /// from adding the addons is not a <see cref="IAddonGraphFailureResult"/>.
  /// </summary>
  IEnumerable<IAddon> Addons { get; }

  /// <summary>
  /// Add an addon to the graph. If the addon doesn't conflict with any other
  /// addons, a <see cref="AddonResolved"/> is returned. Otherwise, a
  /// relevant <see cref="IAddonGraphResult"/> is returned. Addon results that
  /// prohibit an addon from being installed implement
  /// <see cref="IAddonGraphFailureResult"/>.
  /// </summary>
  /// <param name="addon"></param>
  IAddonGraphResult Add(IAddon addon);
}

/// <summary>
/// Represents an addon dependency graph. Addons are resolved in a flat
/// dependency graph.
/// </summary>
public class AddonGraph : IAddonGraph {
  private readonly Dictionary<string, HashSet<IAddon>> _addonsByUrl
    = new();

  private readonly Dictionary<string, IAddon> _addonsByName
    = new();

  private readonly Dictionary<string, IAddon> _canonicalAddonsByUrl
    = new(); // the first addon added for a given url.

  public IEnumerable<IAddon> Addons => _addonsByName.Values.OrderBy(
    addon => addon.Name
  );

  public IAddonGraphResult Add(IAddon addon) {
    // First, check to make sure another addon isn't installed to the same
    // path as the new one.
    if (_addonsByName.TryGetValue(addon.Name, out var installedAddon)) {
      if (
        installedAddon.NormalizedUrl == addon.NormalizedUrl &&
        installedAddon.Subfolder == addon.Subfolder &&
        installedAddon.Checkout == addon.Checkout
      ) {
        // Dependency is already installed under the *same* name.
        return new AddonAlreadyResolved(
          Addon: addon,
          CanonicalAddon: installedAddon
        );
      }

      // We can't install two addons that have the same name but a different
      // url, subfolder, or checkout.
      return new AddonCannotBeResolved(
        Addon: addon,
        CanonicalAddon: installedAddon
      );
    }

    // Check for similar dependencies in the flat dependency graph to
    // alert the user to potential conflicts.
    if (_addonsByUrl.TryGetValue(
      addon.NormalizedUrl, out var addonsWithSameUrl
    )) {
      var conflicts = new List<IAddon>();
      foreach (var existingAddon in addonsWithSameUrl) {
        if (
          existingAddon.Subfolder == addon.Subfolder
          && existingAddon.Checkout == addon.Checkout
        ) {
          // Dependency is already installed under a *different* name.
          return new AddonAlreadyResolved(
            Addon: addon,
            CanonicalAddon: existingAddon
          );
        }
        // name, subfolder, or branch are different, but url is the same
        conflicts.Add(existingAddon);
      }

      // Similar dependency warnings don't prevent an installation from
      // occurring. In case it's a mistake, we warn the user but we still
      // allow it because there are scenarios where it is desirable.
      MarkAdded(addon);

      return new AddonResolvedButMightConflict(
        Addon: addon,
        Conflicts: conflicts,
        CanonicalAddon: _canonicalAddonsByUrl[addon.NormalizedUrl]
      );
    }

    MarkAdded(addon);
    return new AddonResolved(addon);
  }

  private void MarkAdded(IAddon addon) {
    if (_addonsByUrl.TryGetValue(
      addon.NormalizedUrl, out var addonsWithSameUrl
    )) {
      addonsWithSameUrl.Add(addon);
    }
    else {
      _canonicalAddonsByUrl[addon.NormalizedUrl] = addon;
      _addonsByUrl[addon.NormalizedUrl] = new HashSet<IAddon>() { addon };
    }
    _addonsByName.Add(addon.Name, addon);
  }
}
