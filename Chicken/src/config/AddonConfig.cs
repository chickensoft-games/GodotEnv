namespace Chickensoft.Chicken {
  using Newtonsoft.Json;

  public record AddonConfig {
    [JsonProperty("url")]
    public string Url { get; init; }
    [JsonProperty("subfolder")]
    public string Subfolder { get; init; }
    [JsonProperty("checkout")]
    public string Checkout { get; init; }
    [JsonProperty("symlink")]
    public bool Symlink { get; init; }

    [JsonConstructor]
    public AddonConfig(
      string url,
      string? subfolder,
      string? checkout,
      bool symlink = false
    ) {
      Url = url;
      Subfolder = subfolder ?? IApp.DEFAULT_SUBFOLDER;
      Checkout = checkout ?? IApp.DEFAULT_CHECKOUT;
      Symlink = symlink;
    }
  }
}
