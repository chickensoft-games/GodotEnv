namespace Chickensoft.GoDotAddon.Tests {
  using System.Collections.Generic;
  using System.IO.Abstractions.TestingHelpers;
  using global::GoDotAddon;
  using Moq;
  using Shouldly;
  using Xunit;


  public class ConfigFileRepoTest {
    private const string CONFIG_FILE_PATH = "file.json";
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
    public void LoadsFile() {
      var app = new Mock<IApp>();
      var fs = new MockFileSystem(
        new Dictionary<string, MockFileData> {
          { CONFIG_FILE_PATH, new MockFileData("") }
        }
      );
      app.Setup(app => app.FS).Returns(fs);
      app.Setup(app => app.LoadFile<ConfigFile>(CONFIG_FILE_PATH)).Returns(
        _configFile
      );
      var repo = new ConfigFileRepo(app.Object);
      var config = repo.LoadOrCreateConfigFile(CONFIG_FILE_PATH);
      config.ShouldBe(_configFile);
    }

    [Fact]
    public void CreatesFile() {
      var app = new Mock<IApp>();
      var fs = new MockFileSystem();
      app.Setup(app => app.FS).Returns(fs);
      var repo = new ConfigFileRepo(app.Object);
      var config = repo.LoadOrCreateConfigFile(CONFIG_FILE_PATH);
      config.ShouldBeOfType(typeof(ConfigFile));
      config.CachePath.ShouldBe(IApp.DEFAULT_CACHE_PATH);
      config.AddonsPath.ShouldBe(IApp.DEFAULT_ADDONS_PATH);
      config.Addons.ShouldBeEmpty();
    }
  }
}
