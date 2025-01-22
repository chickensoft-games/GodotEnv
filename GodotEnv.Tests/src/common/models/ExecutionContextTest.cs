namespace Chickensoft.GodotEnv.Tests;

using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Common.Utilities;
using CliFx.Infrastructure;
using global::GodotEnv.Common.Utilities;
using Moq;
using Shouldly;
using Xunit;

public class ExecutionContextTest {
  private const string VERSION = "1.2.3";
  private const string WORKING_DIR = "/";
  private readonly string[] _cliArgs = ["a", "b"];
  private readonly string[] _commandArgs = ["c", "d"];

  [Fact]
  public void Initializes() {
    var config = new ConfigFile();
    var addons = new Mock<IAddonsContext>().Object;
    var godot = new Mock<IGodotContext>().Object;

    var executionContext = new ExecutionContext(
      CliArgs: _cliArgs,
      CommandArgs: _commandArgs,
      Version: VERSION,
      WorkingDir: WORKING_DIR,
      Config: config,
      Addons: addons,
      Godot: godot
    );

    executionContext.CliArgs.ShouldBe(_cliArgs);
    executionContext.CommandArgs.ShouldBe(_commandArgs);
    executionContext.Version.ShouldBe(VERSION);
    executionContext.WorkingDir.ShouldBe(WORKING_DIR);
    executionContext.Config.ShouldBe(config);
    executionContext.Addons.ShouldBe(addons);
    executionContext.Godot.ShouldBe(godot);

    executionContext
      .CreateLog(new MockSystemInfo(OSType.Linux, CPUArch.X64), new FakeInMemoryConsole())
      .ShouldBeAssignableTo<ILog>();
  }
}
