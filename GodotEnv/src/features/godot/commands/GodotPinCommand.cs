namespace Chickensoft.GodotEnv.Features.Addons.Commands;

using System;
using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Features.Godot.Models;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

[Command(
  "godot pin",
  Description = "Pins the current Godot project to the active Godot version by "
    + "writing a version-specifier file to the project directory."
)]
public class GodotPinCommand : ICommand, ICliCommand, IWindowsElevationEnabled {
  public IExecutionContext ExecutionContext { get; set; } = default!;

  public bool IsWindowsElevationRequired => false;

  public GodotPinCommand(IExecutionContext context) {
    ExecutionContext = context;
  }

  public async ValueTask ExecuteAsync(IConsole console) {
    var godotRepo = ExecutionContext.Godot.GodotRepo;
    var platform = ExecutionContext.Godot.Platform;

    var log = ExecutionContext.CreateLog(console);
    var output = console.Output;

    SpecificDotnetStatusGodotVersion? version = null;

    foreach (var result in godotRepo.GetInstallationsList()) {
      if (
        result.Value is GodotInstallation installation
          && installation.IsActiveVersion
      ) {
        version = installation.Version;
      }
    }

    if (version is null) {
      log.Err("No active Godot version found.");
      log.Print("To install a Godot version, run:");
      log.Print("");
      log.Success("    godotenv godot install <version>");
      log.Print("");
      log.Print("To activate an installed version, run:");
      log.Print("");
      log.Success("    godotenv godot use <version>");
      return;
    }

    var versionRepo = ExecutionContext.Godot.VersionRepo;

    var projectDir = versionRepo.GetProjectDefinitionDirectory();
    if (string.IsNullOrEmpty(projectDir)) {
      log.Err("No Godot project found in this or any ancestor directory.");
      log.Print("Please run this command in a directory or subdirectory of");
      log.Print("a Godot project containing either a .sln file or");
      log.Print("a project.godot file.");
      return;
    }

    try {
      versionRepo.PinVersion(version, projectDir, log);
    }
    catch (Exception e) {
      log.Err("An error occurred while pinning the current Godot version:");
      log.Print(e.Message);
    }

    await Task.CompletedTask;

    log.Print("");
    log.Success(
      $"Godot version `{godotRepo.VersionSerializer.SerializeWithDotnetStatus(version)}` is now pinned as the preferred project version."
    );
  }
}
