namespace Chickensoft.Chicken;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;
using CliFx.Exceptions;

public interface IAddonRepo {
  /// <summary>
  /// Returns a dictionary of addons in the cache. Each key is the addon's
  /// url and each value is the directory in the cache containing a git clone
  /// of the addon.
  ///
  /// The cache is just a folder (typically `.addons` in a project folder)
  /// which contains git clones of addon repositories.
  /// </summary>
  /// <param name="config">Addon configuration containing paths.</param>
  /// <returns>Map of url's to addon cache directories.</returns>
  void EnsureCacheExists(Config config);
  Task CacheAddon(RequiredAddon addon, Config config);
  Task DeleteAddon(RequiredAddon addon, Config config);
  Task CopyAddonFromCache(
    RequiredAddon addon, Config config, IFileCopier copier
  );
  void InstallAddonWithSymlink(RequiredAddon addon, Config config);
}

public class AddonRepo : IAddonRepo {
  private readonly IApp _app;
  private readonly IFileSystem _fs;

  public AddonRepo(IApp app, IFileSystem fs) { _app = app; _fs = fs; }

  public void EnsureCacheExists(Config config) {
    if (!_fs.Directory.Exists(config.CachePath)) {
      _fs.Directory.CreateDirectory(config.CachePath);
    }
  }

  public async Task CacheAddon(RequiredAddon addon, Config config) {
    var addonCachePath = Path.Combine(config.CachePath, addon.Name);
    if (_fs.Directory.Exists(addonCachePath)) { return; }
    var cachePathShell = _app.CreateShell(config.CachePath);
    await cachePathShell.Run(
      "git", "clone", addon.Url, "--recurse-submodules", addon.Name
    );
  }

  public async Task DeleteAddon(RequiredAddon addon, Config config) {
    var addonPath = Path.Combine(config.AddonsPath, addon.Name);
    if (!_fs.Directory.Exists(addonPath)) { return; }
    if (_app.IsDirectorySymlink(_fs, addonPath)) {
      // We don't need to check git status for symlink'd addons.
      _app.DeleteDirectory(_fs, addonPath);
      return;
    }
    var shell = _app.CreateShell(addonPath);
    var status = await shell.RunUnchecked(
      "git", "status", "--porcelain"
    );
    if (status.StandardOutput.Length == 0) {
      // Installed addon is unmodified by the user, free to delete.
      if (_fs.Path.DirectorySeparatorChar == '\\') {
        // on windows, delete files using command prompt since C# fails
        // to delete .git folders using .net file api's
        await shell.Run("cmd.exe", "/c", "erase", "/s", "/f", "/q", "*");
        var addonsShell = _app.CreateShell(config.AddonsPath);
        await addonsShell.RunUnchecked("cmd.exe", "/c", "rmdir", addon.Name, "/s", "/q");
        return;
      }
      _app.DeleteDirectory(_fs, addonPath);
    }
    else {
      throw new CommandException(
        $"Cannot delete modified addon {addon.Name}. Please backup or " +
        "discard your changes and delete the addon manually." +
        "\n" + status.StandardOutput
      );
    }
  }

  public async Task CopyAddonFromCache(
    RequiredAddon addon, Config config, IFileCopier copier
  ) {
    // make sure correct branch is checked out in cache
    var addonCachePath = Path.Combine(config.CachePath, addon.Name);
    var addonCacheShell = _app.CreateShell(addonCachePath);
    await addonCacheShell.Run("git", "checkout", addon.Checkout);
    await addonCacheShell.RunUnchecked("git", "pull");
    await addonCacheShell.RunUnchecked(
      "git", "submodule", "update", "--init", "--recursive"
    );
    // copy addon from cache to installation location
    var workingShell = _app.CreateShell(config.ProjectPath);
    var copyFromPath = Path.Combine(config.CachePath, addon.Name);
    var subfolder = addon.Subfolder;
    if (subfolder != "/") {
      copyFromPath = Path.Combine(copyFromPath, subfolder);
    }
    // Add a trailing slash to the source directory we are copying from.
    // This is very important for rsync to copy correctly.
    // https://unix.stackexchange.com/a/178095
    copyFromPath = copyFromPath.TrimEnd(Path.DirectorySeparatorChar) +
      Path.DirectorySeparatorChar;
    var addonInstallPath = Path.Combine(config.AddonsPath, addon.Name)
      .TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
    // copy files from addon cache to addon dir, excluding git folders.
    await copier.Copy(_fs, workingShell, copyFromPath, addonInstallPath);
    var addonShell = _app.CreateShell(addonInstallPath);
    // Make a junk repo in the installed addon dir. We use this for change
    // tracking to avoid deleting a modified addon.
    await addonShell.Run("git", "init");
    await addonShell.Run("git", "add", "-A");
    await addonShell.Run(
      "git", "commit", "-m", "Initial commit"
    );
  }

  // Creates a symlink to the addon's url (which should be a local file path)
  public void InstallAddonWithSymlink(RequiredAddon addon, Config config) {
    if (!(addon as ISourceRepository).IsSymlink) {
      throw new CommandException(
        $"Addon {addon.Name} is not a symlink addon."
      );
    }

    var source = addon.Url;

    var subfolder = addon.Subfolder;
    if (subfolder != "/") {
      source = Path.Combine(source, subfolder);
    }

    var target = Path.Combine(config.AddonsPath, addon.Name);

    if (_fs.Directory.Exists(target)) {
      throw new CommandException(
        $"Addon \"{addon.Name}\" already installed. Please delete the " +
        "existing addon and try again."
      );
    }

    if (!_fs.Directory.Exists(source)) {
      throw new CommandException(
        $"Addon \"{addon.Name}\" cannot be found at `{source}`."
      );
    }

    try {
      _app.CreateSymlink(_fs, target, source);
    }
    catch {
      throw new CommandException(
        $"Failed to create symlink for addon \"{addon.Name}\"."
      );
    }
  }
}
