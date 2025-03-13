namespace Chickensoft.GodotEnv.Features.Godot.Models;

using System;

public partial class IOVersionStringConverter : IVersionStringConverter {
  private readonly ReleaseVersionStringConverter _releaseConverter = new();
  private readonly SharpVersionStringConverter _sharpConverter = new();

  public DotnetAgnosticGodotVersion ParseVersion(string version) {
    var trimmedVersion = version.TrimStart('v');
    Exception releaseEx = null!;
    try {
      return _releaseConverter.ParseVersion(trimmedVersion);
    }
    catch (Exception ex) {
      releaseEx = ex;
    }
    try {
      return _sharpConverter.ParseVersion(trimmedVersion);
    }
    catch (Exception ex) {
      throw new ArgumentException($"Version string {version} is neither release style ({releaseEx.Message}) nor GodotSharp style ({ex.Message})");
    }
  }

  public DotnetSpecificGodotVersion ParseVersion(string version, bool isDotnet) {
    var trimmedVersion = version.TrimStart('v');
    Exception releaseEx = null!;
    try {
      return _releaseConverter.ParseVersion(trimmedVersion, isDotnet);
    }
    catch (Exception ex) {
      releaseEx = ex;
    }
    try {
      return _sharpConverter.ParseVersion(trimmedVersion, isDotnet);
    }
    catch (Exception ex) {
      throw new ArgumentException($"Version string {version} is neither release style ({releaseEx.Message}) nor GodotSharp style ({ex.Message})");
    }
  }

  public string VersionString(GodotVersion version)
    => _releaseConverter.VersionString(version);
}
