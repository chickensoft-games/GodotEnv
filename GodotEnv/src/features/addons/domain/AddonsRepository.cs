namespace Chickensoft.GodotEnv.Features.Addons.Domain;

using System.IO;
using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Common.Utilities;
using Chickensoft.GodotEnv.Features.Addons.Models;

public interface IAddonsRepository {
  IFileClient FileClient { get; }
  AddonsConfiguration Config { get; }
  IComputer Computer { get; }

  /// <summary>
  /// <para>
  /// Given an asset and the path where the addon config resides,
  /// compute the actual addon's source url.
  /// </para>
  /// <para>
  /// For addons sourced on the local machine, this will convert relative
  /// paths into absolute paths.
  /// </para>
  /// </summary>
  /// <param name="asset">Asset.</param>
  /// <param name="path">Path containing the addons.json the addon was
  /// required from.</param>
  /// <returns>Resolved addon source.</returns>
  string ResolveUrl(IAsset asset, string path);
  /// <summary>
  /// Caches an addon and returns the fully qualified path to the cached addon.
  /// An addon is cached by copying it or cloning it to a project's addon cache
  /// directory which can be configured in an addons.json file.
  /// </summary>
  /// <param name="addon">Addon.</param>
  /// <param name="cacheName">Name of the directory in the cache to clone the
  /// addon to.</param>
  /// <returns>Fully qualified path to the cached addon.</returns>
  Task<string> CacheAddon(IAddon addon, string cacheName);
  /// <summary>
  /// Updates a cached addon by pulling the latest changes from the source
  /// repository and initializing or updating submodules.
  /// </summary>
  /// <param name="addon">Addon.</param>
  /// <param name="cacheName">Name of the directory in the cache to clone the
  /// addon to.</param>
  /// <returns>Task that completes when the cache is updated.</returns>
  Task UpdateCache(IAddon addon, string cacheName);
  /// <summary>
  /// Prepares a cached addon by ensuring the branch the addon requires is
  /// checked out.
  /// </summary>
  /// <param name="addon">Addon.</param>
  /// <param name="cacheName">Name of the directory in the cache containing
  /// the addon assets.</param>
  /// <returns>Task that completes when the cache is prepared.</returns>
  Task PrepareCache(IAddon addon, string cacheName);
  /// <summary>
  /// Copies a cached addon to a project's addons installation directory.
  /// </summary>
  /// <param name="addon"></param>
  /// <param name="cacheName">Name of the directory in the cache containing
  /// the addon assets.</param>
  /// <returns>Task that completes when the addon is copied.</returns>
  Task InstallAddonFromCache(IAddon addon, string cacheName);
  /// <summary>
  /// Deletes an installed addon.
  /// </summary>
  /// <param name="addon">Addon.</param>
  Task DeleteAddon(IAddon addon);
  /// <summary>
  /// Ensures the cache directory exists.
  /// </summary>
  void EnsureCacheAndAddonsDirectoriesExists();
  /// <summary>
  /// Installs a local addon using a symlink instead of copying the addon.
  /// </summary>
  /// <param name="addon"></param>
  void InstallAddonWithSymlink(IAddon addon);
}

