namespace Chickensoft.Chicken {
  using Newtonsoft.Json;
  using Newtonsoft.Json.Converters;
  using Newtonsoft.Json.Serialization;

  public enum AddonSource {
    [JsonProperty("local")]
    Local,
    [JsonProperty("remote")]
    Remote,
    [JsonProperty("symlink")]
    Symlink
  }

  public record AddonConfig {
    [JsonProperty("url")]
    public string Url { get; init; }
    [JsonProperty("subfolder")]
    public string Subfolder { get; init; }
    [JsonProperty("checkout")]
    public string Checkout { get; init; }
    [JsonProperty("source")]
    [JsonConverter(typeof(StringEnumConverter))]
    public AddonSource Source { get; init; }

    public bool IsLocal => Source == AddonSource.Local;
    public bool IsRemote => Source == AddonSource.Remote;
    public bool IsSymlink => Source == AddonSource.Symlink;

    [System.Text.Json.Serialization.JsonConstructor]
    public AddonConfig(
      string url,
      string? subfolder,
      string? checkout,
      AddonSource? source = null
    ) {
      Url = url;
      Subfolder = subfolder ?? IApp.DEFAULT_SUBFOLDER;
      Checkout = checkout ?? IApp.DEFAULT_CHECKOUT;
      Source = source ?? AddonSource.Remote;
    }
  }
}
