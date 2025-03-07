namespace Chickensoft.GodotEnv.Features.Godot.Models;

using System;
using System.Text.RegularExpressions;

public partial record GodotPackageVersion(
  string Major, string Minor, string Patch, string Label
) {
  public GodotPackageVersion(GodotSharpVersion sharpVersion)
    : this(sharpVersion.Major,
           sharpVersion.Minor,
           sharpVersion.Patch == "0" ? string.Empty : sharpVersion.Patch,
           CanonicalizeSharpLabel(sharpVersion.Label)) {
  }

  public string VersionString() {
    var result = $"{Major}.{Minor}";
    if (Patch.Length != 0) {
      result += $".{Patch}";
    }
    result += $"-{Label}";
    return result;
  }

  public static GodotPackageVersion? Parse(string version) {
    var match = VersionStringRegex().Match(version);
    if (!match.Success) {
      return null;
    }
    var major = match.Groups[1].Value;
    var minor = match.Groups[2].Value;
    var patch = match.Groups[3].Value;
    var label = match.Groups[4].Value;
    if (patch != string.Empty) {
      patch = patch[1..];
    }
    label = label[1..];
    return new GodotPackageVersion(major, minor, patch, label);
  }

  public static string CanonicalizeSharpLabel(string sharpLabel) {
    if (sharpLabel.Length == 0) {
      return "stable";
    }

    var match = SharpLabelRegex().Match(sharpLabel);
    if (!match.Success) {
      throw new InvalidOperationException(
        $"Invalid GodotSharp version label: {sharpLabel}"
      );
    }

    // Drop the "." between label text and number
    return match.Groups[1].Value + match.Groups[2].Value[1..];
  }

  [GeneratedRegex(@"^([a-zA-Z]+)(\.\d+)?$")]
  public static partial Regex SharpLabelRegex();

  // All published Godot 4+ packages have a label ("-stable" if not prerelease)
  // "-stable" labels do not have a number
  // Versions with a patch number of 0 do not have a patch number
  [GeneratedRegex(@"^(\d+)\.(\d+)(\.[1-9]\d*)?(-[a-z]+(?:\d*))$")]
  public static partial Regex VersionStringRegex();
}
