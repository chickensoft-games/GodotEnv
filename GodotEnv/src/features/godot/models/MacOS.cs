namespace Chickensoft.GodotEnv.Features.Godot.Models;

using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Common.Utilities;
using global::GodotEnv.Common.Utilities;

public class MacOS : Unix {
  public MacOS(ISystemInfo systemInfo, IFileClient fileClient, IComputer computer)
    : base(systemInfo, fileClient, computer) { }

  public override string ExportTemplatesBasePath =>
    FileClient.GetFullPath(
      FileClient.Combine(
        FileClient.UserDirectory, "/Library/Application Support/Godot/"
      )
    );

  public override string GetInstallerNameSuffix(bool isDotnetVersion, SemanticVersion version) {
    var major = int.Parse(version.Major);
    var minor = int.Parse(version.Minor);
    var patch = int.Parse(version.Patch);
    var hasUniversalSuffix = major > 3 || (major == 3 && minor > 3 && patch > 2);
    var universalSuffix = hasUniversalSuffix ? ".universal" : ".64";

    return $"{(isDotnetVersion ? "_mono" : "")}_{(major == 3 ? "osx" : "macos")}{universalSuffix}";
  }

  public override void Describe(ILog log) => log.Info("🍏 Running on macOS");

  public override string GetRelativeExtractedExecutablePath(
    SemanticVersion version, bool isDotnetVersion
  ) => $"Godot{(isDotnetVersion ? "_mono" : "")}.app/Contents/MacOS/Godot";

  public override string GetRelativeGodotSharpPath(
    SemanticVersion version,
    bool isDotnetVersion
  ) => "Godot_mono.app/Contents/Resources/GodotSharp";
}
