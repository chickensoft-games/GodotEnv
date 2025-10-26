namespace Chickensoft.GodotEnv.Features.Godot.Models;

using System;
using System.Xml;
using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Features.Godot.Serializers;

public class CsprojFile : IGodotVersionFile, IEquatable<CsprojFile>
{
  private static readonly SharpVersionDeserializer _versionDeserializer = new();

  /// <inheritdoc/>
  public string FilePath { get; }

  public CsprojFile(string filePath)
  {
    FilePath = filePath;
  }

  /// <inheritdoc/>
  public SpecificDotnetStatusGodotVersion? ParseGodotVersion(
    IFileClient fileClient
  )
  {
    string version;
    try
    {
      var xmlDocument = new XmlDocument();
      using (var reader = fileClient.GetReader(FilePath))
      {
        xmlDocument.Load(reader);
      }
      var projectNode = xmlDocument.DocumentElement;
      if (projectNode is null
        || projectNode.Name != "Project"
        || projectNode.Attributes is null
      )
      {
        return null;
      }
      var sdk = projectNode.Attributes["Sdk"];
      if (sdk is null
        || !sdk.Value.StartsWith(
              "Godot.NET.Sdk/",
              StringComparison.InvariantCulture
            )
      )
      {
        return null;
      }
      version = sdk.Value.Split('/')[1];
    }
    catch (Exception)
    {
      return null;
    }
    // If the version is from a csproj, we definitely want .NET
    return _versionDeserializer.Deserialize(version, true);
  }

  /// <inheritdoc/>
  public void WriteGodotVersion(
    SpecificDotnetStatusGodotVersion version,
    IFileClient fileClient
  ) =>
    throw new NotSupportedException("Writing Godot version information to csproj files is not supported.");

  public bool Equals(CsprojFile? other) =>
    other is not null && FilePath == other.FilePath;

  public override bool Equals(object? obj) =>
    obj is CsprojFile csprojFile && Equals(csprojFile);

  public override int GetHashCode() => FilePath.GetHashCode();
}
