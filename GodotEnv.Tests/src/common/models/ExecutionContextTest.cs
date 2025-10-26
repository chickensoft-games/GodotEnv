namespace Chickensoft.GodotEnv.Tests;

using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Common.Utilities;
using CliFx.Infrastructure;
using Moq;
using Shouldly;
using Xunit;

public class ExecutionContextTest
{
  private const string VERSION = "1.2.3";
  private const string WORKING_DIR = "/";
  private readonly string[] _cliArgs = ["a", "b"];
  private readonly string[] _commandArgs = ["c", "d"];

  [Fact]
  public void Initializes()
  {
    var config = new Config();
    var systemInfo = new MockSystemInfo(OSType.Linux, CpuArch.X64);
    var addons = new Mock<IAddonsContext>().Object;
    var godot = new Mock<IGodotContext>().Object;

    var executionContext = new ExecutionContext(
      CliArgs: _cliArgs,
      CommandArgs: _commandArgs,
      Version: VERSION,
      WorkingDir: WORKING_DIR,
      SystemInfo: systemInfo,
      Config: config,
      Addons: addons,
      Godot: godot
    );

    executionContext.CliArgs.ShouldBe(_cliArgs);
    executionContext.CommandArgs.ShouldBe(_commandArgs);
    executionContext.Version.ShouldBe(VERSION);
    executionContext.WorkingDir.ShouldBe(WORKING_DIR);
    executionContext.SystemInfo.ShouldBe(systemInfo);
    executionContext.Config.ShouldBe(config);
    executionContext.Addons.ShouldBe(addons);
    executionContext.Godot.ShouldBe(godot);

    executionContext
      .CreateLog(new FakeInMemoryConsole())
      .ShouldBeAssignableTo<ILog>();
  }
}
