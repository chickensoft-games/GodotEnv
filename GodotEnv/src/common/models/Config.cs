namespace Chickensoft.GodotEnv.Common.Models;

using System;
using System.Text.Json.Serialization;

// required to be able to write out new values, since Extensions.Configuration
// doesn't directly support that
public class Config {
  /// <summary>
  /// The old location of Godot.InstallationsPath, kept for backwards
  /// compatibility with old installations.
  /// </summary>
  [
    Obsolete(
      "GodotInstallationsPath is deprecated. Please use Godot.InstallationsPath"
    ),
    JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull),
    JsonPropertyName("godotInstallationsPath"),
  ]
  public string? GodotInstallationsPath { get; set; }
  /// <summary>
  /// Config options relating to Godot.
  /// </summary>
  public GodotConfig Godot { get; set; } = new();
  /// <summary>
  /// Config options relating to the terminal.
  /// </summary>
  public TerminalConfig Terminal { get; set; } = new();
}

public class GodotConfig {
  public string InstallationsPath { get; set; }
    = Defaults.CONFIG_GODOT_INSTALLATIONS_PATH;
}

public class TerminalConfig {
  public bool UseEmoji { get; set; } = Defaults.CONFIG_TERMINAL_USE_EMOJI;
}
