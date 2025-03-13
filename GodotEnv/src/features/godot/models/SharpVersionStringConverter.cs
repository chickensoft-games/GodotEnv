namespace Chickensoft.GodotEnv.Features.Godot.Models;

using System;
using System.Text.RegularExpressions;

public partial class SharpVersionStringConverter : IVersionStringConverter {
  public DotnetAgnosticGodotVersion ParseVersion(string version) =>
    new(ParseVersionNumber(version));

  public DotnetSpecificGodotVersion ParseVersion(string version, bool isDotnet) =>
    new(ParseVersionNumber(version), isDotnet);

  public string VersionString(GodotVersion version) {
    var versionString = string.Join(
      ".", version.Number.Major, version.Number.Minor, version.Number.Patch
    );
    if (version.Number.Label != "stable") {
      versionString += $"-{LabelString(version)}";
    }
    return versionString;
  }

  public string LabelString(GodotVersion version) {
    var result = version.Number.Label;
    if (result != "stable") {
      result += $".{version.Number.LabelNumber}";
    }
    return result;
  }

  private static GodotVersionNumber ParseVersionNumber(string version) {
    var match = VersionStringRegex().Match(version);
    if (!match.Success) {
      throw new ArgumentException($"Couldn't match \"{version}\" to known GodotSharp version patterns.");
    }
    // we can safely convert major, minor, and patch, since the regex only
    // matches digit characters
    var major = int.Parse(match.Groups[1].Value);
    var minor = int.Parse(match.Groups[2].Value);
    var patch = int.Parse(match.Groups[3].Value);
    // 4 is the entire label inclusive of numeric ID, so skip to 5
    var label = match.Groups[5].Value;
    var labelNum = -1;
    if (label.Length == 0) {
      label = "stable";
    }
    else {
      labelNum = int.Parse(match.Groups[6].Value);
    }
    return new GodotVersionNumber(major, minor, patch, label, labelNum);
  }

  // All GodotSharp versions with a prerelease label include a number
  // Fails on one published GodotSharp version string, 4.0.0-alpha17
  //   (no dot separator)
  [GeneratedRegex(@"^(\d+)\.(\d+)\.(\d+)(-([a-z]+)\.(\d+))?$")]
  public static partial Regex VersionStringRegex();
}
