namespace Chickensoft.GodotEnv.Features.Godot.Models;

using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Common.Utilities;

public class Linux : Unix {
  public Linux(IFileClient fileClient, IComputer computer)
    : base(fileClient, computer) { }

  public override string ExportTemplatesBasePath => FileClient.Combine(
    FileClient.UserDirectory, ".local/share/godot"
  );

  public override string GetInstallerNameSuffix(bool isDotnetVersion) =>
    isDotnetVersion ? "_mono_linux_x86_64" : "_linux.x86_64";

  public override void Describe(ILog log) => log.Info("🐧 Running on Linux");

  public override string GetRelativeExtractedExecutablePath(
    SemanticVersion version, bool isDotnetVersion
  ) {
    var fsVersionString = GetFilenameVersionString(version);
    var name = fsVersionString +
      $"{(isDotnetVersion ? "_mono" : "")}_linux.x86_64";

    if (isDotnetVersion) {
      // Dotnet version extracts to a folder, whereas the non-dotnet version
      // does not.
      return FileClient.Combine(fsVersionString + "_mono_linux_x86_64", name);
    }

    return name;
  }

  public override string GetRelativeGodotSharpDebugPath(
    SemanticVersion version
  ) => FileClient.Combine(
    GetFilenameVersionString(version) + "_mono_linux_x86_64",
    "GodotSharp/Api/Debug/"
  );

  public override string GetRelativeGodotSharpReleasePath(
    SemanticVersion version
  ) => FileClient.Combine(
    GetFilenameVersionString(version) + "_mono_linux_x86_64",
    "GodotSharp/Api/Release/"
  );
}
