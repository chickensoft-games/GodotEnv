namespace Chickensoft.GoDotAddon.Tests {
  using System.Collections.Generic;
  using System.IO.Abstractions;
  using System.IO.Abstractions.TestingHelpers;
  using global::GoDotAddon;
  using Moq;
  using Newtonsoft.Json;
  using Shouldly;
  using Xunit;


  public class LockFileRepoTest {
    private const string LOCK_FILE_PATH = "file.json";
    private readonly LockFile _lockFile = new() {
      Addons = new Dictionary<string, Dictionary<string, LockFileEntry>>()
    };

    [Fact]
    public void Initializes() {
      var app = new Mock<IApp>();
      var repo = new LockFileRepo(app.Object);
      repo.ShouldBeOfType(typeof(LockFileRepo));
    }


    [Fact]
    public void LoadsFile() {
      var app = new Mock<IApp>();
      var fs = new MockFileSystem(
        new Dictionary<string, MockFileData> {
          { LOCK_FILE_PATH, new MockFileData("") }
        }
      );
      app.Setup(app => app.FS).Returns(fs);
      app.Setup(app => app.LoadFile<LockFile>(LOCK_FILE_PATH)).Returns(
        _lockFile
      );
      var repo = new LockFileRepo(app.Object);
      var lockFile = repo.LoadOrCreateLockFile(LOCK_FILE_PATH);
      lockFile.ShouldBe(_lockFile);
    }

    [Fact]
    public void CreatesFile() {
      var app = new Mock<IApp>();
      var fs = new MockFileSystem();
      app.Setup(app => app.FS).Returns(fs);
      var repo = new LockFileRepo(app.Object);
      var lockFile = repo.LoadOrCreateLockFile(LOCK_FILE_PATH);
      lockFile.ShouldBeOfType(typeof(LockFile));
      lockFile.Addons.ShouldBeEmpty();
    }

    [Fact]
    public void SavesLockFile() {
      var serialized
        = JsonConvert.SerializeObject(_lockFile, Formatting.Indented);
      var app = new Mock<IApp>();
      var fs = new Mock<IFileSystem>();
      var file = new Mock<IFile>();
      fs.Setup(fs => fs.File).Returns(file.Object);
      file.Setup(file => file.WriteAllText(LOCK_FILE_PATH, serialized));
      app.Setup(app => app.FS).Returns(fs.Object);
      var repo = new LockFileRepo(app.Object);
      repo.SaveLockFile(LOCK_FILE_PATH, _lockFile);
      file.VerifyAll();
    }
  }
}
