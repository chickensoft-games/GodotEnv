namespace Chickensoft.GodotEnv.Features.Godot.Domain;

using System;
using System.Collections.Generic;
using System.Linq;
using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Common.Utilities;
using Chickensoft.GodotEnv.Features.Godot.Models;
using Chickensoft.GodotEnv.Features.Godot.Serializers;

public interface IGodotVersionSpecifierRepository {
  public string WorkingDir { get; set; }
  public IFileClient FileClient { get; set; }
  /// <summary>
  /// Version serializer used for log messages (not version-specifier files).
  /// </summary>
  public IVersionSerializer VersionSerializer { get; set; }

  public SpecificDotnetStatusGodotVersion? InferVersion(
    IEnumerable<IGodotVersionFile> godotVersionFiles,
    ILog log
  );
  public SpecificDotnetStatusGodotVersion? GetValidatedGodotVersion(
    IGodotVersionFile file,
    ILog log
  );
  public IEnumerable<IGodotVersionFile> GetVersionFiles();

  /// <summary>
  /// Find the highest directory from the working directory that contains a
  /// ".sln" file (if one exists), or the highest directory from the working
  /// directory that contains a "project.godot" file (if no ".sln" file exists).
  /// </summary>
  /// <returns>
  /// The full path to the highest directory up the directory tree containing a
  /// ".sln" file, or the full path to the highest directory up the directory
  /// tree containing a "project.godot" file if no directory in the hierarchy
  /// contains a ".sln" file, or the empty string if no directory in the
  /// hierarchy contains a ".sln" or "project.godot" file.
  /// </returns>
  public string GetProjectDefinitionDirectory();

  /// <summary>
  /// Write a version-specifier file into the given project directory,
  /// indicating the provided Godot version as the preferred version for the
  /// project.
  ///
  /// The file used will be global.json for a .NET Godot version, or .godotrc
  /// for a non-.NET Godot version.
  /// </summary>
  /// <param name="version">
  /// The Godot version to write to a version-specifying file.
  /// </param>
  /// <param name="projectDir">
  /// The project directory in which to write a version-specifying file.
  /// </param>
  /// <param name="log">Log object for messaging.</param>
  public void PinVersion(
    SpecificDotnetStatusGodotVersion version,
    string projectDir,
    ILog log
  );
}

public class GodotVersionSpecifierRepository : IGodotVersionSpecifierRepository {
  public string WorkingDir { get; set; }
  public IFileClient FileClient { get; set; }
  /// <inheritdoc/>
  public IVersionSerializer VersionSerializer { get; set; }

  public GodotVersionSpecifierRepository(
    string workingDir,
    IFileClient fileClient
  ) {
    WorkingDir = workingDir;
    FileClient = fileClient;
    VersionSerializer = new IoVersionSerializer();
  }

  public SpecificDotnetStatusGodotVersion? InferVersion(
    IEnumerable<IGodotVersionFile> godotVersionFiles,
    ILog log
  ) {
    foreach (var godotVersionFile in godotVersionFiles) {
      var version = GetValidatedGodotVersion(godotVersionFile, log);
      if (version is not null) {
        log.Info($"Retrieved version from {godotVersionFile.FilePath}.");
        return version;
      }
    }
    return null;
  }

  public SpecificDotnetStatusGodotVersion? GetValidatedGodotVersion(
    IGodotVersionFile file,
    ILog log
  ) {
    try {
      return file.ParseGodotVersion(FileClient);
    }
    catch (Exception e) {
      log.Warn($"{file.FilePath} contains invalid version string; skipping");
      log.Warn(e.Message);
      return null;
    }
  }

  public IEnumerable<IGodotVersionFile> GetVersionFiles() {
    List<GlobalJsonFile> globalJsonFiles = [];
    List<CsprojFile> csprojFiles = [];
    List<GodotrcFile> godotrcFiles = [];
    foreach (var directory in FileClient.GetAncestorDirectories(WorkingDir)) {
      var globalJsonPath = FileClient.Combine(directory.FullName, "global.json");
      if (FileClient.FileExists(globalJsonPath)) {
        globalJsonFiles.Add(new GlobalJsonFile(globalJsonPath));
      }
      foreach (var csprojPath
        in FileClient.GetFiles(directory.FullName, "*.csproj")
      ) {
        csprojFiles.Add(new CsprojFile(csprojPath));
      }
      var godotrcPath = FileClient.Combine(directory.FullName, ".godotrc");
      if (FileClient.FileExists(godotrcPath)) {
        godotrcFiles.Add(new GodotrcFile(godotrcPath));
      }
    }
    List<IGodotVersionFile> files = [];
    files.AddRange(globalJsonFiles);
    files.AddRange(csprojFiles);
    files.AddRange(godotrcFiles);
    return files;
  }

  /// <inheritdoc/>
  public string GetProjectDefinitionDirectory() {
    var slnDir = string.Empty;
    var godotProjectDir = string.Empty;
    foreach (var directory in FileClient.GetAncestorDirectories(WorkingDir)) {
      if (FileClient.GetFiles(directory.FullName, "*.sln").Any()) {
        slnDir = directory.FullName;
      }
      if (FileClient.GetFiles(directory.FullName, "project.godot").Any()) {
        godotProjectDir = directory.FullName;
      }
    }
    if (!string.IsNullOrEmpty(slnDir)) {
      return slnDir;
    }
    return godotProjectDir;
  }

  /// <inheritdoc/>
  public void PinVersion(
    SpecificDotnetStatusGodotVersion version,
    string projectDir,
    ILog log
  ) {
    IGodotVersionFile file = version.IsDotnetEnabled
      ? new GlobalJsonFile(FileClient.Combine(projectDir, "global.json"))
      : new GodotrcFile(FileClient.Combine(projectDir, ".godotrc"));
    log.Info($"Writing Godot version \"{VersionSerializer.SerializeWithDotnetStatus(version)}\" to {file.FilePath}");
    file.WriteGodotVersion(version, FileClient);
  }
}
