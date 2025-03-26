namespace Chickensoft.GodotEnv.Features.Godot.Serializers;

using Chickensoft.GodotEnv.Features.Godot.Models;

public interface IVersionSerializer {
  public string Serialize(GodotVersion version);
}
