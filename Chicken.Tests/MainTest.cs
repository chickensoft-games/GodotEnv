namespace Chickensoft.Chicken.Tests {
  using System.Threading.Tasks;
  using Shouldly;
  using Xunit;

  public class MainTest {
    [Fact]
    public async Task CallsCliFx()
      => await Should.NotThrow<Task<int>>(
        async () => await Chicken.Main(new string[] { "error" })
      );
  }
}
