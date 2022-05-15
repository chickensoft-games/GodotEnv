namespace GoDotAddon {
  using CliFx;
  using CliFx.Attributes;
  using CliFx.Infrastructure;

  [Command]
  public class GoDotAddonApp : ICommand {
    public ValueTask ExecuteAsync(IConsole console) {
      console.Output.WriteLine("Hello, world!");
      return default;
    }
  }

  [Command("install")]
  public class InstallCommand : ICommand {
    [CommandOption("clear-cache", 'c',
      Description = "Clears the cache and downloads packages again.")]
    public bool ClearCache { get; init; } = false;

    public async ValueTask ExecuteAsync(IConsole console) {
      await Task.CompletedTask;
      console.Output.WriteLine("Hello, installation.");
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
