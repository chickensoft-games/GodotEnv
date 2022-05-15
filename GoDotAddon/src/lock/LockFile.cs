namespace GoDotAddon {
  using System.Text.Json.Serialization;

  public class LockFile {
    // [url][subfolder] = LockFileEntry
    [JsonPropertyName("addons")]
    public Dictionary<string, Dictionary<string, LockFileEntry>>
      Addons { get; init; } = new();
  }

  public class LockFileEntry {
    [JsonPropertyName("name")]
    public string Name { get; init; }
    [JsonPropertyName("checkout")]
    public string Checkout { get; init; }
    [JsonPropertyName("main")]
    public string Main { get; init; }

    [JsonConstructor]
    public LockFileEntry(string name, string checkout, string main) {
      Name = name;
      Checkout = checkout;
      Main = main;
    }
  }
}
