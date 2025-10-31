namespace Chickensoft.GodotEnv.Features.Godot.Models;

using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Common.Utilities;

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
  /// If a Godot version exists in the file and parses correctly: A success
  /// <see cref="Result"/> with the version information. If a Godot version
  /// does not exist in the file or a version exists in the file but does not
  /// parse correctly: A failure <see cref="Result"/>.
  /// </returns>
  Result<SpecificDotnetStatusGodotVersion> ParseGodotVersion(
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
