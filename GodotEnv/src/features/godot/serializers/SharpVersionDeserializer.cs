namespace Chickensoft.GodotEnv.Features.Godot.Serializers;

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Chickensoft.GodotEnv.Features.Godot.Models;

public partial class SharpVersionDeserializer : IVersionDeserializer
{
  public AnyDotnetStatusGodotVersion Deserialize(string version)
    => new(ParseVersionNumber(version));

  public SpecificDotnetStatusGodotVersion Deserialize(string version, bool isDotnet)
    => new(ParseVersionNumber(version), isDotnet);

  private static GodotVersionNumber ParseVersionNumber(string version)
  {
    var match = VersionStringRegex().Match(version);
    if (!match.Success)
    {
      throw new ArgumentException($"Couldn't match \"{version}\" to known GodotSharp version patterns.");
    }
    // we can safely convert major, minor, and patch, since the regex only
    // matches digit characters
    var major = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
    var minor = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
    var patch = int.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);
    // 4 is the entire label inclusive of numeric ID, so skip to 5
    var label = match.Groups[5].Value;
    var labelNum = -1;
    if (label.Length == 0)
    {
      label = "stable";
    }
    else
    {
      labelNum = int.Parse(match.Groups[6].Value, CultureInfo.InvariantCulture);
    }
    return new GodotVersionNumber(major, minor, patch, label, labelNum);
  }

  // All GodotSharp versions with a prerelease label include a number
  // Fails on one published GodotSharp version string, 4.0.0-alpha17
  //   (no dot separator)
  [GeneratedRegex(@"^(\d+)\.(\d+)\.(\d+)(-([a-z]+)\.(\d+))?$")]
  public static partial Regex VersionStringRegex();
}
