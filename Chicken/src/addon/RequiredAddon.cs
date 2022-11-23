namespace Chickensoft.Chicken;
using System.IO;

public record RequiredAddon : ISourceRepository {
  /// <summary>Addon name.</summary>
  public string Name { get; init; }
  /// <summary>Addon url (git url or local path).</summary>
  public string Url { get; init; }
  /// <summary>Git checkout spec.</summary>
  public string Checkout { get; init; }
  /// <summary>Addon source location.</summary>
  public RepositorySource Source { get; init; }
  /// <summary>
  /// Subfolder of the git repo to copy for the installation. Defaults to "/"
  /// </summary>
  public string Subfolder { get; init; }
  /// <summary>Path of the config file that required this addon.</summary>
  public string ConfigFilePath { get; init; }

  public RequiredAddon(
    string name,
    string configFilePath,
    string url,
    string checkout,
    string subfolder,
    RepositorySource source = RepositorySource.Remote
  ) {
    Name = name;
    ConfigFilePath = configFilePath;
    Url = url;
    Checkout = checkout;
    Source = source;
    Subfolder = Path.TrimEndingDirectorySeparator(subfolder).TrimEnd(
      Path.DirectorySeparatorChar,
      Path.AltDirectorySeparatorChar
    );
  }

  public override string ToString() => $"Addon \"{Name}\" from " +
    $"`{ConfigFilePath}` at `{Subfolder}/` on branch `{Checkout}` of `{Url}`";
}
