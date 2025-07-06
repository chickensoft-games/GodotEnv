namespace Chickensoft.GodotEnv.Tests;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Common.Utilities;
using Moq;
using Shouldly;
using Xunit;

public class FileClientTest {
  public record TestJsonModel {
    [JsonPropertyName("name")]
    public string Name { get; }

    [JsonConstructor]
    public TestJsonModel(string name) {
      Name = name;
    }
  }

  public const string JSON_FILE_CONTENTS =
    /*lang=json,strict*/
    """
    {
      "name": "test"
    }
    """;

  public const string JSON_FILE_CONTENTS_TRAILING_COMMA =
    /*lang=json*/
    """
    {
      "name": "test",
    }
    """;

  public const string JSON_FILE_CONTENTS_ALT =
    /*lang=json,strict*/
    """
    {
      "name": "alternative"
    }
    """;

  [Theory]
  [MemberData(nameof(GetSystemInfoForUnixOSes))]
  public void InitializesUnix(ISystemInfo systemInfo) {
    // var systemInfo = new MockSystemInfo(OSType.MacOS, CPUArch.Arm64);
    var fs = GetFs('/');
    var computer = new Mock<IComputer>();
    var client = new FileClient(systemInfo, fs.Object, computer.Object, new Mock<IProcessRunner>().Object);
    client.ShouldBeAssignableTo<IFileClient>();
    client.Files.ShouldBe(fs.Object);

    client.Separator.ShouldBe('/');
  }

  [Fact]
  public void InitializesWindows() {
    var systemInfo = new MockSystemInfo(OSType.Windows, CPUArch.X64);
    var fs = GetFs('\\');
    var computer = new Mock<IComputer>();
    var client = new FileClient(
      systemInfo, fs.Object, computer.Object, new Mock<IProcessRunner>().Object
    );
    client.ShouldBeAssignableTo<IFileClient>();
    client.Files.ShouldBe(fs.Object);

    client.Separator.ShouldBe('\\');
  }

  [Theory]
  [MemberData(nameof(GetSystemInfoForUnixOSes))]
  public async Task DeleteDirectoryDeletesNonSymlinkOnUnix(ISystemInfo systemInfo) {
    const string path = "/a/b/c";

    var fs = new MockFileSystem(new Dictionary<string, MockFileData> { { path, new MockFileData("test") } });

    var computer = new Mock<IComputer>();
    var client = new FileClient(
      systemInfo, fs, computer.Object, new Mock<IProcessRunner>().Object
    );

    await client.DeleteDirectory(path);

    fs.Directory.Exists(path).ShouldBe(false);
  }

  [Fact]
  public async Task DeleteDirectoryDeletesNonSymlinkOnWindows() {
    var systemInfo = new MockSystemInfo(OSType.Windows, CPUArch.X64);
    const string path = "/a/b/c";

    var fs = new MockFileSystem(new Dictionary<string, MockFileData> { { path, new MockFileData("test") } });

    var computer = new Mock<IComputer>();
    var client = new FileClient(
      systemInfo, fs, computer.Object, new Mock<IProcessRunner>().Object
    );

    await client.DeleteDirectory(path);

    fs.Directory.Exists(path).ShouldBe(false);
  }


  [Theory]
  [MemberData(nameof(GetSystemInfoForUnixOSes))]
  public void GetsUserDirectoryOnUnix(ISystemInfo systemInfo) {
    var fs = GetFs('/');
    var computer = new Mock<IComputer>();
    var client = new FileClient(
      systemInfo, fs.Object, computer.Object, new Mock<IProcessRunner>().Object
    );

    client.UserDirectory.ShouldBe(
      Path.TrimEndingDirectorySeparator(
        Environment.GetFolderPath(
          Environment.SpecialFolder.UserProfile,
          Environment.SpecialFolderOption.DoNotVerify
        )
      )
    );
  }

  [Fact]
  public void GetsUserDirectoryOnWindows() {
    var systemInfo = new MockSystemInfo(OSType.Windows, CPUArch.X64);
    var fs = GetFs('/');
    var computer = new Mock<IComputer>();
    var client = new FileClient(
      systemInfo, fs.Object, computer.Object, new Mock<IProcessRunner>().Object
    );

    client.UserDirectory.ShouldBe(
      Path.TrimEndingDirectorySeparator(
        Environment.GetFolderPath(
          Environment.SpecialFolder.UserProfile,
          Environment.SpecialFolderOption.DoNotVerify
        )
      )
    );
  }

