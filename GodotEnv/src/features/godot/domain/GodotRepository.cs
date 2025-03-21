namespace Chickensoft.GodotEnv.Features.Godot.Domain;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Common.Utilities;
using Chickensoft.GodotEnv.Features.Godot.Models;
using CliFx.Exceptions;
using global::GodotEnv.Common.Utilities;
using Newtonsoft.Json;

public struct RemoteVersion {
  public string Name { get; set; }
}

public interface IGodotRepository {
  ISystemInfo SystemInfo { get; }
  ConfigFile Config { get; }
  IFileClient FileClient { get; }
  INetworkClient NetworkClient { get; }
  IZipClient ZipClient { get; }
  IEnvironmentVariableClient EnvironmentVariableClient { get; }
  IGodotEnvironment Platform { get; }
  IProcessRunner ProcessRunner { get; }
  IVersionStringConverter VersionStringConverter { get; }
  string GodotInstallationsPath { get; }
  string GodotCachePath { get; }
  string GodotSymlinkPath { get; }
  string GodotSymlinkTarget { get; }
  string GodotSharpSymlinkPath { get; }

  /// <summary>
  /// Clears the Godot installations cache and recreates the cache directory.
  /// </summary>
  void ClearCache();

  /// <summary>
  /// Gets the installation associated with the specified version of Godot.
  /// If both the .NET-enabled and the non-.NET-enabled versions of Godot with
  /// the same version are installed, this returns the .NET-enabled version.
  /// </summary>
  /// <param name="version">Godot version.</param>
  /// <param name="isDotnetVersion">True to search for an installed
  /// .NET-enabled version of Godot. False to search for an installed non-.NET
  /// version of Godot. Null to search for either.</param>
  /// <returns>Godot installation, or null if none found.</returns>
  GodotInstallation? GetInstallation(
    GodotVersion version, bool? isDotnetVersion = null
  );

  /// <summary>
  /// Name shown when listing Godot versions installed.
  /// </summary>
  /// <param name="installation">Installation whose version will be shown.</param>
  string InstallationVersionName(GodotInstallation installation);

  /// <summary>
  /// Downloads the specified version of Godot.
  /// </summary>
  /// <param name="version">Godot version.</param>
  /// <param name="isDotnetVersion">True to download the .NET version.</param>
  /// <param name="skipChecksumVerification">True if checksum verification should be skipped</param>
  /// <param name="log">Output log.</param>
  /// <param name="token">Cancellation token.</param>
  /// <param name="proxyUrl">Optional proxy URL.</param>
  /// <returns>The fully resolved / absolute path of the Godot installation zip
  /// file for the Platform.</returns>
  Task<GodotCompressedArchive> DownloadGodot(
      GodotVersion version,
      bool isDotnetVersion,
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
  Task<GodotInstallation> ExtractGodotInstaller(
    GodotCompressedArchive archive, ILog log
  );

  /// <summary>
  /// Updates the symlink to point to the specified Godot installation.
  /// </summary>
  /// <param name="installation">Godot installation.</param>
  /// <param name="log">Output log.</param>
  Task UpdateGodotSymlink(GodotInstallation installation, ILog log);

  /// <summary>
  /// <para>Updates (or creates if non-existent) the desktop shortcut pointing to the newly created symlink.</para>
  /// <para>Promotes integration with the desktop environment.</para>
  /// </summary>
  /// <param name="installation">Godot installation.</param>
  /// <param name="log">Output log.</param>
  Task UpdateDesktopShortcut(GodotInstallation installation, ILog log);

  /// <summary>
  /// Adds (or updates) the GODOT user environment variable to point to the
  /// symlink which points to the active version of Godot. Updates the user's PATH
  /// to include the 'bin' folder containing the godot symlink.
  /// </summary>
  /// <param name="log">Output log.</param>
  /// <returns>Completion task.</returns>
  Task AddOrUpdateGodotEnvVariable(ILog log);

  /// <summary>
  /// Gets the GODOT user environment variable.
  /// </summary>
  /// <returns>GODOT user environment variable value.</returns>
  Task<string> GetGodotEnvVariable();

  /// <summary>
  /// Get the list of installed Godot versions.
  /// </summary>
  /// <param name="installations">The list of installed versions.</param>
  /// <param name="unrecognizedDirectories">
  /// A list of subdirectories in the installation directory that were not
  /// recognized as Godot versions.
  /// </param>
  /// <param name="failedGodotInstallations">
  /// A list of subdirectories in the installation directory that matched
  /// Godot versions, but could not be loaded as installations.
  /// </param>
  void GetInstallationsList(out List<GodotInstallation> installations,
                            out List<string> unrecognizedDirectories,
                            out List<string> failedGodotInstallations);

  /// <summary>
  /// Get the list of available Godot versions.
  /// </summary>
  /// <returns></returns>
  Task<List<string>> GetRemoteVersionsList();

  /// <summary>
  /// Uninstalls the specified version of Godot.
  /// </summary>
  /// <param name="version">Godot version.</param>
  /// <param name="isDotnetVersion">True to uninstall the .NET version.</param>
  /// <param name="log">Output log.</param>
  /// <returns>True if successful, false if installation doesn't exist.
  /// </returns>
  Task<bool> Uninstall(
    GodotVersion version, bool isDotnetVersion, ILog log
  );
}

public partial class GodotRepository : IGodotRepository {
  public ISystemInfo SystemInfo { get; }
  public ConfigFile Config { get; }
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

