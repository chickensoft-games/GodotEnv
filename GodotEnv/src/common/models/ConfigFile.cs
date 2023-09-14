namespace Chickensoft.GodotEnv.Common.Models;

using Newtonsoft.Json;

public record ConfigFile {
  /// <summary>Directory where Godot installations should be stored.</summary>
  [JsonProperty("godotInstallationsPath")]
  public string GodotInstallationsPath { get; set; }

  [JsonConstructor]
  public ConfigFile(string? godotInstallationsPath = null) {
    GodotInstallationsPath = godotInstallationsPath ??
      Defaults.GODOT_INSTALLATIONS_PATH;
  }
}