public class AddonsRepository(
  IFileClient fileClient,
  IComputer computer,
  AddonsConfiguration config
) : IAddonsRepository {
  public IFileClient FileClient { get; } = fileClient;
  public IComputer Computer { get; } = computer;
  public AddonsConfiguration Config { get; } = config;

  public string ResolveUrl(IAsset asset, string path) {
    var url = asset.Url;
    if (asset.IsRemote) { return url; }
    // If the path containing the addons.json is a symlink, determine the
    // actual path containing the addons.json file. This allows addons
    // that have their own addons with relative paths to be relative to
    // where the addon is actually stored, which is more intuitive.
    if (FileClient.IsDirectorySymlink(path)) {
      path = FileClient.DirectorySymlinkTarget(path);
    }

    // Support user directory (~ tilde expansion) in file paths.
    url = url.Replace("~", FileClient.UserDirectory);

    // Locally sourced addons with relative paths are relative to the
    // addons.json file that defines them.
    return FileClient.GetRootedPath(url, path);
  }

  public void EnsureCacheAndAddonsDirectoriesExists() {
    if (!FileClient.DirectoryExists(Config.CachePath)) {
      FileClient.CreateDirectory(Config.CachePath);
    }
    if (!FileClient.DirectoryExists(Config.AddonsPath)) {
      FileClient.CreateDirectory(Config.AddonsPath);
    }
  }

  public async Task<string> CacheAddon(IAddon addon, string cacheName) {
    if (addon.IsSymlink) {
      // Return what should be the resolved url: that is, what the symlink
      // is actually pointing to. Symlink addons are not cached.
      return FileClient.Combine(
        addon.Url, addon.Subfolder.TrimEnd(FileClient.Separator)
      );
    }
    var addonCachePath = FileClient.Combine(Config.CachePath, cacheName);
    if (!FileClient.DirectoryExists(addonCachePath)) {
      var addonsCacheShell = Computer.CreateShell(Config.CachePath);
      await addonsCacheShell.Run(
        "git", "clone", addon.Url, "--recurse-submodules", cacheName
      );
    }
    return FileClient.Combine(
      addonCachePath, addon.Subfolder.TrimEnd(FileClient.Separator)
    );
  }

  public async Task UpdateCache(IAddon addon, string cacheName) {
    if (addon.IsSymlink) { return; }
    var addonCachePath = FileClient.Combine(Config.CachePath, cacheName);
    var addonCacheShell = Computer.CreateShell(addonCachePath);
    await addonCacheShell.RunUnchecked("git", "clean", "-fdx");
    await addonCacheShell.RunUnchecked("git", "pull");
    await addonCacheShell.RunUnchecked(
      "git",
      "submodule", "update", "--init", "--recursive", "--rebase", "--force"
    );
  }

  public async Task PrepareCache(IAddon addon, string cacheName) {
    if (addon.IsSymlink) { return; }
    var addonCachePath = FileClient.Combine(Config.CachePath, cacheName);
    var addonCacheShell = Computer.CreateShell(addonCachePath);
    await addonCacheShell.RunUnchecked("git", "clean", "-fdx");
    await addonCacheShell.Run("git", "checkout", addon.Checkout);
  }

  public async Task DeleteAddon(IAddon addon) {
    var addonPath = FileClient.Combine(Config.AddonsPath, addon.Name);
    if (!FileClient.DirectoryExists(addonPath)) { return; }
    if (FileClient.IsDirectorySymlink(addonPath)) {
      // We don't need to check git status for symlink'd addons.
      await FileClient.DeleteDirectory(addonPath);
      return;
    }
    var addonShell = Computer.CreateShell(addonPath);
    var status = await addonShell.RunUnchecked(
      "git", "status", "--porcelain"
    );
    if (status.StandardOutput.Length == 0) {
      // Installed addon is unmodified by the user, free to delete.
      if (FileClient.OSFamily == OSFamily.Windows) {
        // On windows, delete files using command prompt (since C# fails
        // to delete .git folders using .net file api's)
        // TODO: Use FileClient.DeleteDirectory() on windows
        await addonShell.Run("cmd.exe", "/c", "erase", "/s", "/f", "/q", "*");
        var addonsPathShell = Computer.CreateShell(Config.AddonsPath);
        await addonsPathShell.RunUnchecked(
          "cmd.exe", "/c", "rmdir", addon.Name, "/s", "/q"
        );
        return;
      }
      await FileClient.DeleteDirectory(addonPath);
    }
    else {
      throw new IOException(
        $"Cannot delete modified addon {addon.Name}. Please backup or " +
        "discard your changes and delete the addon manually." +
        "\n" + status.StandardOutput
      );
    }
  }

  public async Task InstallAddonFromCache(IAddon addon, string cacheName) {
    var addonCachePath = FileClient.Combine(Config.CachePath, cacheName);
    // copy addon from cache to installation location
    var projectShell = Computer.CreateShell(Config.ProjectPath);
    var copyFromPath = addonCachePath;
    var subfolder = addon.Subfolder;
    if (subfolder != "/") {
      copyFromPath = FileClient.Combine(copyFromPath, subfolder);
    }
    // Add a trailing slash to the source directory we are copying from.
    // This is very important for rsync to copy correctly.
    // https://unix.stackexchange.com/a/178095
    copyFromPath = copyFromPath.TrimEnd(FileClient.Separator) +
      FileClient.Separator;
    var addonInstallPath = FileClient.Combine(Config.AddonsPath, addon.Name)
      .TrimEnd(FileClient.Separator) + FileClient.Separator;
    // copy files from addon cache to addon dir, excluding git folders.
    await FileClient.CopyBulk(projectShell, copyFromPath, addonInstallPath);
    var addonShell = Computer.CreateShell(addonInstallPath);
    // Make a junk repo in the installed addon dir. We use this for change
    // tracking to avoid deleting a modified addon.
    await addonShell.Run("git", "init");
    await addonShell.Run(
      "git", "config", "--local", "user.email", "godotenv@godotenv.com"
    );
    await addonShell.Run(
      "git", "config", "--local", "user.name", "GodotEnv"
    );
    await addonShell.Run("git", "add", "-A");
    await addonShell.Run(
      "git", "commit", "-m", "Initial commit"
    );
  }

  public void InstallAddonWithSymlink(IAddon addon) {
    // Creates a symlink to the addon's url (which should be a local file path)
    if (!addon.IsSymlink) {
      throw new IOException(
        $"Addon {addon.Name} is not a symlink addon."
      );
    }

    var symlinkSource = addon.Url;
    var symlinkTarget = FileClient.Combine(Config.AddonsPath, addon.Name);

    if (addon.Subfolder != "/") {
      symlinkSource = FileClient.Combine(symlinkSource, addon.Subfolder);
    }

    if (FileClient.DirectoryExists(symlinkTarget)) {
      throw new IOException(
        $"Addon \"{addon.Name}\" already installed. Please delete the " +
        "existing addon and try again."
      );
    }

    if (!FileClient.DirectoryExists(symlinkSource)) {
      throw new IOException(
        $"Addon \"{addon.Name}\" cannot be found at `{symlinkSource}`."
      );
    }

    try {
      FileClient.CreateSymlink(symlinkTarget, symlinkSource);
    }
    catch {
      throw new IOException(
        $"Failed to create symlink for addon \"{addon.Name}\"."
      );
    }
  }
}
