namespace Chickensoft.GodotEnv.Features.Godot.Domain;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Common.Utilities;
using Chickensoft.GodotEnv.Features.Godot.Models;
using Chickensoft.GodotEnv.Features.Godot.Serializers;

public struct RemoteVersion {
  public string Name { get; set; }
}

public interface IGodotRepository {
  public ISystemInfo SystemInfo { get; }
  public Config Config { get; }
  public IFileClient FileClient { get; }
  public INetworkClient NetworkClient { get; }
  public IZipClient ZipClient { get; }
  public IEnvironmentVariableClient EnvironmentVariableClient { get; }
  public IGodotEnvironment Platform { get; }
  public IProcessRunner ProcessRunner { get; }
  public IVersionDeserializer VersionDeserializer { get; }
  public IVersionSerializer VersionSerializer { get; }
  public string GodotInstallationsPath { get; }
  public string GodotCachePath { get; }
  public string GodotSymlinkPath { get; }
  [MemberNotNullWhen(true, nameof(IsGodotSymlinkTargetAvailable))]
  public bool IsGodotSymlinkTargetAvailable { get; }
  public string? GodotSymlinkTarget { get; }
  public string GodotSharpSymlinkPath { get; }

  /// <summary>
  /// Clears the Godot installations cache and recreates the cache directory.
  /// </summary>
  public void ClearCache();

  /// <summary>
  /// Gets the installation associated with the specified version of Godot.
  /// If both the .NET-enabled and the non-.NET-enabled versions of Godot with
  /// the same version are installed, this returns the .NET-enabled version.
  /// </summary>
  /// <param name="version">Godot version.</param>
  /// <returns>Godot installation, or null if none found.</returns>
  public GodotInstallation? GetInstallation(AnyDotnetStatusGodotVersion version);

  /// <summary>
  /// Gets the installation associated with the specified version of Godot.
  /// Returns only an installation of Godot matching the specified .NET status.
  /// </summary>
  /// <param name="version">Godot version.</param>
  /// <returns>Godot installation, or null if none found.</returns>
  public GodotInstallation? GetInstallation(SpecificDotnetStatusGodotVersion version);

  /// <summary>
  /// Name shown when listing Godot versions installed.
  /// </summary>
  /// <param name="installation">Installation whose version will be shown.</param>
  public string InstallationVersionName(GodotInstallation installation);

  /// <summary>
  /// Downloads the specified version of Godot.
  /// </summary>
  /// <param name="version">Godot version.</param>
  /// <param name="skipChecksumVerification">True if checksum verification should be skipped</param>
  /// <param name="log">Output log.</param>
  /// <param name="token">Cancellation token.</param>
  /// <param name="proxyUrl">Optional proxy URL.</param>
  /// <returns>The fully resolved / absolute path of the Godot installation zip
  /// file for the Platform.</returns>
  public Task<GodotCompressedArchive> DownloadGodot(
      SpecificDotnetStatusGodotVersion version,
      bool skipChecksumVerification,
      ILog log,
      CancellationToken token,
      string? proxyUrl = null
    );

  /// <summary>
  /// Extracts the Godot compressed archive files into the correct directory.
  /// </summary>
  /// <param name="archive">Godot installation archive.</param>
  /// <param name="log">Output log.</param>
  /// <returns>Path to the subfolder in the Godot installations directory
  /// containing the extracted contents.</returns>
  public Task<GodotInstallation> ExtractGodotInstaller(
    GodotCompressedArchive archive, ILog log
  );

  /// <summary>
  /// Updates the symlink to point to the specified Godot installation.
  /// </summary>
  /// <param name="installation">Godot installation.</param>
  /// <param name="log">Output log.</param>
  public Task UpdateGodotSymlink(GodotInstallation installation, ILog log);

  /// <summary>
  /// <para>Updates (or creates if non-existent) the desktop shortcut pointing to the newly created symlink.</para>
  /// <para>Promotes integration with the desktop environment.</para>
  /// </summary>
  /// <param name="installation">Godot installation.</param>
  /// <param name="log">Output log.</param>
  public Task UpdateDesktopShortcut(GodotInstallation installation, ILog log);

