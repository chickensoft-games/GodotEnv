namespace Chickensoft.GodotEnv.Features.Godot.Models;

using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Common.Utilities;
using global::GodotEnv.Common.Utilities;

public class MacOS : Unix {
  public MacOS(
    ISystemInfo systemInfo,
    IFileClient fileClient,
    IComputer computer,
    IVersionStringConverter versionStringConverter
  )
    : base(systemInfo, fileClient, computer, versionStringConverter) { }

  public override string GetInstallerNameSuffix(bool isDotnetVersion, GodotVersion version) {
    var hasUniversalSuffix = version.Major > 3 || (version.Major == 3 && version.Minor > 3 && version.Patch > 2);
    var universalSuffix = hasUniversalSuffix ? ".universal" : ".64";

    return $"{(isDotnetVersion ? "_mono" : "")}_{(version.Major == 3 ? "osx" : "macos")}{universalSuffix}";
  }

  public override void Describe(ILog log) => log.Info("🍏 Running on macOS");

  public override string GetRelativeExtractedExecutablePath(
    GodotVersion version, bool isDotnetVersion
  ) => $"Godot{(isDotnetVersion ? "_mono" : "")}.app/Contents/MacOS/Godot";

  public override string GetRelativeGodotSharpPath(
    GodotVersion version,
    bool isDotnetVersion
  ) => "Godot_mono.app/Contents/Resources/GodotSharp";
}
