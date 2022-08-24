namespace Chickensoft.Chicken.Tests {
  using CliFx.Infrastructure;
  using Moq;
  using Xunit;

  public class EggCommandTest {
    [Fact]
    public async void DoesNothing() {
      var command = new EggCommand();
      await command.ExecuteAsync(new Mock<IConsole>().Object);
    }
  }
}
