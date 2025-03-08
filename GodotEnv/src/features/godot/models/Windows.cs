namespace Chickensoft.GodotEnv.Features.Godot.Models;
using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Common.Utilities;
using global::GodotEnv.Common.Utilities;

public class Windows : GodotEnvironment {
  public Windows(
    ISystemInfo systemInfo,
    IFileClient fileClient,
    IComputer computer,
    IVersionStringConverter versionStringConverter
  )
    : base(systemInfo, fileClient, computer, versionStringConverter) { }

  private static readonly (int major, int minor) _firstKnownArmVersion = (4, 3);

  private string GetProcessorArchitecture(GodotVersion version) {
    var noKnownArmVersion =
      version.Major < _firstKnownArmVersion.major
        || version.Minor < _firstKnownArmVersion.minor;

    if (noKnownArmVersion || SystemInfo.CPUArch != CPUArch.Arm64) {
      return "win64";
    }

    return "windows_arm64";
  }

  public override string GetInstallerNameSuffix(
    bool isDotnetVersion, GodotVersion version
  )
    => isDotnetVersion
      ? $"_mono_{GetProcessorArchitecture(version)}"
      : $"_{GetProcessorArchitecture(version)}.exe";

  public override void Describe(ILog log) => log.Info("⧉ Running on Windows");

  public override string GetRelativeExtractedExecutablePath(
    GodotVersion version, bool isDotnetVersion
  ) {
    var fsVersionString = GetFilenameVersionString(version);
    var name = fsVersionString +
      $"{(isDotnetVersion ? "_mono" : "")}_{GetProcessorArchitecture(version)}.exe";

    // Both versions extract to a folder. The dotnet folder name is different
    // from the non-dotnet folder name :P

    if (isDotnetVersion) {
      return FileClient.Combine(fsVersionString + $"_mono_{GetProcessorArchitecture(version)}", name);
    }

    // There is no subfolder for non-dotnet versions.
    return name;
  }

  public override string GetRelativeGodotSharpPath(
    GodotVersion version,
    bool isDotnetVersion
  ) => FileClient.Combine(
    GetFilenameVersionString(version) + $"_mono_{GetProcessorArchitecture(version)}", "GodotSharp"
  );
}