  /// <summary>
  /// Adds (or updates) the GODOT user environment variable to point to the
  /// symlink which points to the active version of Godot. Updates the user's PATH
  /// to include the 'bin' folder containing the godot symlink.
  /// </summary>
  /// <param name="log">Output log.</param>
  /// <returns>Completion task.</returns>
  public Task AddOrUpdateGodotEnvVariable(ILog log);

  /// <summary>
  /// Gets the GODOT user environment variable.
  /// </summary>
  /// <returns>GODOT user environment variable value.</returns>
  public Task<string> GetGodotEnvVariable();

  /// <summary>
  /// Get the list of installed Godot versions.
  /// </summary>
  /// <returns>
  /// A list of installed versions and directories that couldn't be loaded
  /// as Godot versions.
  /// </returns>
  public List<Result<GodotInstallation>> GetInstallationsList();

  /// <summary>
  /// Get the list of available Godot versions.
  /// </summary>
  /// <param name="log">Output log.</param>
  /// <param name="proxyUrl">Proxy URL to use for the request</param>
  /// <returns></returns>
  public Task<List<string>> GetRemoteVersionsList(ILog log, string? proxyUrl = null);

  /// <summary>
  /// Uninstalls the specified version of Godot.
  /// </summary>
  /// <param name="version">Godot version.</param>
  /// <param name="log">Output log.</param>
  /// <returns>True if successful, false if installation doesn't exist.
  /// </returns>
  public Task<bool> Uninstall(
    SpecificDotnetStatusGodotVersion version, ILog log
  );
}

public partial class GodotRepository : IGodotRepository {
  public ISystemInfo SystemInfo { get; }
  public Config Config { get; }
  public IFileClient FileClient { get; }
  public INetworkClient NetworkClient { get; }
  public IZipClient ZipClient { get; }
  public IGodotEnvironment Platform { get; }
  public IEnvironmentVariableClient EnvironmentVariableClient {
    get;
  }
  public IProcessRunner ProcessRunner { get; }

  private const string GODOT_REMOTE_VERSIONS_URL = "https://api.github.com/repos/godotengine/godot-builds/contents/releases";

  private IGodotChecksumClient ChecksumClient { get; }

  public IVersionDeserializer VersionDeserializer { get; }
  public IVersionSerializer VersionSerializer { get; }

  public string GodotInstallationsPath => FileClient.Combine(
    FileClient.AppDataDirectory,
    Defaults.GODOT_PATH,
    Config.ConfigValues.Godot.InstallationsPath
  );

  public string GodotCachePath => FileClient.Combine(
    FileClient.AppDataDirectory, Defaults.GODOT_PATH, Defaults.GODOT_CACHE_PATH
  );

  public string GodotBinPath => FileClient.Combine(
    FileClient.AppDataDirectory, Defaults.GODOT_PATH, Defaults.GODOT_BIN_PATH
  );

  public string GodotSymlinkPath {
    get {
      var val = FileClient.Combine(
        FileClient.AppDataDirectory, Defaults.GODOT_PATH, Defaults.GODOT_BIN_PATH, Defaults.GODOT_BIN_NAME
      );
      if (SystemInfo.OS == OSType.Windows) {
        val = $"{val}.exe";
      }
      return val;
    }
  }

  public string GodotSharpSymlinkPath => FileClient.Combine(
    FileClient.AppDataDirectory, Defaults.GODOT_PATH, Defaults.GODOT_BIN_PATH, Defaults.GODOT_SHARP_PATH
  );

  [MemberNotNullWhen(true, nameof(IsGodotSymlinkTargetAvailable))]
  public bool IsGodotSymlinkTargetAvailable => GodotSymlinkTarget is not null;

  public string? GodotSymlinkTarget => FileClient.FileSymlinkTarget(
    GodotSymlinkPath
  );

  // TODO: Rely on platform to provide our file version-name conversion
  public GodotRepository(
    ISystemInfo systemInfo,
    Config config,
    IFileClient fileClient,
    INetworkClient networkClient,
    IZipClient zipClient,
    IGodotEnvironment platform,
    IEnvironmentVariableClient environmentVariableClient,
    IProcessRunner processRunner,
    IGodotChecksumClient checksumClient,
    IVersionDeserializer versionDeserializer,
    IVersionSerializer versionSerializer
  ) {
    SystemInfo = systemInfo;
    Config = config;
    FileClient = fileClient;
    NetworkClient = networkClient;
    ZipClient = zipClient;
    Platform = platform;
    EnvironmentVariableClient = environmentVariableClient;
    ProcessRunner = processRunner;
    ChecksumClient = checksumClient;
    VersionDeserializer = versionDeserializer;
    VersionSerializer = versionSerializer;
  }