  [Fact]
  public void DirectorySymlinkTargetGetsTarget() {
    const string path = "/a/b/c";
    const string pathToTarget = "/a/a2";

    var systemInfo = new MockSystemInfo(OSType.Linux, CPUArch.X64);
    var fs = GetFs('/');
    var computer = new Mock<IComputer>();
    var client = new FileClient(
      systemInfo, fs.Object, computer.Object, new Mock<IProcessRunner>().Object
    );
    var dif = new Mock<IDirectoryInfoFactory>();
    var di = new Mock<IDirectoryInfo>();
    fs.Setup(fs => fs.DirectoryInfo).Returns(dif.Object);
    dif.Setup(dif => dif.New(path)).Returns(di.Object);
    di.Setup(di => di.LinkTarget).Returns(pathToTarget);

    client.DirectorySymlinkTarget(path).ShouldBe(pathToTarget);
  }

  [Fact]
  public void CreateDirectoryCreatesDirectory() {
    const string path = "/a/b/c";

    var systemInfo = new MockSystemInfo(OSType.Linux, CPUArch.X64);
    var fs = GetFs('/');
    var computer = new Mock<IComputer>();
    var client = new FileClient(
      systemInfo, fs.Object, computer.Object, new Mock<IProcessRunner>().Object
    );
    var dir = new Mock<IDirectory>();
    fs.Setup(fs => fs.Directory).Returns(dir.Object);
    dir.Setup(dir => dir.CreateDirectory(path));

    client.CreateDirectory(path);

    dir.Verify(dir => dir.CreateDirectory(path));
  }

  [Fact]
  public void CombineCombinesPathComponents() {
    var systemInfo = new MockSystemInfo(OSType.Linux, CPUArch.X64);
    var fs = new Mock<IFileSystem>();
    var path = new Mock<IPath>();
    fs.Setup(fs => fs.Path).Returns(path.Object);
    path.Setup(path => path.DirectorySeparatorChar)
      .Returns('/');
    var expectedArgs = new[] { "a", "b" };
    path.Setup(path => path.Combine(
      It.Is<string[]>(args => args.SequenceEqual(expectedArgs)))
    ).Returns("a/b");

    var computer = new Mock<IComputer>();
    var client = new FileClient(
      systemInfo, fs.Object, computer.Object, new Mock<IProcessRunner>().Object
    );

    client.Combine("a", "b");

    path.VerifyAll();
  }

  // [Fact]
  // public async Task CreateSymlinkCreatesSymlink() {
  //   const string path = "/a/b/c";
  //   const string pathToTarget = "/a/a2";

  //   var fs = GetFs('/');
  //   var computer = new Mock<IComputer>();
  //   var client = new FileClient(
  //     fs.Object, computer.Object, new Mock<IProcessRunner>().Object
  //   );
  //   var dir = new Mock<IDirectory>();
  //   fs.Setup(fs => fs.Directory).Returns(dir.Object);
  //   dir.Setup(dir => dir.CreateSymbolicLink(path, pathToTarget));

  //   await client.CreateSymlink(path, pathToTarget);

  //   dir.Verify(dir => dir.CreateSymbolicLink(path, pathToTarget));
  // }

  [Fact]
  public void IsDirectorySymlinkVerifiesSymlink() {
    const string path = "/a/b/c";
    const string pathToTarget = "/a/a2";

    var systemInfo = new MockSystemInfo(OSType.Linux, CPUArch.X64);
    var fs = GetFs('/');
    var computer = new Mock<IComputer>();
    var client = new FileClient(
      systemInfo, fs.Object, computer.Object, new Mock<IProcessRunner>().Object
    );
    var dif = new Mock<IDirectoryInfoFactory>();
    var di = new Mock<IDirectoryInfo>();
    fs.Setup(fs => fs.DirectoryInfo).Returns(dif.Object);
    dif.Setup(dif => dif.New(path)).Returns(di.Object);
    di.Setup(di => di.LinkTarget).Returns(pathToTarget);

    client.IsDirectorySymlink(path).ShouldBe(true);
  }

  [Fact]
  public void IsDirectorySymlinkVerifiesNonSymlink() {
    const string path = "/a/b/c";

    var systemInfo = new MockSystemInfo(OSType.Linux, CPUArch.X64);
    var fs = new MockFileSystem(new Dictionary<string, MockFileData> {
      { path, new MockFileData("test") }
    });

    var computer = new Mock<IComputer>();
    var client = new FileClient(
      systemInfo, fs, computer.Object, new Mock<IProcessRunner>().Object
    );

    client.IsDirectorySymlink(path).ShouldBe(false);
  }

