namespace Chickensoft.GodotEnv.Features.Godot.Serializers;

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Chickensoft.GodotEnv.Features.Godot.Models;

public partial class ReleaseVersionDeserializer : IVersionDeserializer
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
      throw new ArgumentException(
        $"Couldn't match \"{version}\" to known Godot version patterns."
      );
    }
    // we can safely convert major and minor, since the regex only matches
    // digit characters
    var major = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
    var minor = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
    // patch string is optional "-\d+", so we can safely convert it after
    // the first character if it has length
    var patch = 0;
    var patchStr = match.Groups[3].Value;
    if (patchStr.Length > 0)
    {
      patch = int.Parse(patchStr[1..], CultureInfo.InvariantCulture);
    }

    var label = match.Groups[4].Value;
    var labelNum = -1;
    if (label != "stable")
    {
      label = match.Groups[5].Value;
      // If group 4 is not "stable", group 6 must have a positive number of
      // digits
      labelNum = int.Parse(match.Groups[6].Value, CultureInfo.InvariantCulture);
    }
    return new GodotVersionNumber(major, minor, patch, label, labelNum);
  }

  // All published Godot 4+ packages have a label ("-stable" if not prerelease)
  // "-stable" labels do not have a number, others do
  // Versions with a patch number of 0 do not have a patch number
  [GeneratedRegex(@"^(\d+)\.(\d+)(\.[1-9]\d*)?-(stable|([a-z]+)(\d+))$")]
  public static partial Regex VersionStringRegex();
}
