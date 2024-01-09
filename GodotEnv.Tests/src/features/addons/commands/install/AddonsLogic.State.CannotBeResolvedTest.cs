namespace Chickensoft.GodotEnv.Tests;

using System.Linq;
using Chickensoft.GodotEnv.Features.Addons.Commands;
using Shouldly;
using Xunit;

public class CannotBeResolvedTest {
  [Fact]
  public void ReportsOnEnter() {
    var state = new AddonsLogic.State.CannotBeResolved();
    var context = state.CreateFakeContext();

    state.Enter();

    var output =
      context.Outputs.Single().ShouldBeOfType<AddonsLogic.Output.Report>();
  }
}
