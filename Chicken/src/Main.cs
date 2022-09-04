using System.Runtime.CompilerServices;

// IMPORTANT: Allow us to test internal methods in our test project.
[assembly: InternalsVisibleTo("Chickensoft.Chicken.Tests")]
namespace Chickensoft.Chicken {
  using System.Threading.Tasks;
  using CliFx;

  public class Chicken {
    public static Task<int> Main(string[] args)
      => new CliApplicationBuilder()
        .AddCommandsFromThisAssembly()
        .Build().RunAsync(args).AsTask();
  }
}
