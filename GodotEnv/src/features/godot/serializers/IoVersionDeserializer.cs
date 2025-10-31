namespace Chickensoft.GodotEnv.Features.Godot.Serializers;

using Chickensoft.GodotEnv.Common.Utilities;
using Chickensoft.GodotEnv.Features.Godot.Models;

public class IoVersionDeserializer : IVersionDeserializer
{
  private readonly ReleaseVersionDeserializer _releaseDeserializer = new();
  private readonly SharpVersionDeserializer _sharpDeserializer = new();

  public Result<AnyDotnetStatusGodotVersion> Deserialize(string version)
  {
    var trimmedVersion = version.TrimStart('v');
    var releaseVersion = _releaseDeserializer.Deserialize(trimmedVersion);
    if (releaseVersion.IsSuccess)
    {
      return releaseVersion;
    }
    var sharpVersion = _sharpDeserializer.Deserialize(trimmedVersion);
    if (sharpVersion.IsSuccess)
    {
      return sharpVersion;
    }
    return new(
      false,
      null,
      $"Version string {version} is neither release style ({releaseVersion.Error}) nor GodotSharp style ({sharpVersion.Error})"
    );
  }

  public Result<SpecificDotnetStatusGodotVersion> Deserialize(string version, bool isDotnet)
  {
    var trimmedVersion = version.TrimStart('v');
    var releaseVersion = _releaseDeserializer.Deserialize(
      trimmedVersion,
      isDotnet
    );
    if (releaseVersion.IsSuccess)
    {
      return releaseVersion;
    }
    var sharpVersion = _sharpDeserializer.Deserialize(trimmedVersion, isDotnet);
    if (sharpVersion.IsSuccess)
    {
      return sharpVersion;
    }
    return new(
      false,
      null,
      $"Version string {version} is neither release style ({releaseVersion.Error}) nor GodotSharp style ({sharpVersion.Error})"
    );
  }
}
