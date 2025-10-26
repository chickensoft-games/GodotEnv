namespace Chickensoft.GodotEnv.Tests;

using System;
using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Common.Utilities;
using Chickensoft.GodotEnv.Features.Addons.Commands;
using Chickensoft.GodotEnv.Features.Addons.Domain;
using Chickensoft.GodotEnv.Features.Addons.Models;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using Moq;
using Shouldly;
using Xunit;

public class AddonsInstallCommandTest
{
  private sealed class TestException : Exception
  {
    public TestException(string message) : base(message) { }
    public override string ToString() => Message;
  }

  [Fact]
  public void Initializes()
  {
    var context = new Mock<IExecutionContext>();
    var command = new AddonsInstallCommand(context.Object);

    command.ExecutionContext.ShouldBeSameAs(context.Object);
  }

  private static AddonsInstallCommand BuildSubject(
    out Mock<IAddonsInstaller> addonsInstaller,
    out FakeInMemoryConsole console,
    out ILog log
  )
  {
    // Addons command operates at a high level of abstraction given the number
    // of systems required to install addons, so we'll setup all the mocks here
    // and provide access to the relevant mocks for testing via out vars.

    var systemInfo = new Mock<ISystemInfo>();
    systemInfo.Setup(sys => sys.CPUArch).Returns(CPUArch.X64);
    systemInfo.Setup(sys => sys.OS).Returns(OSType.Linux);
    systemInfo.Setup(sys => sys.OSFamily).Returns(OSFamily.Unix);
    var config = MockConfig.Get();
    var context = new Mock<IExecutionContext>();
    var fakeConsole = new FakeInMemoryConsole();
    console = fakeConsole;

    // Use real log to test colors in output
    log = new Log(systemInfo.Object, config.Object, console);

    var workingDir = "/";

    context.Setup(context => context.SystemInfo).Returns(systemInfo.Object);
    context.Setup(context => context.CreateLog(fakeConsole)).Returns(log);
    context.Setup(context => context.WorkingDir).Returns(workingDir);

    var addonsFileRepo = new Mock<IAddonsFileRepository>();
    var addonGraph = new Mock<IAddonGraph>();
    var addonsRepo = new Mock<IAddonsRepository>();
    var fileClient = new Mock<IFileClient>();

    addonsInstaller = new Mock<IAddonsInstaller>();

    var addonsContext = new Mock<IAddonsContext>();
    addonsContext.Setup(ctx => ctx.AddonsFileRepo)
      .Returns(addonsFileRepo.Object);
    addonsContext.Setup(ctx => ctx.AddonGraph).Returns(addonGraph.Object);
    addonsContext.Setup(ctx => ctx.AddonsRepo).Returns(addonsRepo.Object);
    addonsContext.Setup(ctx => ctx.AddonsInstaller)
      .Returns(addonsInstaller.Object);

    context.Setup(context => context.Addons).Returns(addonsContext.Object);

    return new AddonsInstallCommand(context.Object);
  }

  [Fact]
  public async Task AddonsInstallCommandSucceedsWhenInstallerHasNoErrors()
  {
    var command = BuildSubject(
      out var addonsInstaller, out var console, out var log
    );

    addonsInstaller
      .Setup(
        i => i.Install(
          It.IsAny<string>(),
          It.IsAny<int?>(),
          It.IsAny<Action<IReportableEvent>>(),
          It.IsAny<Action<Addon, DownloadProgress>>(),
          It.IsAny<Action<Addon, double>>(),
          It.IsAny<System.Threading.CancellationToken>(),
          It.IsAny<string?>()
        )
      )
      .Returns(Task.FromResult(AddonsInstaller.Result.Succeeded));

    await command.ExecuteAsync(console);

    addonsInstaller.VerifyAll();

    log.ToString().ShouldBe(
      """
      [style fg="green"]✅ Addons installed successfully.
      [/style]
      """,
      StringCompareShould.IgnoreLineEndings
    );
  }

  [Fact]
  public async Task AddonsInstallCommandFailsWhenInstallerHasErrors()
  {
    var command = BuildSubject(
      out var addonsInstaller, out var console, out var log
    );

    var e = new TestException("An error occurred.");

    addonsInstaller
      .Setup(
        i => i.Install(
          It.IsAny<string>(),
          It.IsAny<int?>(),
          It.IsAny<Action<IReportableEvent>>(),
          It.IsAny<Action<Addon, DownloadProgress>>(),
          It.IsAny<Action<Addon, double>>(),
          It.IsAny<System.Threading.CancellationToken>(),
          It.IsAny<string?>()
        )
      )
      .Throws(e);

    var ex = await Should.ThrowAsync<CommandException>(
      async () => await command.ExecuteAsync(console)
    );

    ex.InnerException.ShouldBeSameAs(e);

    addonsInstaller.VerifyAll();

    log.ToString().ShouldBe(
      """
      [style fg="red"]An unknown error was encountered while attempting to install addons.

      An error occurred.

      [/style]
      """,
      StringCompareShould.IgnoreLineEndings
    );
  }


  [Fact]
  public void FinishThrowsExceptionOnCannotBeResolved()
  {
    var log = new Mock<ILog>();

    Should.Throw<CommandException>(
      () => AddonsInstallCommand.Finish(
        AddonsInstaller.Result.CannotBeResolved, log.Object
      )
    );

    log.VerifyAll();
  }

  [Fact]
  public void FinishThrowsExceptionOnNotAttempted()
  {
    var log = new Mock<ILog>();

    Should.Throw<CommandException>(
      () => AddonsInstallCommand.Finish(
        AddonsInstaller.Result.NotAttempted, log.Object
      )
    );

    log.VerifyAll();
  }

  [Fact]
  public void FinishOutputsSuccessOnSucceeded()
  {
    var log = new Mock<ILog>();

    AddonsInstallCommand.Finish(
      AddonsInstaller.Result.Succeeded, log.Object
    );

    log.Verify(log => log.Success(It.IsAny<string>()));

    log.VerifyAll();
  }

  [Fact]
  public void FinishOutputsSuccessOnNothingToInstall()
  {
    var log = new Mock<ILog>();

    AddonsInstallCommand.Finish(
      AddonsInstaller.Result.NothingToInstall, log.Object
    );

    log.Verify(log => log.Success(It.IsAny<string>()));

    log.VerifyAll();
  }
}
