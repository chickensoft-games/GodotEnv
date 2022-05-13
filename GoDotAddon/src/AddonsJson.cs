namespace GoDotAddon {
  using System.Text.Json.Serialization;

  /// <summary>
  /// Represents an addons.json file.
  /// </summary>
  public class AddonsJson {
    [JsonPropertyName("addons")]
    public List<string> Addons { get; init; } = new List<string>();

    [JsonPropertyName("cache")]
    public string Cache { get; init; } = Info.DEFAULT_CACHE_DIR;
  }
}
