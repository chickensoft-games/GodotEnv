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