  // TODO: Update test.
  // [Fact]
  // public async Task DeleteDirectoryDeletesSymlink() {
  //   const string path = "/a/b/c";
  //   const string pathToTarget = "/a/a2";

  //   var fs = GetFs('/');
  //   var computer = new Mock<IComputer>();
  //   var client = new FileClient(
  //     fs.Object, computer.Object, new Mock<IProcessRunner>().Object
  //   );
  //   var dif = new Mock<IDirectoryInfoFactory>();
  //   var di = new Mock<IDirectoryInfo>();
  //   var dir = new Mock<IDirectory>();
  //   fs.Setup(fs => fs.DirectoryInfo).Returns(dif.Object);
  //   dif.Setup(dif => dif.FromDirectoryName(path)).Returns(di.Object);
  //   di.Setup(di => di.LinkTarget).Returns(pathToTarget);

  //   fs.Setup(fs => fs.Directory).Returns(dir.Object);
  //   dir.Setup(dir => dir.Delete(path));

  //   await client.DeleteDirectory(path);

  //   dir.VerifyAll();
  // }

  [Theory]
  [MemberData(nameof(GetSystemInfoForAllOSes))]
  public async Task DeleteDirectoryDeletesNonSymlink(ISystemInfo systemInfo) {
    const string path = "/a/b/c";

    var fs = new MockFileSystem(new Dictionary<string, MockFileData> {
      { path, new MockFileData("test") }
    });

    var computer = new Mock<IComputer>();
    var client = new FileClient(
      systemInfo, fs, computer.Object, new Mock<IProcessRunner>().Object
    );

    await client.DeleteDirectory(path);

    fs.Directory.Exists(path).ShouldBe(false);
  }

  [Fact]
  public async Task CopyBulkCopiesOnWindows() {
    var systemInfo = new MockSystemInfo(OSType.Windows, CPUArch.X64);
    var fs = GetFs('\\');
    var path = new Mock<IPath>();
    path.Setup(path => path.DirectorySeparatorChar).Returns('\\');
    fs.Setup(fs => fs.Path).Returns(path.Object);

    var cli = new ShellVerifier();
    var shell = cli.CreateShell("/");

    cli.RunsUnchecked(
      "/", new ProcessResult(0),
      "robocopy", "a", "b", "/e", "/xd", ".git"
    );

    var computer = new Mock<IComputer>();
    var client = new FileClient(
      systemInfo, fs.Object, computer.Object, new Mock<IProcessRunner>().Object
    );

    await client.CopyBulk(shell.Object, "a", "b");

    cli.VerifyAll();
  }

  [Fact]
  public async Task CopyBulkCopiesOnUnix() {
    var systemInfo = new MockSystemInfo(OSType.Linux, CPUArch.X64);
    var fs = GetFs('/');
    var path = new Mock<IPath>();
    path.Setup(path => path.DirectorySeparatorChar).Returns('/');
    fs.Setup(fs => fs.Path).Returns(path.Object);

    var cli = new ShellVerifier();
    var shell = cli.CreateShell("/");

    cli.Runs(
      "/", new ProcessResult(0),
      "rsync", "-av", "a", "b", "--exclude", ".git"
    );

    var computer = new Mock<IComputer>();
    var client = new FileClient(
      systemInfo, fs.Object, computer.Object, new Mock<IProcessRunner>().Object
    );

    await client.CopyBulk(shell.Object, "a", "b");

    cli.VerifyAll();
  }

  [Fact]
  public async Task CopyBulkOnWindowsFailsIfRobocopyHasBadExitCode() {
    var systemInfo = new MockSystemInfo(OSType.Windows, CPUArch.X64);
    var fs = GetFs('\\');
    var path = new Mock<IPath>();
    path.Setup(path => path.DirectorySeparatorChar).Returns('\\');
    fs.Setup(fs => fs.Path).Returns(path.Object);

    var cli = new ShellVerifier();
    var shell = cli.CreateShell("/");

    cli.RunsUnchecked(
      "/", new ProcessResult(8),
      "robocopy", "a", "b", "/e", "/xd", ".git"
    );

    var computer = new Mock<IComputer>();
    var client = new FileClient(
      systemInfo, fs.Object, computer.Object, new Mock<IProcessRunner>().Object
    );

    await Should.ThrowAsync<IOException>(
      async () => await client.CopyBulk(shell.Object, "a", "b")
    );

    cli.VerifyAll();
  }

