namespace GoDotAddon {
  using System.IO.Abstractions;
  using CliFx.Exceptions;

  public interface IAddonRepo {
    Task CacheAddon(RequiredAddon addon, string cachePath);
    Task DeleteExistingInstalledAddon(
      RequiredAddon addon, string addonsPath
    );
    Task CopyAddonFromCacheToDestination(
      string workingDir, string cachedAddonDir, string addonDir
    );
  }

  public class AddonRepo : IAddonRepo {
    private readonly IApp _app;
    private readonly IFileSystem _fs;

    public AddonRepo(IApp app) {
      _app = app;
      _fs = app.FS;
    }

    public async Task CacheAddon(
      RequiredAddon addon, string cachePath
    ) {
      var cachedAddonDir = Path.Combine(cachePath, addon.Name);
      if (!_fs.Directory.Exists(cachedAddonDir)) {
        var cachePathShell = _app.CreateShell(cachePath);
        await cachePathShell.Run("git", "clone", "--recurse-submodules", addon.Url, addon.Name);
      }
      var addonShell = _app.CreateShell(cachedAddonDir);
      await addonShell.Run("git", "checkout", addon.Checkout);
      await addonShell.RunUnchecked("git", "pull");
      await addonShell.RunUnchecked(
        "git", "submodule", "update", "--init", "--recursive"
      );
    }

    public async Task DeleteExistingInstalledAddon(
      RequiredAddon addon, string addonsPath
    ) {
      var addonDir = Path.Combine(addonsPath, addon.Name);
      if (_fs.Directory.Exists(addonDir)) {
        var status = await _app.CreateShell(addonDir).RunUnchecked(
          "git", "status", "--porcelain"
        );
        if (status.Success) {
          // Installed addon is unmodified by the user, free to delete.
          await _app.CreateShell(addonsPath).Run("rm", "-rf", addonDir);
        }
        else {
          throw new CommandException(
            $"Cannot delete modified addon {addon}. Please backup or discard " +
            "your changes and delete the addon manually." +
            "\n" + status.StandardOutput
          );
        }
      }
    }

    // installs the addon from the
    public async Task CopyAddonFromCacheToDestination(
      string workingDir, string cachedAddonDir, string addonDir
    ) {
      // copy addon from cache to installation location
      var addonShell = _app.CreateShell(workingDir);
      await addonShell.Run(
        "cp", "-r", cachedAddonDir, addonDir
      );
      // find any `.git` folders and remove them
      var gitDirs = _fs.Directory.GetDirectories(addonDir, ".git");
      // remove any nested git folders from submodules, etc
      foreach (var gitDir in gitDirs) {
        _fs.Directory.Delete(gitDir, recursive: true);
      }
      // Make a junk repo in the installed addon dir. We use this for change
      // tracking to avoid deleting a modified addon.
      await addonShell.Run("git", "init");
      await addonShell.Run("git", "add", ".");
      await addonShell.Run(
        "git", "commit", "-m", "Initial commit"
      );
    }
  }
}
