namespace Chickensoft.Chicken.Tests;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Text.Json;
using CliFx.Exceptions;
using Moq;
using Shouldly;
using Xunit;


public class ConfigFileLoaderTest {
  private const string PROJECT_PATH = "/";
  private const string CONFIG_FILE_PATH = "/addons.json";
  private readonly ConfigFile _configFile = new(
    addons: new(), cachePath: "./.addons", addonsPath: "./addons"
  );

  [Fact]
  public void Initializes() {
    var app = new Mock<IApp>();
    var fs = new Mock<IFileSystem>();
    var repo = new ConfigFileLoader(app.Object, fs.Object);
    repo.ShouldBeOfType(typeof(ConfigFileLoader));
  }


  [Fact]
  public void LoadsFileAndCreatesDirectories() {
    var app = new Mock<IApp>();

    var fs = new MockFileSystem(new Dictionary<string, MockFileData>() {
      ["/addons.json"] = new MockFileData(JsonSerializer.Serialize(_configFile))
    });

    var repo = new ConfigFileLoader(app.Object, fs);
    var config = repo.Load(PROJECT_PATH);
    config.CachePath.ShouldBe(_configFile.CachePath);
    config.AddonsPath.ShouldBe(_configFile.AddonsPath);
    config.Addons.ShouldBe(_configFile.Addons);
    fs.AllPaths.ShouldContain("/addons");
    fs.AllPaths.ShouldContain("/.addons");
  }

  [Fact]
  public void CreatesFile() {
    var app = new Mock<IApp>();
    var fs = new Mock<IFileSystem>();
    var file = new Mock<IFile>();

    fs.Setup(fs => fs.File).Returns(file.Object);
    file.Setup(file => file.Exists(CONFIG_FILE_PATH)).Returns(false);

    var dir = new Mock<IDirectory>();
    fs.Setup(fs => fs.Directory).Returns(dir.Object);
    dir.Setup(dir => dir.Exists(_configFile.AddonsPath)).Returns(false);
    dir.Setup(dir => dir.CreateDirectory(_configFile.AddonsPath));
    dir.Setup(dir => dir.Exists(_configFile.CachePath)).Returns(false);
    dir.Setup(dir => dir.CreateDirectory(_configFile.CachePath));

    var repo = new ConfigFileLoader(app.Object, fs.Object);
    var config = repo.Load(PROJECT_PATH);
    config.ShouldBeOfType(typeof(ConfigFile));
    config.CachePath.ShouldBe(App.DEFAULT_CACHE_PATH);
    config.AddonsPath.ShouldBe(App.DEFAULT_ADDONS_PATH);
    config.Addons.ShouldBeEmpty();
  }

  [Fact]
  public void ThrowsErrorWhenDeserializationIsNull() {
    var app = new Mock<IApp>();

    var fs = new MockFileSystem(new Dictionary<string, MockFileData>() {
      ["/addons.json"] = new MockFileData("")
    });

    var repo = new ConfigFileLoader(app.Object, fs);
    Should.Throw<CommandException>(() => repo.Load(PROJECT_PATH));
  }

  [Fact]
  public void ThrowsErrorWhenReadingFileDoesNotWork() {
    var app = new Mock<IApp>();
    var fs = new Mock<IFileSystem>();
    var file = new Mock<IFile>();
    fs.Setup(fs => fs.File).Returns(file.Object);
    file.Setup(file => file.Exists(CONFIG_FILE_PATH)).Returns(true);
    file.Setup(file => file.ReadAllText(CONFIG_FILE_PATH)).Throws<Exception>();

    var repo = new ConfigFileLoader(app.Object, fs.Object);
    Should.Throw<CommandException>(() => repo.Load(PROJECT_PATH));
  }
}