  public IVersionStringConverter VersionStringConverter { get; }

  public string GodotInstallationsPath => FileClient.Combine(
    FileClient.AppDataDirectory,
    Defaults.GODOT_PATH,
    Config.GodotInstallationsPath
  );

  public string GodotCachePath => FileClient.Combine(
    FileClient.AppDataDirectory, Defaults.GODOT_PATH, Defaults.GODOT_CACHE_PATH
  );

  public string GodotBinPath => FileClient.Combine(
    FileClient.AppDataDirectory, Defaults.GODOT_PATH, Defaults.GODOT_BIN_PATH
  );

  public string GodotSymlinkPath => FileClient.Combine(
    FileClient.AppDataDirectory, Defaults.GODOT_PATH, Defaults.GODOT_BIN_PATH, Defaults.GODOT_BIN_NAME
  );

  public string GodotSharpSymlinkPath => FileClient.Combine(
    FileClient.AppDataDirectory, Defaults.GODOT_PATH, Defaults.GODOT_BIN_PATH, Defaults.GODOT_SHARP_PATH
  );

  public string GodotSymlinkTarget => FileClient.FileSymlinkTarget(
    GodotSymlinkPath
  );

  // TODO: Rely on platform to provide our file version-name conversion
  public GodotRepository(
    ISystemInfo systemInfo,
    ConfigFile config,
    IFileClient fileClient,
    INetworkClient networkClient,
    IZipClient zipClient,
    IGodotEnvironment platform,
    IEnvironmentVariableClient environmentVariableClient,
    IProcessRunner processRunner,
    IGodotChecksumClient checksumClient,
    IVersionStringConverter versionStringConverter
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
    VersionStringConverter = versionStringConverter;
  }

  public GodotInstallation? GetInstallation(
    GodotVersion version, bool? isDotnetVersion = null
  ) {
    if (isDotnetVersion is bool isDotnet) {
      return ReadInstallation(version, isDotnet);
    }

    return ReadInstallation(version, isDotnetVersion: true) ??
      ReadInstallation(version, isDotnetVersion: false);
  }

  public string InstallationVersionName(GodotInstallation installation) =>
    VersionStringConverter.VersionString(installation.Version) +
      (installation.IsDotnetVersion ? " dotnet" : " not-dotnet");

  public void ClearCache() {
    if (FileClient.DirectoryExists(GodotCachePath)) {
      FileClient.DeleteDirectory(GodotCachePath);
    }
    FileClient.CreateDirectory(GodotCachePath);
  }

