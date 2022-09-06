namespace Chickensoft.Chicken {
  using System;
  using System.Threading.Tasks;
  using CliFx;
  using CliFx.Attributes;
  using CliFx.Infrastructure;

  [Command("addon install", Description = "Installs addons.")]
  public class InstallCommand : ICommand {
    private readonly IApp _app;

    public InstallCommand() => _app = new App();

    public InstallCommand(IApp app) => _app = app;

    public async ValueTask ExecuteAsync(IConsole console) {
      var startDir = Environment.CurrentDirectory;
      var output = console.Output;

      var addonRepo = _app.CreateAddonRepo();
      var configFileRepo = _app.CreateConfigFileRepo();
      var reporter = _app.CreateReporter(console);
      var configFile = configFileRepo.LoadOrCreateConfigFile(startDir);
      var config = configFile.ToConfig(projectPath: startDir);
      var dependencyGraph = new DependencyGraph();

      var addonManger = _app.CreateAddonManager(
        addonRepo: addonRepo,
        configFileRepo: configFileRepo,
        reporter: reporter,
        dependencyGraph: dependencyGraph
      );

      await addonManger.InstallAddons(projectPath: startDir);
    }
  }
}
