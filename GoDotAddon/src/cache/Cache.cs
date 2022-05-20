namespace Chickensoft.GoDotAddon {
  using System.Collections.Generic;

  /// <summary>
  /// Represents an addon cache. An addon cache is just a folder which contains
  /// git clones of addon repositories.
  /// </summary>
  /// <param name="AddonsInCache">Addons that are cached.</param>
  /// <param name="AddonsNotInCache">Addons that were supposed to be cached
  /// (per the last loaded lock file), but aren't actually in the cache.</param>
  public record Cache(
    HashSet<string> AddonsInCache,
    HashSet<string> AddonsNotInCache
  ) {
    public bool IsInCache(string name) => AddonsInCache.Contains(name);
  }
}
