namespace Chickensoft.Chicken.Tests {
  using System.IO;
  using CliFx.Infrastructure;
  using Moq;
  using Xunit;

  public class AddonCommandTest {
    [Fact]
    public async void DoesNothing() {
      var command = new AddonCommand();
      var console = new Mock<IConsole>();
      var consoleWriter = new ConsoleWriter(
        console: new FakeInMemoryConsole(),
        stream: new MemoryStream()
      );
      console.SetupGet(c => c.Output).Returns(consoleWriter);
      await command.ExecuteAsync(console.Object);
    }
  }
}
