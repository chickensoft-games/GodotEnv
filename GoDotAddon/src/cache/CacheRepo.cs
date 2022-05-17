namespace GoDotAddon {
  using System.IO.Abstractions;

  public class CacheRepo {
    private readonly IFileSystem _fs;
    private readonly IApp _app;

    public CacheRepo(IApp app) {
      _app = app;
      _fs = app.FS;
    }

    public Cache LoadCache(
      Config config, ILockFile lockFile
    ) {
      var requiredUrls = lockFile.Addons.Keys.ToArray();
      // names of addons that we don't have in the cache.
      var addonsNotInCache = new HashSet<string>();
      // figure out which addons we are *supposed* to have in the cache,
      // based on the last saved lock file. start the list of addons we
      // don't have with everything we think we might have, then narrow it down.
      foreach (var requiredUrl in requiredUrls) {
        var subfolders = lockFile.Addons[requiredUrl].Keys.ToArray();
        foreach (var subfolder in subfolders) {
          var lockFileEntry = lockFile.Addons[requiredUrl][subfolder];
          addonsNotInCache.Add(lockFileEntry.Name);
        }
      }
      // get all the directories that are actually in the cache
      var cacheDirs = _fs.Directory.GetDirectories(config.CachePath);
      var addonsInCache = new HashSet<string>();
      foreach (var cacheDir in cacheDirs) {
        // get the name of the last subfolder in the path:
        var name = new DirectoryInfo(cacheDir).Name;
        addonsInCache.Add(name);
        // since the addon is in the cache, it shouldn't be in our list of
        // addons not in the cache.
        addonsNotInCache.Remove(name);
      }
      return new Cache(
        AddonsInCache: addonsInCache,
        AddonsNotInCache: addonsNotInCache
      );
    }
  }
}
