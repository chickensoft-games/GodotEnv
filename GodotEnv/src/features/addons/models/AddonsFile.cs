namespace Chickensoft.GodotEnv.Features.Addons.Models;

using System.Collections.Generic;
using System.Text.Json.Serialization;
using Chickensoft.GodotEnv.Common.Models;

/// <summary>
/// Addons configuration file loaded from an <c>addons.json</c> file.
/// </summary>
public class AddonsFile {
  /// <summary>
  /// Addons entries. Each entry contains information about the addon to install
  /// and is keyed by the name of the addon.
  /// </summary>
  [JsonPropertyName("addons")]
  public Dictionary<string, AddonsFileEntry> Addons { get; }

  /// <summary>Cache path, relative to the project.</summary>
  [JsonPropertyName("cache")]
  public string CacheRelativePath { get; }

  /// <summary>Addons installation path, relative to the project.</summary>
  [JsonPropertyName("path")]
  public string PathRelativePath { get; }

  /// <summary>
  /// Creates a new addons file.
  /// </summary>
  /// <param name="addons">Addons entries.</param>
  /// <param name="cacheRelativePath">Cache path, relative to the project.
  /// </param>
  /// <param name="pathRelativePath">Addons installation path, relative to the
  /// project.</param>
  [JsonConstructor]
  public AddonsFile(
    Dictionary<string, AddonsFileEntry>? addons = null,
    string? cacheRelativePath = null,
    string? pathRelativePath = null
  ) {
    Addons = addons ?? [];
    CacheRelativePath = cacheRelativePath ?? Defaults.CACHE_PATH;
    PathRelativePath = pathRelativePath ?? Defaults.ADDONS_PATH;
  }
}
