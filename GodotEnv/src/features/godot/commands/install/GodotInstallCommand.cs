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
  ICommand, ICliCommand, IWindowsElevationEnabled
{
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

  [CommandOption(
    "unsafe-skip-checksum-verification",
    Description = "UNSAFE! Specify to skip checksum verification."
  )]
  public bool SkipChecksumVerification { get; set; }

  [CommandOption(
    "proxy", 'x',
    Description = "Specify a proxy server URL to use for downloads (e.g., http://127.0.0.1:1080)."
  )]
  public string? ProxyUrl { get; set; }

  public IExecutionContext ExecutionContext { get; set; } = default!;

  public bool IsWindowsElevationRequired => true;

  public GodotInstallCommand(IExecutionContext context)
  {
    ExecutionContext = context;
  }

  public async ValueTask ExecuteAsync(IConsole console)
  {
    var godotRepo = ExecutionContext.Godot.GodotRepo;
    var platform = ExecutionContext.Godot.Platform;

    var log = ExecutionContext.CreateLog(console);
    var token = console.RegisterCancellationHandler();

    SpecificDotnetStatusGodotVersion? version;
    if (!string.IsNullOrEmpty(RawVersion))
    {
      var isDotnetVersion = !NoDotnet;
      // We know this won't throw because the validator okayed it
      version = godotRepo.VersionDeserializer.Deserialize(RawVersion, isDotnetVersion);
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

    var existingInstallation =
      godotRepo.GetInstallation(version);

    // Log information to show we understood.
    platform.Describe(log);
    log.Info($"🤖 Godot v{RawVersion}");
    log.Info($"🍯 Parsed version: {version}");
    log.Info(
      version.IsDotnetEnabled ? "😁 Using Godot with .NET" : "😢 Using Godot without .NET"
    );

    if (!string.IsNullOrEmpty(ProxyUrl))
    {
      log.Info($"🌐 Using proxy: {ProxyUrl}");
    }

    // Check for existing installation.
    if (existingInstallation is GodotInstallation installation)
    {
      log.Warn(
        $"🤔 Godot v{RawVersion} is already installed:"
      );
      log.Warn(installation);
    }

    var godotCompressedArchive =
      await godotRepo.DownloadGodot(
        version, SkipChecksumVerification, log, token, ProxyUrl
      );

    var newInstallation =
      await godotRepo.ExtractGodotInstaller(godotCompressedArchive, log);

    await godotRepo.UpdateGodotSymlink(newInstallation, log);

    await godotRepo.UpdateDesktopShortcut(newInstallation, log);

    await godotRepo.AddOrUpdateGodotEnvVariable(log);
  }
}
