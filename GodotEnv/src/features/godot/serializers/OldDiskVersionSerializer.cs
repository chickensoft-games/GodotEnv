namespace Chickensoft.GodotEnv.Features.Godot.Serializers;

using Chickensoft.GodotEnv.Features.Godot.Models;

/// <summary>
/// An <see cref="IVersionSerializer"> for Godot installations created by
/// pre-2.11 versions of GodotEnv.
/// </summary>
public partial class OldDiskVersionSerializer : VersionSerializer
{
  public bool OutputLabelSeparator { get; set; }
  public bool OutputPatchNumber { get; set; }
  public bool OutputStableLabel { get; set; }

  public OldDiskVersionSerializer() { }

  public OldDiskVersionSerializer(
    bool outputLabelSeparator, bool outputPatchNumber, bool outputStableLabel
  )
  {
    OutputLabelSeparator = outputLabelSeparator;
    OutputPatchNumber = outputPatchNumber;
    OutputStableLabel = outputStableLabel;
  }

  public override string Serialize(GodotVersion version)
  {
    var result = $"{version.Number.Major}.{version.Number.Minor}";
    if (OutputPatchNumber || version.Number.Patch != 0)
    {
      result += $".{version.Number.Patch}";
    }
    var label = LabelString(version);
    if (OutputStableLabel || label != "stable")
    {
      result += $"-{label}";
    }
    return result;
  }

  public string LabelString(GodotVersion version)
  {
    var result = version.Number.Label;
    if (result != "stable")
    {
      if (OutputLabelSeparator)
      {
        result += ".";
      }
      result += version.Number.LabelNumber;
    }
    return result;
  }
}
