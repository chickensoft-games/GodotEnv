namespace Chickensoft.GodotEnv.Features.Godot.Models;

using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Common.Utilities;
using global::GodotEnv.Common.Utilities;

public class Linux : Unix {
  public Linux(
    ISystemInfo systemInfo,
    IFileClient fileClient,
    IComputer computer,
    IVersionStringConverter versionStringConverter
  )
    : base(systemInfo, fileClient, computer, versionStringConverter) { }

  public override string GetInstallerNameSuffix(DotnetSpecificGodotVersion version) {
    var (platformName, architecture) = GetPlatformNameAndArchitecture(version);

    return version.IsDotnet ? $"_mono_{platformName}_{architecture}" : $"_{platformName}.{architecture}";
  }

  public override void Describe(ILog log) => log.Info("🐧 Running on Linux");

  public override string GetRelativeExtractedExecutablePath(
    DotnetSpecificGodotVersion version
  ) {
    var fsVersionString = GetFilenameVersionString(version);
    var nameSuffix = GetInstallerNameSuffix(version);
    var (platformName, architecture) = GetPlatformNameAndArchitecture(version);

    var pathSuffix = fsVersionString +
               $"{(version.IsDotnet ? "_mono" : "")}_{platformName}.{architecture}";

    if (version.IsDotnet) {
      // Dotnet version extracts to a folder, whereas the non-dotnet version
      // does not.
      return FileClient.Combine(fsVersionString + nameSuffix, pathSuffix);
    }

    return pathSuffix;
  }

  public override string GetRelativeGodotSharpPath(
    DotnetSpecificGodotVersion version
  ) => FileClient.Combine(
      GetFilenameVersionString(version) + GetInstallerNameSuffix(version),
      "GodotSharp/"
    );

  private static (string platformName, string architecture) GetPlatformNameAndArchitecture(
    GodotVersion version
  ) {
    var architecture = "x86_64";
    var platformName = "linux";

    if (version.Number.Major == 3) {
      architecture = "64";
      platformName = "x11";
    }

    return (platformName, architecture);
  }
}
