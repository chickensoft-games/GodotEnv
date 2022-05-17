namespace GoDotAddon {
  using System.Text.Json.Serialization;

  public record LockFileEntry {
    [JsonPropertyName("name")]
    public string Name { get; init; }
    [JsonPropertyName("checkout")]
    public string Checkout { get; init; }

    [JsonConstructor]
    public LockFileEntry(string name, string checkout) {
      Name = name;
      Checkout = checkout;
    }
  }
}
