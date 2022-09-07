namespace Chickensoft.Chicken.Tests {
  using System.IO.Abstractions;
  using System.Threading.Tasks;
  using CliFx.Exceptions;
  using CliFx.Infrastructure;
  using Moq;
  using Shouldly;
  using Xunit;

  public class AddonUnlinkCommandTest {
    [Fact]
    public void Initializes()
      => new AddonUnlinkCommand().ShouldBeOfType<AddonUnlinkCommand>();

    [Fact]
    public async Task RemovesSymlink() {
      var app = new Mock<IApp>();
      var fs = new Mock<IFileSystem>();
      var dir = new Mock<IDirectory>();

      var addonRepo = new Mock<IAddonRepo>(MockBehavior.Strict);

      var path = "some/symlink";

      app.Setup(a => a.FS).Returns(fs.Object);
      app.Setup(a => a.CreateAddonRepo()).Returns(addonRepo.Object);
      fs.Setup(f => f.Directory).Returns(dir.Object);
      dir.Setup(d => d.Exists(path)).Returns(true);

      addonRepo.Setup(repo => repo.IsDirectorySymlink(path)).Returns(true);
      addonRepo.Setup(repo => repo.DeleteDirectory(path));

      var console = new FakeInMemoryConsole();

      var command = new AddonUnlinkCommand(app.Object) { Path = path };

      await command.ExecuteAsync(console);

      console.ReadOutputString().ShouldContain(
        $"Successfully removed symlink at `{path}`."
      );
    }

    [Fact]
    public async Task ThrowsErrorIfPathDoesNotExist() {
      var app = new Mock<IApp>();
      var fs = new Mock<IFileSystem>();
      var dir = new Mock<IDirectory>();

      var addonRepo = new Mock<IAddonRepo>(MockBehavior.Strict);

      var path = "some/symlink";

      app.Setup(a => a.FS).Returns(fs.Object);
      app.Setup(a => a.CreateAddonRepo()).Returns(addonRepo.Object);
      fs.Setup(f => f.Directory).Returns(dir.Object);
      dir.Setup(d => d.Exists(path)).Returns(false);

      var console = new FakeInMemoryConsole();

      var command = new AddonUnlinkCommand(app.Object) { Path = path };

      (await Should.ThrowAsync<CommandException>(
        async () => await command.ExecuteAsync(console)
      )).Message.ShouldBe(
        $"Directory `{path}` does not exist."
      );
    }

    [Fact]
    public async Task ThrowsErrorIfPathIsNotASymlink() {
      var app = new Mock<IApp>();
      var fs = new Mock<IFileSystem>();
      var dir = new Mock<IDirectory>();

      var addonRepo = new Mock<IAddonRepo>(MockBehavior.Strict);

      var path = "some/symlink";

      app.Setup(a => a.FS).Returns(fs.Object);
      app.Setup(a => a.CreateAddonRepo()).Returns(addonRepo.Object);
      fs.Setup(f => f.Directory).Returns(dir.Object);
      dir.Setup(d => d.Exists(path)).Returns(true);

      addonRepo.Setup(repo => repo.IsDirectorySymlink(path)).Returns(false);

      var console = new FakeInMemoryConsole();

      var command = new AddonUnlinkCommand(app.Object) { Path = path };

      (await Should.ThrowAsync<CommandException>(
        async () => await command.ExecuteAsync(console)
      )).Message.ShouldBe(
        $"Directory `{path}` is not a symlink."
      );
    }
  }
}
