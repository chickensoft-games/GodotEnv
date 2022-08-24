// Figured out how to test CliFx console from:
// https://github.com/Tyrrrz/CliFx/blob/master/CliFx.Tests/ConsoleSpecs.cs#L192

namespace Chickensoft.Chicken.Tests {
  using System.IO;
  using CliFx.Infrastructure;
  using Moq;
  using Shouldly;
  using Xunit;


  public class ReporterTest {
    [Fact]
    public void DependencyEventOutputs() {
      var console = new Mock<IConsole>();
      var stream = new MemoryStream();
      var output = new ConsoleWriter(
        console: new FakeInMemoryConsole(),
        stream: stream
      );

      var e = new Mock<IReportableDependencyEvent>();
      e.Setup(e => e.ToString()).Returns("DependencyEvent");
      var reporter = new Reporter(output);
      reporter.DependencyEvent(e.Object);

      output.Flush();

      var contents = output.Encoding.GetString(stream.ToArray());
      contents.ShouldBe("DependencyEvent");
    }
  }
}
