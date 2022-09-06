namespace Chickensoft.Chicken.Tests {
  using System.IO.Abstractions;
  using System.Threading.Tasks;
  using CliFx.Exceptions;
  using CliFx.Infrastructure;
  using Moq;
  using Shouldly;
  using Xunit;

  public class AddonLinkCommandTest {
    [Fact]
    public void Initializes()
      => new AddonLinkCommand().ShouldBeOfType<AddonLinkCommand>();

    [Fact]
    public async Task CreatesASymlink() {
      var app = new Mock<IApp>();
      var fs = new Mock<IFileSystem>();
      var dir = new Mock<IDirectory>();

      var addonRepo = new Mock<IAddonRepo>(MockBehavior.Strict);

      var source = "some/source";
      var target = "some/target";

      app.Setup(a => a.FS).Returns(fs.Object);
      app.Setup(a => a.CreateAddonRepo()).Returns(addonRepo.Object);
      fs.Setup(f => f.Directory).Returns(dir.Object);
      dir.Setup(d => d.Exists(source)).Returns(true);
      dir.Setup(d => d.Exists(target)).Returns(false);

      addonRepo.Setup(ar => ar.CreateSymlink(target, source));

      var console = new FakeInMemoryConsole();

      var command = new AddonLinkCommand(app.Object) {
        Source = source,
        Target = target
      };

      await command.ExecuteAsync(console);

      console.ReadOutputString().ShouldContain(
        "Created symlink from `some/source` to `some/target`."
      );
    }

    [Fact]
    public async Task ThrowsErrorIfTargetExists() {
      var app = new Mock<IApp>();
      var fs = new Mock<IFileSystem>();
      var dir = new Mock<IDirectory>();

      var addonRepo = new Mock<IAddonRepo>(MockBehavior.Strict);

      var source = "some/source";
      var target = "some/target";

      app.Setup(a => a.FS).Returns(fs.Object);
      app.Setup(a => a.CreateAddonRepo()).Returns(addonRepo.Object);
      fs.Setup(f => f.Directory).Returns(dir.Object);

      dir.Setup(d => d.Exists(target)).Returns(true);

      var console = new FakeInMemoryConsole();

      var command = new AddonLinkCommand(app.Object) {
        Source = source,
        Target = target
      };

      (await Should.ThrowAsync<CommandException>(
        async () => await command.ExecuteAsync(console)
      )).Message.ShouldContain(
        $"Target directory `{target}` already exists."
      );
    }

    [Fact]
    public async Task ThrowsErrorIfSourceDoesNotExist() {
      var app = new Mock<IApp>();
      var fs = new Mock<IFileSystem>();
      var dir = new Mock<IDirectory>();

      var addonRepo = new Mock<IAddonRepo>(MockBehavior.Strict);

      var source = "some/source";
      var target = "some/target";

      app.Setup(a => a.FS).Returns(fs.Object);
      app.Setup(a => a.CreateAddonRepo()).Returns(addonRepo.Object);
      fs.Setup(f => f.Directory).Returns(dir.Object);

      dir.Setup(d => d.Exists(target)).Returns(false);
      dir.Setup(d => d.Exists(source)).Returns(false);

      var console = new FakeInMemoryConsole();

      var command = new AddonLinkCommand(app.Object) {
        Source = source,
        Target = target
      };

      (await Should.ThrowAsync<CommandException>(
        async () => await command.ExecuteAsync(console)
      )).Message.ShouldContain(
        "Source directory `some/source` does not exist."
      );
    }
  }
}
