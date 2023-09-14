namespace Chickensoft.GodotEnv.Tests;

using Chickensoft.GodotEnv.Common.Utilities;
using Chickensoft.GodotEnv.Features.Addons.Commands;
using Moq;
using Xunit;

public class CannotBeResolvedTest {
  [Fact]
  public void ReportsOnEnter() {
    var context = new Mock<AddonsLogic.IContext>();
    var log = new Mock<ILog>();

    log.Setup(log => log.Err(It.IsAny<string>()));
    context
      .Setup(ctx => ctx.Output(It.IsAny<AddonsLogic.Output.Report>()))
      .Callback<AddonsLogic.Output>(
        output => ((AddonsLogic.Output.Report)output).Event.Report(log.Object)
      );

    var state = new AddonsLogic.State.CannotBeResolved(context.Object);
    var stateTester = AddonsLogic.Test(state);

    stateTester.Enter();

    context.VerifyAll();
  }
}
