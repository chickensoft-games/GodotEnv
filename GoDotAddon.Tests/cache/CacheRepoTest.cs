namespace Chickensoft.GoDotAddon.Tests {
  using System.Collections.Generic;
  using System.IO.Abstractions;
  using Chickensoft.GoDotAddon;
  using Moq;
  using Shouldly;
  using Xunit;


  public class CacheRepoTest {
    private const string ADDON_NAME = "GoDotAddon";
    private const string ADDON_URL
      = "git@github.com:chickensoft-games/GoDotAddon.git";
    private const string SUBFOLDER = "/";
    private const string CHECKOUT = "main";

    private const string WORKING_DIR = "./";
    private const string CACHE_PATH = ".addons/";
    private const string ADDONS_PATH = "addons/";
    private readonly string _addonPath
      = $"{CACHE_PATH}{ADDON_NAME}{SUBFOLDER}";

    [Fact]
    public void LoadsCacheCorrectlyWhenAddonIsCached() {
      var app = new Mock<IApp>();
      var fs = new Mock<IFileSystem>();
      app.Setup(app => app.FS).Returns(fs.Object);
      var cacheRepo = new CacheRepo(app.Object);
      var config = new Config(
        ProjectPath: WORKING_DIR,
        CachePath: CACHE_PATH,
        AddonsPath: ADDONS_PATH
      );
      var lockFile = new Mock<ILockFile>();
      lockFile.Setup(lf => lf.Addons).Returns(
        new Dictionary<string, Dictionary<string, LockFileEntry>>() {
          {
            ADDON_URL,
            new Dictionary<string, LockFileEntry>() {
              {
                SUBFOLDER,
                new LockFileEntry(name: ADDON_NAME, checkout: CHECKOUT)
              }
            }
          }
        }
      );
      var directory = new Mock<IDirectory>();
      fs.Setup(fs => fs.Directory).Returns(directory.Object);
      directory.Setup(dir => dir.GetDirectories(CACHE_PATH)).Returns(
        new string[] { _addonPath }
      );
      var cache = cacheRepo.LoadCache(config, lockFile.Object);
      cache.IsInCache(ADDON_NAME).ShouldBeTrue();
      cache.AddonsNotInCache.ShouldNotContain(ADDON_NAME);
    }
  }
}
