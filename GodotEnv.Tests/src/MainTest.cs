namespace Chickensoft.GodotEnv.Tests;

using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Models;
using Moq;
using Shouldly;
using Xunit;

public class MainTest {
  [Fact]
  public async Task CallsCliFx()
    => await Should.NotThrowAsync(
      async () => await GodotEnv.Main(new string[] { "not-a-command" })
    );

  [Fact]
  public void CreateExecutionContextParsesArgs() {
    var config = new ConfigFile();
    var args = new string[] { "a", "--", "b" };
    var workingDir = "/";
    var addonsContext = new Mock<IAddonsContext>();
    var godotContext = new Mock<IGodotContext>();
    var context = GodotEnv.CreateExecutionContext(
      args, config, workingDir, addonsContext.Object,
      godotContext.Object
    );
    context.CliArgs.ShouldBe(new string[] { "a" });
    context.CommandArgs.ShouldBe(new string[] { "b" });
    context.Config.ShouldBe(config);
    context.WorkingDir.ShouldBe(workingDir);
    context.Addons.ShouldBe(addonsContext.Object);
    context.Godot.ShouldBe(godotContext.Object);
  }
}

public class GodotEnvActivatorTest {
  private class ITestCommand(IExecutionContext executionContext) :
    ICliCommand {
    public IExecutionContext ExecutionContext { get; } = executionContext;
  }

  [Fact]
  public void Initializes() {
    var executionContext = new Mock<IExecutionContext>();
    var activator = new GodotEnvActivator(executionContext.Object);
  }

  [Fact]
  public void CreatesObjectInstance() {
    var executionContext = new Mock<IExecutionContext>();
    var activator = new GodotEnvActivator(executionContext.Object);

    var command = (ICliCommand)activator.CreateInstance(typeof(ITestCommand));
    command.ShouldBeOfType<ITestCommand>();
    command.ExecutionContext.ShouldBe(executionContext.Object);
  }
}
