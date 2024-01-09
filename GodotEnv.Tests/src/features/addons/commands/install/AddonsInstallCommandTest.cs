namespace Chickensoft.GodotEnv.Tests;

using System;
using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Common.Utilities;
using Chickensoft.GodotEnv.Features.Addons.Commands;
using Chickensoft.GodotEnv.Features.Addons.Domain;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using Moq;
using Shouldly;
using Xunit;

public class AddonsInstallCommandTest {
  [Fact]
  public void Initializes() {
    var context = new Mock<IExecutionContext>();
    var command = new AddonsInstallCommand(context.Object);

    Assert.Equal(context.Object, command.ExecutionContext);
  }

  [Fact]
  public async Task Executes() {
    var context = new Mock<IExecutionContext>();
    var console = new FakeInMemoryConsole();
    var log = new Log(console); // Use real log to test colors in output

    var workingDir = "/";

    context.Setup(context => context.CreateLog(console)).Returns(log);
    context.Setup(context => context.WorkingDir).Returns(workingDir);

    var addonsFileRepo = new Mock<IAddonsFileRepository>();
    var addonGraph = new Mock<IAddonGraph>();
    var addonsRepo = new Mock<IAddonsRepository>();
    var fileClient = new Mock<IFileClient>();
    var logic = new Mock<AddonsLogic>(
      addonsFileRepo.Object, addonsRepo.Object, addonGraph.Object
    );
    addonsFileRepo.Setup(ctx => ctx.FileClient).Returns(fileClient.Object);
    fileClient.Setup(ctx => ctx.OS).Returns(OSType.Linux);

    var addonsContext = new Mock<IAddonsContext>();
    addonsContext.Setup(ctx => ctx.AddonsFileRepo)
      .Returns(addonsFileRepo.Object);
    addonsContext.Setup(ctx => ctx.AddonGraph).Returns(addonGraph.Object);
    addonsContext.Setup(ctx => ctx.AddonsRepo).Returns(addonsRepo.Object);
    addonsContext.Setup(ctx => ctx.AddonsLogic).Returns(logic.Object);

    context.Setup(context => context.Addons).Returns(addonsContext.Object);

    var command = new AddonsInstallCommand(context.Object);

    var reportOutput = new AddonsLogic.Output.Report(
      new ReportableEvent(log => log.Print("Reportable event"))
    );

    // Mock the binding's report output handler and invoke it to make sure it
    // logs the report output.
    var binding = new Mock<AddonsLogic.IBinding>();
    binding
      .Setup(binding => binding.Handle(
        It.IsAny<Action<AddonsLogic.Output.Report>>()
      ))
      .Callback<Action<AddonsLogic.Output.Report>>(
        action => action(reportOutput)
      )
      .Returns(binding.Object);

    // Mock the binding's exception handler and invoke it to make sure it logs
    // errors as expected.
    var e = new InvalidOperationException("Test exception");
    binding
      .Setup(binding => binding.Catch(It.IsAny<Action<Exception>>()))
      .Callback<Action<Exception>>(action => action(e))
      .Returns(binding.Object);

    binding
      .Setup(binding => binding.Catch(It.IsAny<Action<Exception>>()))
      .Callback((Action<Exception> action) => action(e))
      .Returns(binding.Object);

    logic.Setup(logic => logic.Bind()).Returns(binding.Object);

    logic
      .Setup(logic => logic.Input(It.IsAny<AddonsLogic.Input.Install>()))
      .Returns(
        Task.FromResult<AddonsLogic.State>(
          new AddonsLogic.State.InstallationSucceeded()
        )
      );

    await command.ExecuteAsync(console);

    logic.VerifyAll();

    log.ToString().ShouldBe("""
    Reportable event

    [style fg="red"]An error was encountered while attempting to install addons.

    System.InvalidOperationException: Test exception

    [/style]
    """, StringCompareShould.IgnoreLineEndings);
  }

  [Fact]
  public void CheckSuccessThrowsOnStateCannotBeResolved() {
    var executionContext = new Mock<IExecutionContext>();
    var fileClient = new Mock<IFileClient>();

    var command = new AddonsInstallCommand(executionContext.Object);

    var state = new AddonsLogic.State.CannotBeResolved();

    Assert.Throws<CommandException>(
      () => command.CheckSuccess(state)
    );
  }

  [Fact]
  public void CheckSuccessThrowsOnStateUnresolved() {
    var executionContext = new Mock<IExecutionContext>();
    var fileClient = new Mock<IFileClient>();

    var command = new AddonsInstallCommand(executionContext.Object);

    var state = new AddonsLogic.State.Unresolved();

    Assert.Throws<CommandException>(
      () => command.CheckSuccess(state)
    );
  }
}
