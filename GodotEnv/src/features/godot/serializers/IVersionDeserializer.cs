namespace Chickensoft.GodotEnv.Features.Godot.Serializers;

using Chickensoft.GodotEnv.Features.Godot.Models;

public interface IVersionDeserializer {
  public AnyDotnetStatusGodotVersion Deserialize(string version);
  public SpecificDotnetStatusGodotVersion Deserialize(string version, bool isDotnet);
}
