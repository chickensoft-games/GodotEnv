namespace Chickensoft.GoDotAddon.Tests {
  using System.Threading.Tasks;
  using Chickensoft.GoDotAddon;
  using Shouldly;
  using Xunit;

  public class MainTest {
    [Fact]
    public async Task CallsCliFx()
      => await Should.NotThrow<Task<int>>(
        async () => await GoDotAddon.Main(new string[] { "error" })
      );
  }
}
