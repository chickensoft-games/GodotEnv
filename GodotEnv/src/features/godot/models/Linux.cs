namespace Chickensoft.GodotEnv.Features.Godot.Models;

using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Common.Utilities;
using Chickensoft.GodotEnv.Features.Godot.Serializers;

public class Linux : Unix
{
  public Linux(
    ISystemInfo systemInfo,
    IFileClient fileClient,
    IComputer computer,
    IVersionDeserializer versionDeserializer,
    IVersionSerializer versionSerializer
  )
    : base(systemInfo, fileClient, computer, versionDeserializer, versionSerializer) { }

  public override string GetInstallerNameSuffix(SpecificDotnetStatusGodotVersion version)
  {
    var (platformName, architecture) = GetPlatformNameAndArchitecture(version);

    return version.IsDotnetEnabled ? $"_mono_{platformName}_{architecture}" : $"_{platformName}.{architecture}";
  }

  public override void Describe(ILog log) => log.Info("🐧 Running on Linux");

  public override string GetRelativeExtractedExecutablePath(
    SpecificDotnetStatusGodotVersion version
  )
  {
    var fsVersionString = GetFilenameVersionString(version);
    var nameSuffix = GetInstallerNameSuffix(version);
    var (platformName, architecture) = GetPlatformNameAndArchitecture(version);

    var pathSuffix = fsVersionString +
               $"{(version.IsDotnetEnabled ? "_mono" : "")}_{platformName}.{architecture}";

    if (version.IsDotnetEnabled)
    {
      // Dotnet version extracts to a folder, whereas the non-dotnet version
      // does not.
      return FileClient.Combine(fsVersionString + nameSuffix, pathSuffix);
    }

    return pathSuffix;
  }

  public override string GetRelativeGodotSharpPath(
    SpecificDotnetStatusGodotVersion version
  ) => FileClient.Combine(
      GetFilenameVersionString(version) + GetInstallerNameSuffix(version),
      "GodotSharp/"
    );

  private static (string platformName, string architecture) GetPlatformNameAndArchitecture(
    GodotVersion version
  )
  {
    var architecture = "x86_64";
    var platformName = "linux";

    if (version.Number.Major == 3)
    {
      architecture = "64";
      platformName = "x11";
    }

    return (platformName, architecture);
  }
}
