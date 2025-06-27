namespace Chickensoft.GodotEnv.Tests;

using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Models;
using global::GodotEnv.Common.Utilities;
using Moq;
using Shouldly;
using Xunit;

public class MainTest {
  [Fact]
  public async Task CallsCliFx()
    => await Should.NotThrowAsync(
      async () => await GodotEnv.Main(["not-a-command"])
    );

  [Fact]
  public void CreateExecutionContextParsesArgs() {
    var config = new Config();
    var args = new string[] { "a", "--", "b" };
    var workingDir = "/";
    var systemInfo = new Mock<ISystemInfo>();
    var addonsContext = new Mock<IAddonsContext>();
    var godotContext = new Mock<IGodotContext>();
    var context = GodotEnv.CreateExecutionContext(
      args, config, workingDir, systemInfo.Object, addonsContext.Object,
      godotContext.Object
    );
    context.CliArgs.ShouldBe(["a"]);
    context.CommandArgs.ShouldBe(["b"]);
    context.Config.ShouldBe(config);
    context.WorkingDir.ShouldBe(workingDir);
    context.Addons.ShouldBe(addonsContext.Object);
    context.Godot.ShouldBe(godotContext.Object);
  }
}

public class GodotEnvActivatorTest {
  private sealed class ITestCommand(IExecutionContext executionContext) :
    ICliCommand {
    public IExecutionContext ExecutionContext { get; } = executionContext;
  }

  [Fact]
  public void Initializes() {
    var executionContext = new Mock<IExecutionContext>();
    var activator = new GodotEnvActivator(
      executionContext.Object, OSType.Windows
    );
    activator.ShouldBeOfType<GodotEnvActivator>();
  }

  [Fact]
  public void CreatesObjectInstance() {
    var executionContext = new Mock<IExecutionContext>();
    var activator = new GodotEnvActivator(
      executionContext.Object, OSType.MacOS
    );

    var command = (ICliCommand)activator.CreateInstance(typeof(ITestCommand));
    command.ShouldBeOfType<ITestCommand>();
    command.ExecutionContext.ShouldBe(executionContext.Object);
  }
}
