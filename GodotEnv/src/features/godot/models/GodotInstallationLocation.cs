namespace Chickensoft.GodotEnv.Features.Godot.Models;

/// <summary>
/// Represents the on-disk location of a Godot installation.
/// </summary>
/// <param name="Name">Name of the folder containing this Godot
/// installation.</param>
/// <param name="InstallationDirectory">Absolute path to the directory
/// containing this Godot installation.</param>
public record GodotInstallationLocation(
  string Name, string InstallationDirectory
);
