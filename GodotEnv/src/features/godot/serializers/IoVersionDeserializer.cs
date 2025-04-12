namespace Chickensoft.GodotEnv.Features.Godot.Serializers;

using System;
using Chickensoft.GodotEnv.Features.Godot.Models;

public class IoVersionDeserializer : IVersionDeserializer {
  private readonly ReleaseVersionDeserializer _releaseDeserializer = new();
  private readonly SharpVersionDeserializer _sharpDeserializer = new();

  public AnyDotnetStatusGodotVersion Deserialize(string version) {
    var trimmedVersion = version.TrimStart('v');
    Exception releaseEx = null!;
    try {
      return _releaseDeserializer.Deserialize(trimmedVersion);
    }
    catch (Exception ex) {
      releaseEx = ex;
    }
    try {
      return _sharpDeserializer.Deserialize(trimmedVersion);
    }
    catch (Exception ex) {
      throw new ArgumentException($"Version string {version} is neither release style ({releaseEx.Message}) nor GodotSharp style ({ex.Message})");
    }
  }

  public SpecificDotnetStatusGodotVersion Deserialize(string version, bool isDotnet) {
    var trimmedVersion = version.TrimStart('v');
    Exception releaseEx = null!;
    try {
      return _releaseDeserializer.Deserialize(trimmedVersion, isDotnet);
    }
    catch (Exception ex) {
      releaseEx = ex;
    }
    try {
      return _sharpDeserializer.Deserialize(trimmedVersion, isDotnet);
    }
    catch (Exception ex) {
      throw new ArgumentException($"Version string {version} is neither release style ({releaseEx.Message}) nor GodotSharp style ({ex.Message})");
    }
  }
}
