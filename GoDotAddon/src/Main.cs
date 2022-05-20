namespace Chickensoft.GoDotAddon {
  using System;
  using System.Threading.Tasks;
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

    private IApp _app { get; init; } = Info.App;

    public InstallCommand() { }
    public InstallCommand(IApp app) => _app = app;

    public async ValueTask ExecuteAsync(IConsole console) {
      var addonRepo = new AddonRepo(app: Info.App);
      var configFileRepo = new ConfigFileRepo(app: Info.App);
      var reporter = new Reporter(console.Output);
      var dependencyGraph = new DependencyGraph();

      var addonManger = new AddonManager(
        addonRepo: addonRepo,
        configFileRepo: configFileRepo,
        reporter: reporter,
        dependencyGraph: dependencyGraph
      );

      await addonManger.InstallAddons(
        projectPath: Environment.CurrentDirectory
      );
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
