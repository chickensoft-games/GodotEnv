using System.Threading.Tasks;
using CliFx.Infrastructure;
using GoDotAddon;
using Xunit;

namespace Chickensoft.GoDotAddon.Tests
{
  public class UnitTest1
  {
    [Fact]
    public async Task InstallDoesSomething()
    {
      using FakeInMemoryConsole? console = new();
      InstallCommand? command = new();

      await command.ExecuteAsync(console);
      string? stdOut = console.ReadOutputString();
    }
  }
}
