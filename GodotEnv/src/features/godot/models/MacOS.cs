namespace Chickensoft.GodotEnv.Features.Godot.Models;

using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Common.Utilities;

public class MacOS : Unix {
  public MacOS(IFileClient fileClient, IComputer computer)
    : base(fileClient, computer) { }

  public override string ExportTemplatesBasePath =>
    FileClient.GetFullPath(
      FileClient.Combine(
        FileClient.UserDirectory, "/Library/Application Support/Godot/"
      )
    );

  public override string GetInstallerNameSuffix(bool isDotnetVersion, SemanticVersion version) =>
    $"{(isDotnetVersion ? "_mono" : "")}_{(int.Parse(version.Major) == 3 ? "osx" : "macos")}.universal";

  public override void Describe(ILog log) => log.Info("🍏 Running on macOS");

  public override string GetRelativeExtractedExecutablePath(
    SemanticVersion version, bool isDotnetVersion
  ) => $"Godot{(isDotnetVersion ? "_mono" : "")}.app/Contents/MacOS/Godot";

  public override string GetRelativeGodotSharpPath(
    SemanticVersion version,
    bool isDotnetVersion
  ) => "Godot_mono.app/Contents/Resources/GodotSharp";
}
