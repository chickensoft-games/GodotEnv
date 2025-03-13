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
public class GodotUninstallCommand :
  ICommand, ICliCommand, IWindowsElevationEnabled {
  public IExecutionContext ExecutionContext { get; set; }

  [CommandParameter(
    0,
    Name = "Version",
    Validators = [typeof(GodotVersionValidator)],
    Description = "Godot version to install: e.g., 4.1.0-rc.2, 4.2.0, etc." +
      " Should match a version of Godot " +
      "(https://github.com/godotengine/godot-builds/tags) or GodotSharp " +
      "(https://www.nuget.org/packages/GodotSharp/)"
)]
  public string RawVersion { get; set; } = default!;

  [CommandOption(
    "no-dotnet", 'n',
    Description =
      "Specify the version of Godot that does not support C#/.NET."
  )]
  public bool NoDotnet { get; set; }

  public bool IsWindowsElevationRequired => true;

  public GodotUninstallCommand(IExecutionContext context) {
    ExecutionContext = context;
  }

  public async ValueTask ExecuteAsync(IConsole console) {
    var godotRepo = ExecutionContext.Godot.GodotRepo;
    var platform = ExecutionContext.Godot.Platform;
    var versionConverter = godotRepo.VersionStringConverter;

    var log = ExecutionContext.CreateLog(console);

    var isDotnetVersion = !NoDotnet;
    // We know this won't throw because the validator okayed it
    var version = versionConverter.ParseVersion(RawVersion, isDotnetVersion);

    log.Print("");
    if (await godotRepo.Uninstall(version, log)) {
      log.Success($"Godot {versionConverter.VersionString(version)} uninstalled.");
    }
    else {
      log.Err($"Godot {versionConverter.VersionString(version)} not found.");
    }
    log.Print("");
  }
}
