namespace Chickensoft.GodotEnv.Tests;

using System.IO;
using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Common.Utilities;
using Chickensoft.GodotEnv.Features.Addons.Domain;
using Chickensoft.GodotEnv.Features.Addons.Models;
using CliFx.Infrastructure;
using Moq;
using Shouldly;
using Xunit;

public class AddonsRepositoryTest {
  private class Subject(
    IConsole console,
    Mock<IFileClient> client,
    Mock<ILog> log,
    Mock<IComputer> computer,
    AddonsConfiguration config,
    AddonsRepository repo
  ) {
    public IConsole Console { get; } = console;
    public Mock<IFileClient> Client { get; } = client;
    public Mock<ILog> Log { get; } = log;
    public Mock<IComputer> Computer { get; } = computer;
    public AddonsConfiguration Config { get; } = config;
    public AddonsRepository Repo { get; } = repo;
  }

  private const string PROJECT_PATH = "/";
  private const string ADDONS_DIR = "/addons";
  private const string CACHE_DIR = "/.addons";
  private const string WORKING_DIR = "/";

  private static Subject BuildSubject(
    string projectPath = PROJECT_PATH,
    string addonsDir = ADDONS_DIR,
    string cacheDir = CACHE_DIR,
    string workingDir = WORKING_DIR,
    ShellVerifier? cli = null
  ) {
    // Keep tests shorter by using a helper method to build the test subject.
    var console = new FakeInMemoryConsole();
    var client = new Mock<IFileClient>();
    var log = new Mock<ILog>();
    var computer = new Mock<IComputer>();
    var processRunner = new Mock<IProcessRunner>();
    computer
      .Setup(pc => pc.CreateShell(It.IsAny<string>()))
      .Returns((string path) => {
        if (cli?.GetShell(path) is Mock<IShell> shell) {
          return shell.Object;
        }
        throw new System.InvalidOperationException(
          $"Mock shell not found for `{path}`. Please use a " +
          "ShellVerifier to create a mock shell for this directory and stub " +
          "the results of the processes that are expected to run."
        );
      });
    var config = new AddonsConfiguration(projectPath, addonsDir, cacheDir);
    var repo = new AddonsRepository(client.Object, computer.Object, config, processRunner.Object);
    return new Subject(
      console: console,
      client: client,
      log: log,
      computer: computer,
      config: config,
      repo: repo
    );
  }

  [Fact]
  public void Initializes() {
    var subject = BuildSubject();
    var repo = subject.Repo;
    repo.ShouldBeAssignableTo<IAddonsRepository>();
    repo.FileClient.ShouldBe(subject.Client.Object);
    repo.Config.ShouldBe(subject.Config);
    repo.Computer.ShouldBe(subject.Computer.Object);
  }

  [Fact]
  public void ResolveUrlResolvesRemoteUrl() {
    var addon = TestData.Addon with { };

    var subject = BuildSubject();
    var repo = subject.Repo;
    var client = subject.Client;

    var result = repo.ResolveUrl(addon, "/");

    result.ShouldBe(addon.Url);
  }

  [Fact]
  public void ResolveUrlResolvesSymlinkUrl() {
    var url = "/some/local/path";
    var addon = TestData.Addon with { Url = url, Source = AssetSource.Symlink };

    var subject = BuildSubject();
    var repo = subject.Repo;
    var client = subject.Client;

    var path = "/";
    client.Setup(c => c.IsDirectorySymlink(path)).Returns(true);
    client.Setup(c => c.DirectorySymlinkTarget(path)).Returns(path);
    client.Setup(c => c.GetRootedPath(addon.Url, path)).Returns(addon.Url);

    var result = repo.ResolveUrl(addon, path);

    result.ShouldBe(addon.Url);

    client.VerifyAll();
  }

  [Fact]
  public void EnsuresExistsCreatesCacheDir() {
    var subject = BuildSubject();
    var repo = subject.Repo;
    var client = subject.Client;
    client.Setup(c => c.DirectoryExists(CACHE_DIR)).Returns(false);
    repo.EnsureCacheAndAddonsDirectoriesExists();
    client.VerifyAll();
  }

