namespace Chickensoft.GodotEnv.Features.Godot.Serializers;

using Chickensoft.GodotEnv.Features.Godot.Models;

public partial class ReleaseVersionSerializer : VersionSerializer {
  public override string Serialize(GodotVersion version) {
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
}
