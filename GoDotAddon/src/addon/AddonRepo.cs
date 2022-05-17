namespace GoDotAddon {
  using System.IO.Abstractions;
  using CliFx.Exceptions;

  public interface IAddonRepo {
    Task InstallAddon(RequiredAddon addon, Config config, bool force);
  }

  public class AddonRepo : IAddonRepo {
    private readonly IApp _app;
    private readonly IFileSystem _fs;

    public AddonRepo(IApp app) {
      _app = app;
      _fs = app.FS;
    }

    private async Task CacheAddon(
      RequiredAddon addon, string cachePath
    ) {
      // makes sure we have a clone of the addon in the .addons cache directory.
      var cachedAddonDir = Path.Combine(addon.Name, cachePath);
      if (!_fs.Directory.Exists(cachedAddonDir)) {
        await _app.CreateShell(
          cachePath
        ).Run("git", "clone", "--recurse-submodule", addon.Url, addon.Name);
      }
      // Folder for addon already exists. Make sure we checkout the
      // appropriate ref and make sure it's up to date.
      await _app.CreateShell(cachePath).Run("git", "checkout", addon.Checkout);
      // If we're on a tag or something that isn't a branch, this will fail.
      // We don't care in that situation, so ignore the error.
      // If we did specify a branch, this will make sure the branch is
      // up-to-date.
      var addonShell = _app.CreateShell(cachedAddonDir);
      await addonShell.RunUnchecked("git", "pull");
      await addonShell.RunUnchecked(
        "git", "submodule", "update", "--init", "--recursive"
      );
    }

    private async Task DeleteExistingInstalledAddon(
      RequiredAddon addon, string addonsPath, bool force
    ) {
      var addonDir = Path.Combine(addonsPath, addon.Name);
      if (_fs.Directory.Exists(addonDir)) {
        var status = await _app.CreateShell(addonDir).RunUnchecked(
          "git", "status", "--porcelain"
        );
        if (status.ExitCode != 0) {
          if (force) {
            await _app.CreateShell(addonsPath).Run("rm", "-rf", addonDir);
          }
          else {
            // git status dirty, don't delete an existing installed addon that
            // has been modified.
            throw new CommandException(
              $"Cannot delete modified addon {addon}. You may backup your " +
              "changes elsewhere, delete the addon yourself, or use --force." +
              "\n" + status.StandardOutput
            );
          }
        }
      }
    }

    // installs the addon from the
    private async Task CopyAddonFromCacheToDestination(
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

    public async Task InstallAddon(
      RequiredAddon addon, Config config, bool force
    ) {
      var cachedAddonDir = Path.Combine(
        config.CachePath, addon.Name, addon.Subfolder
      );
      var addonDir = Path.Combine(config.AddonsPath, addon.Name);

      await CacheAddon(addon: addon, cachePath: config.CachePath);
      await DeleteExistingInstalledAddon(
        addon: addon, addonsPath: config.AddonsPath, force: force
      );
      await CopyAddonFromCacheToDestination(
        workingDir: config.WorkingDir,
        cachedAddonDir: cachedAddonDir,
        addonDir: addonDir
      );
    }
  }
}
