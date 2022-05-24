namespace Chickensoft.GoDotAddon {
  using System;
  using System.Threading.Tasks;
  using CliFx;
  using CliFx.Attributes;
  using CliFx.Infrastructure;

  [Command("install")]
  public class InstallCommand : ICommand {
    [CommandOption("dry-run", 'd',
      Description = "Doesn't actually install anything, just outputs what it" +
      "would do.")]
    public bool DryRun { get; init; } = false;

    public async ValueTask ExecuteAsync(IConsole console) {
      var startDir = Environment.CurrentDirectory;
      var output = console.Output;
      var app = DryRun
        ? new DryRunApp(startDir, Info.App.FS, output)
        : Info.App;

      var addonRepo = new AddonRepo(app: app);
      var configFileRepo = new ConfigFileRepo(app: app);
      var reporter = new Reporter(output);
      var dependencyGraph = new DependencyGraph();

      var addonManger = new AddonManager(
        addonRepo: addonRepo,
        configFileRepo: configFileRepo,
        reporter: reporter,
        dependencyGraph: dependencyGraph
      );

      await addonManger.InstallAddons(projectPath: startDir);
    }
  }
}
