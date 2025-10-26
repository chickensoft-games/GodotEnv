namespace Chickensoft.GodotEnv.Features.Godot.Serializers;

using Chickensoft.GodotEnv.Features.Godot.Models;

public interface IVersionDeserializer
{
  AnyDotnetStatusGodotVersion Deserialize(string version);
  SpecificDotnetStatusGodotVersion Deserialize(string version, bool isDotnet);
}
