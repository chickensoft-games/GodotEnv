namespace Chickensoft.GodotEnv.Features.Godot.Models;

using System;
using System.Linq;
using System.Text.RegularExpressions;

public partial record GodotVersion {
  public string Major { get; }
  public string Minor { get; }
  public string Patch { get; }
  public string GodotLabel { get; }
  public string SharpLabel { get; }

  internal GodotVersion(string major, string minor, string patch, string godotLabel, string sharpLabel) {
    if (!IsNumeric(major)) {
      throw new ArgumentException($"Major number is not numeric: {major}");
    }
    if (!IsNumeric(minor)) {
      throw new ArgumentException($"Minor number is not numeric: {minor}");
    }
    if (!IsNumeric(patch)) {
      throw new ArgumentException($"Patch number is not numeric: {patch}");
    }
    if (!GodotLabelRegex().Match(godotLabel).Success) {
      throw new ArgumentException($"Godot version label is invalid: ${godotLabel}");
    }
    if (sharpLabel.Length > 0 && !SharpLabelRegex().Match(sharpLabel).Success) {
      throw new ArgumentException($"GodotSharp version label is invalid: ${sharpLabel}");
    }

    Major = major;
    Minor = minor;
    Patch = patch;
    GodotLabel = godotLabel;
    SharpLabel = sharpLabel;
  }

  public string GodotVersionString() {
    var result = $"{Major}.{Minor}";
    if (Patch != "0") {
      result += $".{Patch}";
    }
    result += $"-{GodotLabel}";
    return result;
  }

  public string SharpVersionString() {
    var versionString = string.Join(".", Major, Minor, Patch);
    if (SharpLabel.Length > 0) {
      versionString += $"-{SharpLabel}";
    }
    return versionString;
  }

  public static GodotVersion? Parse(string version) {
    var result = ParseGodotVersion(version);
    return result ?? ParseSharpVersion(version);
  }

  public static GodotVersion? ParseGodotVersion(string version) {
    var match = GodotVersionStringRegex().Match(version);
    if (!match.Success) {
      return null;
    }
    var major = match.Groups[1].Value;
    var minor = match.Groups[2].Value;
    var patch = match.Groups[3].Value;
    patch = patch.Length == 0 ? "0" : patch[1..];
    var label = match.Groups[4].Value;
    label = label[1..];
    return new GodotVersion(major, minor, patch, label, SharpLabelFromGodotLabel(label));
  }

  public static GodotVersion? ParseSharpVersion(string version) {
    var match = SharpVersionStringRegex().Match(version);
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
    return new GodotVersion(major, minor, patch, GodotLabelFromSharpLabel(label), label);
  }

  public static string GodotLabelFromSharpLabel(string sharpLabel) {
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

  public static string SharpLabelFromGodotLabel(string godotLabel) {
    if (godotLabel == "stable") {
      return string.Empty;
    }

    var match = GodotLabelRegex().Match(godotLabel);
    if (!match.Success) {
      throw new InvalidOperationException(
        $"Invalid GodotSharp version label: {godotLabel}"
      );
    }

    var result = match.Groups[1].Value;
    if (match.Groups[2].Value != string.Empty) {
      result += $".{match.Groups[2].Value}";
    }
    return result;
  }

  public static bool IsNumeric(string s) =>
    s.Length > 0 && s.All(char.IsDigit);

  [GeneratedRegex(@"^([a-z]+)(\.\d+)$")]
  public static partial Regex SharpLabelRegex();

  [GeneratedRegex(@"^stable|([a-z]+)(\d+)$")]
  public static partial Regex GodotLabelRegex();

  // All published Godot 4+ packages have a label ("-stable" if not prerelease)
  // "-stable" labels do not have a number, others do
  // Versions with a patch number of 0 do not have a patch number
  [GeneratedRegex(@"^(\d+)\.(\d+)(\.[1-9]\d*)?(-(?:stable|[a-z]+\d+))$")]
  public static partial Regex GodotVersionStringRegex();

  // All GodotSharp versions with a prerelease label include a number
  // Fails on one published GodotSharp version string, 4.0.0-alpha17
  //   (no dot separator)
  [GeneratedRegex(@"^(\d+)\.(\d+)\.(\d+)(-[a-z]+\.\d+)?$")]
  public static partial Regex SharpVersionStringRegex();
}
