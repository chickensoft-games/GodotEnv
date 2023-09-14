namespace Chickensoft.GodotEnv.Features.Godot.Commands;

using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Features.Godot.Models;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using CliWrap;

[Command(
  "godot uninstall",
  Description = "Uninstalls the specified version of Godot."
)]
public class GodotUninstallCommand : ICommand, ICliCommand {
  public IExecutionContext ExecutionContext { get; set; }

  [CommandParameter(
  0,
  Name = "Version",
  Validators = new System.Type[] { typeof(GodotVersionValidator) },
  Description = "Godot version to install: e.g., 4.1.0-rc.2, 4.2.0, etc." +
    " No leading 'v'. Should match a version of GodotSharp: " +
    "https://www.nuget.org/packages/GodotSharp/"
)]
  public string RawVersion { get; set; } = default!;

  [CommandOption(
    "no-dotnet", 'n',
    Description =
      "Specify the version of Godot that does not support C#/.NET."
  )]
  public bool NoDotnet { get; set; }

  public GodotUninstallCommand(IExecutionContext context) {
    ExecutionContext = context;
  }

  public async ValueTask ExecuteAsync(IConsole console) {
    var log = ExecutionContext.CreateLog(console);
    var godotRepo = ExecutionContext.Godot.GodotRepo;

    var version = SemanticVersion.Parse(RawVersion);
    var isDotnetVersion = !NoDotnet;

    log.Print("");
    if (await godotRepo.Uninstall(version, isDotnetVersion, log)) {
      log.Success($"Godot {version.VersionString} uninstalled.");
    }
    else {
      log.Err($"Godot {version.VersionString} not found.");
    }
    log.Print("");
  }
}
