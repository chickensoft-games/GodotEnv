namespace Chickensoft.GodotEnv.Features.Godot.Serializers;

using System.Globalization;
using System.Text.RegularExpressions;
using Chickensoft.GodotEnv.Common.Utilities;
using Chickensoft.GodotEnv.Features.Godot.Models;

/// <summary>
/// An <see cref="IVersionDeserializer"> for Godot installations created by
/// pre-2.11 versions of GodotEnv.
/// </summary>
public partial class DiskVersionDeserializer : IVersionDeserializer
{
  public Result<AnyDotnetStatusGodotVersion> Deserialize(string version)
  {
    var versionNum = ParseVersionNumber(version);
    return versionNum.IsSuccess ?
      new(true, new(versionNum.Value), string.Empty) :
      new(false, null, versionNum.Error);
  }

  public Result<SpecificDotnetStatusGodotVersion> Deserialize(string version, bool isDotnet)
  {
    var versionNum = ParseVersionNumber(version);
    return versionNum.IsSuccess ?
      new(true, new(versionNum.Value, isDotnet), string.Empty) :
      new(false, null, versionNum.Error);
  }

  public Result<GodotVersionNumber> ParseVersionNumber(string version)
  {
    var match = VersionStringRegex().Match(version);
    if (!match.Success)
    {
      return new(
        false,
        null,
        $"Couldn't match \"{version}\" to known Godot version patterns."
      );
    }
    // we can safely convert major and minor, since the regex only matches
    // digit characters
    var major = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
    var minor = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
    // patch string is optional ".\d+", so we can safely convert it after
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
    return new(true, new(major, minor, patch, label, labelNum), string.Empty);
  }

  // Version strings prior to 2.11 may or may not have a patch number
  // and may or may not have a "_" separating the label from the label number
  // Label is either "stable" or a string followed by an optional "_" and
  // and the label number
  [GeneratedRegex(@"^(\d+)_(\d+)(_\d+)?_(stable|([a-z]+)_?(\d+))$")]
  public static partial Regex VersionStringRegex();
}
