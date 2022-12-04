namespace Chickensoft.Chicken.Tests;
using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using CliFx.Exceptions;
using Moq;
using Shouldly;
using Xunit;

public class AddonRepoTest {
  private const string PROJECT_DIR = "some/work/dir";
  private const string CACHE_PATH = ".addons";
  private const string ADDONS_PATH = "addons";
  private readonly Config _config = new(
    ProjectPath: PROJECT_DIR,
    CachePath: PROJECT_DIR + "/" + CACHE_PATH,
    AddonsPath: PROJECT_DIR + "/" + ADDONS_PATH
  );

  private readonly RequiredAddon _addon = new(
    name: "chicken",
    configFilePath: "some/working/dir/addons.json",
    url: "git@github.com:chickensoft-games/Chicken.git",
    checkout: "main",
    subfolder: "subfolder"
  );

  [Fact]
  public void LoadsCacheCorrectlyWhenAddonCacheDoesNotExist() {
    var app = new Mock<IApp>(MockBehavior.Strict);
    var fs = new Mock<IFileSystem>(MockBehavior.Strict);
    var cacheRepo = new AddonRepo(app.Object, fs.Object);
    var config = new Config(
      ProjectPath: "project/",
      CachePath: "project/.addons",
      AddonsPath: "project/addons"
    );

    var directory = new Mock<IDirectory>(MockBehavior.Strict);
    fs.Setup(fs => fs.Directory).Returns(directory.Object);
    directory.Setup(dir => dir.Exists("project/.addons")).Returns(false);
    directory.Setup(dir => dir.CreateDirectory("project/.addons")).Returns(
      new Mock<IDirectoryInfo>().Object
    );

    cacheRepo.EnsureCacheExists(config);

    directory.VerifyAll();
  }

  [Fact]
  public async Task CacheAddonCachesAddon() {
    var addonCachePath = $"{_config.CachePath}/{_addon.Name}";

    var app = new Mock<IApp>();
    var fs = new Mock<IFileSystem>();
    var directory = new Mock<IDirectory>();

    fs.Setup(fs => fs.Directory).Returns(directory.Object);
    directory.Setup(dir => dir.Exists(addonCachePath)).Returns(false);

    var cli = new ShellVerifier();

    var cachePathShell = cli.CreateShell(addonCachePath);

    app.Setup(app => app.CreateShell(_config.CachePath))
      .Returns(cachePathShell.Object);

    cli.Setup(
      addonCachePath,
      new ProcessResult(0),
      RunMode.Run,
      "git", "clone", _addon.Url, "--recurse-submodules", _addon.Name
    );

    var addonRepo = new AddonRepo(app.Object, fs.Object);

    await addonRepo.CacheAddon(_addon, _config);

    directory.VerifyAll();
    cli.VerifyAll();
  }

  [Fact]
  public async Task CacheAddonDoesNothingIfAddonIsAlreadyCached() {
    var addonCachePath = $"{_config.CachePath}/{_addon.Name}";

    var app = new Mock<IApp>(MockBehavior.Strict);
    var fs = new Mock<IFileSystem>(MockBehavior.Strict);
    var directory = new Mock<IDirectory>(MockBehavior.Strict);

    fs.Setup(fs => fs.Directory).Returns(directory.Object);
    directory.Setup(dir => dir.Exists(addonCachePath)).Returns(true);

    var addonRepo = new AddonRepo(app.Object, fs.Object);

    await addonRepo.CacheAddon(_addon, _config);

    fs.VerifyAll();
    directory.VerifyAll();
    app.VerifyAll();
  }

  [Fact]
  public async Task DeleteAddonDeletesAddon() {
    var addonDir = _config.AddonsPath + "/" + _addon.Name;

    var app = new Mock<IApp>();
    var fs = new Mock<IFileSystem>();
    var directory = new Mock<IDirectory>();
    var dirInfoFactory = new Mock<IDirectoryInfoFactory>();
    var dirInfo = new Mock<IDirectoryInfo>();
    var path = new Mock<IPath>();

    fs.Setup(fs => fs.Directory).Returns(directory.Object);
    fs.Setup(fs => fs.DirectoryInfo).Returns(dirInfoFactory.Object);
    fs.Setup(fs => fs.Path).Returns(path.Object);
    dirInfoFactory.Setup(dif => dif.FromDirectoryName(addonDir))
      .Returns(dirInfo.Object);
    dirInfo.Setup(info => info.LinkTarget).Returns<string?>(null);
    directory.Setup(dir => dir.Exists(addonDir)).Returns(true);
    path.Setup(p => p.DirectorySeparatorChar).Returns('/');

    var cli = new ShellVerifier();

    var addonShell = cli.CreateShell(addonDir);
    var addonsPathShell = cli.CreateShell(_config.AddonsPath);

    app.Setup(app => app.CreateShell(addonDir))
      .Returns(addonShell.Object);
    app.Setup(app => app.DeleteDirectory(fs.Object, addonDir));

    cli.Setup(
      addonDir,
      new ProcessResult(0),
      RunMode.RunUnchecked,
      "git", "status", "--porcelain"
    );

    var addonRepo = new AddonRepo(app.Object, fs.Object);

    await addonRepo.DeleteAddon(_addon, _config);

    app.VerifyAll();
    directory.VerifyAll();
    cli.VerifyAll();
  }