  public GodotInstallation? GetInstallation(
    AnyDotnetStatusGodotVersion version
  ) => ReadInstallation(new SpecificDotnetStatusGodotVersion(version.Number, isDotnet: true)) ??
      ReadInstallation(new SpecificDotnetStatusGodotVersion(version.Number, isDotnet: false));

  public GodotInstallation? GetInstallation(
    SpecificDotnetStatusGodotVersion version
  ) => ReadInstallation(version);

  public string InstallationVersionName(GodotInstallation installation) =>
    VersionSerializer.Serialize(installation.Version) +
      (installation.Version.IsDotnetEnabled ? " dotnet" : " not-dotnet");

  public void ClearCache() {
    if (FileClient.DirectoryExists(GodotCachePath)) {
      FileClient.DeleteDirectory(GodotCachePath);
    }
    FileClient.CreateDirectory(GodotCachePath);
  }

  public async Task<GodotCompressedArchive> DownloadGodot(
    SpecificDotnetStatusGodotVersion version,
    bool skipChecksumVerification,
    ILog log,
    CancellationToken token,
    string? proxyUrl = null
  ) {
    log.Info("⬇ Preparing to download Godot...");

    var downloadUrl = Platform.GetDownloadUrl(version, isTemplate: false);

    log.Print($"🌏 Godot download url: {downloadUrl}");

    if (!string.IsNullOrEmpty(proxyUrl)) {
      log.Info($"🔄 Using proxy: {proxyUrl}");
    }

    var fsName = GetVersionFsName(
      Platform.VersionSerializer, version
    );
    // Tux server packages use .zip for everything.
    var cacheDir = FileClient.Combine(GodotCachePath, fsName);
    var cacheFilename = fsName + ".zip";
    var didFinishDownloadFilePath = FileClient.Combine(
      cacheDir, Defaults.DID_FINISH_DOWNLOAD_FILE_NAME
    );

    var compressedArchivePath = FileClient.Combine(cacheDir, cacheFilename);

    var didFinishAnyPreviousDownload = File.Exists(didFinishDownloadFilePath);
    var downloadedFileExists = File.Exists(compressedArchivePath);

    var archive = new GodotCompressedArchive(
      Name: fsName,
      Filename: cacheFilename,
      Version: version,
      Path: cacheDir
    );

    if (downloadedFileExists && didFinishAnyPreviousDownload) {
      log.Print($"📦 Found needed archives in cache: {compressedArchivePath}");
      log.Success("📚 Skipping download.");
      log.Print("");
      log.Print("If you want to force a download to occur,");
      log.Print("use the following command to clear the downloads cache:");
      log.Print("");
      log.Print("    godotenv godot cache clear");
      log.Print("");
      return archive;
    }

    log.Info("🧼 Cleaning up...");
    if (didFinishAnyPreviousDownload) {
      log.Info($"🗑 Deleting {didFinishDownloadFilePath}");
      await FileClient.DeleteFile(didFinishDownloadFilePath);
    }

    if (downloadedFileExists) {
      log.Info($"🗑 Deleting {compressedArchivePath}");
      await FileClient.DeleteFile(compressedArchivePath);
    }
    log.Success("✨ All clean!");

    FileClient.CreateDirectory(cacheDir);

    log.Print($"🗄 Cache path: {cacheDir}");
    log.Print($"📄 Cache filename: {cacheFilename}");
    log.Print($"💾 Compressed installer path: {compressedArchivePath}");

    try {
      if (!string.IsNullOrEmpty(proxyUrl)) {
        // if proxyUrl is set, use HttpClient to download through proxy
        log.Info($"🌐 Using proxy for download: {proxyUrl}");
      }

      await NetworkClient.DownloadFileAsync(
        url: downloadUrl,
        destinationDirectory: cacheDir,
        filename: cacheFilename,
        new Progress<DownloadProgress>(
          (progress) => log.InfoInPlace(
            $"🚀 Downloading Godot: {progress.Percent}% at {progress.Speed}" +
            "      "
          )
        ),
        token: token,
        proxyUrl: proxyUrl
      );
      // Force new line after download progress as the cursor remains in the
      // previous line.
      log.Print("");
      log.ClearCurrentLine();
    }
    catch (Exception) {
      log.ClearCurrentLine();
      log.Err("🛑 Aborting Godot installation.");
      throw;
    }

    if (!skipChecksumVerification) {
      await VerifyArchiveChecksum(log, archive, proxyUrl);
    }
    else {
      log.Info("⚠️ Skipping checksum verification due to command-line flag!");
    }

    FileClient.CreateFile(didFinishDownloadFilePath, "done");

    log.Success("✅ Godot successfully downloaded.");

    return archive;
  }

