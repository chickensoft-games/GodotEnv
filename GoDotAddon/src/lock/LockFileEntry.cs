namespace GoDotAddon {
  using System.Text.Json.Serialization;

  public record LockFileEntry {
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
