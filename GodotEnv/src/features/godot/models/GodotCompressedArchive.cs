namespace Chickensoft.GodotEnv.Features.Godot.Models;

/// <summary>
/// Represents a downloaded Godot installer zip file.
/// </summary>
/// <param name="Name">Name of the cache folder containing this installation
/// file.</param>
/// <param name="Filename">Filename of the compressed archive.</param>
/// <param name="Version">Godot version.</param>
/// <param name="Path">
/// Absolute path to the directory containing the installer file.
/// </param>
public record GodotCompressedArchive(
  string Name,
  string Filename,
  SpecificDotnetStatusGodotVersion Version,
  string Path
) {
  public override string ToString() =>
    $$"""
    "{
      "name": {{Name}}",
      "filename": "{{Filename}}",
      "version": "{{Version}}",
      "path": "{{Path}}"
    }
    """;
}
