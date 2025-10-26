namespace Chickensoft.GodotEnv.Features.Addons.Models;

public record ResolvedAddon
{
  public IAddon Addon { get; }

  /// <summary>
  /// The canonical version of this addon that was already resolved in the
  /// dependency graph, if any.
  /// </summary>
  public IAddon? CanonicalAddon { get; }

  public ResolvedAddon(IAddon addon, IAddon? canonicalAddon)
  {
    Addon = addon;
    CanonicalAddon = canonicalAddon;
  }

  public string CacheName => CanonicalAddon?.Name ?? Addon.Name;
}
