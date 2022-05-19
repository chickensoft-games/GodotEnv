namespace GoDotAddon {
  using System.IO.Abstractions;
  using CliFx.Exceptions;

  public interface IAddonRepo {
    Task CacheAddon(RequiredAddon addon, Config config);
    Task DeleteAddon(RequiredAddon addon, Config config);
    Task CopyAddonFromCache(RequiredAddon addon, Config config);
  }

  public class AddonRepo : IAddonRepo {
    private readonly IApp _app;
    private readonly IFileSystem _fs;

    public AddonRepo(IApp app) {
      _app = app;
      _fs = app.FS;
    }

    public async Task CacheAddon(RequiredAddon addon, Config config) {
      var addonCachePath = Path.Combine(config.CachePath, addon.Name);
      if (_fs.Directory.Exists(addonCachePath)) { return; }
      var cachePathShell = _app.CreateShell(config.CachePath);
      await cachePathShell.Run(
        "git", "clone", "--recurse-submodules", addon.Url, addon.Name
      );
    }

    public async Task DeleteAddon(RequiredAddon addon, Config config) {
      var addonDir = Path.Combine(config.AddonsPath, addon.Name);
      if (!_fs.Directory.Exists(addonDir)) { return; }
      var status = await _app.CreateShell(addonDir).RunUnchecked(
        "git", "status", "--porcelain"
      );
      if (status.Success) {
        // Installed addon is unmodified by the user, free to delete.
        await _app.CreateShell(config.AddonsPath).Run("rm", "-rf", addonDir);
      }
      else {
        throw new CommandException(
          $"Cannot delete modified addon {addon}. Please backup or discard " +
          "your changes and delete the addon manually." +
          "\n" + status.StandardOutput
        );
      }

    }

    public async Task CopyAddonFromCache(RequiredAddon addon, Config config) {
      // make sure correct branch is checked out in cache
      var addonCachePath = Path.Combine(config.CachePath, addon.Name);
      var addonCacheShell = _app.CreateShell(addonCachePath);
      await addonCacheShell.Run("git", "checkout", "-f", addon.Checkout);
      await addonCacheShell.RunUnchecked("git", "pull");
      await addonCacheShell.RunUnchecked(
        "git", "submodule", "update", "--init", "--recursive"
      );
      // copy addon from cache to installation location
      var workingShell = _app.CreateShell(config.ProjectPath);
      var copyFromDir
        = Path.Combine(config.CachePath, addon.Name, addon.Subfolder);
      var addonInstallPath = Path.Combine(config.AddonsPath, addon.Name);
      // copy files from addon cache to addon dir, excluding git folders.
      await workingShell.Run(
        "rsync", "-av", copyFromDir, addonInstallPath, "--exclude", ".git"
      );
      var addonShell = _app.CreateShell(addonInstallPath);
      // Make a junk repo in the installed addon dir. We use this for change
      // tracking to avoid deleting a modified addon.
      await addonCacheShell.Run("git", "init");
      await addonCacheShell.Run("git", "add", ".");
      await addonCacheShell.Run(
        "git", "commit", "-m", "Initial commit"
      );
    }
  }
}
