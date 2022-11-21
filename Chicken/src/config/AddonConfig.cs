namespace Chickensoft.Chicken;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

public record AddonConfig : ISourceRepository {
  [JsonProperty("url")]
  public string Url { get; init; }
  [JsonProperty("subfolder")]
  public string Subfolder { get; init; }
  [JsonProperty("checkout")]
  public string Checkout { get; init; }
  [JsonProperty("source")]
  [JsonConverter(typeof(StringEnumConverter))]
  public RepositorySource Source { get; init; }

  public bool IsLocal => Source == RepositorySource.Local;
  public bool IsRemote => Source == RepositorySource.Remote;
  public bool IsSymlink => Source == RepositorySource.Symlink;

  [System.Text.Json.Serialization.JsonConstructor]
  public AddonConfig(
    string url,
    string? subfolder = null,
    string? checkout = null,
    RepositorySource? source = null
  ) {
    Url = url;
    Subfolder = subfolder ?? App.DEFAULT_SUBFOLDER;
    Checkout = checkout ?? App.DEFAULT_CHECKOUT;
    Source = source ?? RepositorySource.Remote;
  }
}
