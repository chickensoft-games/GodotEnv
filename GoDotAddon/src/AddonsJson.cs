namespace GoDotAddon {

  using System.Text.Json.Serialization;

  /// <summary>
  /// Represents an addons.json file.
  /// </summary>
  public class AddonsJson {
    [JsonPropertyName("addons")]
    public List<Addon> Addons { get; init; } = new List<Addon>();

    [JsonPropertyName("cache")]
    public string Cache { get; init; } = IApp.DEFAULT_CACHE_DIR;
  }

  public class Addon {
    [JsonPropertyName("name")]
    public string? Name { get; init; }
    [JsonPropertyName("url")]
    public string? Url { get; init; }
    [JsonPropertyName("subfolder")]
    public string? Subfolder { get; init; }
    [JsonPropertyName("tag")]
    public string? Tag { get; init; }

    [JsonIgnore]
    public bool IsValid => Name != null && Url != null;
  }
}