  private async Task VerifyArchiveChecksum(ILog log, GodotCompressedArchive archive, string? proxyUrl = null) {
    try {
      log.InfoInPlace("⏳ Verifying Checksum.");
      await ChecksumClient.VerifyArchiveChecksum(archive, proxyUrl);
      log.ClearCurrentLine();
      log.Success("✅ Checksum verified.");
    }
    catch (ChecksumMismatchException ex) {
      log.Warn("⚠️ Checksum of downloaded file does not match the one published by Godot!");
      log.Warn($"⚠️ {ex.Message}");
      log.Warn("⚠️ You SHOULD NOT proceed with installation!");
      log.Warn("⚠️ If you have a very good reason, this check can be skipped via '--unsafe-skip-checksum-verification'.");
      log.Err("🛑 Aborting Godot installation.");
      throw;
    }
    catch (MissingChecksumException) {
      log.Warn("⚠️ No Godot-published checksum found for the downloaded file.");
      log.Warn("⚠️ For Godot versions below 3.2.2-beta1, this is expected as none have been published as of 2024-05-01.");
      log.Warn("⚠️ If you still want to proceed with the installation, this check can be skipped via '--unsafe-skip-checksum-verification'.");
      log.Err("🛑 Aborting Godot installation.");
      throw;
    }
  }

  public async Task<GodotInstallation> ExtractGodotInstaller(
    GodotCompressedArchive archive,
    ILog log
  ) {
    var archivePath = FileClient.Combine(archive.Path, archive.Filename);
    var destinationDirName =
      FileClient.Combine(GodotInstallationsPath, archive.Name);

    var numFilesExtracted = await ZipClient.ExtractToDirectory(
      archivePath,
      destinationDirName,
      new Progress<double>((percent) => {
        var p = Math.Round(percent * 100);
        log.InfoInPlace($"🗜 Extracting Godot: {p}%" + "    ");
      })
    );
    log.Print(""); // New line after progress.
    log.ClearCurrentLine();
    log.Print($"    Destination: {destinationDirName}");
    log.Success($"✅ Extracted {numFilesExtracted} file(s).");
    log.Print("");

    var location = new GodotInstallationLocation(
      archive.Name, destinationDirName
    );
    var execPath = GetExecutionPath(
      installationPath: location.InstallationDirectory,
      version: archive.Version
    );

    return new GodotInstallation(
      Location: location,
      IsActiveVersion: true, // we always switch to the newly installed version.
      Version: archive.Version,
      ExecutionPath: execPath
    );
  }

  public async Task UpdateGodotSymlink(
    GodotInstallation installation, ILog log
  ) {
    if (FileClient.IsFileSymlink(GodotBinPath)) {
      // Removes old 'bin' file-symlink.
      await FileClient.DeleteFile(GodotBinPath);
    }

    if (!FileClient.DirectoryExists(GodotBinPath)) {
      FileClient.CreateDirectory(GodotBinPath);
    }

    log.Info("📝 Updating Godot symlink.");
    // Create or update the symlink to the new version of Godot.
    switch (SystemInfo.OS) {
      case OSType.Linux:
      case OSType.MacOS:
      case OSType.Windows:
        await FileClient.CreateSymlink(
          GodotSymlinkPath, installation.ExecutionPath
        );
        break;
      case OSType.Unknown:
      default:
        break;
    }

    if (installation.Version.IsDotnetEnabled) {
      // Update GodotSharp symlinks
      var godotSharpPath = GetGodotSharpPath(
        installation.Location.InstallationDirectory, installation.Version
      );
      await FileClient.CreateSymlink(
        GodotSharpSymlinkPath, godotSharpPath
      );
    }

    if (!FileClient.FileExists(installation.ExecutionPath)) {
      log.Err("🛑 Execution path does not seem to be correct. Is it good?");
      log.Err("Please help me fix it by opening an issue or pull request on Github!");
    }

    log.Print($"    🔗 Godot link: {GodotSymlinkPath} -> {installation.ExecutionPath}");
    log.Success("✅ Godot symlink updated.");
    log.Print("");
  }

