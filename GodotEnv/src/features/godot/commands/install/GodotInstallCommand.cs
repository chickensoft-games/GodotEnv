namespace Chickensoft.GodotEnv.Features.Godot.Commands;

using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Features.Godot.Models;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using CliWrap;

[Command("godot install", Description = "Install a version of Godot.")]
public class GodotInstallCommand : ICommand, ICliCommand {
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

  public IExecutionContext ExecutionContext { get; set; } = default!;

  public GodotInstallCommand(IExecutionContext context) {
    ExecutionContext = context;
  }

  public async ValueTask ExecuteAsync(IConsole console) {
    var log = ExecutionContext.CreateLog(console);

    var token = console.RegisterCancellationHandler();

    var version = SemanticVersion.Parse(RawVersion);
    var isDotnetVersion = !NoDotnet;

    var godotRepo = ExecutionContext.Godot.GodotRepo;
    var platform = ExecutionContext.Godot.Platform;

    var godotInstallationsPath = godotRepo.GodotInstallationsPath;
    var godotCachePath = godotRepo.GodotCachePath;

    var existingInstallation =
      godotRepo.GetInstallation(version, isDotnetVersion);

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
        version, isDotnetVersion, log, token
      );

    var newInstallation =
      await godotRepo.ExtractGodotInstaller(godotCompressedArchive, log);

    await godotRepo.UpdateGodotSymlink(newInstallation, log);

    await godotRepo.AddOrUpdateGodotEnvVariable(log);

    log.Print(godotRepo.GodotSymlinkPath);
  }
}
