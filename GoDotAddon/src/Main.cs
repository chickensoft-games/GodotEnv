namespace Chickensoft.GoDotAddon {
  using System.Threading.Tasks;
  using CliFx;
  using CliFx.Attributes;
  using CliFx.Infrastructure;

  [Command]
  public class GoDotAddonApp : ICommand {
    public ValueTask ExecuteAsync(IConsole console) {
      console.Output.WriteLine("Thanks for choosing GoDotAddon!");
      return default;
    }
  }

  internal class GoDotAddon {
    private static async Task<int> Main(string[] args) {
      var app = new CliApplicationBuilder()
          .AddCommandsFromThisAssembly()
          .Build();

      // Console.WriteLine($"Launched from {Environment.CurrentDirectory}");
      // Console.WriteLine($"Physical location {AppDomain.CurrentDomain.BaseDirectory}");
      // Console.WriteLine($"AppContext.BaseDir {AppContext.BaseDirectory}");
      // Console.WriteLine($"Runtime Call {Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)}")

      return await app.RunAsync(args);
    }
  }
}
