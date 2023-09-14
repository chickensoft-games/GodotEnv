namespace Chickensoft.GodotEnv.Features.Godot.Models;

using System;
using System.Text.RegularExpressions;

public record SemanticVersion(
  string Major, string Minor, string Patch, string Label
) {
  // Borrowed from https://semver.org/

  /// <summary>
  /// Semantic version regex provided by https://semver.org.
  /// <br />
  /// Try it: https://regex101.com/r/vkijKf/1/
  /// </summary>
  public static Regex SemanticVersionRegex { get; } = new(
    @"^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-((?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+([0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$"
  );

  /// <summary>
  /// Parses a string into a semantic version model using the official
  /// Semantic Version regex.
  /// </summary>
  /// <param name="version">Semantic version string.</param>
  /// <returns>A <see cref="SemanticVersion" />.</returns>
  /// <exception cref="InvalidOperationException" />
  public static SemanticVersion Parse(string version) {
    var match = SemanticVersionRegex.Match(version);
    if (!match.Success) {
      throw new InvalidOperationException(
        $"Invalid semantic version: {version}"
      );
    }
    var major = match.Groups[1]?.Value ?? "";
    var minor = match.Groups[2]?.Value ?? "";
    var patch = match.Groups[3]?.Value ?? "";
    var label = match.Groups[4]?.Value ?? "";

    return new SemanticVersion(
      Major: major,
      Minor: minor,
      Patch: patch,
      Label: label
    );
  }

  /// <summary>
  /// Checks to make sure a potential version string is actually a valid
  /// semantic version.
  /// </summary>
  /// <param name="version">Potential version string.</param>
  /// <returns>True if the version is a valid semantic version.</returns>
  public static bool IsValid(string version) =>
    SemanticVersionRegex.IsMatch(version);

  /// <summary>Label without dots.</summary>
  public string LabelNoDots => Label.Replace(".", "");

  /// <summary>Reconstructed version string.</summary>
  public string VersionString => $"{Major}.{Minor}.{Patch}" +
    (Label != "" ? $"-{Label}" : "");

  public override string ToString() =>
    $"Major({Major}).Minor({Minor}).Patch({Patch})-Label({Label})";
}