  public async Task<GodotCompressedArchive> DownloadGodot(
    GodotVersion version,
    bool isDotnetVersion,
    bool skipChecksumVerification,
    ILog log,
    CancellationToken token,
    string? proxyUrl = null
  ) {
    log.Info("⬇ Preparing to download Godot...");

    var downloadUrl = Platform.GetDownloadUrl(
      version, isDotnetVersion, isTemplate: false
    );

    log.Print($"🌏 Godot download url: {downloadUrl}");

    if (!string.IsNullOrEmpty(proxyUrl)) {
      log.Info($"🔄 Using proxy: {proxyUrl}");
    }

    var fsName = GetVersionFsName(version, isDotnetVersion);
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
      IsDotnetVersion: isDotnetVersion,
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
      if (string.IsNullOrEmpty(proxyUrl)) {
        // use no proxy
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
          token: token
        );
      }
      else {
        // use proxy to download
        log.Info($"🌐 Using proxy for download: {proxyUrl}");

        // use System.Net.WebClient to download via proxy
        using (var webClient = new System.Net.WebClient()) {
          // initialize proxy
          webClient.Proxy = new System.Net.WebProxy(proxyUrl);

          // download progress
          webClient.DownloadProgressChanged += (sender, e) => {
            log.InfoInPlace(
              $"🚀 Downloading Godot via proxy: {e.ProgressPercentage}%" +
              "      "
            );
          };

          // download completed
          var completedTask = new TaskCompletionSource<bool>();
          webClient.DownloadFileCompleted += (sender, e) => {
            if (e.Cancelled) {
              completedTask.SetException(new CommandException("Download cancelled!"));
            }
            else if (e.Error != null) {
              completedTask.SetException(e.Error);
            }
            else {
              completedTask.SetResult(true);
            }
          };

          token.Register(() => {
            webClient.CancelAsync();
          });

          // start downloading
          webClient.DownloadFileAsync(
            new Uri(downloadUrl),
            compressedArchivePath
          );

          // wait for download to complete
          await completedTask.Task;
        }
      }

      log.Print("");  // Force new line after download progress as the cursor remains in the previously line.
      log.ClearCurrentLine();
    }
    catch (Exception) {
      log.ClearCurrentLine();
      log.Err("🛑 Aborting Godot installation.");
      throw;
    }

    if (!skipChecksumVerification) {
      await VerifyArchiveChecksum(log, archive);
    }
    else {
      log.Info("⚠️ Skipping checksum verification due to command-line flag!");
    }

    FileClient.CreateFile(didFinishDownloadFilePath, "done");

    log.Success("✅ Godot successfully downloaded.");

