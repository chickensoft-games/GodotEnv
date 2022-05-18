namespace Chickensoft.GoDotAddon.Tests {
  using System.IO.Abstractions;
  using System.Threading.Tasks;
  using global::GoDotAddon;

  using Moq;

  using Xunit;

  public class AddonRepoTest {
    private const string ADDON_NAME = "addon";
    private const string ADDON_URL = "url";
    private const string ADDON_SUBFOLDER = "subfolder";
    private const string ADDON_CHECKOUT = "main";

    [Fact]
    public async void CacheAddonCachesAddon() {
      var cachePath = ".addons/";
      var cachedAddonDir = $"{cachePath}{ADDON_NAME}";

      var app = new Mock<IApp>();
      var fs = new Mock<IFileSystem>();
      var directory = new Mock<IDirectory>();

      fs.Setup(fs => fs.Directory).Returns(directory.Object);
      directory.Setup(dir => dir.Exists(cachedAddonDir)).Returns(false);

      var cachePathShell = new Mock<IShell>(MockBehavior.Strict);
      var addonPathShell = new Mock<IShell>(MockBehavior.Strict);

      app.Setup(app => app.FS).Returns(fs.Object);
      app.Setup(app => app.CreateShell(cachePath))
        .Returns(cachePathShell.Object);
      app.Setup(app => app.CreateShell(cachedAddonDir))
        .Returns(addonPathShell.Object);

      var cachePathShellSeq = new MockSequence();
      cachePathShell.InSequence(cachePathShellSeq).Setup(
        shell => shell.Run(
          "git", "clone", "--recurse-submodules", ADDON_URL, ADDON_NAME
        )
      ).Returns(Task.FromResult(new ProcessResult(0)));

      var addonPathShellSeq = new MockSequence();
      addonPathShell.InSequence(addonPathShellSeq).Setup(
        shell => shell.Run("git", "checkout", ADDON_CHECKOUT)
      ).Returns(Task.FromResult(new ProcessResult(0)));
      addonPathShell.InSequence(addonPathShellSeq).Setup(
        shell => shell.RunUnchecked("git", "pull")
      ).Returns(Task.FromResult(new ProcessResult(0)));
      addonPathShell.InSequence(addonPathShellSeq).Setup(
        shell => shell.RunUnchecked(
          "git", "submodule", "update", "--init", "--recursive"
        )
      ).Returns(Task.FromResult(new ProcessResult(0)));

      var addonRepo = new AddonRepo(app.Object);
      var addon = new RequiredAddon(ADDON_NAME, ADDON_URL, ADDON_SUBFOLDER, ADDON_CHECKOUT);

      await addonRepo.CacheAddon(addon, cachePath);

      directory.VerifyAll();
      cachePathShell.VerifyAll();
      addonPathShell.VerifyAll();
    }
  }
}
