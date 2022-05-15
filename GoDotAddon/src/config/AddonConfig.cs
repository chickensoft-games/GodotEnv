namespace GoDotAddon {
  using Newtonsoft.Json;

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
