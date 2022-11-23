namespace Chickensoft.Chicken.Tests;

using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using CliFx.Exceptions;
using Moq;
using Shouldly;
using Xunit;

public class FileCopierTest {
  [Fact]
  public void Initializes() {
    var copier = new FileCopier();
    copier.ShouldBeOfType<FileCopier>();
  }

  [Fact]
  public async Task CopiesOnWindows() {
    var fs = new Mock<IFileSystem>();
    var path = new Mock<IPath>();
    path.Setup(path => path.DirectorySeparatorChar).Returns('\\');
    fs.Setup(fs => fs.Path).Returns(path.Object);

    var cli = new ShellVerifier();
    var shell = cli.CreateShell("/");

    cli.Setup(
      "/",
      new ProcessResult(0),
      RunMode.RunUnchecked,
      "robocopy", "a", "b", "/e", "/xd", ".git"
    );

    var copier = new FileCopier();

    await copier.Copy(fs.Object, shell.Object, "a", "b");

    cli.VerifyAll();
  }

  [Fact]
  public async Task CopiesOnUnix() {
    var fs = new Mock<IFileSystem>();
    var path = new Mock<IPath>();
    path.Setup(path => path.DirectorySeparatorChar).Returns('/');
    fs.Setup(fs => fs.Path).Returns(path.Object);

    var cli = new ShellVerifier();
    var shell = cli.CreateShell("/");

    cli.Setup(
      "/",
      new ProcessResult(0),
      RunMode.Run,
      "rsync", "-av", "a", "b", "--exclude", ".git"
    );

    var copier = new FileCopier();

    await copier.Copy(fs.Object, shell.Object, "a", "b");

    cli.VerifyAll();
  }

  [Fact]
  public async Task CopyOnWindowsFailsIfRobocopyHasBadExitCode() {
    var fs = new Mock<IFileSystem>();
    var path = new Mock<IPath>();
    path.Setup(path => path.DirectorySeparatorChar).Returns('\\');
    fs.Setup(fs => fs.Path).Returns(path.Object);

    var cli = new ShellVerifier();
    var shell = cli.CreateShell("/");

    cli.Setup(
      "/",
      new ProcessResult(8),
      RunMode.RunUnchecked,
      "robocopy", "a", "b", "/e", "/xd", ".git"
    );

    var copier = new FileCopier();

    await Should.ThrowAsync<CommandException>(
      async () => await copier.Copy(fs.Object, shell.Object, "a", "b")
    );

    cli.VerifyAll();
  }

  [Fact]
  public void CopyDotNetCopiesFiles() {
    var data = new MockFileData("");
    var fs = new MockFileSystem(new Dictionary<string, MockFileData>() {
      ["a/file1.txt"] = data,
      ["a/skip.txt"] = data,
      ["a/folder/file1.txt"] = data,
      ["a/folder/skip.txt"] = data,
      ["a/skip/skip.txt"] = data,
    });

    var copier = new FileCopier();

    var exclusions = new HashSet<string> { "skip**" };

    var result = copier.CopyDotNet(
      fs,
      "a",
      "b",
      exclusions
    );

    result.ShouldBe(new string[] {
      "b/file1.txt",
      "b/folder/",
      "b/folder/file1.txt"
    });

    fs.AllFiles.ShouldBe(new[] {
      "/a/file1.txt",
      "/a/skip.txt",
      "/a/folder/file1.txt",
      "/a/folder/skip.txt",
      "/a/skip/skip.txt",

      "/b/file1.txt",
      "/b/folder/file1.txt",
    });
  }

  [Fact]
  public void CopyDotNetThrowsIfNoSource() {
    var fs = new MockFileSystem(new Dictionary<string, MockFileData>() {
      ["b/file1.txt"] = new MockFileData("b/file1.txt"),
    });

    var copier = new FileCopier();

    var exclusions = new HashSet<string> { };

    Should.Throw<DirectoryNotFoundException>(
      () => copier.CopyDotNet(
        fs,
        "a",
        "b",
        exclusions
      )
    );
  }


  [Fact]
  public void RemoveDirectoriesThrowsIfNoSource() {
    var fs = new MockFileSystem(new Dictionary<string, MockFileData>() {
      ["b/file1.txt"] = new MockFileData(""),
    });

    var copier = new FileCopier();

    var exclusions = new HashSet<string> { };

    Should.Throw<DirectoryNotFoundException>(
      () => copier.RemoveDirectories(
        fs,
        "a",
        "b"
      )
    );
  }

  [Fact]
  public void RemoveDirectoriesRemovesDirectories() {
    var data = new MockFileData("");
    var fs = new MockFileSystem(new Dictionary<string, MockFileData>() {
      ["a/file1.txt"] = data,
      ["a/.git/file1.txt"] = data,
      ["a/folder/file1.txt"] = data,
      ["a/folder/.git/file1.txt"] = data,
    });

    var copier = new FileCopier();

    copier.RemoveDirectories(
      fs,
      "a",
      ".git"
    );

    fs.AllFiles.ShouldBe(new[] {
      "/a/file1.txt",
      "/a/folder/file1.txt",
    });
  }
}
