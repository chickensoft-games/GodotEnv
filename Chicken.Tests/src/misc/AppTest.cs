namespace Chickensoft.Chicken.Tests;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using CliFx.Infrastructure;
using Moq;
using Newtonsoft.Json;
using Shouldly;
using Xunit;

public class AppTest {
  private const string FILENAME = @"c:\test.txt";
  private const string DATA = "data";
  private readonly string _testData = $"{{ \"test\": \"{DATA}\" }}";

  private class TestObject {
    [JsonProperty("test")]
    public string Test { get; init; } = "";
  }

  [Fact]
  public void Initializes() {
    var app = new App();
    app.WorkingDir.ShouldBe(Environment.CurrentDirectory);
  }

  [Fact]
  public void CreatesShell() {
    var app = new App(workingDir: ".");
    var shell = app.CreateShell(".");
    shell.ShouldBeOfType(typeof(Shell));
  }

  [Fact]
  public void CreatesAddonManager() {
    var addonRepo = new Mock<IAddonRepo>();
    var configFileLoader = new Mock<IConfigFileLoader>();
    var log = new Mock<ILog>();
    var dependencyGraph = new Mock<IDependencyGraph>();

    var fs = new Mock<IFileSystem>();
    var file = new Mock<IFile>();
    var app = new App(workingDir: ".");

    app.CreateAddonManager(
      fs: fs.Object,
      addonRepo: addonRepo.Object,
      configFileLoader: configFileLoader.Object,
      log: log.Object,
      dependencyGraph: dependencyGraph.Object
    ).ShouldBeOfType(typeof(AddonManager));
  }

  [Fact]
  public void CreatesReporter() {
    var fs = new Mock<IFileSystem>();
    var console = new FakeInMemoryConsole();
    var app = new App(workingDir: ".");
    app.CreateLog(console).ShouldBeOfType(typeof(Log));
  }

  [Fact]
  public void CreatesAddonRepo() {
    var fs = new Mock<IFileSystem>();
    var app = new App(workingDir: ".");
    app.CreateAddonRepo(fs.Object).ShouldBeOfType(typeof(AddonRepo));
  }

  [Fact]
  public void CreatesConfigFileRepo() {
    var fs = new Mock<IFileSystem>();
    var app = new App(workingDir: ".");
    app.CreateConfigFileRepo(fs.Object)
      .ShouldBeOfType(typeof(ConfigFileLoader));
  }

  [Fact]
  public void CreatesDependencyGraph() {
    var app = new App(workingDir: ".");
    app.CreateDependencyGraph()
      .ShouldBeOfType(typeof(DependencyGraph));
  }

  [Fact]
  public void CreatesEditActionsLoader() {
    var fs = new Mock<IFileSystem>();
    var app = new App(workingDir: ".");
    app.CreateEditActionsLoader(fs.Object)
      .ShouldBeOfType(typeof(EditActionsLoader));
  }

  [Fact]
  public void CreatesEditActionsRepo() {
    var fs = new Mock<IFileSystem>();
    var app = new App(workingDir: ".");
    app.CreateEditActionsRepo(
      fs.Object,
      "/",
      new EditActions(null, null),
      new Dictionary<string, dynamic?>()
    ).ShouldBeOfType(typeof(EditActionsRepo));
  }

  [Fact]
  public void CreatesTemplateGenerator() {
    var editActionsRepo = new Mock<IEditActionsRepo>();
    var log = new Mock<ILog>();
    var app = new App(workingDir: ".");
    app.CreateTemplateGenerator(
      "name",
      "/project",
      "template",
      editActionsRepo.Object,
      new EditActions(null, null),
      log.Object
    ).ShouldBeOfType(typeof(TemplateGenerator));
  }

  [Fact]
  public void GenerateGuidTest() {
    var app = new App();
    app.GenerateGuid().ShouldMatch(
      @"^[0-9A-F]{8}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{12}$"
    );
  }


