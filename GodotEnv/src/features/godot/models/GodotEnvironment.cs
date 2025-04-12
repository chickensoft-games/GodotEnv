namespace Chickensoft.GodotEnv.Features.Godot.Models;

using System;
using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Common.Utilities;
using Chickensoft.GodotEnv.Features.Godot.Serializers;
using global::GodotEnv.Common.Utilities;

public interface IGodotEnvironment {
  ISystemInfo SystemInfo { get; }
  /// <summary>
  /// File client used by the platform to manipulate file paths.
  /// </summary>
  IFileClient FileClient { get; }

  IComputer Computer { get; }

  IVersionDeserializer VersionDeserializer { get; }
  IVersionSerializer VersionSerializer { get; }

  /// <summary>
  /// Godot installation filename suffix.
  /// </summary>
  /// <param name="version">Godot version.</param>
  /// <returns>Godot filename suffix.</returns>
  string GetInstallerNameSuffix(SpecificDotnetStatusGodotVersion version);

  /// <summary>
  /// Computes the Godot download url.
  /// </summary>
  /// <param name="version">Godot version.</param>
  /// <param name="isTemplate">True if computing the download url to the
  /// export templates, false to compute the download url to the Godot
  /// application.</param>
  string GetDownloadUrl(
    SpecificDotnetStatusGodotVersion version,
    bool isTemplate
  );

  /// <summary>
  /// Gets the filename as which an installer is known.
  /// </summary>
  /// <param name="version">Godot version.</param>
  public string GetInstallerFilename(
    SpecificDotnetStatusGodotVersion version
  );

  /// <summary>
  /// Outputs a description of the platform to the log.
  /// </summary>
  /// <param name="log">Output log.</param>
  void Describe(ILog log);

  /// <summary>
  /// Returns the path where the Godot executable itself is located, relative
  /// to the extracted Godot installation directory.
  /// </summary>
  /// <param name="version">Godot version.</param>
  /// <returns>Relative path.</returns>
  string GetRelativeExtractedExecutablePath(
    SpecificDotnetStatusGodotVersion version
  );

  /// <summary>
  /// For dotnet-enabled versions, this gets the path to the GodotSharp
  /// directory that is included with Godot.
  /// </summary>
  /// <param name="version">Godot version.</param>
  /// <returns>Path to the GodotSharp directory.</returns>
  string GetRelativeGodotSharpPath(SpecificDotnetStatusGodotVersion version);
}

public abstract class GodotEnvironment : IGodotEnvironment {
  public const string GODOT_FILENAME_PREFIX = "Godot_v";
  public const string GODOT_URL_PREFIX =
    "https://github.com/godotengine/godot-builds/releases/download/";

  /// <summary>
  /// Creates a platform for the given OS.
  /// </summary>
  /// <param name="systemInfo"></param>
  /// <param name="fileClient">File client.</param>
  /// <param name="computer">Computer.</param>
  /// <param name="versionStringConverter">Version-string converter.</param>
  /// <returns>Platform instance.</returns>
  /// <exception cref="InvalidOperationException" />
  public static GodotEnvironment Create(
    ISystemInfo systemInfo,
    IFileClient fileClient,
    IComputer computer,
    IVersionDeserializer versionDeserializer,
    IVersionSerializer versionSerializer
  ) =>
    systemInfo.OS switch {
      OSType.Windows => new Windows(systemInfo, fileClient, computer, versionDeserializer, versionSerializer),
      OSType.MacOS => new MacOS(systemInfo, fileClient, computer, versionDeserializer, versionSerializer),
      OSType.Linux => new Linux(systemInfo, fileClient, computer, versionDeserializer, versionSerializer),
      OSType.Unknown => throw GetUnknownOSException(),
      _ => throw GetUnknownOSException()
    };

  protected GodotEnvironment(
    ISystemInfo systemInfo,
    IFileClient fileClient,
    IComputer computer,
    IVersionDeserializer versionDeserializer,
    IVersionSerializer versionSerializer
  ) {
    SystemInfo = systemInfo;
    FileClient = fileClient;
    Computer = computer;
    VersionDeserializer = versionDeserializer;
    VersionSerializer = versionSerializer;
  }

  public ISystemInfo SystemInfo { get; }
  public IFileClient FileClient { get; }
  public IComputer Computer { get; }
  public IVersionDeserializer VersionDeserializer { get; }
  public IVersionSerializer VersionSerializer { get; }

  public string ExportTemplatesBasePath => throw new NotImplementedException();

  public abstract string GetInstallerNameSuffix(SpecificDotnetStatusGodotVersion version);
  public abstract void Describe(ILog log);
  public abstract string GetRelativeExtractedExecutablePath(
    SpecificDotnetStatusGodotVersion version
  );
  public abstract string GetRelativeGodotSharpPath(
    SpecificDotnetStatusGodotVersion version
  );

  public string GetDownloadUrl(
    SpecificDotnetStatusGodotVersion version,
    bool isTemplate
  ) {
    // We need to be sure this is a release-style version string to get the
    // correct url
    var versionConverter = new ReleaseVersionSerializer();
    var url = $"{GODOT_URL_PREFIX}{versionConverter.Serialize(version)}/";

    // Godot application download url.
    if (!isTemplate) {
      return url + GetInstallerFilename(version);
    }

    // Export template download url.
    return
      url + GetExportTemplatesInstallerFilename(version);
  }

  protected string GetFilenameVersionString(GodotVersion version) =>
    GODOT_FILENAME_PREFIX + VersionSerializer.Serialize(version);

  // Gets the filename of the Godot installation download for the platform.
  public string GetInstallerFilename(
    SpecificDotnetStatusGodotVersion version
  ) => GetFilenameVersionString(version) + GetInstallerNameSuffix(version) +
    ".zip";

  // Gets the filename of the Godot export templates installation download for
  // the platform.
  private string GetExportTemplatesInstallerFilename(
    SpecificDotnetStatusGodotVersion version
  ) => GetFilenameVersionString(version) + (version.IsDotnetEnabled ? "_mono" : "") +
      "_export_templates.tpz";

  private static InvalidOperationException GetUnknownOSException() =>
    new("🚨 Cannot create a platform an unknown operating system.");
}
