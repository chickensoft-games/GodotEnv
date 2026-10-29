namespace Chickensoft.GodotEnv.Features.Godot.Models;

using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Common.Models;
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

  private static readonly (int major, int minor) _firstSingleArchVersion = (4, 8);

  // Starting with 4.8-beta1, macOS editors are built per architecture instead
  // of as a universal binary. The 4.8 dev snapshots were still universal.
  private static bool HasSingleArchBuilds(GodotVersion version)
  {
    var (major, minor) = (version.Number.Major, version.Number.Minor);

    if (major != _firstSingleArchVersion.major)
    {
      return major > _firstSingleArchVersion.major;
    }

    if (minor != _firstSingleArchVersion.minor)
    {
      return minor > _firstSingleArchVersion.minor;
    }

    return version.Number.Label != "dev";
  }

  public override string GetInstallerNameSuffix(SpecificDotnetStatusGodotVersion version)
  {
    var hasUniversalSuffix =
      version.Number.Major > 3 ||
        (
          version.Number.Major == 3 &&
          version.Number.Minor > 3 &&
          version.Number.Patch > 2
        );
    var archSuffix = hasUniversalSuffix ? ".universal" : ".64";

    if (HasSingleArchBuilds(version))
    {
      archSuffix = SystemInfo.CpuArch == CpuArch.Arm64 ? ".arm64" : ".x86_64";
    }

    return $"{(version.IsDotnetEnabled ? "_mono" : "")}_{(version.Number.Major == 3 ? "osx" : "macos")}{archSuffix}";
  }

  public override void Describe(ILog log) => log.Info("🍏 Running on macOS");

  public override string GetRelativeExtractedExecutablePath(
    SpecificDotnetStatusGodotVersion version
  ) => $"Godot{(version.IsDotnetEnabled ? "_mono" : "")}.app/Contents/MacOS/Godot";

  public override string GetRelativeGodotSharpPath(
    SpecificDotnetStatusGodotVersion version
  ) => "Godot_mono.app/Contents/Resources/GodotSharp";
}