  [Fact]
  public void IsDirectorySymlinkRecognizesSymlinks() {
    var fs = new Mock<IFileSystem>();
    var dirInfoFactory = new Mock<IDirectoryInfoFactory>();
    var dirInfo = new Mock<IDirectoryInfo>();

    var path = "a/folder/to/check";

    fs.Setup(fs => fs.DirectoryInfo).Returns(dirInfoFactory.Object);
    dirInfoFactory.Setup(dif => dif.FromDirectoryName(path))
      .Returns(dirInfo.Object);
    dirInfo.Setup(di => di.LinkTarget)
      .Returns("some/path");

    var app = new App("/");
    var result = app.IsDirectorySymlink(fs.Object, path);

    result.ShouldBeTrue();
  }

  [Fact]
  public void IsDirectorySymlinkRecognizesNormalDirectory() {
    var fs = new Mock<IFileSystem>();
    var dirInfoFactory = new Mock<IDirectoryInfoFactory>();
    var dirInfo = new Mock<IDirectoryInfo>();

    var path = "a/folder/to/check";

    fs.Setup(fs => fs.DirectoryInfo).Returns(dirInfoFactory.Object);
    dirInfoFactory.Setup(dif => dif.FromDirectoryName(path))
      .Returns(dirInfo.Object);
    dirInfo.Setup(di => di.LinkTarget).Returns<string?>(null);

    var app = new App("/");
    var result = app.IsDirectorySymlink(fs.Object, path);

    result.ShouldBeFalse();
  }

  [Fact]
  public void DirectorySymlinkTargetFindsValue() {
    var fs = new Mock<IFileSystem>();
    var dirInfoFactory = new Mock<IDirectoryInfoFactory>();
    var dirInfo = new Mock<IDirectoryInfo>();

    var path = "a/folder/to/check";
    var target = "some/symlink/target";

    fs.Setup(fs => fs.DirectoryInfo).Returns(dirInfoFactory.Object);
    dirInfoFactory.Setup(dif => dif.FromDirectoryName(path))
      .Returns(dirInfo.Object);
    dirInfo.Setup(di => di.LinkTarget).Returns(target);

    var app = new App("/");
    var result = app.DirectorySymlinkTarget(fs.Object, path);

    result.ShouldBe(target);
  }

  [Fact]
  public void CreateSymlinkCreatesSymlink() {
    var fs = new Mock<IFileSystem>();
    var dir = new Mock<IDirectory>();

    var path = "original/folder";
    var pathToTarget = "target/folder";

    fs.Setup(fs => fs.Directory).Returns(dir.Object);
    dir.Setup(dir => dir.CreateSymbolicLink(path, pathToTarget));

    var app = new App("/");
    app.CreateSymlink(fs.Object, path, pathToTarget);

    dir.VerifyAll();
  }

  [Fact]
  public void FileThatExistsReturnsPathOfFileThatExists() {
    var fs = new Mock<IFileSystem>();
    var file = new Mock<IFile>();

    var path = "/";

    fs.Setup(fs => fs.File).Returns(file.Object);
    file.Setup(file => file.Exists("/addons.json")).Returns(true);

    var app = new App(path);
    app.FileThatExists(fs.Object, path, App.ADDONS_CONFIG_FILES)
      .ShouldBe("/addons.json");
  }

  [Fact]
  public void FileThatExistsReturnsFirstIfNoFilesExist() {
    var fs = new Mock<IFileSystem>();
    var file = new Mock<IFile>();

    var path = "/";

    fs.Setup(fs => fs.File).Returns(file.Object);
    file.Setup(file => file.Exists("/addons.json")).Returns(false);
    file.Setup(file => file.Exists("/addons.jsonc")).Returns(false);

    var app = new App(path);
    app.FileThatExists(fs.Object, path, App.ADDONS_CONFIG_FILES).ShouldBe(
      App.ADDONS_CONFIG_FILES.First()
    );
  }

