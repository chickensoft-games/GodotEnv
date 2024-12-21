namespace Chickensoft.GodotEnv.Features.Godot.Models;

using System.IO.Abstractions;
using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Common.Utilities;

public class Windows : GodotEnvironment {
  public Windows(IFileClient fileClient, IComputer computer)
    : base(fileClient, computer) { }

  private static readonly (int major, int minor) _firstKnownArmVersion = (4, 3);

  private string GetProcessorArchitecture(SemanticVersion version) {
    var noKnownArmVersion = int.TryParse(version.Major, out var major)
                            && int.TryParse(version.Minor, out var minor)
                            && (major < _firstKnownArmVersion.major || minor < _firstKnownArmVersion.minor);

    if (noKnownArmVersion || FileClient.Processor != ProcessorType.arm64) {
      return "win64";
    }

    return "windows_arm64";
  }

  public override string ExportTemplatesBasePath =>
    FileClient.GetFullPath(
      FileClient.Combine(FileClient.UserDirectory, "\\AppData\\Roaming\\Godot")
    );

  public override string GetInstallerNameSuffix(bool isDotnetVersion, SemanticVersion version)
    => isDotnetVersion ? $"_mono_{GetProcessorArchitecture(version)}" : $"_{GetProcessorArchitecture(version)}.exe";

  public override Task<bool> IsExecutable(IShell shell, IFileInfo file) =>
    Task.FromResult(file.Name.ToLowerInvariant().EndsWith(".exe"));

  public override void Describe(ILog log) => log.Info("⧉ Running on Windows");

  public override string GetRelativeExtractedExecutablePath(
    SemanticVersion version, bool isDotnetVersion
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
    SemanticVersion version,
    bool isDotnetVersion
  ) => FileClient.Combine(
    GetFilenameVersionString(version) + $"_mono_{GetProcessorArchitecture(version)}", "GodotSharp"
  );
}
