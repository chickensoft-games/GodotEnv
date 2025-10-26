namespace Chickensoft.GodotEnv.Features.Godot.Serializers;

using Chickensoft.GodotEnv.Features.Godot.Models;

public interface IVersionSerializer
{
  /// <summary>
  /// Serialize the provided version to a version string.
  /// </summary>
  /// <param name="version">The version to serialize.</param>
  /// <returns>A version string.</returns>
  /// <remarks>The returned string does not include .NET status.</remarks>
  /// <seealso cref="SerializeWithDotnetStatus"/>
  string Serialize(GodotVersion version);

  /// <summary>
  /// Serialize the provided version to a version string with .NET status.
  /// </summary>
  /// <param name="version">The version to serialize.</param>
  /// <returns>A version string including .NET status.</returns>
  string SerializeWithDotnetStatus(
    SpecificDotnetStatusGodotVersion version
  );
}
