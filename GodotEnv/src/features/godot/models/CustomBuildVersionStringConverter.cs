namespace Chickensoft.GodotEnv.Features.Godot.Models;

using System;
using System.Text.RegularExpressions;

public partial class CustomBuildVersionStringConverter : IVersionStringConverter {
  public GodotVersion ParseVersion(string version) {
    var match = VersionStringRegex().Match(version);
    if (!match.Success) {
      throw new ArgumentException(
        $"Couldn't match \"{version}\" to known Godot custom build version patterns."
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
      var buildVersion = match.Groups[6].Value;

      // Verify if group 6 exists
      if (buildVersion != "") {
        // If yes, get build version at group 8
        labelNum = int.Parse(match.Groups[8].Value);
      }

    }
    return new GodotVersion(major, minor, patch, label, labelNum, true);
  }

  public string VersionString(GodotVersion version) {
    var result = $"{version.Major}.{version.Minor}";
    if (version.Patch != 0) {
      result += $".{version.Patch}";
    }
    result += $"-{LabelString(version)}";
    return result;
  }

  public string LabelString(GodotVersion version) {
    var result = version.Label;
    if (result != "stable") {
      result += version.LabelNumber;
    }
    return result;
  }

  // All published Godot 4+ packages have a label ("-stable" if not prerelease)
  // "-stable" labels do not have a number, others do
  // Versions with a patch number of 0 do not have a patch number
  [GeneratedRegex(@"^(\d+)\.(\d+)(\.[1-9]\d*)?-(stable|([a-zA-Z]+)((\.)?\d+)?)$")]
  public static partial Regex VersionStringRegex();
}
