namespace Chickensoft.GoDotAddon {
  using System.Collections.Generic;
  using System.IO.Abstractions;
  using System.Threading.Tasks;

  public class CacheRepo {
    private readonly IFileSystem _fs;
    private readonly IApp _app;

    public CacheRepo(IApp app) {
      _app = app;
      _fs = app.FS;
    }

    /// <summary>
    /// Returns a set of url's that have been cloned to the cache.
    ///
    /// The cache is just a folder (typically `.addons` in a project folder)
    /// which contains git clones of addon repositories.
    /// </summary>
    /// <param name="config">Addon configuration containing paths.</param>
    /// <returns>Set of url's contained in the cache.</returns>
    public async Task<HashSet<string>> LoadCache(
      Config config
    ) {
      if (!_fs.Directory.Exists(config.CachePath)) {
        _fs.Directory.CreateDirectory(config.CachePath);
      }
      var urls = new HashSet<string>();
      var directoriesInCachePath = _fs.Directory.GetDirectories(
        config.CachePath
      );
      foreach (var directory in directoriesInCachePath) {
        var shell = _app.CreateShell(directory);
        var result = await shell.Run("git", "remote", "get-url", "origin");
        var url = result.StandardOutput.Trim();
        urls.Add(url);
      }
      return urls;
    }
  }
}