  [Fact]
  public async Task CacheAddonReturnsSymlinkAddonTargetWithSubfolder() {
    var addon = TestData.Addon with {
      Source = AssetSource.Symlink,
      Url = "/some/local/path",
      Subfolder = "some/subfolder"
    };

    var subject = BuildSubject();

    var repo = subject.Repo;
    var client = subject.Client;

    var expected = addon.Url + "/" + addon.Subfolder;

    client.Setup(c => c.Combine(addon.Url, addon.Subfolder)).Returns(expected);

    var result = await repo.CacheAddon(addon, addon.Name);
    result.ShouldBe(expected);

    client.VerifyAll();
  }

  [Fact]
  public async Task CacheAddonCachesAddon() {
    var addon = TestData.Addon with { };

    var addonsCachePath = CACHE_DIR + "/" + addon.Name;

    var cli = new ShellVerifier(CACHE_DIR, addonsCachePath);
    var subject = BuildSubject(cli: cli);

    var repo = subject.Repo;
    var client = subject.Client;

    var addonCachePath = CACHE_DIR + "/" + addon.Name;

    client.Setup(c => c.Combine(CACHE_DIR, addon.Name)).Returns(addonCachePath);
    client.Setup(c => c.DirectoryExists(addonCachePath)).Returns(false);

    cli.Runs(
      CACHE_DIR, new ProcessResult(0),
      "git", "clone", addon.Url, "--recurse-submodules", addon.Name
    );

    await repo.CacheAddon(addon, addon.Name);

    client.VerifyAll();
    cli.VerifyAll();
  }

  [Fact]
  public async Task CacheAddonDoesNotCacheIfAddonIsAlreadyCached() {
    var addon = TestData.Addon with { };

    var cli = new ShellVerifier(CACHE_DIR);
    var subject = BuildSubject(cli: cli);

    var repo = subject.Repo;
    var client = subject.Client;

    var addonCachePath = CACHE_DIR + "/" + addon.Name;

    client.Setup(c => c.Combine(CACHE_DIR, addon.Name)).Returns(addonCachePath);
    client.Setup(c => c.DirectoryExists(addonCachePath)).Returns(true);

    await repo.CacheAddon(addon, addon.Name);

    client.VerifyAll();
    cli.VerifyAll();
  }

  [Fact]
  public async Task DeleteAddonWillNotDeleteNonExistentAddon() {
    var addon = TestData.Addon with { };
    var addonPath = ADDONS_DIR + "/" + addon.Name;

    var cli = new ShellVerifier(addonPath);
    var subject = BuildSubject(cli: cli);

    var repo = subject.Repo;
    var client = subject.Client;

    client.Setup(c => c.Combine(ADDONS_DIR, addon.Name)).Returns(addonPath);
    client.Setup(c => c.DirectoryExists(addonPath)).Returns(false);

    await repo.DeleteAddon(addon);

    client.VerifyAll();
    cli.VerifyAll();
  }

  [Fact]
  public async Task DeleteAddonDeletesSymlinkedAddon() {
    var addon = TestData.Addon with { };
    var addonPath = ADDONS_DIR + "/" + addon.Name;

    var cli = new ShellVerifier(addonPath);
    var subject = BuildSubject(cli: cli);

    var repo = subject.Repo;
    var client = subject.Client;

    client.Setup(c => c.Combine(ADDONS_DIR, addon.Name)).Returns(addonPath);
    client.Setup(c => c.DirectoryExists(addonPath)).Returns(true);
    client.Setup(c => c.IsDirectorySymlink(addonPath)).Returns(true);
    client.Setup(c => c.DeleteDirectory(addonPath));

    await repo.DeleteAddon(addon);

    client.VerifyAll();
    cli.VerifyAll();
  }