  public async Task UpdateDesktopShortcut(
    GodotInstallation installation, ILog log
  ) {
    log.Info("📝 Updating Godot desktop shortcut.");
    switch (SystemInfo.OS) {
      case OSType.MacOS: {
          var appFilePath = FileClient.Files.Directory.GetDirectories(
            installation.Location.InstallationDirectory
          ).First();
          var applicationsPath = FileClient.Combine(
            FileClient.UserDirectory, "Applications", "Godot.app"
          );
          await FileClient.DeleteDirectory(applicationsPath);
          await FileClient.CreateSymlinkRecursively(applicationsPath, appFilePath);
          break;
        }

      case OSType.Linux:
        var userApplicationsPath = FileClient.Combine(
          FileClient.UserDirectory, ".local", "share", "applications"
        );
        var userIconsPath = FileClient.Combine(
          FileClient.UserDirectory, ".local", "share", "icons"
        );

        FileClient.CreateDirectory(userApplicationsPath);
        FileClient.CreateDirectory(userIconsPath);

        await NetworkClient.DownloadFileAsync(
          url: "https://godotengine.org/assets/press/icon_color.png",
          destinationDirectory: userIconsPath,
          filename: "godot.png",
          CancellationToken.None);

        // https://github.com/godotengine/godot/blob/master/misc/dist/linux/org.godotengine.Godot.desktop
        FileClient.CreateFile(
          FileClient.Combine(userApplicationsPath, "Godot.desktop"),
          $"""
           [Desktop Entry]
           Name=Godot Engine
           GenericName=Libre game engine
           GenericName[el]=Ελεύθερη μηχανή παιχνιδιού
           GenericName[fr]=Moteur de jeu libre
           GenericName[zh_CN]=自由的游戏引擎
           Comment=Multi-platform 2D and 3D game engine with a feature-rich editor
           Comment[el]=2D και 3D μηχανή παιχνιδιού πολλαπλών πλατφορμών με επεξεργαστή πλούσιο σε χαρακτηριστικά
           Comment[fr]=Moteur de jeu 2D et 3D multiplateforme avec un éditeur riche en fonctionnalités
           Comment[zh_CN]=多平台 2D 和 3D 游戏引擎，带有功能丰富的编辑器
           Exec={GodotSymlinkPath} %f
           Icon=godot
           Terminal=false
           PrefersNonDefaultGPU=true
           Type=Application
           MimeType=application/x-godot-project;
           Categories=Development;IDE;
           StartupWMClass=Godot
           """
        );
        break;

      case OSType.Windows: {
          var linkPath = GodotSymlinkPath;
          var targetPath = FileClient.FileSymlinkTarget(linkPath);
          var commonStartMenuPath = Environment.GetFolderPath(
            Environment.SpecialFolder.StartMenu
          );
          var applicationsPath = FileClient.Combine(
            commonStartMenuPath, "Programs", "Godot.lnk"
          );

          var command = string.Join(";",
            "$ws = New-Object -ComObject (\"WScript.Shell\")",
            $"$s = $ws.CreateShortcut(\"{applicationsPath}\")",
            $"$s.IconLocation = \"{targetPath}, 0\"",
            $"$s.TargetPath = \"{linkPath}\"",
            "$s.save();"
          );
          var task = FileClient.ProcessRunner.Run(
            ".", "powershell", ["-c", command]
          );
          await task;
          if (!string.IsNullOrEmpty(task.Result.StandardError)) {
            log.Warn("Errors or warnings in shortcut creation:");
            using var reader = new StringReader(task.Result.StandardError);
            while (true) {
              var line = reader.ReadLine();
              if (line is null) {
                break;
              }
              log.Warn($"  {line}");
            }
          }
          break;
        }
      case OSType.Unknown:
      default:
        break;
    }

    log.Success("✅ Godot desktop shortcut created.");
    log.Print("");
  }

