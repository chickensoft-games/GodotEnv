namespace Chickensoft.GodotEnv.Features.Godot.Domain;

using System;
using System.Collections.Generic;
using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Common.Utilities;
using Chickensoft.GodotEnv.Features.Godot.Models;

public interface IGodotVersionSpecifierRepository {
  public string WorkingDir { get; set; }
  public IFileClient FileClient { get; set; }

  public SpecificDotnetStatusGodotVersion? InferVersion(
    IEnumerable<IGodotVersionFile> godotVersionFiles,
    ILog log
  );
  public SpecificDotnetStatusGodotVersion? GetValidatedGodotVersion(
    IGodotVersionFile file,
    ILog log
  );
  public IEnumerable<IGodotVersionFile> GetVersionFiles();
}

public class GodotVersionSpecifierRepository : IGodotVersionSpecifierRepository {
  public string WorkingDir { get; set; }
  public IFileClient FileClient { get; set; }

  public GodotVersionSpecifierRepository(
    string workingDir,
    IFileClient fileClient
  ) {
    WorkingDir = workingDir;
    FileClient = fileClient;
  }

  public SpecificDotnetStatusGodotVersion? InferVersion(
    IEnumerable<IGodotVersionFile> godotVersionFiles,
    ILog log
  ) {
    foreach (var godotVersionFile in godotVersionFiles) {
      var version = GetValidatedGodotVersion(godotVersionFile, log);
      if (version is not null) {
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
    foreach (var directory
      in FileClient.GetAncestorDirectories(WorkingDir)
    ) {
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
}
