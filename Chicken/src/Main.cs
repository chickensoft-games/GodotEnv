namespace Chickensoft.GoDotAddon {
  using System.Threading.Tasks;
  using CliFx;

  public class GoDotAddon {
    public static Task<int> Main(string[] args)
      => new CliApplicationBuilder()
        .AddCommandsFromThisAssembly()
        .Build().RunAsync(args).AsTask();
  }
}
