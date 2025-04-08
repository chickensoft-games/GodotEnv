namespace Chickensoft.GodotEnv.Features.Godot.Models;

/// <summary>
/// Represents a Godot installation.
/// </summary>
/// <param name="Location">The on-disk location of this Godot
/// installation.</param>
/// <param name="IsActiveVersion">True if this is the active version of Godot
/// being used by the symlink.</param>
/// <param name="Version">Godot version with specified .NET status.</param>
/// <param name="ExecutionPath">Fully resolved path to the Godot executable
/// for this installation.</param>
public record GodotInstallation(
  GodotInstallationLocation Location,
  bool IsActiveVersion,
  SpecificDotnetStatusGodotVersion Version,
  string ExecutionPath
) {
  public override string ToString() =>
    $$"""
    "{
      "name": {{Location.Name}}",
      "version": "{{Version}}",
      "path": "{{Location.InstallationDirectory}}"
    }
    """;
}
