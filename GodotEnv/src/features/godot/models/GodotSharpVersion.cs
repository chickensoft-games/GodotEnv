namespace Chickensoft.GodotEnv.Features.Godot.Models;

using System;
using System.Text.RegularExpressions;

public partial record GodotSharpVersion(
  string Major, string Minor, string Patch, string Label
) {
  public GodotSharpVersion(GodotPackageVersion packageVersion)
    : this(packageVersion.Major,
           packageVersion.Minor,
           packageVersion.Patch.Length == 0 ? "0" : packageVersion.Patch,
           CanonicalizePackageLabel(packageVersion.Label)) {
  }

  public string VersionString() {
    var versionString = string.Join(".", Major, Minor, Patch);
    if (Label != string.Empty) {
      versionString += $"-{Label}";
    }
    return versionString;
  }

  public static GodotSharpVersion? Parse(string version) {
    var match = VersionStringRegex().Match(version);
    if (!match.Success) {
      return null;
    }
    var major = match.Groups[1].Value;
    var minor = match.Groups[2].Value;
    var patch = match.Groups[3].Value;
    var label = match.Groups[4].Value;
    if (label != string.Empty) {
      label = label[1..];
    }
    return new GodotSharpVersion(major, minor, patch, label);
  }

  public static string CanonicalizePackageLabel(string packageLabel) {
    if (packageLabel == "stable") {
      return string.Empty;
    }

    var match = PackageLabelRegex().Match(packageLabel);
    if (!match.Success) {
      throw new InvalidOperationException(
        $"Invalid GodotSharp version label: {packageLabel}"
      );
    }

    var result = match.Groups[1].Value;
    if (match.Groups[2].Value != string.Empty) {
      result += $".{match.Groups[2].Value}";
    }
    return result;
  }

  [GeneratedRegex(@"^([a-zA-Z]+)(\d+)?$")]
  public static partial Regex PackageLabelRegex();

  // All GodotSharp versions with a prerelease label include a number
  // Fails on one published GodotSharp version string, 4.0.0-alpha17
  //   (no dot separator)
  [GeneratedRegex(@"^(\d+)\.(\d+)\.(\d+)(-[a-z]+\.\d+)?$")]
  public static partial Regex VersionStringRegex();
}
