namespace Chickensoft.GodotEnv.Features.Addons.Models;

using Chickensoft.GodotEnv.Common.Models;

public interface IAddon : IAsset {
  /// <summary>
  /// Name of the addon. The name is the addon's key in the addons configuration
  /// file, acting as an identifier to reference the addon. The app will use the
  /// addon's name as the folder name when copying or referencing the addon in
  /// the addons installation path.
  /// </summary>
  string Name { get; }
  /// <summary>
  /// Fully qualified path to the addons configuration file which declared
  /// this addon.
  /// </summary>
  string AddonsFilePath { get; }
  /// <summary>
  /// Directory within the asset that should be considered the addon.
  /// </summary>
  string Subfolder { get; }
}

/// <summary>
/// Represents a resolve addon. A resolved addon is an addon whose name and
/// addons file path are known.
/// </summary>
public record Addon : Asset, IAddon {
  /// <inheritdoc />
  public string Name { get; init; }
  /// <inheritdoc />
  public string AddonsFilePath { get; init; }
  /// <inheritdoc />
  public string Subfolder { get; init; }

  private static readonly char[] trimChars = ['/', '\\'];

  /// <summary>
  /// Create a new representation of a resolved addon.
  /// </summary>
  /// <param name="name">Addon name.</param>
  /// <param name="addonsFilePath">Addons file which declared the addon.</param>
  /// <param name="url">Addon url or path.</param>
  /// <param name="subfolder">Directory within the asset that should be
  /// considered the addon.</param>
  /// <param name="checkout">Git branch or tag to checkout.</param>
  /// <param name="source">Where the addon is copied or referenced from.</param>
  public Addon(
    string name,
    string addonsFilePath,
    string url,
    string subfolder,
    string checkout,
    AssetSource source
  ) : base(url, checkout, source) {
    Name = name;
    AddonsFilePath = addonsFilePath;
    Subfolder = subfolder.Trim(trimChars);
  }

  public override string ToString()
    =>
      $"Addon \"{Name}\" from `{AddonsFilePath}`" +
      $" at `{Subfolder}/` on branch `{Checkout}` of `{Url}`";
}
