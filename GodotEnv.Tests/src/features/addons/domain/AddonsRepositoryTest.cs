namespace Chickensoft.GodotEnv.Tests;

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Common.Utilities;
using Chickensoft.GodotEnv.Features.Addons.Domain;
using Chickensoft.GodotEnv.Features.Addons.Models;
using CliFx.Infrastructure;
using global::GodotEnv.Common.Utilities;
using Moq;
using Shouldly;
using Xunit;

public class AddonsRepositoryTest {
  private sealed class Subject(
    Mock<ISystemInfo> systemInfo,
    IConsole console,
    Mock<IFileClient> client,
    Mock<INetworkClient> networkClient,
    Mock<IZipClient> zipClient,
    Mock<ILog> log,
    Mock<IComputer> computer,
    AddonsConfiguration config,
    AddonsRepository repo
  ) {
    public Mock<ISystemInfo> SystemInfo { get; } = systemInfo;
    public IConsole Console { get; } = console;
    public Mock<IFileClient> Client { get; } = client;
    public Mock<INetworkClient> NetworkClient { get; } = networkClient;
    public Mock<IZipClient> ZipClient { get; } = zipClient;
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
    // var systemInfo = new MockSystemInfo(OSType.Linux, CPUArch.X64);
    var systemInfo = new Mock<ISystemInfo>();
    systemInfo.Setup(sys => sys.CPUArch).Returns(CPUArch.X64);
    systemInfo.Setup(sys => sys.OS).Returns(OSType.Linux);
    systemInfo.Setup(sys => sys.OSFamily).Returns(OSFamily.Unix);
    var console = new FakeInMemoryConsole();
    var client = new Mock<IFileClient>();
    var networkClient = new Mock<INetworkClient>();
    var zipClient = new Mock<IZipClient>();
    var log = new Mock<ILog>();
    var computer = new Mock<IComputer>();
    var processRunner = new Mock<IProcessRunner>();
    computer
      .Setup(pc => pc.CreateShell(It.IsAny<string>()))
      .Returns((string path) => {
        if (cli?.GetShell(path) is Mock<IShell> shell) {
          return shell.Object;
        }
        throw new InvalidOperationException(
          $"Mock shell not found for `{path}`. Please use a " +
          "ShellVerifier to create a mock shell for this directory and stub " +
          "the results of the processes that are expected to run."
        );
      });
    var config = new AddonsConfiguration(projectPath, addonsDir, cacheDir);
    var repo = new AddonsRepository(
      systemInfo.Object,
      client.Object,
      networkClient.Object,
      zipClient.Object,
      computer.Object,
      config,
      processRunner.Object
    );
    return new Subject(
      systemInfo: systemInfo,
      console: console,
      client: client,
      networkClient: networkClient,
      zipClient: zipClient,
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

     var token = new CancellationToken();
     var downloadProgress = new Mock<IProgress<DownloadProgress>>();
     var extractProgress = new Mock<IProgress<double>>();

     var result = await repo.CacheAddon(
       addon,
       addon.Name,
       downloadProgress.Object,
       extractProgress.Object,
       token
     );

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

     var token = new CancellationToken();
     var downloadProgress = new Mock<IProgress<DownloadProgress>>();
     var extractProgress = new Mock<IProgress<double>>();

     await repo.CacheAddon(
       addon,
       addon.Name,
       downloadProgress.Object,
       extractProgress.Object,
       token
     );

     client.VerifyAll();
     cli.VerifyAll();
   }

   [Fact]
   public async Task CacheAddonReusesZipAddonCache() {
     var addon = TestData.ZipAddon with { };

     var addonsCachePath = CACHE_DIR + "/" + addon.Name;

     var subject = BuildSubject();

     var repo = subject.Repo;
     var client = subject.Client;

     var addonCachePath = CACHE_DIR + "/" + addon.Name;

     client.Setup(c => c.Combine(CACHE_DIR, addon.Name)).Returns(addonCachePath);

     var extractedDir = addonCachePath + "/" + addon.Hash;
     client.Setup(c => c.Combine(addonCachePath, addon.Hash)).Returns(extractedDir);
     client.Setup(c => c.DirectoryExists(extractedDir)).Returns(true);

     var token = new CancellationToken();
     var downloadProgress = new Mock<IProgress<DownloadProgress>>();
     var extractProgress = new Mock<IProgress<double>>();

     await repo.CacheAddon(
       addon,
       addon.Name,
       downloadProgress.Object,
       extractProgress.Object,
       token
     );

     client.VerifyAll();
   }

   [Fact]
   public async Task CacheAddonCachesZipAddon() {
     var addon = TestData.ZipAddon with { };

     var addonsCachePath = CACHE_DIR + "/" + addon.Name;

     var subject = BuildSubject();

     var repo = subject.Repo;
     var client = subject.Client;
     var networkClient = subject.NetworkClient;
     var zipClient = subject.ZipClient;

     var addonCachePath = CACHE_DIR + "/" + addon.Name;

     client.Setup(c => c.Combine(CACHE_DIR, addon.Name)).Returns(addonCachePath);

     var token = new CancellationToken();
     var zipFileName = addon.Hash + ".zip";
     var extractedDir = addonCachePath + "/" + addon.Hash;
     var downloadProgress = new Mock<IProgress<DownloadProgress>>();
     var extractProgress = new Mock<IProgress<double>>();

     client.Setup(c => c.Combine(addonCachePath, addon.Hash)).Returns(extractedDir);
     client.Setup(c => c.DirectoryExists(extractedDir)).Returns(false);

     client.Setup(c => c.DeleteDirectory(addonCachePath));
     client.Setup(c => c.CreateDirectory(addonCachePath));

     networkClient
       .Setup(nc => nc.DownloadFileAsync(
         addon.Url,
         addonCachePath,
         zipFileName,
         downloadProgress.Object,
         token
       ))
       .Returns(Task.CompletedTask);

     var zipFilePath = addonCachePath + "/" + zipFileName;
     client.Setup(c => c.Combine(addonCachePath, zipFileName)).Returns(zipFilePath);

     zipClient
       .Setup(zc => zc.ExtractToDirectory(
         zipFilePath,
         extractedDir,
         extractProgress.Object
       ))
       .Returns(Task.FromResult(1));

     await repo.CacheAddon(
       addon,
       addon.Name,
       downloadProgress.Object,
       extractProgress.Object,
       token
     );

     client.VerifyAll();
     networkClient.VerifyAll();
     zipClient.VerifyAll();
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

     var token = new CancellationToken();
     var downloadProgress = new Mock<IProgress<DownloadProgress>>();
     var extractProgress = new Mock<IProgress<double>>();

     await repo.CacheAddon(
       addon,
       addon.Name,
       downloadProgress.Object,
       extractProgress.Object,
       token
     );

     client.VerifyAll();
     cli.VerifyAll();
   }

   [Fact]
   public async Task DeleteAddonWillNotDeleteNonExistentAddonOnUnix() {
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
   public async Task DeleteAddonWillNotDeleteNonExistentAddonOnWindows() {
     var addon = TestData.Addon with { };
     var addonPath = ADDONS_DIR + "/" + addon.Name;

     var cli = new ShellVerifier(addonPath);
     var subject = BuildSubject(cli: cli);

     var systemInfo = subject.SystemInfo;
     var repo = subject.Repo;
     var client = subject.Client;

     systemInfo.Setup(sys => sys.OS).Returns(OSType.Windows);
     systemInfo.Setup(sys => sys.OSFamily).Returns(OSFamily.Windows);

     client.Setup(c => c.Combine(ADDONS_DIR, addon.Name)).Returns(addonPath);
     client.Setup(c => c.DirectoryExists(addonPath)).Returns(false);

     await repo.DeleteAddon(addon);

     client.VerifyAll();
     cli.VerifyAll();
   }

   [Fact]
   public async Task DeleteAddonDeletesSymlinkedAddonOnUnix() {
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
   public async Task DeleteAddonDeletesSymlinkedAddonOnWindows() {
     var addon = TestData.Addon with { };
     var addonPath = ADDONS_DIR + "/" + addon.Name;

     var cli = new ShellVerifier(addonPath);
     var subject = BuildSubject(cli: cli);

     var systemInfo = subject.SystemInfo;
     var repo = subject.Repo;
     var client = subject.Client;

     systemInfo.Setup(sys => sys.OS).Returns(OSType.Windows);
     systemInfo.Setup(sys => sys.OSFamily).Returns(OSFamily.Windows);

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

     var systemInfo = subject.SystemInfo;
     var repo = subject.Repo;
     var client = subject.Client;

     systemInfo.Setup(sys => sys.OS).Returns(OSType.Windows);
     systemInfo.Setup(sys => sys.OSFamily).Returns(OSFamily.Windows);

     client.Setup(c => c.Combine(ADDONS_DIR, addon.Name)).Returns(addonPath);
     client.Setup(c => c.DirectoryExists(addonPath)).Returns(true);
     client.Setup(c => c.IsDirectorySymlink(addonPath)).Returns(false);

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
     var subfolderWithSeparatorPath = copyFromPath + "/";

     var cli = new ShellVerifier(addonCachePath, PROJECT_PATH, addonInstallPath);
     var subject = BuildSubject(cli: cli);

     var repo = subject.Repo;
     var client = subject.Client;

     client.Setup(c => c.Combine(CACHE_DIR, addon.Name)).Returns(addonCachePath);

     client.Setup(c => c.Separator).Returns('/');

     client.Setup(c => c.Combine(addonCachePath, addon.Subfolder))
       .Returns(copyFromPath);

     client.Setup(c => c.DirectoryExists(subfolderWithSeparatorPath)).Returns(true);

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
       "git", "config", "--local", "commit.gpgsign", "false"
     );
     cli.Runs(
       addonInstallPath, new ProcessResult(0),
       "git", "commit", "-m", "\"Initial commit\""
     );

     await repo.InstallAddonFromCache(addon, addon.Name);

     client.VerifyAll();
     cli.VerifyAll();
   }

   [Fact]
   public void TestValidateSubfolderThrowsIfNoDirectory() {
     var addon = TestData.Addon with { };
     var addonCachePath = CACHE_DIR + "/" + addon.Name;
     var copyFromPath = addonCachePath + "/" + addon.Subfolder;
     var subfolderWithSeparatorPath = copyFromPath + "/";

     var subject = BuildSubject();

     var repo = subject.Repo;
     var client = subject.Client;

     client.Setup(c => c.DirectoryExists(subfolderWithSeparatorPath)).Returns(false);

     Should.Throw<IOException>(() => repo.ValidateSubfolder(subfolderWithSeparatorPath, addon.Name));

     client.VerifyAll();
   }

   [Fact]
   public void GetCachedAddonPathDeterminesZipAddonCachePath() {

     var subject = BuildSubject();
     var client = subject.Client;
     var addon = TestData.ZipAddon with { };

     var cacheName = "test_addon";
     var result = "cached_addon_path";

     client.Setup(c => c.Combine(CACHE_DIR, cacheName, addon.Hash))
       .Returns(result);

     var cachedAddonPath = subject.Repo.GetCachedAddonPath(addon, cacheName);

     cachedAddonPath.ShouldBe(result);
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
