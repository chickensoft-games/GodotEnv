namespace Chickensoft.GodotEnv.Features.Godot.Models;

using Chickensoft.GodotEnv.Common.Clients;

/// <summary>
/// Represents a file on disk that may contain a Godot version string we can use
/// to execute a command.
/// </summary>
public interface IGodotVersionFile
{
  /// <summary>
  /// The path of the file to be parsed.
  /// </summary>
  string FilePath { get; }

  /// <summary>
  /// Parses and returns a Godot version from the file, if one exists.
  /// </summary>
  /// <param name="fileClient">
  /// The file client to use for reading the file.
  /// </param>
  /// <returns>
  /// A Godot version, if found. Null if no possible version string was found.
  /// </returns>
  /// <exception cref="InvalidOperationException">
  /// If the file contained a possible Godot-version string, but it didn't
  /// deserialize correctly.
  /// </exception>
  SpecificDotnetStatusGodotVersion? ParseGodotVersion(
    IFileClient fileClient
  );

  /// <summary>
  /// Writes a given Godot version into this file, if supported. Will overwrite
  /// any existing Godot version information in the file.
  /// </summary>
  /// <param name="version">The version to write into the file.</param>
  /// <param name="fileClient">
  /// The file client to use for writing the file.
  /// </param>
  /// <exception cref="NotSupportedException">
  /// If this file type does not support writing Godot versions.
  /// </exception>
  void WriteGodotVersion(
    SpecificDotnetStatusGodotVersion version,
    IFileClient fileClient
  );
}
