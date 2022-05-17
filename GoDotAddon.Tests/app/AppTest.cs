namespace Chickensoft.GoDotAddon.Tests {
  using System;
  using System.Collections.Generic;
  using System.IO.Abstractions;
  using System.IO.Abstractions.TestingHelpers;
  using System.Threading.Tasks;
  using CliFx.Exceptions;
  using global::GoDotAddon;
  using Moq;
  using Newtonsoft.Json;
  using Shouldly;
  using Xunit;

  public class AppTest {
    private const string FILENAME = @"c:\test.txt";
    private const string DATA = "data";
    private const string TEST_DATA = $"{{ \"test\": \"{DATA}\" }}";

    private class TestObject {
      [JsonProperty("test")]
      public string Test { get; init; } = "";
    }

    [Fact]
    public void AppInitializes() {
      var app = new App();
      app.WorkingDir.ShouldBe(Environment.CurrentDirectory);
      app.FS.ShouldBeOfType<FileSystem>();
      Info.App.ShouldBeOfType(typeof(App));
    }

    [Fact]
    public void AppCreatesShell() {
      var app = new App(workingDir: ".", fs: new Mock<IFileSystem>().Object);
      var shell = app.CreateShell(".");
      shell.ShouldBeOfType(typeof(Shell));
    }

    [Fact]
    public void AppLoadsFile() {
      var fs = new MockFileSystem(new Dictionary<string, MockFileData> {
        { FILENAME, new MockFileData(TEST_DATA) },
      });
      var app = new App(workingDir: ".", fs: fs);
      var file = app.LoadFile<TestObject>(FILENAME);
      file.ShouldNotBeNull();
      file.Test.ShouldBe(DATA);
    }

    [Fact]
    public void AppThrowsErrorWhenReadingFileDoesNotWork() {
      var fs = new MockFileSystem(new Dictionary<string, MockFileData> { });
      var app = new App(workingDir: ".", fs: fs);
      Should.Throw<CommandException>(
        () => Task.FromResult(app.LoadFile<TestObject>(FILENAME))
      );
    }

    [Fact]
    public void AppThrowsErrorWhenDeserializationFails() {
      var fs = new MockFileSystem(new Dictionary<string, MockFileData> {
        { FILENAME, new MockFileData("") },
      });
      var app = new App(workingDir: ".", fs: fs);
      Should.Throw<CommandException>(
        () => Task.FromResult(app.LoadFile<TestObject>(FILENAME))
      )
      .InnerException?.ShouldNotBeNull()
      .Message.ShouldBe($"Couldn't load file `{FILENAME}`");
    }
  }
}
