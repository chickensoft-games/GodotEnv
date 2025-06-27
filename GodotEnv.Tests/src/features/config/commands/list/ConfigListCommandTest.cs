namespace Chickensoft.GodotEnv.Tests.Features.Config.Commands.List;

using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Common.Utilities;
using Chickensoft.GodotEnv.Features.Config.Commands.List;
using CliFx.Infrastructure;
using Moq;
using Shouldly;
using Xunit;

public class ConfigListCommandTest {
  [Fact]
  public async Task Executes() {
    var systemInfo = new MockSystemInfo(OSType.Linux, CPUArch.X64);
    var config = new Config();
    var context = new Mock<IExecutionContext>();
    var console = new FakeInMemoryConsole();
    var log = new Log(systemInfo, config, console); // Use real log to test colors in output

    context.Setup(ctx => ctx.SystemInfo).Returns(systemInfo);
    context.Setup(ctx => ctx.CreateLog(console)).Returns(log);
    context.Setup(ctx => ctx.Config).Returns(config);

    var addonCommand = new ConfigListCommand(context.Object);
    await addonCommand.ExecuteAsync(console);

    log.ToString().ShouldBe(
      $"""

      Godot:InstallationsPath = {Defaults.CONFIG_GODOT_INSTALLATIONS_PATH}
      Terminal:DisplayEmoji = {Defaults.CONFIG_TERMINAL_DISPLAY_EMOJI}

      """, StringCompareShould.IgnoreLineEndings);
  }

  [Fact]
  public async Task DisplaysOnlyRequestedValue() {
    var systemInfo = new MockSystemInfo(OSType.Linux, CPUArch.X64);
    var config = new Config();
    var context = new Mock<IExecutionContext>();
    var console = new FakeInMemoryConsole();
    var log = new Log(systemInfo, config, console); // Use real log to test colors in output

    context.Setup(ctx => ctx.SystemInfo).Returns(systemInfo);
    context.Setup(ctx => ctx.CreateLog(console)).Returns(log);
    context.Setup(ctx => ctx.Config).Returns(config);

    var addonCommand = new ConfigListCommand(context.Object) {
      ConfigKey = "Terminal:DisplayEmoji"
    };
    await addonCommand.ExecuteAsync(console);

    log.ToString().ShouldBe(
      $"""

      Terminal:DisplayEmoji = {Defaults.CONFIG_TERMINAL_DISPLAY_EMOJI}

      """, StringCompareShould.IgnoreLineEndings);
  }

  [Fact]
  public async Task FailsGracefullyWhenRequestedKeyDoesNotExist() {
    var systemInfo = new MockSystemInfo(OSType.Linux, CPUArch.X64);
    var config = new Config();
    var context = new Mock<IExecutionContext>();
    var console = new FakeInMemoryConsole();
    var log = new Log(systemInfo, config, console); // Use real log to test colors in output

    context.Setup(ctx => ctx.SystemInfo).Returns(systemInfo);
    context.Setup(ctx => ctx.CreateLog(console)).Returns(log);
    context.Setup(ctx => ctx.Config).Returns(config);

    var addonCommand = new ConfigListCommand(context.Object) {
      ConfigKey = "FakeSection:FakeKey"
    };
    await addonCommand.ExecuteAsync(console);

    log.ToString().ShouldBe(
      $"""

      "{addonCommand.ConfigKey}" is not a valid configuration key. Try
      "godotenv config list" for a complete list of all entries.

      """, StringCompareShould.IgnoreLineEndings);
  }
}
