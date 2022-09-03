namespace Chickensoft.Chicken.Tests {
  using System;
  using System.IO;
  using System.IO.Abstractions;
  using System.Threading.Tasks;
  using CliFx.Exceptions;
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
      app.FS.ShouldBeOfType<FileSystem>();
    }

    [Fact]
    public void CreatesShell() {
      var app = new App(workingDir: ".", fs: new Mock<IFileSystem>().Object);
      var shell = app.CreateShell(".");
      shell.ShouldBeOfType(typeof(Shell));
    }

    [Fact]
    public void LoadsFile() {
      var fs = new Mock<IFileSystem>();
      var file = new Mock<IFile>();
      fs.Setup(fs => fs.File).Returns(file.Object);
      file.Setup(file => file.ReadAllText(FILENAME)).Returns(_testData);

      var app = new App(workingDir: ".", fs: fs.Object);
      var testObject = app.LoadFile<TestObject>(FILENAME);
      testObject.ShouldNotBeNull();
      testObject.Test.ShouldBe(DATA);
    }

    [Fact]
    public void ThrowsErrorWhenReadingFileDoesNotWork() {
      var fs = new Mock<IFileSystem>();
      var file = new Mock<IFile>();
      fs.Setup(fs => fs.File).Returns(file.Object);
      file.Setup(file => file.ReadAllText(FILENAME)).Throws<Exception>();

      var app = new App(workingDir: ".", fs: fs.Object);
      Should.Throw<CommandException>(
        () => Task.FromResult(app.LoadFile<TestObject>(FILENAME))
      );
    }

    [Fact]
    public void ThrowsErrorWhenDeserializationFails() {
      var fs = new Mock<IFileSystem>();
      var file = new Mock<IFile>();
      fs.Setup(fs => fs.File).Returns(file.Object);
      file.Setup(file => file.ReadAllText(FILENAME)).Returns("invalid json");
      var app = new App(workingDir: ".", fs: fs.Object);
      var e = Should.Throw<CommandException>(
        () => Task.FromResult(app.LoadFile<TestObject>(FILENAME))
      );
      e.InnerException?.ShouldNotBeNull();
      e.Message.ShouldBe($"Failed to deserialize file `{FILENAME}`");
    }

    [Fact]
    public void SavesFile() {
      var fs = new Mock<IFileSystem>();
      var file = new Mock<IFile>();
      fs.Setup(fs => fs.File).Returns(file.Object);
      file.Setup(file => file.WriteAllText(FILENAME, DATA));

      var app = new App(workingDir: ".", fs: fs.Object);

      app.SaveFile(FILENAME, DATA);

      file.VerifyAll();
    }

    [Fact]
    public void ThrowsErrorWhenSavingFileDoesNotWork() {
      var fs = new Mock<IFileSystem>(MockBehavior.Strict);
      var file = new Mock<IFile>(MockBehavior.Strict);
      file.Setup(f => f.WriteAllText(FILENAME, DATA)).Throws<Exception>();
      fs.Setup(fs => fs.File).Returns(file.Object);
      var app = new App(workingDir: ".", fs: fs.Object);
      Should.Throw<CommandException>(
        () => app.SaveFile(FILENAME, DATA)
      );
    }

    [Fact]
    public void CreatesAddonManager() {
      var addonRepo = new Mock<IAddonRepo>();
      var configFileRepo = new Mock<IConfigFileRepo>();
      var reporter = new Mock<IReporter>();
      var dependencyGraph = new Mock<IDependencyGraph>();

      var fs = new Mock<IFileSystem>();
      var file = new Mock<IFile>();
      var app = new App(workingDir: ".", fs: fs.Object);

      app.CreateAddonManager(
        addonRepo: addonRepo.Object,
        configFileRepo: configFileRepo.Object,
        reporter: reporter.Object,
        dependencyGraph: dependencyGraph.Object
      ).ShouldBeOfType(typeof(AddonManager));
    }

    [Fact]
    public void CreatesReporter() {
      var fs = new Mock<IFileSystem>();
      var app = new App(workingDir: ".", fs: fs.Object);
      app.CreateReporter(new ConsoleWriter(
        console: new FakeInMemoryConsole(),
        stream: new MemoryStream()
      )).ShouldBeOfType(typeof(Reporter));
    }

    [Fact]
    public void CreatesAddonRepo() {
      var fs = new Mock<IFileSystem>();
      var app = new App(workingDir: ".", fs: fs.Object);
      app.CreateAddonRepo().ShouldBeOfType(typeof(AddonRepo));
    }

    [Fact]
    public void CreatesConfigFileRepo() {
      var fs = new Mock<IFileSystem>();
      var app = new App(workingDir: ".", fs: fs.Object);
      app.CreateConfigFileRepo().ShouldBeOfType(typeof(ConfigFileRepo));
    }
  }
}
