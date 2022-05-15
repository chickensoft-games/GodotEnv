namespace GoDotAddon {
  using Newtonsoft.Json;

  /// <summary>
  /// Represents an addons.json file.
  /// </summary>
  public class ConfigFile {
    [JsonProperty("addons")]
    public Dictionary<string, AddonConfig> Addons { get; init; }
    [JsonProperty("cache")]
    public string CacheDir { get; init; }
    [JsonProperty("path")]
    public string Path { get; init; }

    [JsonConstructor]
    public ConfigFile(
      Dictionary<string, AddonConfig>? addons,
      string? cacheDir,
      string? path
    ) {
      Addons = addons ?? new();
      CacheDir = cacheDir ?? IApp.DEFAULT_CACHE_DIR;
      Path = path ?? IApp.DEFAULT_PATH_DIR;
    }

    public Config ToConfig() => new(
      WorkingDir: Info.App.WorkingDir, CacheDir: CacheDir, Path: Path
    );
  }

  public record AddonConfig {
    [JsonProperty("url")]
    public string? Url { get; init; }
    [JsonProperty("subfolder")]
    public string Subfolder { get; init; } = IApp.DEFAULT_SUBFOLDER;
    [JsonProperty("checkout")]
    public string Checkout { get; init; } = IApp.DEFAULT_CHECKOUT;

    [JsonConstructor]
    public AddonConfig(
      string? url,
      string? subfolder,
      string? checkout
    ) {
      Url = url;
      Subfolder = subfolder ?? IApp.DEFAULT_SUBFOLDER;
      Checkout = checkout ?? IApp.DEFAULT_CHECKOUT;
    }

    [JsonIgnore]
    public bool IsValid => Url != null;
  }
}