    return archive;
  }

  private async Task VerifyArchiveChecksum(ILog log, GodotCompressedArchive archive) {
    try {
      log.InfoInPlace("⏳ Verifying Checksum.");
      await ChecksumClient.VerifyArchiveChecksum(archive);
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

    var execPath = GetExecutionPath(
      installationPath: destinationDirName,
      version: archive.Version,
      isDotnetVersion: archive.IsDotnetVersion
    );

    return new GodotInstallation(
      Name: archive.Name,
      IsActiveVersion: true, // we always switch to the newly installed version.
      Version: archive.Version,
      IsDotnetVersion: archive.IsDotnetVersion,
      Path: destinationDirName,
      ExecutionPath: execPath
    );
  }

  public async Task UpdateGodotSymlink(
    GodotInstallation installation, ILog log
  ) {
    if (FileClient.IsFileSymlink(GodotBinPath)) {  // Removes old 'bin' file-symlink.
      await FileClient.DeleteFile(GodotBinPath);
    }

    if (!FileClient.DirectoryExists(GodotBinPath)) {
      FileClient.CreateDirectory(GodotBinPath);
    }

    log.Info("📝 Updating Godot symlink.");
    // log.Print($"    Linking Godot: {GodotSymlinkPath} -> {installation.ExecutionPath}");
    // Create or update the symlink to the new version of Godot.
    switch (SystemInfo.OS) {
      case OSType.Linux:
      case OSType.MacOS:
        await FileClient.CreateSymlink(GodotSymlinkPath, installation.ExecutionPath);
        break;
      // NOTE: Windows demands the file extension to be in the name.
      case OSType.Windows: {
          var hardLinkPath = $"{GodotSymlinkPath}.exe";
          await FileClient.CreateSymlink(hardLinkPath, installation.ExecutionPath);
        }
        break;
      case OSType.Unknown:
      default:
        break;
    }

    if (installation.IsDotnetVersion) {
      // Update GodotSharp symlinks
      var godotSharpPath = GetGodotSharpPath(
        installation.Path, installation.Version, installation.IsDotnetVersion
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

  public async Task UpdateDesktopShortcut(GodotInstallation installation, ILog log) {
    log.Info("📝 Updating Godot desktop shortcut.");
    switch (SystemInfo.OS) {
      case OSType.MacOS: {
          var appFilePath = FileClient.Files.Directory.GetDirectories(installation.Path).First();
          var applicationsPath = FileClient.Combine(FileClient.UserDirectory, "Applications", "Godot.app");
          await FileClient.DeleteDirectory(applicationsPath);
          await FileClient.CreateSymlinkRecursively(applicationsPath, appFilePath);
          break;
        }

      case OSType.Linux:
        var userApplicationsPath = FileClient.Combine(FileClient.UserDirectory, ".local", "share", "applications");
        var userIconsPath = FileClient.Combine(FileClient.UserDirectory, ".local", "share", "icons");

        FileClient.CreateDirectory(userApplicationsPath);
        FileClient.CreateDirectory(userIconsPath);

        await NetworkClient.DownloadFileAsync(
          url: "https://godotengine.org/assets/press/icon_color.png",
          destinationDirectory: userIconsPath,
          filename: "godot.png",
          CancellationToken.None);

        // https://github.com/godotengine/godot/blob/master/misc/dist/linux/org.godotengine.Godot.desktop
        FileClient.CreateFile(FileClient.Combine(userApplicationsPath, "Godot.desktop"),
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
           """);
        break;

      case OSType.Windows: {
          var hardLinkPath = $"{GodotSymlinkPath}.exe";
          var commonStartMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
          var applicationsPath = FileClient.Combine(commonStartMenuPath, "Programs", "Godot.lnk");

          var command = string.Join(";",
            "$ws = New-Object -ComObject (\"WScript.Shell\")",
            $"$s = $ws.CreateShortcut(\"{applicationsPath}\")",
            $"$s.TargetPath = \"{hardLinkPath}\"",
            "$s.save();"
          );
          await FileClient.ProcessRunner.Run(".", "powershell", ["-c", command]);
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

  public async Task<string> GetGodotEnvVariable() => await EnvironmentVariableClient.GetUserEnv(Defaults.GODOT_ENV_VAR_NAME);

  public void GetInstallationsList(
    out List<GodotInstallation> installations,
    out List<string> unrecognizedDirectories,
    out List<string> failedGodotInstallations
  ) {
    installations = [];
    unrecognizedDirectories = [];
    failedGodotInstallations = [];

    if (!FileClient.DirectoryExists(GodotInstallationsPath)) {
      return;
    }

    foreach (var dir in FileClient.GetSubdirectories(GodotInstallationsPath)) {
      DirectoryToVersion(dir.Name, out var version, out var isDotnetVersion);
      if (version is null) {
        unrecognizedDirectories.Add(dir.Name);
      }
      else {
        var installation = GetInstallation(version, isDotnetVersion);
        if (installation is null) {
          failedGodotInstallations.Add(dir.Name);
        }
        else {
          installations.Add(installation);
        }
      }
    }

    installations = [.. installations.OrderBy(InstallationVersionName)];
    unrecognizedDirectories = [.. unrecognizedDirectories.Order()];
    failedGodotInstallations = [.. failedGodotInstallations.Order()];
  }

  public async Task<List<string>> GetRemoteVersionsList() {
    var response = await NetworkClient.WebRequestGetAsync(GODOT_REMOTE_VERSIONS_URL, true);
    response.EnsureSuccessStatusCode();

    var responseBody = await response.Content.ReadAsStringAsync();
    var deserializedBody = JsonConvert.DeserializeObject<List<RemoteVersion>>(responseBody);
    deserializedBody?.Reverse();

    var versions = new List<string>();
    // format version name
    for (var i = 0; i < deserializedBody?.Count; i++) {
      var deserializedVersion = deserializedBody[i];
      deserializedVersion.Name = deserializedVersion.Name.Replace("godot-", "").Replace(".json", "");

      // limit versions to godot 3 and above
      if (deserializedVersion.Name[0] == '2') {
        break;
      }

      try {
        // Version strings coming from remote will (mostly) be in release style
        var version = new ReleaseVersionStringConverter().ParseVersion(deserializedVersion.Name);
        // Output in our preferred format so the user has a consistent picture of versioning
        versions.Add(VersionStringConverter.VersionString(version));
      }
      // Discard remote versions that aren't canonical, like "3.2-alpha0-unofficial"
      catch (ArgumentException) { }
    }

    return versions;
  }

  public async Task<bool> Uninstall(
    GodotVersion version, bool isDotnetVersion, ILog log
  ) {
    var potentialInstallation = GetInstallation(version, isDotnetVersion);

    if (potentialInstallation is not GodotInstallation installation) {
      return false;
    }

    await FileClient.DeleteDirectory(installation.Path);

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
    string installationPath, GodotVersion version, bool isDotnetVersion
  ) =>
  FileClient.Combine(
    installationPath,
    Platform.GetRelativeExtractedExecutablePath(version, isDotnetVersion)
  );

  private string GetGodotSharpPath(
    string installationPath, GodotVersion version, bool isDotnetVersion
  ) => FileClient.Combine(
    installationPath,
    Platform.GetRelativeGodotSharpPath(version, isDotnetVersion)
  );

  private GodotInstallation? ReadInstallation(
    GodotVersion version, bool isDotnetVersion
  ) {
    var directoryName = GetVersionFsName(version, isDotnetVersion);
    var symlinkTarget = GodotSymlinkTarget;
    var installationDir = FileClient.Combine(
      GodotInstallationsPath, directoryName
    );

    if (!FileClient.DirectoryExists(installationDir)) { return null; }

    var executionPath = GetExecutionPath(
      installationPath: installationDir,
      version: version,
      isDotnetVersion: isDotnetVersion
    );

    return new GodotInstallation(
      Name: directoryName,
      IsActiveVersion: symlinkTarget == executionPath,
      Version: version,
      IsDotnetVersion: isDotnetVersion,
      Path: installationDir,
      ExecutionPath: executionPath
    );
  }

  internal string GetVersionFsName(
    GodotVersion version, bool isDotnetVersion
  ) =>
    $"godot_{(isDotnetVersion ? "dotnet_" : "")}" +
      FileClient.Sanitize(Platform.VersionStringConverter.VersionString(version))
        .Replace(".", "_")
        .Replace("-", "_");

  internal void DirectoryToVersion(
    string directory,
    out GodotVersion? version,
    out bool isDotnet
  ) {
    var versionParts = DirectoryToVersionStringRegex().Match(directory);
    if (!versionParts.Success) {
      version = null;
      isDotnet = false;
      return;
    }

    var versionString = $"{versionParts.Groups["major"].Value}." +
      $"{versionParts.Groups["minor"].Value}";
    if (versionParts.Groups["patch"].Value.Length > 0) {
      versionString += $".{versionParts.Groups["patch"].Value[..^1]}";
    }

    var label = versionParts.Groups["label"].Value;
    if (label.Length == 0) {
      label = "stable";
    }
    versionString += $"-{label.Replace("_", ".")}";

    isDotnet = directory.Contains("dotnet");
    version = Platform.VersionStringConverter.ParseVersion(versionString);
  }

  // Regex for converting directory names back into version strings to see
  // what versions we have installed.
  [GeneratedRegex(@"godot_(dotnet_)?(?<major>\d+)_(?<minor>\d+)_(?<patch>\d+_)?(?<label>[a-z]+[\d]+)?")]
  private static partial Regex DirectoryToVersionStringRegex();
}
