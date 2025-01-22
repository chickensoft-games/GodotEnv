namespace Chickensoft.GodotEnv.Features.Addons.Commands;

using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Features.Godot.Commands;
using Chickensoft.GodotEnv.Features.Godot.Models;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

[Command(
  "godot use",
  Description = "Changes the active version of Godot by updating the symlink " +
    "to point to the specified version."
)]
public class GodotUseCommand : ICommand, ICliCommand, IWindowsElevationEnabled {
  public IExecutionContext ExecutionContext { get; set; } = default!;

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
    "Specify to use the version of Godot that does not support C#/.NET."
)]
  public bool NoDotnet { get; set; }

  public bool IsWindowsElevationRequired => true;

  public GodotUseCommand(IExecutionContext context) {
    ExecutionContext = context;
  }

  public async ValueTask ExecuteAsync(IConsole console) {
    var systemInfo = ExecutionContext.Godot.Platform.SystemInfo;
    var godotRepo = ExecutionContext.Godot.GodotRepo;
    var platform = ExecutionContext.Godot.Platform;

    var log = ExecutionContext.CreateLog(systemInfo, console);
    var output = console.Output;

    var version = SemanticVersion.Parse(RawVersion);
    var isDotnetVersion = !NoDotnet;

    var noDotnetFlag = isDotnetVersion ? "" : " --no-dotnet";

    var potentialInstallation =
      godotRepo.GetInstallation(version, isDotnetVersion);

    await Task.CompletedTask;

    if (potentialInstallation is not GodotInstallation installation) {
      log.Print("");
      log.Warn($"Godot version {version.VersionString} is not installed.");
      log.Print("To install this version of Godot, run:");
      log.Print("");
      log.Success($"    godotenv godot install {version.VersionString}{noDotnetFlag}");
      log.Print("");

      return;
    }

    await godotRepo.UpdateGodotSymlink(installation, log);

    log.Print("");
    log.Success($"Godot version `{installation.VersionName}` is now active.");
    log.Print("");
    log.Info("Please make sure your GODOT environment variable is set to the ");
    log.Info("symlink pointing to the current version of Godot:");
    log.Print("");
    log.Print(godotRepo.GodotInstallationsPath);
  }
}
