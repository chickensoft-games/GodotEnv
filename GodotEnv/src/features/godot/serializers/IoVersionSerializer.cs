namespace Chickensoft.GodotEnv.Features.Godot.Serializers;

using Chickensoft.GodotEnv.Features.Godot.Models;

public partial class IoVersionSerializer : VersionSerializer
{
  private readonly ReleaseVersionSerializer _releaseConverter = new();

  public override string Serialize(GodotVersion version)
    => _releaseConverter.Serialize(version);
}
