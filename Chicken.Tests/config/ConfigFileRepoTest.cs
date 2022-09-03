namespace Chickensoft.Chicken.Tests {
  using System.IO.Abstractions;
  using Moq;
  using Shouldly;
  using Xunit;


  public class ConfigFileRepoTest {
    private const string PROJECT_PATH = ".";
    private const string CONFIG_FILE_PATH = "./addons.json";
    private readonly ConfigFile _configFile = new(
      addons: new(), cachePath: "./.addons", addonsPath: "./addons"
    );

    [Fact]
    public void Initializes() {
      var app = new Mock<IApp>();
      var repo = new ConfigFileRepo(app.Object);
      repo.ShouldBeOfType(typeof(ConfigFileRepo));
    }


    [Fact]
    public void LoadsFileAndCreatesDirectories() {
      var app = new Mock<IApp>();

      var fs = new Mock<IFileSystem>();
      var file = new Mock<IFile>();
      app.Setup(app => app.FS).Returns(fs.Object);
      fs.Setup(fs => fs.File).Returns(file.Object);
      file.Setup(file => file.Exists(CONFIG_FILE_PATH)).Returns(true);
      app.Setup(app => app.LoadFile<ConfigFile>(CONFIG_FILE_PATH)).Returns(
        _configFile
      );

      var dir = new Mock<IDirectory>();
      fs.Setup(fs => fs.Directory).Returns(dir.Object);
      dir.Setup(dir => dir.Exists(_configFile.AddonsPath)).Returns(false);
      dir.Setup(dir => dir.CreateDirectory(_configFile.AddonsPath));
      dir.Setup(dir => dir.Exists(_configFile.CachePath)).Returns(false);
      dir.Setup(dir => dir.CreateDirectory(_configFile.CachePath));

      var repo = new ConfigFileRepo(app.Object);
      var config = repo.LoadOrCreateConfigFile(PROJECT_PATH);
      config.ShouldBe(_configFile);
    }

    [Fact]
    public void CreatesFile() {
      var app = new Mock<IApp>();
      var fs = new Mock<IFileSystem>();
      var file = new Mock<IFile>();
      app.Setup(app => app.FS).Returns(fs.Object);
      fs.Setup(fs => fs.File).Returns(file.Object);
      file.Setup(file => file.Exists(CONFIG_FILE_PATH)).Returns(false);

      var dir = new Mock<IDirectory>();
      fs.Setup(fs => fs.Directory).Returns(dir.Object);
      dir.Setup(dir => dir.Exists(_configFile.AddonsPath)).Returns(false);
      dir.Setup(dir => dir.CreateDirectory(_configFile.AddonsPath));
      dir.Setup(dir => dir.Exists(_configFile.CachePath)).Returns(false);
      dir.Setup(dir => dir.CreateDirectory(_configFile.CachePath));

      var repo = new ConfigFileRepo(app.Object);
      var config = repo.LoadOrCreateConfigFile(PROJECT_PATH);
      config.ShouldBeOfType(typeof(ConfigFile));
      config.CachePath.ShouldBe(IApp.DEFAULT_CACHE_PATH);
      config.AddonsPath.ShouldBe(IApp.DEFAULT_ADDONS_PATH);
      config.Addons.ShouldBeEmpty();
    }
  }
}
