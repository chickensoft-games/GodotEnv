namespace Chickensoft.GodotEnv.Features.Godot.Models;

using System;
using System.Text.RegularExpressions;

public partial class SharpVersionStringConverter : IVersionStringConverter {
  public GodotVersion ParseVersion(string version) {
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
    return new GodotVersion(major, minor, patch, label, labelNum);
  }

  public string VersionString(GodotVersion version) {
    var versionString = string.Join(
      ".", version.Major, version.Minor, version.Patch
    );
    if (version.Label != "stable") {
      versionString += $"-{LabelString(version)}";
    }
    return versionString;
  }

  public string LabelString(GodotVersion version) {
    var result = version.Label;
    if (result != "stable") {
      result += $".{version.LabelNumber}";
    }
    return result;
  }

  // All GodotSharp versions with a prerelease label include a number
  // Fails on one published GodotSharp version string, 4.0.0-alpha17
  //   (no dot separator)
  [GeneratedRegex(@"^(\d+)\.(\d+)\.(\d+)(-([a-z]+)\.(\d+))?$")]
  public static partial Regex VersionStringRegex();
}
