namespace Chickensoft.GodotEnv.Features.Godot.Models;

/// <summary>
/// Represents a downloaded Godot installer zip file.
/// </summary>
/// <param name="Name">Name of the cache folder containing this installation
/// file.</param>
/// <param name="Filename">Filename of the compressed archive.</param>
/// <param name="Version">Godot version.</param>
/// <param name="IsDotnetVersion">True if this installation of Godot is the
/// .NET-enabled version of Godot.</param>
/// <param name="Path">
/// Absolute path to the directory containing the installer file.
/// </param>
public record GodotCompressedArchive(
  string Name,
  string Filename,
  SemanticVersion Version,
  bool IsDotnetVersion,
  string Path
) {
  public override string ToString() =>
    $$"""
    "{
      "name": {{Name}}",
      "filename": "{{Filename}}",
      "version": "{{Version}}",
      "isDotnetVersion": {{IsDotnetVersion}},
      "path": "{{Path}}"
    }
    """;
}
