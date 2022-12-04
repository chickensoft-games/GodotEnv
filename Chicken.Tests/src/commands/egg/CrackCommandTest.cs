namespace Chickensoft.Chicken.Tests;

using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading.Tasks;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using Moq;
using Shouldly;
using Xunit;

public class CreateCommandTest {
  [Fact]
  public void Initializes() {
    var command = new CreateCommand();
    command.AppContext.ShouldBeOfType(typeof(App));
    command.Fs.ShouldBeOfType(typeof(FileSystem));
    command.Copier.ShouldBeOfType(typeof(FileCopier));
    command.ArgParser.ShouldBeOfType(typeof(AdditionalArgParser));
  }

  [Fact]
  public void InitializesWithValues() {
    var app = new Mock<IApp>();
    var fs = new Mock<IFileSystem>();
    var copier = new Mock<IFileCopier>();
    var argParser = new Mock<IAdditionalArgParser>();
    var command = new CreateCommand(
      app.Object, fs.Object, copier.Object, argParser.Object
    ) {
      Egg = "template",
      Checkout = "checkout",
      Name = "name",
    };
    command.AppContext.ShouldBeSameAs(app.Object);
    command.Fs.ShouldBeSameAs(fs.Object);
    command.Copier.ShouldBeSameAs(copier.Object);
    command.ArgParser.ShouldBeSameAs(argParser.Object);
    command.Egg.ShouldBe("template");
    command.Checkout.ShouldBe("checkout");
    command.Name.ShouldBe("name");
  }

  [Fact]
  public async Task ThrowsIfNoEgg() {
    var app = new Mock<IApp>();
    var fs = new Mock<IFileSystem>();
    var copier = new Mock<IFileCopier>();
    var argParser = new Mock<IAdditionalArgParser>();
    var command = new CreateCommand(
      app.Object, fs.Object, copier.Object, argParser.Object
    ) { Name = "name" };
    var console = new FakeInMemoryConsole();
    (await Should.ThrowAsync<CommandException>(
      async () => await command.ExecuteAsync(console)
    )).Message.ShouldContain("Template (-t) is required");
  }

  [Fact]
  public async Task ThrowsIfNoName() {
    var app = new Mock<IApp>();
    var fs = new Mock<IFileSystem>();
    var copier = new Mock<IFileCopier>();
    var argParser = new Mock<IAdditionalArgParser>();
    var command = new CreateCommand(
      app.Object, fs.Object, copier.Object, argParser.Object
    ) { Egg = "template" };
    var console = new FakeInMemoryConsole();
    (await Should.ThrowAsync<CommandException>(
      async () => await command.ExecuteAsync(console)
    )).Message.ShouldContain("Output name is required");
  }

  [Fact]
  public async Task ThrowsIfOutputPathExists() {
    var app = new Mock<IApp>();
    var fs = new Mock<IFileSystem>();
    var copier = new Mock<IFileCopier>();
    var argParser = new Mock<IAdditionalArgParser>();
    var command = new CreateCommand(
      app.Object, fs.Object, copier.Object, argParser.Object
    ) {
      Egg = "template",
      Name = "name",
    };

    var console = new FakeInMemoryConsole();
    app.Setup(app => app.WorkingDir).Returns("/");
    app.Setup(app => app.CreateShell("/")).Returns(new Mock<IShell>().Object);
    app.Setup(app => app.CreateLog(console)).Returns(new Mock<ILog>().Object);
    fs.Setup(f => f.Directory.Exists("/name")).Returns(true);

    (await Should.ThrowAsync<CommandException>(
      async () => await command.ExecuteAsync(console)
    )).Message.ShouldContain("already exists");
  }

  [Fact]
  public async Task ThrowsIfLocalSourceAndSourceDirDoesNotExist() {
    var app = new Mock<IApp>();
    var fs = new Mock<IFileSystem>();
    var copier = new Mock<IFileCopier>();
    var argParser = new Mock<IAdditionalArgParser>();
    var command = new CreateCommand(
      app.Object, fs.Object, copier.Object, argParser.Object
    ) {
      Egg = "template",
      Name = "name",
    };

    var console = new FakeInMemoryConsole();
    app.Setup(app => app.WorkingDir).Returns("/");
    app.Setup(app => app.CreateShell("/")).Returns(new Mock<IShell>().Object);
    app.Setup(app => app.CreateLog(console)).Returns(new Mock<ILog>().Object);
    fs.Setup(f => f.Directory.Exists("/name")).Returns(false);
    fs.Setup(f => f.Directory.Exists("/template")).Returns(false);

    (await Should.ThrowAsync<CommandException>(
      async () => await command.ExecuteAsync(console)
    )).Message.ShouldContain("Cannot find template");
  }