  [Fact]
  public async Task DeleteAddonDeletesAddonOnWindows() {
    var addon = TestData.Addon with { };
    var addonPath = ADDONS_DIR + "/" + addon.Name;

    var cli = new ShellVerifier(addonPath, ADDONS_DIR);
    var subject = BuildSubject(cli: cli);

    var repo = subject.Repo;
    var client = subject.Client;

    client.Setup(c => c.Combine(ADDONS_DIR, addon.Name)).Returns(addonPath);
    client.Setup(c => c.DirectoryExists(addonPath)).Returns(true);
    client.Setup(c => c.IsDirectorySymlink(addonPath)).Returns(false);
    client.Setup(c => c.OSFamily).Returns(OSFamily.Windows);

    cli.RunsUnchecked(
      addonPath, new ProcessResult(0), "git", "status", "--porcelain"
    );
    cli.Runs(
      addonPath, new ProcessResult(0),
      "cmd.exe", "/c", "erase", "/s", "/f", "/q", "*"
    );
    cli.RunsUnchecked(
      ADDONS_DIR, new ProcessResult(0),
      "cmd.exe", "/c", "rmdir", addon.Name, "/s", "/q"
    );

    await repo.DeleteAddon(addon);

    client.VerifyAll();
    cli.VerifyAll();
  }

  [Fact]
  public async Task DeleteAddonDeletesAddonOnUnix() {
    var addon = TestData.Addon with { };
    var addonPath = ADDONS_DIR + "/" + addon.Name;

    var cli = new ShellVerifier(addonPath, ADDONS_DIR);
    var subject = BuildSubject(cli: cli);

    var repo = subject.Repo;
    var client = subject.Client;

    client.Setup(c => c.Combine(ADDONS_DIR, addon.Name)).Returns(addonPath);
    client.Setup(c => c.DirectoryExists(addonPath)).Returns(true);
    client.Setup(c => c.IsDirectorySymlink(addonPath)).Returns(false);
    client.Setup(c => c.OSFamily).Returns(OSFamily.Unix);

    cli.RunsUnchecked(
      addonPath, new ProcessResult(0), "git", "status", "--porcelain"
    );

    client.Setup(c => c.DeleteDirectory(addonPath));

    await repo.DeleteAddon(addon);

    client.VerifyAll();
    cli.VerifyAll();
  }

  [Fact]
  public async Task DeleteAddonThrowsIfAddonWasModified() {
    var addon = TestData.Addon with { };
    var addonPath = ADDONS_DIR + "/" + addon.Name;

    var cli = new ShellVerifier(addonPath);
    var subject = BuildSubject(cli: cli);

    var repo = subject.Repo;
    var client = subject.Client;

    client.Setup(c => c.Combine(ADDONS_DIR, addon.Name)).Returns(addonPath);
    client.Setup(c => c.DirectoryExists(addonPath)).Returns(true);
    client.Setup(c => c.IsDirectorySymlink(addonPath)).Returns(false);

    cli.RunsUnchecked(
      addonPath, new ProcessResult(0, StandardOutput: " M file.txt"),
      "git", "status", "--porcelain"
    );

    await Should.ThrowAsync<IOException>(
      async () => await repo.DeleteAddon(addon)
    );

    client.VerifyAll();
    cli.VerifyAll();
  }

