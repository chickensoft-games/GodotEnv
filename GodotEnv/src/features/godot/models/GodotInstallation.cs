namespace Chickensoft.GodotEnv.Features.Godot.Models;

/// <summary>
/// Represents a Godot installation.
/// </summary>
/// <param name="Name">Name of the folder containing this Godot
/// installation.</param>
/// <param name="IsActiveVersion">True if this is the active version of Godot
/// being used by the symlink.</param>
/// <param name="Version">Godot version.</param>
/// <param name="IsDotnetVersion">True if this installation of Godot is the
/// .NET-enabled version of Godot.</param>
/// <param name="Path">Absolute path to the directory containing this Godot
/// installation.</param>
/// <param name="ExecutionPath">Fully resolved path to the Godot executable
/// for this installation.</param>
public record GodotInstallation(
  string Name,
  bool IsActiveVersion,
  GodotVersion Version,
  bool IsDotnetVersion,
  string Path,
  string ExecutionPath
) {
  public override string ToString() =>
    $$"""
    "{
      "name": {{Name}}",
      "version": "{{Version}}",
      "isDotnetVersion": {{IsDotnetVersion}},
      "path": "{{Path}}"
    }
    """;
}
