namespace Chickensoft.GodotEnv.Tests;

using Chickensoft.GodotEnv.Features.Addons.Commands;
using Chickensoft.GodotEnv.Features.Addons.Domain;
using Moq;
using Shouldly;
using Xunit;

public partial class AddonsLogicTest {
  [Fact]
  public void Initializes() {
    var addonsFileRepo = new Mock<IAddonsFileRepository>();
    var addonsRepo = new Mock<IAddonsRepository>();
    var addonGraph = new Mock<IAddonGraph>();
    var logic = new AddonsLogic(
      addonsFileRepo.Object, addonsRepo.Object, addonGraph.Object
    );

    logic.Value.ShouldBeOfType<AddonsLogic.State.Unresolved>();
  }
}
