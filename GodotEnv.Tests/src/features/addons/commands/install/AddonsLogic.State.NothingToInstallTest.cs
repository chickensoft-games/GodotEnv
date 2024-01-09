namespace Chickensoft.GodotEnv.Tests;

using System.Linq;
using Chickensoft.GodotEnv.Features.Addons.Commands;
using Shouldly;
using Xunit;

public class NothingToInstallTest {
  [Fact]
  public void ReportsOnEnter() {
    var state = new AddonsLogic.State.NothingToInstall();
    var context = state.CreateFakeContext();

    state.Enter();

    context.Outputs.Single().ShouldBeOfType<AddonsLogic.Output.Report>();
  }
}
