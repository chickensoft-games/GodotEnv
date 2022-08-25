namespace Chickensoft.Chicken.Tests {
  using System.IO.Abstractions;
  using System.Threading.Tasks;
  using Moq;
  using Shouldly;
  using Xunit;

  public class FileCopierTest {
    [Fact]
    public void Initializes() {
      var fs = new Mock<IFileSystem>();
      var shell = new Mock<IShell>();
      var copier = new FileCopier(shell.Object, fs.Object);
      copier.ShouldBeOfType<FileCopier>();
      copier.Shell.ShouldBe(shell.Object);
      copier.FS.ShouldBe(fs.Object);
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
        RunMode.Run,
        "robocopy", "a", "b", "/e", "/xd", ".git"
      );

      var copier = new FileCopier(shell.Object, fs.Object);

      await copier.Copy("a", "b");

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

      var copier = new FileCopier(shell.Object, fs.Object);

      await copier.Copy("a", "b");

      cli.VerifyAll();
    }
  }
}
