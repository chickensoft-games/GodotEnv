namespace Chickensoft.GodotEnv.Tests.Features.Config.Commands.Set;

using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Common.Utilities;
using Chickensoft.GodotEnv.Features.Config.Commands.List;
using CliFx.Infrastructure;
using Moq;
using Shouldly;
using Xunit;

public class ConfigSetCommandTest
{
  [Fact]
  public async Task SetsIndicatedValue()
  {
    var systemInfo = new MockSystemInfo(OSType.Linux, CpuArch.X64);
    var config = new Config();
    var context = new Mock<IExecutionContext>();
    var console = new FakeInMemoryConsole();
    var log = new Log(systemInfo, config, console); // Use real log to test colors in output

    var testEmojiValue = !!config.ConfigValues.Terminal.DisplayEmoji;

    context.Setup(ctx => ctx.SystemInfo).Returns(systemInfo);
    context.Setup(ctx => ctx.CreateLog(console)).Returns(log);
    context.Setup(ctx => ctx.Config).Returns(config);

    var addonCommand = new ConfigSetCommand(context.Object)
    {
      ConfigKey = "Terminal.DisplayEmoji",
      ConfigValue = testEmojiValue.ToString(),
    };
    await addonCommand.ExecuteAsync(console);
    config.ConfigValues.Terminal.DisplayEmoji.ShouldBe(testEmojiValue);
  }

  [Fact]
  public async Task FailsGracefullyWhenIndicatedKeyDoesNotExist()
  {
    var systemInfo = new MockSystemInfo(OSType.Linux, CpuArch.X64);
    var config = new Config();
    var context = new Mock<IExecutionContext>();
    var console = new FakeInMemoryConsole();
    var log = new Log(systemInfo, config, console); // Use real log to test colors in output

    context.Setup(ctx => ctx.SystemInfo).Returns(systemInfo);
    context.Setup(ctx => ctx.CreateLog(console)).Returns(log);
    context.Setup(ctx => ctx.Config).Returns(config);

    var addonCommand = new ConfigSetCommand(context.Object)
    {
      ConfigKey = "FakeSection.FakeKey",
      ConfigValue = "FakeValue"
    };
    await addonCommand.ExecuteAsync(console);

    log.ToString().ShouldBe(
      $"""

      "{addonCommand.ConfigKey}" is not a valid configuration key, or "{addonCommand.ConfigValue}" is
      not a valid value for "{addonCommand.ConfigKey}". Try "godotenv config list" for a
      complete list of all entries.

      """, StringCompareShould.IgnoreLineEndings);
  }

  [Fact]
  public async Task FailsGracefullyWhenIndicatedValueIsInvalid()
  {
    var systemInfo = new MockSystemInfo(OSType.Linux, CpuArch.X64);
    var config = new Config();
    var context = new Mock<IExecutionContext>();
    var console = new FakeInMemoryConsole();
    var log = new Log(systemInfo, config, console); // Use real log to test colors in output

    context.Setup(ctx => ctx.SystemInfo).Returns(systemInfo);
    context.Setup(ctx => ctx.CreateLog(console)).Returns(log);
    context.Setup(ctx => ctx.Config).Returns(config);

    var addonCommand = new ConfigSetCommand(context.Object)
    {
      ConfigKey = "Terminal.DisplayEmoji",
      ConfigValue = "FakeValue"
    };
    await addonCommand.ExecuteAsync(console);

    log.ToString().ShouldBe(
      $"""

      "{addonCommand.ConfigKey}" is not a valid configuration key, or "{addonCommand.ConfigValue}" is
      not a valid value for "{addonCommand.ConfigKey}". Try "godotenv config list" for a
      complete list of all entries.

      """, StringCompareShould.IgnoreLineEndings);
  }
}
