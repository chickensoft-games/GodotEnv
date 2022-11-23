namespace Chickensoft.Chicken;
using System.IO.Abstractions;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

[Command("addons install", Description = "Installs addons.")]
public class AddonsInstallCommand : ICommand {
  public readonly IApp App;
  public readonly IFileSystem Fs;
  public readonly IFileCopier Copier;
  public readonly IAddonRepo AddonRepo;
  public readonly IConfigFileLoader ConfigFileLoader;
  public readonly IDependencyGraph DependencyGraph;

  [CommandOption(
    "max-depth",
    'd',
    Description = "The maximum depth to recurse while installing addons."
  )]
  public int? MaxDepth { get; init; } = null;

  public AddonsInstallCommand() {
    App = new App();
    Fs = new FileSystem();
    Copier = new FileCopier();

    AddonRepo = new AddonRepo(App, Fs);
    ConfigFileLoader = new ConfigFileLoader(App, Fs);
    DependencyGraph = new DependencyGraph();
  }

  public AddonsInstallCommand(
    IApp app,
    IFileSystem fs,
    IFileCopier copier,
    IAddonRepo addonRepo,
    IConfigFileLoader configFileLoader,
    IDependencyGraph dependencyGraph
  ) {
    App = app;
    Fs = fs;
    Copier = copier;
    AddonRepo = addonRepo;
    ConfigFileLoader = configFileLoader;
    DependencyGraph = dependencyGraph;
  }

  public async ValueTask ExecuteAsync(IConsole console) {
    var startDir = App.WorkingDir;
    var output = console.Output;

    var log = App.CreateLog(console);
    var configFile = ConfigFileLoader.Load(startDir);
    var config = configFile.ToConfig(projectPath: startDir);

    var addonManger = App.CreateAddonManager(
      fs: Fs,
      addonRepo: AddonRepo,
      configFileLoader: ConfigFileLoader,
      log: log,
      dependencyGraph: DependencyGraph
    );

    await addonManger.InstallAddons(
      App,
      projectPath: startDir,
      copier: Copier,
      maxDepth: MaxDepth
    );
  }
}
