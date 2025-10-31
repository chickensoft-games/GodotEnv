namespace Chickensoft.GodotEnv.Features.Godot.Models;

using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Common.Utilities;
using Chickensoft.GodotEnv.Features.Godot.Serializers;

public class GlobalJsonFile : IGodotVersionFile, IEquatable<GlobalJsonFile>
{
  private static readonly SharpVersionDeserializer _versionDeserializer = new();
  private static readonly SharpVersionSerializer _versionSerializer = new();

  /// <inheritdoc/>
  public string FilePath { get; }
  public JsonDocumentOptions JsonDocumentOptions { get; }
  public JsonNodeOptions JsonNodeOptions { get; }
  public JsonSerializerOptions JsonSerializerOptions { get; }

  public GlobalJsonFile(string filePath)
  {
    FilePath = filePath;
    JsonDocumentOptions = new()
    {
      CommentHandling = JsonCommentHandling.Skip,
      AllowTrailingCommas = true,
    };
    JsonNodeOptions = new();
    JsonSerializerOptions = new()
    {
      WriteIndented = true,
    };
  }

  /// <inheritdoc/>
  public Result<SpecificDotnetStatusGodotVersion> ParseGodotVersion(
    IFileClient fileClient
  )
  {
    var version = string.Empty;
    try
    {
      using (var stream = fileClient.GetReadStream(FilePath))
      {
        using (
          var jsonDocument = JsonDocument.Parse(stream, JsonDocumentOptions)
        )
        {
          var msbuildSdks =
            jsonDocument.RootElement.GetProperty("msbuild-sdks");
          var godotSdk = msbuildSdks.GetProperty("Godot.NET.Sdk");
          version = godotSdk.ToString();
        }
      }
    }
    catch (Exception)
    {
      return new(
        false,
        null,
        $"global.json file {FilePath} does not exist or does not contain Godot.NET.Sdk information"
      );
    }
    // if the version is from a global.json, we definitely want .NET
    return _versionDeserializer.Deserialize(version, true);
  }

  /// <inheritdoc/>
  public void WriteGodotVersion(
    SpecificDotnetStatusGodotVersion version,
    IFileClient fileClient
  )
  {
    JsonNode jsonNode = new JsonObject();
    // preserve existing global.json data
    if (fileClient.FileExists(FilePath))
    {
      using var stream = fileClient.GetReadStream(FilePath);
      jsonNode = JsonNode.Parse(stream, JsonNodeOptions, JsonDocumentOptions)
        ?? jsonNode;
    }

    var msbuildSdks = jsonNode["msbuild-sdks"] ?? new JsonObject();
    msbuildSdks["Godot.NET.Sdk"] = _versionSerializer.Serialize(version);
    jsonNode["msbuild-sdks"] = msbuildSdks;
    var jsonString = jsonNode.ToJsonString(JsonSerializerOptions);
    using (var writer = fileClient.GetWriter(FilePath))
    {
      writer.WriteLine(jsonString);
    }
  }

  public bool Equals(GlobalJsonFile? other) =>
    other is not null && FilePath == other.FilePath;

  public override bool Equals(object? obj) =>
    obj is GlobalJsonFile globalJsonFile && Equals(globalJsonFile);

  public override int GetHashCode() => FilePath.GetHashCode();
}