  [Fact]
  public void FileThatExistsFindsFirstPossibleName() {
    var systemInfo = new MockSystemInfo(OSType.Linux, CPUArch.X64);
    var possibleNames = new string[] {
      "/a.txt",
      "/b.txt",
      "/c.txt"
    };

    var fs = new MockFileSystem(possibleNames.ToDictionary(
      name => name,
      _ => new MockFileData("test")
    ));

    var computer = new Mock<IComputer>();
    var client = new FileClient(
      systemInfo, fs, computer.Object, new Mock<IProcessRunner>().Object
    );

    client.FileThatExists(["0.txt", "b.txt", "a.txt"], "/")
      .ShouldBe("/b.txt");
    client.FileThatExists(["0.txt", "a.txt", "b.txt"], "/")
      .ShouldBe("/a.txt");
    client.FileThatExists(["0.txt", "1.txt"], "/")
      .ShouldBeNull();
  }

  [Fact]
  public void GetRootedPathDeterminesRootedPath() {
    var systemInfo = new MockSystemInfo(OSType.Linux, CPUArch.X64);
    var fs = new MockFileSystem();
    var computer = new Mock<IComputer>();
    var client = new FileClient(
      systemInfo, fs, computer.Object, new Mock<IProcessRunner>().Object
    );

    var expectedBasePath = fs.Path.GetFullPath(fs.Path.Combine("/", "/a/b"));
    client.GetRootedPath("a/b", "/").ShouldBe(expectedBasePath);
    client.GetRootedPath("/a/b/c", "/").ShouldBe("/a/b/c");
  }

  [Fact]
  public void FileExistsDeterminesExistence() {
    var systemInfo = new MockSystemInfo(OSType.Linux, CPUArch.X64);
    var fs = new MockFileSystem(new Dictionary<string, MockFileData> {
      { "/a.txt", new MockFileData("test") }
    });

    var computer = new Mock<IComputer>();
    var client = new FileClient(
      systemInfo, fs, computer.Object, new Mock<IProcessRunner>().Object
    );

    client.FileExists("/a.txt").ShouldBe(true);
    client.FileExists("/b.txt").ShouldBe(false);
  }

  [Fact]
  public void DirectoryExistsDeterminesExistence() {
    var systemInfo = new MockSystemInfo(OSType.Linux, CPUArch.X64);
    var fs = new MockFileSystem(new Dictionary<string, MockFileData> {
      { "/a/b/c", new MockDirectoryData() }
    });

    var computer = new Mock<IComputer>();
    var client = new FileClient(
      systemInfo, fs, computer.Object, new Mock<IProcessRunner>().Object
    );

    client.DirectoryExists("/a/b/c").ShouldBe(true);
    client.DirectoryExists("/a/b/d").ShouldBe(false);
  }

  [Fact]
  public void ReadJsonFileReturnsModel() {
    var systemInfo = new MockSystemInfo(OSType.Linux, CPUArch.X64);
    var fs = new MockFileSystem(new Dictionary<string, MockFileData> {
      { "model.json", new MockFileData(JSON_FILE_CONTENTS) }
    });

    var computer = new Mock<IComputer>();
    var client = new FileClient(
      systemInfo, fs, computer.Object, new Mock<IProcessRunner>().Object
    );

    client.ReadJsonFile<TestJsonModel>("model.json")
      .ShouldBe(new TestJsonModel(name: "test"));
  }

  [Fact]
  public void ReadJsonFilesAllowsTrailingCommas() {
    var systemInfo = new MockSystemInfo(OSType.Linux, CPUArch.X64);
    var fs = new MockFileSystem(new Dictionary<string, MockFileData> {
      { "model.json", new MockFileData(JSON_FILE_CONTENTS_TRAILING_COMMA) }
    });

    var computer = new Mock<IComputer>();
    var client = new FileClient(
      systemInfo, fs, computer.Object, new Mock<IProcessRunner>().Object
    );

    client.ReadJsonFile<TestJsonModel>("model.json")
      .ShouldBe(new TestJsonModel(name: "test"));
  }

  [Fact]
  public void ReadJsonFileThrowsIfFileFailsToBeDeserialized() {
    var systemInfo = new MockSystemInfo(OSType.Linux, CPUArch.X64);
    var fs = new MockFileSystem(new Dictionary<string, MockFileData> {
      { "model.json", new MockFileData("") }
    });

    var computer = new Mock<IComputer>();
    var client = new FileClient(
      systemInfo, fs, computer.Object, new Mock<IProcessRunner>().Object
    );

    Should.Throw<JsonException>(
      () => client.ReadJsonFile<TestJsonModel>("model.json")
    );
  }

