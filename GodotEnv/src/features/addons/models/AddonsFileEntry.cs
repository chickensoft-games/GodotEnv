namespace Chickensoft.GodotEnv.Features.Addons.Models;

using Chickensoft.GodotEnv.Common.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

/// <summary>
/// Represents an addon entry in an addons configuration file.
/// </summary>
public record AddonsFileEntry : IAsset {
  [JsonProperty("url")]
  public string Url { get; }

  [JsonProperty("checkout")]
  public string Checkout { get; }

  [JsonProperty("source")]
  [JsonConverter(typeof(StringEnumConverter))]
  public AssetSource Source { get; }

  /// <summary>
  /// Which folder inside the addon that should actually be copied or
  /// referenced.
  /// </summary>
  [JsonProperty("subfolder")]
  public string Subfolder { get; }

  /// <summary>
  /// Create a new addon entry.
  /// </summary>
  /// <param name="url">Addon url or path.</param>
  /// <param name="subfolder">Which folder inside the addon that should actually
  /// be copied or referenced.</param>
  /// <param name="checkout">Git branch or tag to checkout.</param>
  /// <param name="source">Where the asset is copied or referenced from.</param>
  [JsonConstructor]
  public AddonsFileEntry(
    string url,
    string? subfolder = null,
    string? checkout = null,
    AssetSource? source = null
  ) {
    Url = url;
    Subfolder = subfolder ?? Defaults.SUBFOLDER;
    Checkout = checkout ?? Defaults.CHECKOUT;
    Source = source ?? Defaults.SOURCE;
  }

  /// <summary>
  /// Converts this addons file entry to an addon object.
  /// </summary>
  /// <param name="name">Name of the addon.</param>
  /// <param name="resolvedUrl">Resolved url.</param>
  /// <param name="addonsFilePath">Path of the addons file containing this
  /// addon entry.</param>
  /// <returns>Addon object.</returns>
  public Addon ToAddon(
    string name, string resolvedUrl, string addonsFilePath
  ) => new(
    name: name,
    addonsFilePath: addonsFilePath,
    url: resolvedUrl,
    subfolder: Subfolder,
    checkout: Checkout,
    source: Source
  );
}
