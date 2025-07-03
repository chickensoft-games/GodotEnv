namespace Chickensoft.GodotEnv.Features.Godot.Models;

using System;
using System.Text.Json;
using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Features.Godot.Serializers;

public class GlobalJsonFile : IGodotVersionFile, IEquatable<GlobalJsonFile> {
  private static readonly SharpVersionDeserializer _versionDeserializer = new();

  /// <inheritdoc/>
  public string FilePath { get; }
  public JsonDocumentOptions JsonDocumentOptions { get; }

  public GlobalJsonFile(string filePath) {
    FilePath = filePath;
    JsonDocumentOptions = new() {
      CommentHandling = JsonCommentHandling.Skip,
      AllowTrailingCommas = true,
    };
  }

  /// <inheritdoc/>
  public SpecificDotnetStatusGodotVersion? ParseGodotVersion(
    IFileClient fileClient
  ) {
    var version = string.Empty;
    try {
      using (var stream = fileClient.GetReadStream(FilePath)) {
        using (
          var jsonDocument = JsonDocument.Parse(stream, JsonDocumentOptions)
        ) {
          var msbuildSdks =
            jsonDocument.RootElement.GetProperty("msbuild-sdks");
          var godotSdk = msbuildSdks.GetProperty("Godot.NET.Sdk");
          version = godotSdk.ToString();
        }
      }
    }
    catch (Exception) {
      return null;
    }
    // if the version is from a global.json, we definitely want .NET
    return _versionDeserializer.Deserialize(version, true);
  }

  public bool Equals(GlobalJsonFile? other) =>
    other is not null && FilePath == other.FilePath;

  public override bool Equals(object? obj) =>
    obj is GlobalJsonFile globalJsonFile && Equals(globalJsonFile);

  public override int GetHashCode() => FilePath.GetHashCode();
}