  [Fact]
  public async Task InstallAddonFromCacheCopiesCachedAddonToAddonsPath() {
    var addon = TestData.Addon with { };
    var addonCachePath = CACHE_DIR + "/" + addon.Name;
    var copyFromPath = addonCachePath + "/" + addon.Subfolder;
    var addonInstallPath = ADDONS_DIR + "/" + addon.Name + "/";

    var cli = new ShellVerifier(addonCachePath, PROJECT_PATH, addonInstallPath);
    var subject = BuildSubject(cli: cli);

    var repo = subject.Repo;
    var client = subject.Client;

    client.Setup(c => c.Combine(CACHE_DIR, addon.Name)).Returns(addonCachePath);

    client.Setup(c => c.Separator).Returns('/');

    client.Setup(c => c.Combine(addonCachePath, addon.Subfolder))
      .Returns(copyFromPath);

    client.Setup(c => c.Combine(ADDONS_DIR, addon.Name))
      .Returns(addonInstallPath);

    client.Setup(
      c => c.CopyBulk(
        cli.GetShell(PROJECT_PATH).Object,
        copyFromPath + "/",
        addonInstallPath
      )
    );

    cli.Runs(addonInstallPath, new ProcessResult(0), "git", "init");
    cli.Runs(addonInstallPath, new ProcessResult(0), 
      "git", "config", "--local", "user.email", "godotenv@godotenv.com"
    );
    cli.Runs(addonInstallPath, new ProcessResult(0),
      "git", "config", "--local", "user.name", "GodotEnv"
    );
    cli.Runs(addonInstallPath, new ProcessResult(0), "git", "add", "-A");
    cli.Runs(
      addonInstallPath, new ProcessResult(0),
      "git", "commit", "-m", "Initial commit"
    );

    await repo.InstallAddonFromCache(addon, addon.Name);

    client.VerifyAll();
    cli.VerifyAll();
  }

  [Fact]
  public void InstallAddonWithSymlinkRejectsAddonsWithWrongSource() {
    var addon = TestData.Addon with { };

    var subject = BuildSubject();
    var repo = subject.Repo;

    Should.Throw<IOException>(() => repo.InstallAddonWithSymlink(addon));
  }

  [Fact]
  public void InstallAddonWithSymlinkWillNotInstallIfTargetAlreadyExists() {
    var addon = TestData.Addon with { Source = AssetSource.Symlink };

    var subject = BuildSubject();
    var repo = subject.Repo;
    var client = subject.Client;

    var symlinkTarget = ADDONS_DIR + "/" + addon.Name;
    var symlinkSource = addon.Url + "/" + addon.Subfolder;

    client.Setup(c => c.Combine(ADDONS_DIR, addon.Name)).Returns(symlinkTarget);
    client.Setup(c => c.Combine(addon.Url, addon.Subfolder))
      .Returns(symlinkSource);

    client.Setup(c => c.DirectoryExists(symlinkTarget)).Returns(true);

    Should.Throw<IOException>(() => repo.InstallAddonWithSymlink(addon));

    client.VerifyAll();
  }

  [Fact]
  public void InstallAddonWithSymlinkWillNotInstallIfSourceDoesNotExist() {
    var addon = TestData.Addon with { Source = AssetSource.Symlink };

    var subject = BuildSubject();
    var repo = subject.Repo;
    var client = subject.Client;

    var symlinkTarget = ADDONS_DIR + "/" + addon.Name;
    var symlinkSource = addon.Url + "/" + addon.Subfolder;

    client.Setup(c => c.Combine(ADDONS_DIR, addon.Name)).Returns(symlinkTarget);
    client.Setup(c => c.Combine(addon.Url, addon.Subfolder))
      .Returns(symlinkSource);

    client.Setup(c => c.DirectoryExists(symlinkTarget)).Returns(false);
    client.Setup(c => c.DirectoryExists(symlinkSource)).Returns(false);

    Should.Throw<IOException>(() => repo.InstallAddonWithSymlink(addon));

    client.VerifyAll();
  }

  [Fact]
  public void InstallAddonWithSymlinkCreatesSymlink() {
    var addon = TestData.Addon with { Source = AssetSource.Symlink };

    var subject = BuildSubject();
    var repo = subject.Repo;
    var client = subject.Client;

    var symlinkTarget = ADDONS_DIR + "/" + addon.Name;
    var symlinkSource = addon.Url + "/" + addon.Subfolder;

    client.Setup(c => c.Combine(ADDONS_DIR, addon.Name)).Returns(symlinkTarget);
    client.Setup(c => c.Combine(addon.Url, addon.Subfolder))
      .Returns(symlinkSource);

    client.Setup(c => c.DirectoryExists(symlinkTarget)).Returns(false);
    client.Setup(c => c.DirectoryExists(symlinkSource)).Returns(true);
    client.Setup(c => c.CreateSymlink(symlinkTarget, symlinkSource));

    repo.InstallAddonWithSymlink(addon);

    client.VerifyAll();
  }