  public async Task AddOrUpdateGodotEnvVariable(ILog log) {
    log.Info("📝 Updating GodotEnv environment variables.");

    await EnvironmentVariableClient.UpdateGodotEnvEnvironment(GodotSymlinkPath, GodotBinPath);

    log.Success("✅ Success.");
    log.Print("");
    log.Warn("Please, restart your shell to update the environment variables.");
    log.Print("");

    if (SystemInfo.OSFamily == OSFamily.Unix) {
      log.Warn(
        $"""
           GodotEnv patches the shell initialization files and POSIX compatible shells should work
           out of the box (bash, zsh). You may need to manually export the GODOT env-var and update PATH
           if your shell isn't a POSIX compatible one (i.e., fish). Take a look into
           '{FileClient.Combine(FileClient.AppDataDirectory, "env")}' file for inspiration.
         """);
      log.Print("");
    }
  }

  public async Task<string> GetGodotEnvVariable() =>
    await EnvironmentVariableClient.GetUserEnv(Defaults.GODOT_ENV_VAR_NAME);

  public List<Result<GodotInstallation>> GetInstallationsList() {
    var results = new List<Result<GodotInstallation>>();
    var successes = new List<GodotInstallation>();
    var failures = new List<string>();

    if (!FileClient.DirectoryExists(GodotInstallationsPath)) {
      results.Add(Result.Failure<GodotInstallation>(
        null, "Godot installation directory does not exist"
      ));
      return results;
    }

    foreach (var dir in FileClient.GetSubdirectories(GodotInstallationsPath)) {
      var version = DirectoryToVersion(dir.Name);
      if (version is null) {
        failures.Add(
          $"Unrecognized subfolder in Godot installation directory (may be a non-conforming version identifier): {dir.Name}"
        );
      }
      else {
        var installation = GetInstallation(version);
        if (installation is null) {
          failures.Add(
            $"Installation directory matches Godot version but failed to load: {dir.Name}"
          );
        }
        else {
          successes.Add(installation);
        }
      }
    }
    // order failures after successes
    results = [..
      (
        from s in successes.OrderBy(InstallationVersionName)
        select Result.Success(s)
      ).Concat(
        from f in failures.Order()
        select Result.Failure<GodotInstallation>(null, f)
      )];
    return results;
  }

  public async Task<List<string>> GetRemoteVersionsList(ILog log, string? proxyUrl = null) {
    if (proxyUrl is not null) {
      log.Info($"🔄 Using proxy: {proxyUrl}");
    }
    var response = await NetworkClient.WebRequestGetAsync(
      GODOT_REMOTE_VERSIONS_URL, true, proxyUrl
      );
    response.EnsureSuccessStatusCode();

    var responseBody = await response.Content.ReadAsStringAsync();
    var deserializedBody =
      JsonSerializer.Deserialize<List<RemoteVersion>>(responseBody);
    deserializedBody?.Reverse();

    var versions = new List<string>();
    // format version name
    for (var i = 0; i < deserializedBody?.Count; i++) {
      var remoteVersion = deserializedBody[i];
      remoteVersion.Name =
        remoteVersion.Name.Replace("godot-", "").Replace(".json", "");

      // limit versions to godot 3 and above
      if (remoteVersion.Name[0] == '2') {
        break;
      }

      try {
        // Version strings coming from remote will (mostly) be in release style
        var version = new ReleaseVersionDeserializer().Deserialize(
          remoteVersion.Name
          );
        // Output in our preferred format
        // so the user has a consistent picture of versioning
        versions.Add(VersionSerializer.Serialize(version));
      }
      // Discard remote versions that aren't canonical,
      // like "3.2-alpha0-unofficial"
      catch (ArgumentException) { }
    }

    return versions;
  }

  public async Task<bool> Uninstall(
    SpecificDotnetStatusGodotVersion version, ILog log
  ) {
    var potentialInstallation = GetInstallation(version);

    if (potentialInstallation is not GodotInstallation installation) {
      return false;
    }

    await FileClient.DeleteDirectory(
      installation.Location.InstallationDirectory
    );

    if (installation.IsActiveVersion) {
      // Remove symlink if we're deleting the active version.
      await FileClient.DeleteFile(GodotSymlinkPath);
      log.Print("");
      log.Warn("Removed the active version of Godot — your GODOT environment");
      log.Warn("may still be pointing to a non-existent symlink.");
      log.Print("");
      log.Warn("Please consider switching to a different version to");
      log.Warn("reconstruct the proper symlinks.");
      log.Print("");
      log.Warn("    godotenv godot use <version>");
      log.Print("");
    }

    return true;
  }

