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
public class GodotUseCommand : ICommand, ICliCommand, IWindowsElevationEnabled
{
  public IExecutionContext ExecutionContext { get; set; } = default!;

  [CommandParameter(
    0,
    Name = "Version",
    Validators = [typeof(GodotVersionValidator)],
    Description = "Godot version to install: e.g., 4.1.0-rc.2, 4.2.0, etc." +
      " Should match a version of Godot " +
      "(https://github.com/godotengine/godot-builds/tags) or GodotSharp " +
      "(https://www.nuget.org/packages/GodotSharp/)",
    IsRequired = false
  )]
  public string RawVersion { get; set; } = default!;

  [CommandOption(
  "no-dotnet", 'n',
  Description =
    "Specify to use the version of Godot that does not support C#/.NET."
)]
  public bool NoDotnet { get; set; }

  public bool IsWindowsElevationRequired => true;

  public GodotUseCommand(IExecutionContext context)
  {
    ExecutionContext = context;
  }

  public async ValueTask ExecuteAsync(IConsole console)
  {
    var godotRepo = ExecutionContext.Godot.GodotRepo;

    var log = ExecutionContext.CreateLog(console);

    SpecificDotnetStatusGodotVersion? version;
    if (!string.IsNullOrEmpty(RawVersion))
    {
      var isDotnetVersion = !NoDotnet;
      // We know this won't throw because the validator okayed it
      version =
        godotRepo.VersionDeserializer.Deserialize(RawVersion, isDotnetVersion);
    }
    else
    {
      var versionRepo = ExecutionContext.Godot.VersionRepo;
      var versionFiles = versionRepo.GetVersionFiles();
      version = versionRepo.InferVersion(versionFiles, log);
    }

    if (version is null)
    {
      log.Err(
        """
        No version specified and couldn't find version file in directory tree.
        Please specify a version or execute in a directory with access to a
        global.json, .csproj, or .godotrc file with the appropriate Godot
        version for your project.
        """
      );
      return;
    }

    var noDotnetFlag = version.IsDotnetEnabled ? "" : " --no-dotnet";

    var potentialInstallation =
      godotRepo.GetInstallation(version);

    await Task.CompletedTask;

    if (potentialInstallation is not GodotInstallation installation)
    {
      log.Print("");
      log.Warn(
        $"Godot version {godotRepo.VersionSerializer.Serialize(version)} is not installed."
      );
      log.Print("To install this version of Godot, run:");
      log.Print("");
      log.Success(
        $"    godotenv godot install {godotRepo.VersionSerializer.Serialize(version)}{noDotnetFlag}"
      );
      log.Print("");

      return;
    }

    await godotRepo.UpdateGodotSymlink(installation, log);

    log.Print("");
    log.Success(
      $"Godot version `{godotRepo.InstallationVersionName(installation)}` is now active."
    );
    log.Print("");
    log.Info("Please make sure your GODOT environment variable is set to the ");
    log.Info("symlink pointing to the current version of Godot:");
    log.Print("");
    log.Print(godotRepo.GodotInstallationsPath);
  }
}
