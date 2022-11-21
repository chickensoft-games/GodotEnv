namespace Chickensoft.Chicken.Tests;
using System.Threading.Tasks;
using CliFx.Infrastructure;
using Shouldly;
using Xunit;

public class AddonCommandTest {
  [Fact]
  public async Task OutputsInfoMessage() {
    var command = new AddonCommand();

    var console = new FakeInMemoryConsole();

    await command.ExecuteAsync(console);

    console.ReadOutputString().ShouldContain(
      "Please use a subcommand to manage addons."
    );
  }
}