  [Fact]
  public void ResolveUrlDoesNothingForRemoteAddons() {
    var url = "http://example.com/addon1.git";
    var path = "/volume/directory";
    var addonConfig = new AddonConfig(
      url: url
    );

    var fs = new Mock<IFileSystem>();
    var dirInfoFactory = new Mock<IDirectoryInfoFactory>();
    fs.Setup(fs => fs.DirectoryInfo).Returns(dirInfoFactory.Object);
    var dirInfo = new Mock<IDirectoryInfo>();
    dirInfoFactory.Setup(di => di.FromDirectoryName(path))
      .Returns(dirInfo.Object);
    dirInfo.Setup(di => di.LinkTarget).Returns<string>(null);

    var app = new App("/");
    app.ResolveUrl(fs.Object, addonConfig, path).ShouldBe(url);
  }

  [Fact]
  public void ResolveUrlResolvesNonRootedPath() {
    var url = "../some/relative/path";
    var path = "/volume/old/directory";
    var resolved = "/volume/other/directory";
    var addonConfig = new AddonConfig(
      url: url,
      source: RepositorySource.Local
    );

    var fs = new Mock<IFileSystem>();
    var dirInfoFactory = new Mock<IDirectoryInfoFactory>();
    fs.Setup(fs => fs.DirectoryInfo).Returns(dirInfoFactory.Object);
    var dirInfo = new Mock<IDirectoryInfo>();
    dirInfoFactory.Setup(di => di.FromDirectoryName(path))
      .Returns(dirInfo.Object);
    dirInfo.Setup(di => di.LinkTarget).Returns(resolved);

    var app = new App("/");
    app.ResolveUrl(fs.Object, addonConfig, path)
      .ShouldBe("/volume/other/some/relative/path");
  }

  [Fact]
  public void ResolveUrlDoesNotResolveRootedPath() {
    var url = "/volume2/some/path";
    var path = "/volume/directory";
    var addonConfig = new AddonConfig(
      url: url,
      source: RepositorySource.Local
    );

    var fs = new Mock<IFileSystem>();
    var dirInfoFactory = new Mock<IDirectoryInfoFactory>();
    fs.Setup(fs => fs.DirectoryInfo).Returns(dirInfoFactory.Object);
    var dirInfo = new Mock<IDirectoryInfo>();
    dirInfoFactory.Setup(di => di.FromDirectoryName(path))
      .Returns(dirInfo.Object);
    dirInfo.Setup(di => di.LinkTarget).Returns(path);

    var app = new App("/");
    app.ResolveUrl(fs.Object, addonConfig, path).ShouldBe(url);
  }

  [Fact]
  public void DeleteDirectoryDeletesSymlink() {
    var fs = new Mock<IFileSystem>();
    var dir = new Mock<IDirectory>();

    var path = "a/symlink/to/delete";

    fs.Setup(fs => fs.Directory).Returns(dir.Object);
    var dirInfoFactory = new Mock<IDirectoryInfoFactory>();
    fs.Setup(fs => fs.DirectoryInfo).Returns(dirInfoFactory.Object);
    var dirInfo = new Mock<IDirectoryInfo>();
    dirInfoFactory.Setup(di => di.FromDirectoryName(path))
      .Returns(dirInfo.Object);
    dirInfo.Setup(di => di.LinkTarget).Returns("some/target");
    dir.Setup(dir => dir.Delete(path));

    var app = new App("/");
    app.DeleteDirectory(fs.Object, path);

    dir.VerifyAll();
  }

  [Fact]
  public void DeleteDirectoryDeletesNormalFolder() {
    var fs = new Mock<IFileSystem>();
    var dir = new Mock<IDirectory>();

    var path = "a/folder/to/delete";

    fs.Setup(fs => fs.Directory).Returns(dir.Object);
    var dirInfoFactory = new Mock<IDirectoryInfoFactory>();
    fs.Setup(fs => fs.DirectoryInfo).Returns(dirInfoFactory.Object);
    var dirInfo = new Mock<IDirectoryInfo>();
    dirInfoFactory.Setup(di => di.FromDirectoryName(path))
      .Returns(dirInfo.Object);
    dirInfo.Setup(di => di.LinkTarget).Returns<string?>(null);
    dir.Setup(dir => dir.Delete(path, true));

    var app = new App("/");
    app.DeleteDirectory(fs.Object, path);

    dir.VerifyAll();
  }

}
