namespace GoDotAddon {
  public class AddonManager {
    private readonly IAddonRepo _addonRepo;

    public AddonManager(IAddonRepo addonRepo) => _addonRepo = addonRepo;

    public async Task InstallAddon(
      RequiredAddon addon, Config config, bool force
    ) {
      var cachedAddonDir = Path.Combine(
        config.CachePath, addon.Name, addon.Subfolder
      );
      var addonDir = Path.Combine(config.AddonsPath, addon.Name);
      await _addonRepo.CacheAddon(addon: addon, cachePath: config.CachePath);
      await _addonRepo.DeleteExistingInstalledAddon(
        addon: addon, addonsPath: config.AddonsPath, force: force
      );
      await _addonRepo.CopyAddonFromCacheToDestination(
        workingDir: config.WorkingDir,
        cachedAddonDir: cachedAddonDir,
        addonDir: addonDir
      );
    }
  }
}