  [Fact]
  public void ReadJsonFileReadsFromFirstFileItFounds() {
    var systemInfo = new MockSystemInfo(OSType.Linux, CPUArch.X64);
    var fs = new MockFileSystem(new Dictionary<string, MockFileData> {
      { "model_a.json", new MockFileData(JSON_FILE_CONTENTS) },
      { "model_b.json", new MockFileData(JSON_FILE_CONTENTS_ALT) }
    });

    var computer = new Mock<IComputer>();
    var client = new FileClient(
      systemInfo, fs, computer.Object, new Mock<IProcessRunner>().Object
    );

    client.ReadJsonFile(
      "",
      ["model_a.json", "model_b.json"],
      out var filename,
      new TestJsonModel(name: "default")
    ).ShouldBe(new TestJsonModel(name: "test"));

    filename.ShouldBe("model_a.json");
  }

  [Fact]
  public void ReadJsonFileThrowsIfDeserializedValueIsNull() {
    var systemInfo = new MockSystemInfo(OSType.Linux, CPUArch.X64);
    var fs = new MockFileSystem(new Dictionary<string, MockFileData> {
      { "model.json", new MockFileData("") }
    });

    var computer = new Mock<IComputer>();
    var client = new FileClient(
      systemInfo, fs, computer.Object, new Mock<IProcessRunner>().Object
    );

    var e = Should.Throw<IOException>(
      () => client.ReadJsonFile(
        "",
        ["model.json"],
        out var filename,
        new TestJsonModel(name: "default")
      )
    );

    e.InnerException.ShouldBeOfType<JsonException>();
  }

  [Fact]
  public void ReadJsonFileThrowsIOExceptionOnOtherError() {
    var systemInfo = new MockSystemInfo(OSType.Linux, CPUArch.X64);
    var fs = new MockFileSystem(new Dictionary<string, MockFileData> {
      { "model.json", new MockFileData("[}") }
    });

    var computer = new Mock<IComputer>();
    var client = new FileClient(
      systemInfo, fs, computer.Object, new Mock<IProcessRunner>().Object
    );

    Should.Throw<IOException>(
      () => client.ReadJsonFile(
        "",
        ["model.json", "model_a.json", "model_b.json"],
        out var filename,
        new TestJsonModel(name: "default")
      )
    );
  }

  [Fact]
  public void ReadJsonFileChecksOtherPossibleFilenames() {
    var systemInfo = new MockSystemInfo(OSType.Linux, CPUArch.X64);
    var fs = new MockFileSystem(new Dictionary<string, MockFileData> {
      { "model_b.json", new MockFileData(JSON_FILE_CONTENTS_ALT) }
    });

    var computer = new Mock<IComputer>();
    var client = new FileClient(
      systemInfo, fs, computer.Object, new Mock<IProcessRunner>().Object
    );

    client.ReadJsonFile(
      "",
      ["model.json", "model_a.json", "model_b.json"],
      out var filename,
      new TestJsonModel(name: "default")
    ).ShouldBe(new TestJsonModel(name: "alternative"));

    filename.ShouldBe("model_b.json");
  }

  [Fact]
  public void ReadJsonFileReturnsDefaultValues() {
    var systemInfo = new MockSystemInfo(OSType.Linux, CPUArch.X64);
    var fs = new MockFileSystem();

    var computer = new Mock<IComputer>();
    var client = new FileClient(
      systemInfo, fs, computer.Object, new Mock<IProcessRunner>().Object
    );

    client.ReadJsonFile(
      "",
      ["model.json", "model_a.json", "model_b.json"],
      out var filename,
      new TestJsonModel(name: "default")
    ).ShouldBe(new TestJsonModel(name: "default"));

    filename.ShouldBe("model.json");
  }

  private static Mock<IFileSystem> GetFs(
    char directorySeparatorChar, Action<Mock<IPath>>? pathSetups = null
  ) {
    var fs = new Mock<IFileSystem>();
    var path = new Mock<IPath>();
    fs.Setup(fs => fs.Path).Returns(path.Object);
    path.Setup(path => path.DirectorySeparatorChar)
      .Returns(directorySeparatorChar);
    pathSetups?.Invoke(path);
    return fs;
  }

  public static IEnumerable<object[]> GetSystemInfoForUnixOSes() {
    yield return [
      new MockSystemInfo(OSType.Linux, CPUArch.X64)
    ];
    yield return [
      new MockSystemInfo(OSType.MacOS, CPUArch.Arm64)
    ];
  }

  public static IEnumerable<object[]> GetSystemInfoForAllOSes() {
    var oSes = GetSystemInfoForUnixOSes();

    oSes = oSes.Append([
      new MockSystemInfo(OSType.Windows, CPUArch.X64)
    ]);

    foreach (var os in oSes) {
      yield return os;
    }
  }
}
