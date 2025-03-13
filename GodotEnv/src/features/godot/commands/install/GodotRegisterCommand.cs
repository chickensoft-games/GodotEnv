namespace Chickensoft.GodotEnv.Features.Godot.Commands;

using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Features.Godot.Models;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using CliWrap;

[Command("godot register", Description = "Register a version of Godot.")]
public class GodotRegisterCommand :
  ICommand, ICliCommand, IWindowsElevationEnabled {
  [CommandParameter(
    0,
    Name = "Name",
    Validators = [typeof(GodotVersionValidator)],
    Description = "Godot version name: e.g., 4.1.0.CustomBuild, 4.2.0.GodotSteam, etc." +
      " Should be a unique Godot version name"
  )]
  public string VersionName { get; set; } = default!;

  [CommandParameter(
    1,
    Name = "Path",
    Description = "Godot version path: e.g. /user/godot/version/path/"
  )]
  public string VersionPath { get; set; } = default!;

  [CommandParameter(
    2,
    Name = "Executable Path",
    Description = "Godot executable name: e.g. \"godot-steam-4.3/godotsteam.multiplayer.43.editor.windows.64.exe\""
  )]
  public string ExecutablePath { get; set; } = default!;

  [CommandOption(
    "no-dotnet", 'n',
    Description =
      "Specify to use the version of Godot that does not support C#/.NET."
  )]
  public bool NoDotnet { get; set; }

  public IExecutionContext ExecutionContext { get; set; } = default!;

  public bool IsWindowsElevationRequired => true;

  public GodotRegisterCommand(IExecutionContext context) {
    ExecutionContext = context;
  }

  public async ValueTask ExecuteAsync(IConsole console) {
    var godotRepo = ExecutionContext.Godot.GodotRepo;
    var platform = ExecutionContext.Godot.Platform;

    var log = ExecutionContext.CreateLog(console);
    var token = console.RegisterCancellationHandler();

    // We know this won't throw because the validator okayed it
    var version = godotRepo.VersionStringConverter.ParseVersion(VersionName);
    var isDotnetVersion = !NoDotnet;

    var godotInstallationsPath = godotRepo.GodotInstallationsPath;
    var godotCachePath = godotRepo.GodotCachePath;

    var existingInstallation =
      godotRepo.GetInstallation(version, isDotnetVersion);

    // Log information to show we understood.
    platform.Describe(log);
    log.Info($"🤖 Godot v{VersionName}");
    log.Info($"🍯 Parsed version: {version}");
    log.Info(
      isDotnetVersion ? "😁 Using Godot with .NET" : "😢 Using Godot without .NET"
    );

    // Check for existing installation.
    if (existingInstallation is GodotInstallation installation) {
      log.Warn(
        $"🤔 Godot v{VersionName} is already installed:"
      );
      log.Warn(installation);
    }


    // FIX
    var newInstallation =
      await godotRepo.ExtractGodotCustomBuild(VersionPath, ExecutablePath, version, isDotnetVersion, log);

    await godotRepo.UpdateGodotSymlink(newInstallation, log);

    await godotRepo.UpdateDesktopShortcut(newInstallation, log);

    await godotRepo.AddOrUpdateGodotEnvVariable(log);
  }
}
