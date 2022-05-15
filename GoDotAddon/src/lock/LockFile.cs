namespace GoDotAddon {
  using System.Text.Json.Serialization;

  public record LockFile {
    // [url][subfolder] = LockFileEntry
    [JsonPropertyName("addons")]
    public Dictionary<string, Dictionary<string, LockFileEntry>>
      Addons { get; init; } = new();
  }
}
