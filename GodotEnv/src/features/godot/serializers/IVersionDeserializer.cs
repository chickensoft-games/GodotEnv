namespace Chickensoft.GodotEnv.Features.Godot.Serializers;

using Chickensoft.GodotEnv.Common.Utilities;
using Chickensoft.GodotEnv.Features.Godot.Models;

public interface IVersionDeserializer
{
  Result<AnyDotnetStatusGodotVersion> Deserialize(string version);
  Result<SpecificDotnetStatusGodotVersion> Deserialize(string version, bool isDotnet);
}
