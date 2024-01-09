namespace Chickensoft.GodotEnv.Tests;

using System.Linq;
using Chickensoft.GodotEnv.Features.Addons.Commands;
using Shouldly;
using Xunit;

public class InstallationSucceededTest {
  [Fact]
  public void ReportsOnEnter() {
    var state = new AddonsLogic.State.InstallationSucceeded();
    var context = state.CreateFakeContext();

    state.Enter();

    context.Outputs.Single().ShouldBeOfType<AddonsLogic.Output.Report>();
  }
}