  [Fact]
  public async Task DeleteAddonDeletesAddonOnWindows() {
    var addonDir = _config.AddonsPath + "/" + _addon.Name;

    var app = new Mock<IApp>();
    var fs = new Mock<IFileSystem>();
    var directory = new Mock<IDirectory>();
    var dirInfoFactory = new Mock<IDirectoryInfoFactory>();
    var dirInfo = new Mock<IDirectoryInfo>();
    var path = new Mock<IPath>();

    fs.Setup(fs => fs.Directory).Returns(directory.Object);
    fs.Setup(fs => fs.DirectoryInfo).Returns(dirInfoFactory.Object);
    fs.Setup(fs => fs.Path).Returns(path.Object);
    dirInfoFactory.Setup(dif => dif.FromDirectoryName(addonDir))
      .Returns(dirInfo.Object);
    dirInfo.Setup(info => info.LinkTarget).Returns<string?>(null);
    directory.Setup(dir => dir.Exists(addonDir)).Returns(true);
    path.Setup(p => p.DirectorySeparatorChar).Returns('\\');

    var cli = new ShellVerifier();

    var addonShell = cli.CreateShell(addonDir);
    var addonsPathShell = cli.CreateShell(_config.AddonsPath);

    app.Setup(app => app.CreateShell(addonDir))
      .Returns(addonShell.Object);
    app.Setup(app => app.CreateShell(_config.AddonsPath))
      .Returns(addonsPathShell.Object);

    cli.Setup(
      addonDir,
      new ProcessResult(0),
      RunMode.RunUnchecked,
      "git", "status", "--porcelain"
    );

    cli.Setup(
      addonDir,
      new ProcessResult(0),
      RunMode.Run,
      "cmd.exe", "/c", "erase", "/s", "/f", "/q", "*"
    );

    cli.Setup(
      _config.AddonsPath,
      new ProcessResult(0),
      RunMode.RunUnchecked,
      "cmd.exe", "/c", "rmdir", _addon.Name, "/s", "/q"
    );

    var addonRepo = new AddonRepo(app.Object, fs.Object);

    await addonRepo.DeleteAddon(_addon, _config);

    app.VerifyAll();
    directory.VerifyAll();
    cli.VerifyAll();
  }

  [Fact]
  public async Task DeleteAddonDeletesSymlinkAddon() {
    var addonDir = _config.AddonsPath + "/" + _addon.Name;

    var app = new Mock<IApp>();
    var fs = new Mock<IFileSystem>();
    var directory = new Mock<IDirectory>();
    var dirInfoFactory = new Mock<IDirectoryInfoFactory>();
    var dirInfo = new Mock<IDirectoryInfo>();

    app.Setup(app => app.IsDirectorySymlink(fs.Object, addonDir)).Returns(true);
    app.Setup(app => app.DeleteDirectory(fs.Object, addonDir));
    fs.Setup(fs => fs.Directory).Returns(directory.Object);
    directory.Setup(dir => dir.Exists(addonDir)).Returns(true);

    var addonRepo = new AddonRepo(app.Object, fs.Object);

    await addonRepo.DeleteAddon(_addon, _config);

    directory.VerifyAll();
  }

  [Fact]
  public async Task DeleteAddonDoesNothingIfAddonDoesNotExist() {
    var addonDir = _config.AddonsPath + "/" + _addon.Name;

    var app = new Mock<IApp>(MockBehavior.Strict);
    var fs = new Mock<IFileSystem>(MockBehavior.Strict);
    var directory = new Mock<IDirectory>(MockBehavior.Strict);

    fs.Setup(fs => fs.Directory).Returns(directory.Object);
    directory.Setup(dir => dir.Exists(addonDir)).Returns(false);

    var addonRepo = new AddonRepo(app.Object, fs.Object);

    await addonRepo.DeleteAddon(_addon, _config);

    fs.VerifyAll();
    directory.VerifyAll();
    app.VerifyAll();
  }