  [Fact]
  public async Task GeneratesFromLocalGitRepo() {
    var app = new Mock<IApp>();
    var fs = new Mock<IFileSystem>();
    var copier = new Mock<IFileCopier>();
    var argParser = new Mock<IAdditionalArgParser>();
    var workingDir = "/";
    var name = "name";
    var checkout = "checkout";
    var template = "template";

    var command = new CreateCommand(
      app.Object, fs.Object, copier.Object, argParser.Object
    ) {
      Egg = template,
      Checkout = checkout,
      Name = name,
    };

    var projectPath = "/name";
    var sourceUrl = "/template";

    var console = new FakeInMemoryConsole();
    app.Setup(app => app.WorkingDir).Returns(workingDir);
    var cli = new ShellVerifier();
    var shell = cli.CreateShell(workingDir);
    var projectShell = cli.CreateShell(projectPath);
    var log = new Mock<ILog>();
    app.Setup(app => app.CreateLog(console)).Returns(log.Object);
    fs.Setup(f => f.Directory.Exists(projectPath)).Returns(false);
    app.Setup(app => app.GetRootedPath(template, workingDir))
      .Returns(sourceUrl);
    fs.Setup(f => f.Directory.Exists(sourceUrl)).Returns(true);
    fs.Setup(f => f.Directory.Exists($"{sourceUrl}/.git")).Returns(true);

    app.Setup(app => app.CreateShell(workingDir)).Returns(shell.Object);

    cli.Setup(
      workingDir, new ProcessResult(0), RunMode.Run,
      "git", "clone", "/template", "--recurse-submodules", projectPath
    );

    app.Setup(app => app.CreateShell(projectPath)).Returns(projectShell.Object);

    cli.Setup(
      projectPath, new ProcessResult(0), RunMode.Run,
      "git", "checkout", checkout
    );

    copier.Setup(c => c.RemoveDirectories(fs.Object, "/name", ".git"));
    log.Setup(l => l.Info("Cloned name from /template."));

    var inputs = new Dictionary<string, dynamic?>();

    argParser.Setup(a => a.Parse()).Returns(inputs);
    var loader = new Mock<IEditActionsLoader>();
    app.Setup(a => a.CreateEditActionsLoader(fs.Object)).Returns(loader.Object);
    var editActions = new EditActions(null, null);
    loader.Setup(l => l.Load(projectPath)).Returns(editActions);
    var editActionsRepo = new Mock<IEditActionsRepo>();
    app.Setup(a => a.CreateEditActionsRepo(
      fs.Object, projectPath, editActions, inputs
    )).Returns(editActionsRepo.Object);
    var generator = new Mock<ITemplateGenerator>();
    app.Setup(a => a.CreateTemplateGenerator(
      "name",
      projectPath,
      "template",
      editActionsRepo.Object,
      editActions,
      log.Object
    )).Returns(generator.Object);

    generator.Setup(g => g.Generate());

    await command.ExecuteAsync(console);

    cli.VerifyAll();
    copier.VerifyAll();
    log.VerifyAll();
    generator.VerifyAll();
    argParser.VerifyAll();
    loader.VerifyAll();
    editActionsRepo.VerifyAll();
    app.VerifyAll();
  }

  [Fact]
  public async Task GeneratesFromLocalFolder() {
    var app = new Mock<IApp>();
    var fs = new Mock<IFileSystem>();
    var copier = new Mock<IFileCopier>();
    var argParser = new Mock<IAdditionalArgParser>();
    var workingDir = "/";
    var name = "name";
    var checkout = "checkout";
    var template = "template";

    var command = new CreateCommand(
      app.Object, fs.Object, copier.Object, argParser.Object
    ) {
      Egg = template,
      Checkout = checkout,
      Name = name,
    };

    var projectPath = "/name";
    var sourceUrl = "/template";

    var console = new FakeInMemoryConsole();
    app.Setup(app => app.WorkingDir).Returns(workingDir);
    var cli = new ShellVerifier();
    var log = new Mock<ILog>();
    app.Setup(app => app.CreateLog(console)).Returns(log.Object);
    fs.Setup(f => f.Directory.Exists(projectPath)).Returns(false);
    app.Setup(app => app.GetRootedPath(template, workingDir))
      .Returns(sourceUrl);
    fs.Setup(f => f.Directory.Exists(sourceUrl)).Returns(true);
    fs.Setup(f => f.Directory.Exists($"{sourceUrl}/.git")).Returns(false);

    var filesCopied = new List<string>() {
      $"{projectPath}/file1.txt",
      $"{projectPath}/file2.txt"
    };

    copier.Setup(c => c.CopyDotNet(
      fs.Object, sourceUrl, projectPath, App.DEFAULT_EXCLUSIONS
    )).Returns(filesCopied);

    log.Setup(log => log.Info($"Copied: {filesCopied[0]}"));
    log.Setup(log => log.Info($"Copied: {filesCopied[1]}"));
    log.Setup(log => log.Print(""));

    var inputs = new Dictionary<string, dynamic?>();

    argParser.Setup(a => a.Parse()).Returns(inputs);
    var loader = new Mock<IEditActionsLoader>();
    app.Setup(a => a.CreateEditActionsLoader(fs.Object)).Returns(loader.Object);
    var editActions = new EditActions(null, null);
    loader.Setup(l => l.Load(projectPath)).Returns(editActions);
    var editActionsRepo = new Mock<IEditActionsRepo>();
    app.Setup(a => a.CreateEditActionsRepo(
      fs.Object, projectPath, editActions, inputs
    )).Returns(editActionsRepo.Object);
    var generator = new Mock<ITemplateGenerator>();
    app.Setup(a => a.CreateTemplateGenerator(
      "name",
      projectPath,
      "template",
      editActionsRepo.Object,
      editActions,
      log.Object
    )).Returns(generator.Object);

    generator.Setup(g => g.Generate());

    await command.ExecuteAsync(console);

    cli.VerifyAll();
    copier.VerifyAll();
    log.VerifyAll();
    generator.VerifyAll();
    argParser.VerifyAll();
    loader.VerifyAll();
    editActionsRepo.VerifyAll();
    app.VerifyAll();
  }
}
