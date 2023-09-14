namespace Chickensoft.GodotEnv.Features.Addons.Models;

public record ResolvedAddon {
  public IAddon Addon { get; }
  public IAddon? CanonicalAddon { get; }

  public ResolvedAddon(IAddon addon, IAddon? canonicalAddon) {
    Addon = addon;
    CanonicalAddon = canonicalAddon;
  }

  public string CacheName => CanonicalAddon?.Name ?? Addon.Name;
}
