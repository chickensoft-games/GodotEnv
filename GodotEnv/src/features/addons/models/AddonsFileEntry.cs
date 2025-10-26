namespace Chickensoft.GodotEnv.Features.Addons.Models;

using System.Text.Json.Serialization;
using Chickensoft.GodotEnv.Common.Models;

/// <summary>
/// Represents an addon entry in an addons configuration file.
/// </summary>
public record AddonsFileEntry : IAsset
{
  [JsonPropertyName("url")]
  public required string Url { get; init; }

  [JsonPropertyName("checkout")]
  public string Checkout { get; init; } = Defaults.CHECKOUT;

  [JsonPropertyName("source")]
  public AssetSource Source { get; init; } = Defaults.SOURCE;

  /// <summary>
  /// Which folder inside the addon that should actually be copied or
  /// referenced.
  /// </summary>
  [JsonPropertyName("subfolder")]
  public string Subfolder { get; init; } = Defaults.SUBFOLDER;

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