  [Fact]
  public async Task DeleteAddonThrowsIfAddonModified() {
    var addonDir = _config.AddonsPath + "/" + _addon.Name;

    var app = new Mock<IApp>();
    var fs = new Mock<IFileSystem>();
    var directory = new Mock<IDirectory>();
    var dirInfoFactory = new Mock<IDirectoryInfoFactory>();
    var dirInfo = new Mock<IDirectoryInfo>();

    fs.Setup(fs => fs.Directory).Returns(directory.Object);
    fs.Setup(fs => fs.DirectoryInfo).Returns(dirInfoFactory.Object);
    dirInfoFactory.Setup(dif => dif.FromDirectoryName(addonDir))
      .Returns(dirInfo.Object);
    dirInfo.Setup(info => info.LinkTarget).Returns<string?>(null);
    directory.Setup(dir => dir.Exists(addonDir)).Returns(true);

    var cli = new ShellVerifier();

    var addonShell = cli.CreateShell(addonDir);
    var addonsPathShell = cli.CreateShell(_config.AddonsPath);

    app.Setup(app => app.CreateShell(addonDir))
      .Returns(addonShell.Object);
    app.Setup(app => app.CreateShell(_config.AddonsPath))
      .Returns(addonsPathShell.Object);

    cli.Setup(
      addonDir,
      new ProcessResult(0, StandardOutput: "a file was changed"),
      RunMode.RunUnchecked,
      "git", "status", "--porcelain"
    );

    var addonRepo = new AddonRepo(app.Object, fs.Object);

    await Should.ThrowAsync<CommandException>(
      async ()
        => await addonRepo.DeleteAddon(_addon, _config)
    );

    directory.VerifyAll();
    cli.VerifyAll();
  }

  [Fact]
  public async Task CopyAddonCopiesAddon() {
    var addonCachePath = _config.CachePath + "/" + _addon.Name;
    var copyFromPath = addonCachePath + "/" + _addon.Subfolder + "/";
    var workingDir = _config.ProjectPath;
    var addonDir = _config.AddonsPath + "/" + _addon.Name + "/";

    var app = new Mock<IApp>(MockBehavior.Strict);
    var fs = new Mock<IFileSystem>(MockBehavior.Strict);
    var cli = new ShellVerifier();
    var copier = new Mock<IFileCopier>(MockBehavior.Strict);

    var addonCacheShell = cli.CreateShell(addonCachePath);
    var workingShell = cli.CreateShell(workingDir);
    var addonShell = cli.CreateShell(addonDir);

    app.Setup(app => app.CreateShell(addonCachePath))
      .Returns(addonCacheShell.Object);
    app.Setup(app => app.CreateShell(workingDir))
      .Returns(workingShell.Object);
    app.Setup(app => app.CreateShell(addonDir))
      .Returns(addonShell.Object);

    copier.Setup(copier => copier.Copy(
      fs.Object, workingShell.Object, copyFromPath, addonDir
    )).Returns(Task.CompletedTask);

    cli.Setup(
      addonCachePath,
      new ProcessResult(0),
      RunMode.Run,
      "git", "checkout", _addon.Checkout
    );

    cli.Setup(
      addonCachePath,
      new ProcessResult(0),
      RunMode.RunUnchecked,
      "git", "pull"
    );

    cli.Setup(
      addonCachePath,
      new ProcessResult(0),
      RunMode.RunUnchecked,
      "git", "submodule", "update", "--init", "--recursive"
    );

    cli.Setup(
      addonDir,
      new ProcessResult(0),
      RunMode.Run,
      "git",
      "init"
    );

    cli.Setup(
      addonDir,
      new ProcessResult(0),
      RunMode.Run,
      "git",
      "add", "-A"
    );

    cli.Setup(
      addonDir,
      new ProcessResult(0),
      RunMode.Run,
      "git",
      "commit", "-m", "Initial commit"
    );

    var addonRepo = new AddonRepo(app.Object, fs.Object);
    await addonRepo.CopyAddonFromCache(_addon, _config, copier.Object);

    app.VerifyAll();
    cli.VerifyAll();
  }

