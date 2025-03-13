namespace Chickensoft.GodotEnv.Features.Godot.Models;

/// <summary>
/// Represents a Godot installation.
/// </summary>
/// <param name="Name">Name of the folder containing this Godot
/// installation.</param>
/// <param name="IsActiveVersion">True if this is the active version of Godot
/// being used by the symlink.</param>
/// <param name="Version">Godot version with specified .NET status.</param>
/// <param name="Path">Absolute path to the directory containing this Godot
/// installation.</param>
/// <param name="ExecutionPath">Fully resolved path to the Godot executable
/// for this installation.</param>
public record GodotInstallation(
  string Name,
  bool IsActiveVersion,
  DotnetSpecificGodotVersion Version,
  string Path,
  string ExecutionPath
) {
  public override string ToString() =>
    $$"""
    "{
      "name": {{Name}}",
      "version": "{{Version}}",
      "path": "{{Path}}"
    }
    """;
}
