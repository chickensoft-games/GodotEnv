namespace Chickensoft.GodotEnv.Features.Godot.Models;

using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Common.Utilities;
using Chickensoft.GodotEnv.Features.Godot.Serializers;

public class MacOS : Unix
{
  public MacOS(
    ISystemInfo systemInfo,
    IFileClient fileClient,
    IComputer computer,
    IVersionDeserializer versionDeserializer,
    IVersionSerializer versionSerializer
  )
    : base(systemInfo, fileClient, computer, versionDeserializer, versionSerializer) { }

  public override string GetInstallerNameSuffix(SpecificDotnetStatusGodotVersion version)
  {
    var hasUniversalSuffix =
      version.Number.Major > 3 ||
        (
          version.Number.Major == 3 &&
          version.Number.Minor > 3 &&
          version.Number.Patch > 2
        );
    var universalSuffix = hasUniversalSuffix ? ".universal" : ".64";

    return $"{(version.IsDotnetEnabled ? "_mono" : "")}_{(version.Number.Major == 3 ? "osx" : "macos")}{universalSuffix}";
  }

  public override void Describe(ILog log) => log.Info("🍏 Running on macOS");

  public override string GetRelativeExtractedExecutablePath(
    SpecificDotnetStatusGodotVersion version
  ) => $"Godot{(version.IsDotnetEnabled ? "_mono" : "")}.app/Contents/MacOS/Godot";

  public override string GetRelativeGodotSharpPath(
    SpecificDotnetStatusGodotVersion version
  ) => "Godot_mono.app/Contents/Resources/GodotSharp";
}
