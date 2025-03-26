namespace Chickensoft.GodotEnv.Features.Godot.Serializers;

using Chickensoft.GodotEnv.Features.Godot.Models;

public partial class IoVersionSerializer : IVersionSerializer {
  private readonly ReleaseVersionSerializer _releaseConverter = new();

  public string Serialize(GodotVersion version)
    => _releaseConverter.Serialize(version);
}
