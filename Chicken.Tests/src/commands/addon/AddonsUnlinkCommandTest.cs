namespace Chickensoft.Chicken.Tests;
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
    => new AddonsUnlinkCommand().ShouldBeOfType<AddonsUnlinkCommand>();

  [Fact]
  public async Task RemovesSymlink() {
    var app = new Mock<IApp>();
    var fs = new Mock<IFileSystem>();
    var dir = new Mock<IDirectory>();

    var addonRepo = new Mock<IAddonRepo>(MockBehavior.Strict);

    var path = "some/symlink";

    app.Setup(a => a.CreateAddonRepo(fs.Object)).Returns(addonRepo.Object);
    fs.Setup(f => f.Directory).Returns(dir.Object);
    dir.Setup(d => d.Exists(path)).Returns(true);

    app.Setup(app => app.IsDirectorySymlink(fs.Object, path)).Returns(true);
    app.Setup(app => app.DeleteDirectory(fs.Object, path));

    var console = new FakeInMemoryConsole();

    var command = new AddonsUnlinkCommand(app.Object, fs.Object) {
      Path = path
    };

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

    app.Setup(a => a.CreateAddonRepo(fs.Object)).Returns(addonRepo.Object);
    fs.Setup(f => f.Directory).Returns(dir.Object);
    dir.Setup(d => d.Exists(path)).Returns(false);

    var console = new FakeInMemoryConsole();

    var command = new AddonsUnlinkCommand(app.Object, fs.Object) { Path = path };

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

    app.Setup(a => a.CreateAddonRepo(fs.Object)).Returns(addonRepo.Object);
    fs.Setup(f => f.Directory).Returns(dir.Object);
    dir.Setup(d => d.Exists(path)).Returns(true);

    app.Setup(app => app.IsDirectorySymlink(fs.Object, path)).Returns(false);

    var console = new FakeInMemoryConsole();

    var command = new AddonsUnlinkCommand(app.Object, fs.Object) {
      Path = path
    };

    (await Should.ThrowAsync<CommandException>(
      async () => await command.ExecuteAsync(console)
    )).Message.ShouldBe(
      $"Directory `{path}` is not a symlink."
    );
  }
}
