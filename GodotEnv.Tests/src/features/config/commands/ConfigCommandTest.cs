namespace Chickensoft.GodotEnv.Tests.Features.Config.Commands;

using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Common.Utilities;
using Chickensoft.GodotEnv.Features.Config.Commands;
using CliFx.Infrastructure;
using Moq;
using Shouldly;
using Xunit;

public class ConfigCommandTest
{

  [Fact]
  public async Task Executes()
  {
    var systemInfo = new MockSystemInfo(OSType.Linux, CpuArch.X64);
    var config = MockConfig.Get();
    var context = new Mock<IExecutionContext>();
    var console = new FakeInMemoryConsole();
    var log = new Log(systemInfo, config.Object, console); // Use real log to test colors in output

    context.Setup(ctx => ctx.SystemInfo).Returns(systemInfo);
    context.Setup(ctx => ctx.CreateLog(console)).Returns(log);

    var addonCommand = new ConfigCommand(context.Object);
    await addonCommand.ExecuteAsync(console);

    log.ToString().ShouldBe(
      """

      [style fg="yellow"]Please use a subcommand to manage configuration.

      [/style][style]To see a list of available subcommands:

      [/style][style fg="green"]    godotenv config --help

      [/style]
      """, StringCompareShould.IgnoreLineEndings);
  }
}
