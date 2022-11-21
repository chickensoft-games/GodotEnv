namespace Chickensoft.Chicken.Tests;

using System;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

public class MainTest {
  [Fact]
  public async Task CallsCliFx()
    => await Should.NotThrowAsync(
      async () => await Chicken.Main(new string[] { "error" })
    );

  [Fact]
  public async Task CallsCliFxWithCommandArgs() {
    var args = new string[] { "a", "--", "b" };
    App.CommandArgs = Array.Empty<string>();
    await Should.NotThrowAsync(async () => await Chicken.Main(args));
    App.CommandArgs.ShouldBe(new string[] { "b" });
    App.CommandArgs = Array.Empty<string>();
  }
}
