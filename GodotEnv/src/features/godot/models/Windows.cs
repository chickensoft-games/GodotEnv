namespace Chickensoft.GodotEnv.Features.Godot.Models;

using System.IO.Abstractions;
using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Common.Utilities;

public class Windows : GodotEnvironment {
  public Windows(IFileClient fileClient, IComputer computer)
    : base(fileClient, computer) { }

  public override string ExportTemplatesBasePath =>
    FileClient.GetFullPath(
      FileClient.Combine(FileClient.UserDirectory, "\\AppData\\Roaming\\Godot")
    );

  public override string GetInstallerNameSuffix(bool isDotnetVersion, SemanticVersion version) =>
    isDotnetVersion ? "_mono_win64" : "_win64.exe";

  public override Task<bool> IsExecutable(IShell shell, IFileInfo file) =>
    Task.FromResult(file.Name.ToLower().EndsWith(".exe"));

  public override void Describe(ILog log) => log.Info("⧉ Running on Windows");

  public override string GetRelativeExtractedExecutablePath(
    SemanticVersion version, bool isDotnetVersion
  ) {
    var fsVersionString = GetFilenameVersionString(version);
    var name = fsVersionString +
      $"{(isDotnetVersion ? "_mono" : "")}_win64.exe";

    // Both versions extract to a folder. The dotnet folder name is different
    // from the non-dotnet folder name :P

    if (isDotnetVersion) {
      return FileClient.Combine(fsVersionString + "_mono_win64", name);
    }

    // There is no subfolder for non-dotnet versions.
    //return FileClient.Combine(fsVersionString + "_win64.exe", name);
    return name;
  }

  public override string GetRelativeGodotSharpPath(
    SemanticVersion version,
    bool isDotnetVersion
  ) => FileClient.Combine(
    GetFilenameVersionString(version) + "_mono_win64", "GodotSharp"
  );
}
