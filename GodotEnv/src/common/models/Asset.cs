namespace Chickensoft.GodotEnv.Common.Models;

using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using CaseExtensions;
using Chickensoft.GodotEnv.Common.Utilities;
using Newtonsoft.Json;

/// <summary>Represents how an asset is copied or accessed.</summary>
public enum AssetSource {
  /// <summary>Asset is copied from a path on the local machine.</summary>
  [JsonProperty("local")]
  Local = 0,
  /// <summary>Asset is copied from a remote git repository.</summary>
  [JsonProperty("remote")]
  Remote = 1,
  /// <summary>
  /// Asset is directly referenced via a symbolic link from a path on the
  /// local machine.
  /// </summary>
  [JsonProperty("symlink")]
  Symlink = 2,
  /// <summary>Asset is a zip file.</summary>
  [JsonProperty("zip")]
  Zip = 3,
}

/// <summary>
/// Represents a resource accessed via a remote path, local path, or symlink.
/// </summary>
public partial interface IAsset {
  /// <summary>
  /// Asset path if <see cref="Source" /> is <see cref="AssetSource.Local" />
  /// or <see cref="AssetSource.Symlink" />. Otherwise, if
  /// <see cref="Source" /> is <see cref="AssetSource.Remote" />, the url is
  /// the git repository url.
  /// </summary>
  string Url { get; }

  /// <summary>
  /// Git branch or tag to checkout (only meaningful if the asset is a valid
  /// git repository).
  /// </summary>
  string Checkout { get; }

  /// <summary>Where the asset is copied or referenced from.</summary>
  AssetSource Source { get; }

  /// <summary>
  /// True if the asset is copied from a path on the local machine.
  /// </summary>
  bool IsLocal => Source == AssetSource.Local;

  /// <summary>
  /// True if the asset is copied from a remote git url.
  /// </summary>
  bool IsRemote => Source == AssetSource.Remote;

  /// <summary>
  /// True if the asset is referenced from a symlink on the local machine.
  /// </summary>
  bool IsSymlink => Source == AssetSource.Symlink;

  /// <summary>True if the asset is a zip file.</summary>
  bool IsZip => Source == AssetSource.Zip;

  /// <summary>Lowercase version of the url.</summary>
  string NormalizedUrl => Url.ToLower(CultureInfo.InvariantCulture);

  /// <summary>Deterministic id based only on the url.</summary>
  string Id {
    get {
      var match = UrlRegex.Match(Url);
      return (
        match.Success
          ? match.Groups[6].Value + "_" + match.Groups[7].Value
          : new DirectoryInfo(Url).Name
      ).SanitizeForFs().ToSnakeCase();
    }
  }

  /// <summary>
  /// Git URL regex. If a url matches this regex, it is considered a valid
  /// git repository url. <br />
  /// Credit: https://serverfault.com/a/917253
  /// </summary>
  public static readonly Regex UrlRegex = urlRegex();

  [GeneratedRegex(@"^((https?|ssh|git|ftps?|git\+ssh|git\+https):\/\/)?(([^\/@]+)@)?([^\/:]+)[\/:]([^\/:]+)\/(.+).git\/?$")]
  private static partial Regex urlRegex();
}

/// <summary>
/// Represents a resource accessed via a remote path, local path, or symlink.
/// </summary>
public abstract record Asset : IAsset {
  /// <inheritdoc />
  public string Url { get; init; }

  /// <inheritdoc />
  public string Checkout { get; init; }

  /// <inheritdoc />
  public AssetSource Source { get; init; }

  public Asset(string url, string checkout, AssetSource source) {
    Url = url;
    Checkout = checkout;
    Source = source;
  }
}
