namespace Chickensoft.GodotEnv.Features.Godot.Serializers;

using Chickensoft.GodotEnv.Features.Godot.Models;

public partial class SharpVersionSerializer : VersionSerializer {
  public override string Serialize(GodotVersion version) {
    var versionString = string.Join(
      ".", version.Number.Major, version.Number.Minor, version.Number.Patch
    );
    if (version.Number.Label != "stable") {
      versionString += $"-{LabelString(version)}";
    }
    return versionString;
  }

  public string LabelString(GodotVersion version) {
    var result = version.Number.Label;
    if (result != "stable") {
      result += $".{version.Number.LabelNumber}";
    }
    return result;
  }
}