  [Fact]
  public void InstallAddonWithSymlinkInstallsSymlinkAddon() {
    var app = new Mock<IApp>();
    var fs = new Mock<IFileSystem>();
    var dir = new Mock<IDirectory>();

    var addon = new RequiredAddon(
      name: "addon",
      configFilePath: "some/working/dir/addons.json",
      url: "some/local/path",
      checkout: "main",
      subfolder: "subfolder",
      source: RepositorySource.Symlink
    );

    var url = addon.Url + "/" + addon.Subfolder;

    var addonDir = _config.AddonsPath + "/" + addon.Name;

    app.Setup(app => app.CreateSymlink(fs.Object, addonDir, url));
    fs.Setup(fs => fs.Directory).Returns(dir.Object);
    dir.Setup(dir => dir.Exists(addonDir)).Returns(false);
    dir.Setup(dir => dir.Exists(url))
      .Returns(true);

    var addonRepo = new AddonRepo(app.Object, fs.Object);

    addonRepo.InstallAddonWithSymlink(addon, _config);

    dir.VerifyAll();
  }

  [Fact]
  public void InstallAddonWithSymlinkOnlyInstallsSymlinkAddon() {
    var app = new Mock<IApp>();
    var fs = new Mock<IFileSystem>();

    var addonDir = _config.AddonsPath + "/" + _addon.Name;

    var addonRepo = new AddonRepo(app.Object, fs.Object);

    Should.Throw<CommandException>(
      () => addonRepo.InstallAddonWithSymlink(_addon, _config)
    );
  }

  [Fact]
  public void InstallAddonWithSymlinkThrowsIfAddonDirExists() {
    var app = new Mock<IApp>();
    var fs = new Mock<IFileSystem>();
    var dir = new Mock<IDirectory>();

    var addon = new RequiredAddon(
      name: "addon",
      configFilePath: "some/working/dir/addons.json",
      url: "some/local/path",
      checkout: "main",
      subfolder: "subfolder",
      source: RepositorySource.Symlink
    );

    var addonDir = _config.AddonsPath + "/" + addon.Name;

    fs.Setup(fs => fs.Directory).Returns(dir.Object);
    dir.Setup(dir => dir.Exists(addonDir)).Returns(true);

    var addonRepo = new AddonRepo(app.Object, fs.Object);

    Should.Throw<CommandException>(
      () => addonRepo.InstallAddonWithSymlink(addon, _config)
    );
  }

  [Fact]
  public void InstallAddonWithSymlinkThrowsIfAddonSourceDirDoesNotExist() {
    var app = new Mock<IApp>();
    var fs = new Mock<IFileSystem>();
    var dir = new Mock<IDirectory>();

    var addon = new RequiredAddon(
      name: "addon",
      configFilePath: "some/working/dir/addons.json",
      url: "some/local/path",
      checkout: "main",
      subfolder: "subfolder",
      source: RepositorySource.Symlink
    );

    var addonDir = _config.AddonsPath + "/" + addon.Name;
    var url = addon.Url + "/" + addon.Subfolder;

    fs.Setup(fs => fs.Directory).Returns(dir.Object);
    dir.Setup(dir => dir.Exists(addonDir)).Returns(false);
    dir.Setup(dir => dir.Exists(url)).Returns(false);

    var addonRepo = new AddonRepo(app.Object, fs.Object);

    Should.Throw<CommandException>(
      () => addonRepo.InstallAddonWithSymlink(addon, _config)
    );
  }

  [Fact]
  public void InstallAddonWithSymlinkThrowsIfSymlinkCreationFails() {
    var app = new Mock<IApp>();
    var fs = new Mock<IFileSystem>();
    var dir = new Mock<IDirectory>();

    var addon = new RequiredAddon(
      name: "addon",
      configFilePath: "some/working/dir/addons.json",
      url: "some/local/path",
      checkout: "main",
      subfolder: "subfolder",
      source: RepositorySource.Symlink
    );

    var url = addon.Url + "/" + addon.Subfolder; // source
    var addonDir = _config.AddonsPath + "/" + addon.Name; // target

    app.Setup(app => app.CreateSymlink(fs.Object, addonDir, url))
      .Throws(new InvalidOperationException("dummy exception"));
    fs.Setup(fs => fs.Directory).Returns(dir.Object);
    dir.Setup(dir => dir.Exists(addonDir)).Returns(false);
    dir.Setup(dir => dir.Exists(url)).Returns(true);
    dir.Setup(dir => dir.CreateSymbolicLink(addonDir, url))
      .Throws<InvalidOperationException>();

    var addonRepo = new AddonRepo(app.Object, fs.Object);

    Should.Throw<CommandException>(
      () => addonRepo.InstallAddonWithSymlink(addon, _config)
    );
  }
}