  private string GetExecutionPath(
    string installationPath, SpecificDotnetStatusGodotVersion version
  ) =>
  FileClient.Combine(
    installationPath,
    Platform.GetRelativeExtractedExecutablePath(version)
  );

  private string GetGodotSharpPath(
    string installationPath, SpecificDotnetStatusGodotVersion version
  ) => FileClient.Combine(
    installationPath,
    Platform.GetRelativeGodotSharpPath(version)
  );

  private GodotInstallation? ReadInstallation(
    SpecificDotnetStatusGodotVersion version
  ) {
    var canonicalName = GetVersionFsName(
      Platform.VersionSerializer, version
    );

    var installationLocation = GetInstallationLocation(version);
    if (installationLocation is null) { return null; }

    var executionPath = GetExecutionPath(
      installationPath: installationLocation.InstallationDirectory,
      version: version
    );

    return new GodotInstallation(
      Location: installationLocation,
      IsActiveVersion: GodotSymlinkTarget == executionPath,
      Version: version,
      ExecutionPath: executionPath
    );
  }

  private GodotInstallationLocation? GetInstallationLocation(
    SpecificDotnetStatusGodotVersion version
  ) {
    var directoryName = GetVersionFsName(
      Platform.VersionSerializer, version
    );
    var installationDir = FileClient.Combine(
      GodotInstallationsPath, directoryName
    );
    if (FileClient.DirectoryExists(installationDir)) {
      return new GodotInstallationLocation(directoryName, installationDir);
    }
    return GetOldInstallationLocation(version);
  }

  private GodotInstallationLocation? GetOldInstallationLocation(
    SpecificDotnetStatusGodotVersion version
  ) {
    bool[] flagVals = [true, false];
    var serializers =
      from labelFlag in flagVals
      from patchFlag in flagVals
      from stableFlag in flagVals
      select new OldDiskVersionSerializer(labelFlag, patchFlag, stableFlag);
    foreach (var serializer in serializers) {
      var directoryName = GetVersionFsName(
        serializer, version
      );
      var installationDir = FileClient.Combine(
        GodotInstallationsPath, directoryName
      );
      if (FileClient.DirectoryExists(installationDir)) {
        return new GodotInstallationLocation(directoryName, installationDir);
      }
    }
    return null;
  }

  internal string GetVersionFsName(
    IVersionSerializer versionSerializer,
    SpecificDotnetStatusGodotVersion version
  ) =>
    $"godot_{(version.IsDotnetEnabled ? "dotnet_" : "")}" +
    FileClient.Sanitize(versionSerializer.Serialize(version))
      .Replace(".", "_")
      .Replace("-", "_");

  internal SpecificDotnetStatusGodotVersion? DirectoryToVersion(
    string directory
  ) {
    var versionParts = DirectoryToVersionStringRegex().Match(directory);
    if (!versionParts.Success) {
      return null;
    }

    var major = int.Parse(versionParts.Groups["major"].Value);
    var minor = int.Parse(versionParts.Groups["minor"].Value);
    var patch = versionParts.Groups["patch"].Value.Length > 0 ? int.Parse(versionParts.Groups["patch"].Value[..^1]) : 0;
    var label = versionParts.Groups["label"].Value.Length > 0 ? versionParts.Groups["labelName"].Value : "stable";
    var labelNumber = versionParts.Groups["labelNumber"].Value.Length > 0 ? int.Parse(versionParts.Groups["labelNumber"].Value) : -1;

    var isDotnet = directory.Contains("dotnet");
    return new SpecificDotnetStatusGodotVersion(major, minor, patch, label, labelNumber, isDotnet);
  }

  // Regex for converting directory names back into version strings to see
  // what versions we have installed.
  // Ideally, all version strings on disk would be in release format, but old
  // installs may have a mix of release and GodotSharp format (e.g., "4.4.0-dev6")
  [GeneratedRegex(@"godot_(dotnet_)?(?<major>\d+)_(?<minor>\d+)_(?<patch>\d+_)?(?<label>(?<labelName>[a-z]+)(?<labelNumber>\d*))?")]
  private static partial Regex DirectoryToVersionStringRegex();
}
