// Figured out how to test CliFx console from:
// https://github.com/Tyrrrz/CliFx/blob/master/CliFx.Tests/ConsoleSpecs.cs#L192

namespace Chickensoft.Chicken.Tests {
  using CliFx.Infrastructure;
  using Moq;
  using Shouldly;
  using Xunit;


  public class ReporterTest {
    [Fact]
    public void DependencyEventOutputs() {
      var console = new FakeInMemoryConsole();

      var e = new Mock<IReportableDependencyEvent>();
      e.Setup(e => e.ToString()).Returns("DependencyEvent");
      var reporter = new Reporter(console.Output);
      reporter.DependencyEvent(e.Object);

      var contents = console.ReadOutputString();
      contents.ShouldBe("DependencyEvent\n\n");
    }
  }
}
