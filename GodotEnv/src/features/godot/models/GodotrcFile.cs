namespace Chickensoft.GodotEnv.Features.Godot.Models;

using System;
using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Features.Godot.Serializers;

public class GodotrcFile : IGodotVersionFile, IEquatable<GodotrcFile> {
  private static readonly IoVersionDeserializer _versionDeserializer = new();

  /// <inheritdoc/>
  public string FilePath { get; }

  public GodotrcFile(string filePath) {
    FilePath = filePath;
  }

  /// <inheritdoc/>
  public SpecificDotnetStatusGodotVersion? ParseGodotVersion(
    IFileClient fileClient
  ) {
    var version = string.Empty;
    try {
      using (var reader = fileClient.GetReader(FilePath)) {
        version = reader.ReadLine();
      }
    }
    catch (Exception) {
      return null;
    }
    if (string.IsNullOrEmpty(version)) {
      return null;
    }
    var isDotnet = true;
    if (version[0] == '~') {
      isDotnet = false;
      version = version[1..];
    }
    return _versionDeserializer.Deserialize(version, isDotnet);
  }

  public bool Equals(GodotrcFile? other) =>
    other is not null && FilePath == other.FilePath;

  public override bool Equals(object? obj) =>
    obj is GodotrcFile godotrcFile && Equals(godotrcFile);

  public override int GetHashCode() => FilePath.GetHashCode();
}
