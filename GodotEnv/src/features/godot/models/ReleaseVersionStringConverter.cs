namespace Chickensoft.GodotEnv.Features.Godot.Models;

using System;
using System.Text.RegularExpressions;

public partial class ReleaseVersionStringConverter : IVersionStringConverter {
  public AnyDotnetStatusGodotVersion ParseVersion(string version) =>
    new(ParseVersionNumber(version));

  public SpecificDotnetStatusGodotVersion ParseVersion(string version, bool isDotnet) =>
    new(ParseVersionNumber(version), isDotnet);

  public string VersionString(GodotVersion version) {
    var result = $"{version.Number.Major}.{version.Number.Minor}";
    if (version.Number.Patch != 0) {
      result += $".{version.Number.Patch}";
    }
    result += $"-{LabelString(version)}";
    return result;
  }

  public string LabelString(GodotVersion version) {
    var result = version.Number.Label;
    if (result != "stable") {
      result += version.Number.LabelNumber;
    }
    return result;
  }

  private static GodotVersionNumber ParseVersionNumber(string version) {
    var match = VersionStringRegex().Match(version);
    if (!match.Success) {
      throw new ArgumentException(
        $"Couldn't match \"{version}\" to known Godot version patterns."
      );
    }
    // we can safely convert major and minor, since the regex only matches
    // digit characters
    var major = int.Parse(match.Groups[1].Value);
    var minor = int.Parse(match.Groups[2].Value);
    // patch string is optional "-\d+", so we can safely convert it after
    // the first character if it has length
    var patch = 0;
    var patchStr = match.Groups[3].Value;
    if (patchStr.Length > 0) {
      patch = int.Parse(patchStr[1..]);
    }

    var label = match.Groups[4].Value;
    var labelNum = -1;
    if (label != "stable") {
      label = match.Groups[5].Value;
      // If group 4 is not "stable", group 6 must have a positive number of
      // digits
      labelNum = int.Parse(match.Groups[6].Value);
    }
    return new GodotVersionNumber(major, minor, patch, label, labelNum);
  }

  // All published Godot 4+ packages have a label ("-stable" if not prerelease)
  // "-stable" labels do not have a number, others do
  // Versions with a patch number of 0 do not have a patch number
  [GeneratedRegex(@"^(\d+)\.(\d+)(\.[1-9]\d*)?-(stable|([a-z]+)(\d+))$")]
  public static partial Regex VersionStringRegex();
}