  [Fact]
  public void InstallAddonWithSymlinkThrowsErrorIfSymlinkCreationFailed() {
    var addon = TestData.Addon with { Source = AssetSource.Symlink };

    var subject = BuildSubject();
    var repo = subject.Repo;
    var client = subject.Client;

    var symlinkTarget = ADDONS_DIR + "/" + addon.Name;
    var symlinkSource = addon.Url + "/" + addon.Subfolder;

    client.Setup(c => c.Combine(ADDONS_DIR, addon.Name)).Returns(symlinkTarget);
    client.Setup(c => c.Combine(addon.Url, addon.Subfolder))
      .Returns(symlinkSource);

    client.Setup(c => c.DirectoryExists(symlinkTarget)).Returns(false);
    client.Setup(c => c.DirectoryExists(symlinkSource)).Returns(true);
    client.Setup(c => c.CreateSymlink(symlinkTarget, symlinkSource)).Throws(
      new IOException("Failed to create symlink.")
    );

    Should.Throw<IOException>(() => repo.InstallAddonWithSymlink(addon));

    client.VerifyAll();
  }

  [Fact]
  public async Task UpdateCacheIgnoresSymlinkAddons() {
    var addon = TestData.Addon with { Source = AssetSource.Symlink };

    var subject = BuildSubject();
    var repo = subject.Repo;
    var client = subject.Client;
    var config = subject.Config;
    var cacheName = "cache_name";

    await repo.UpdateCache(addon, cacheName);

    client.VerifyAll();
  }

  [Fact]
  public async Task UpdateCacheCallsGitCorrectlyInAddonCacheLocation() {
    var addon = TestData.Addon with { };
    var addonCachePath = CACHE_DIR + "/" + addon.Name;

    var cli = new ShellVerifier(addonCachePath);
    var subject = BuildSubject(cli: cli);

    var repo = subject.Repo;
    var client = subject.Client;
    var config = subject.Config;
    var cacheName = "cache_name";

    client.Setup(c => c.Combine(config.CachePath, cacheName))
      .Returns(addonCachePath);

    // TODO:
    cli.RunsUnchecked(
      addonCachePath, new ProcessResult(0), "git", "clean", "-fdx"
    );
    cli.RunsUnchecked(addonCachePath, new ProcessResult(0), "git", "pull");
    cli.RunsUnchecked(
      addonCachePath, new ProcessResult(0),
      "git",
      "submodule", "update", "--init", "--recursive", "--rebase", "--force"
    );

    await repo.UpdateCache(addon, cacheName);

    client.VerifyAll();
    cli.VerifyAll();
  }

  [Fact]
  public async Task PrepareCacheIgnoresSymlinkAddons() {
    var addon = TestData.Addon with { Source = AssetSource.Symlink };

    var subject = BuildSubject();
    var repo = subject.Repo;
    var client = subject.Client;
    var config = subject.Config;
    var cacheName = "cache_name";

    await repo.PrepareCache(addon, cacheName);

    client.VerifyAll();
  }

  [Fact]
  public async Task PrepareCacheCallsGitCorrectlyInAddonCacheLocation() {
    var addon = TestData.Addon with { };
    var addonCachePath = CACHE_DIR + "/" + addon.Name;

    var cli = new ShellVerifier(addonCachePath);
    var subject = BuildSubject(cli: cli);

    var repo = subject.Repo;
    var client = subject.Client;
    var config = subject.Config;
    var cacheName = "cache_name";

    client.Setup(c => c.Combine(config.CachePath, cacheName))
      .Returns(addonCachePath);

    cli.RunsUnchecked(
      addonCachePath, new ProcessResult(0), "git", "clean", "-fdx"
    );
    cli.Runs(
      addonCachePath, new ProcessResult(0), "git", "checkout", addon.Checkout
    );

    await repo.PrepareCache(addon, cacheName);

    client.VerifyAll();
    cli.VerifyAll();
  }
}
