namespace Chickensoft.GodotEnv.Features.Godot.Serializers;

using Chickensoft.GodotEnv.Features.Godot.Models;

public abstract class VersionSerializer : IVersionSerializer {
  /// <inheritdoc/>
  public abstract string Serialize(GodotVersion version);

  /// <inheritdoc/>
  public virtual string SerializeWithDotnetStatus(
    SpecificDotnetStatusGodotVersion version
  ) =>
    Serialize(version)
      + (version.IsDotnetEnabled ? " dotnet" : " no-dotnet");
}
