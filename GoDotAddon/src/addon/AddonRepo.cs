namespace GoDotAddon {
  using System.IO.Abstractions;
  using CliFx.Exceptions;

  public interface IProvisionRepo {
    Task InstallAddon(RequiredAddon addon, Config config, bool force);
  }

  public class ProvisionRepo : IProvisionRepo {
    private readonly IApp _app;
    private readonly FileSystem _fs;

    public ProvisionRepo(IApp app) {
      _app = app;
      _fs = app.FS;
    }

    private async Task CacheAddon(
      RequiredAddon addon, string cacheDir
    ) {
      // makes sure we have a clone of the addon in the .addons cache directory.
      var cachedAddonDir = Path.Combine(addon.Name, cacheDir);
      if (!_fs.Directory.Exists(cachedAddonDir)) {
        await _app.CreateShell(
          cacheDir
        ).Run("git", "clone", "--recurse-submodule", addon.Url, addon.Name);
      }
      // Folder for addon already exists. Make sure we checkout the
      // appropriate ref and make sure it's up to date.
      await _app.CreateShell(cacheDir).Run("git", "checkout", addon.Checkout);
      // If we're on a tag or something that isn't a branch, this will fail.
      // We don't care in that situation, so ignore the error.
      // If we did specify a branch, this will make sure the branch is
      // up-to-date.
      var addonShell = _app.CreateShell(cachedAddonDir);
      await addonShell.RunRegardless("git", "pull");
      await addonShell.RunRegardless(
        "git", "submodule", "update", "--init", "--recursive"
      );
    }

    private async Task DeleteExistingInstalledAddon(
      RequiredAddon addon, string addonsPath, bool force
    ) {
      var addonDir = Path.Combine(addonsPath, addon.Name);
      if (_fs.Directory.Exists(addonDir)) {
        var status = await _app.CreateShell(addonDir).ManualRun(
          "git", "status", "--porcelain"
        );
        if (!status.Success) {
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
    private async Task InstallRequiredAddon(
      RequiredAddon addon, string cacheDir, string workingDir, string addonDir
    ) {
      var cachedAddonDir = Path.Combine(
        cacheDir, addon.Name, addon.Subfolder
      );
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
      var addonsPath = Path.Combine(config.WorkingDir, config.Path);
      var cacheDir = Path.Combine(config.WorkingDir, config.CacheDir);
      var addonDir = Path.Combine(addonsPath, addon.Name);

      await CacheAddon(addon: addon, cacheDir: cacheDir);
      await DeleteExistingInstalledAddon(
        addon: addon, addonsPath: addonsPath, force: force
      );
      await InstallRequiredAddon(
        addon: addon,
        cacheDir: cacheDir,
        workingDir: config.WorkingDir,
        addonDir: addonDir
      );
    }
  }
}
