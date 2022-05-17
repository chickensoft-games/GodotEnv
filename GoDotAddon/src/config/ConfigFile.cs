namespace GoDotAddon {
  using Newtonsoft.Json;

  /// <summary>
  /// Represents an addons.json file.
  /// </summary>
  public record ConfigFile {
    [JsonProperty("addons")]
    public Dictionary<string, AddonConfig> Addons { get; init; }
    [JsonProperty("cache")]
    public string CachePath { get; init; }
    [JsonProperty("path")]
    public string AddonsPath { get; init; }

    [JsonConstructor]
    public ConfigFile(
      Dictionary<string, AddonConfig>? addons,
      string? cachePath,
      string? addonsPath
    ) {
      Addons = addons ?? new();
      CachePath = cachePath ?? IApp.DEFAULT_CACHE_PATH;
      AddonsPath = addonsPath ?? IApp.DEFAULT_ADDONS_PATH;
    }

    public Config ToConfig(string workingDir) => new(
      WorkingDir: workingDir,
      CachePath: Path.Combine(workingDir, CachePath),
      AddonsPath: Path.Combine(workingDir, AddonsPath)
    );
  }
}
