namespace Chickensoft.Chicken.Tests;
using System.Threading.Tasks;
using CliFx.Infrastructure;
using Shouldly;
using Xunit;

public class EggCommandTest {
  [Fact]
  public async Task OutputsInfoMessage() {
    var command = new EggCommand();

    var console = new FakeInMemoryConsole();

    await command.ExecuteAsync(console);

    console.ReadOutputString().ShouldContain(
      "Please use a subcommand to generate a project or " +
      "feature from a template."
    );
  }
}
