namespace Chickensoft.GodotEnv.Tests;

using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Common.Utilities;
using Chickensoft.GodotEnv.Features.Addons.Commands;
using CliFx.Infrastructure;
using Moq;
using Shouldly;
using Xunit;

public class AddonsCommandTest {
  [Fact]
  public async Task Executes() {
    var context = new Mock<IExecutionContext>();
    var console = new FakeInMemoryConsole();
    // Use real log to test colors in output
    var log = new Log(console) { TestEnvironment = true };

    context.Setup(context => context.CreateLog(console)).Returns(log);

    var addonCommand = new AddonsCommand(context.Object);
    await addonCommand.ExecuteAsync(console);

    log.ToString().ShouldBe(
      """

      [style fg="yellow"]Please use a subcommand to manage addons.

      [/style][style]To see a list of available subcommands:

      [/style][style fg="green"]    godotenv addons --help


      [/style]
      """, StringCompareShould.IgnoreLineEndings);
  }
}
