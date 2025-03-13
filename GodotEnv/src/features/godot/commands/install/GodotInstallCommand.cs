namespace Chickensoft.GodotEnv.Features.Godot.Commands;

using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Features.Godot.Models;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using CliWrap;

[Command("godot install", Description = "Install a version of Godot.")]
public class GodotInstallCommand :
  ICommand, ICliCommand, IWindowsElevationEnabled {
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
      "Specify to use the version of Godot that does not support C#/.NET."
  )]
  public bool NoDotnet { get; set; }

  [CommandOption(
    "unsafe-skip-checksum-verification",
    Description = "UNSAFE! Specify to skip checksum verification."
  )]
  public bool SkipChecksumVerification { get; set; }

  public IExecutionContext ExecutionContext { get; set; } = default!;

  public bool IsWindowsElevationRequired => true;

  public GodotInstallCommand(IExecutionContext context) {
    ExecutionContext = context;
  }

  public async ValueTask ExecuteAsync(IConsole console) {
    var godotRepo = ExecutionContext.Godot.GodotRepo;
    var platform = ExecutionContext.Godot.Platform;

    var log = ExecutionContext.CreateLog(console);
    var token = console.RegisterCancellationHandler();

    var isDotnetVersion = !NoDotnet;
    // We know this won't throw because the validator okayed it
    var version = godotRepo.VersionStringConverter.ParseVersion(RawVersion, isDotnetVersion);

    var godotInstallationsPath = godotRepo.GodotInstallationsPath;
    var godotCachePath = godotRepo.GodotCachePath;

    var existingInstallation =
      godotRepo.GetInstallation(version);

    // Log information to show we understood.
    platform.Describe(log);
    log.Info($"🤖 Godot v{RawVersion}");
    log.Info($"🍯 Parsed version: {version}");
    log.Info(
      isDotnetVersion ? "😁 Using Godot with .NET" : "😢 Using Godot without .NET"
    );

    // Check for existing installation.
    if (existingInstallation is GodotInstallation installation) {
      log.Warn(
        $"🤔 Godot v{RawVersion} is already installed:"
      );
      log.Warn(installation);
    }

    var godotCompressedArchive =
      await godotRepo.DownloadGodot(
        version, SkipChecksumVerification, log, token
      );

    var newInstallation =
      await godotRepo.ExtractGodotInstaller(godotCompressedArchive, log);

    await godotRepo.UpdateGodotSymlink(newInstallation, log);

    await godotRepo.UpdateDesktopShortcut(newInstallation, log);

    await godotRepo.AddOrUpdateGodotEnvVariable(log);
  }
}
