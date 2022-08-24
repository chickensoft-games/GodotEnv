namespace Chickensoft.Chicken {
  using System.IO;
  using System.Text.RegularExpressions;
  using CaseExtensions;

  public record RequiredAddon {
    // Based on: https://serverfault.com/a/917253
    private static readonly Regex _urlRegex = new(
      @"^((https?|ssh|git|ftps?|git\+ssh|git\+https):\/\/)?(([^\/@]+)@)?" +
      @"([^\/:]+)[\/:]([^\/:]+)\/(.+).git\/?$"
    );

    /// <summary>Addon name.</summary>
    public string Name { get; init; }
    /// <summary>Git repository url.</summary>
    public string Url { get; init; }
    /// <summary>Git checkout (branch, tag, or other valid reference).</summary>
    public string Checkout { get; init; }
    /// <summary>
    /// Subfolder of the git repo to copy for the installation. Defaults to "/"
    /// </summary>
    public string Subfolder { get; init; }
    /// <summary>Path of the config file that required this addon.</summary>
    public string ConfigFilePath { get; init; }

    /// <summary>Deterministic id based only on the url.</summary>
    public string Id {
      get {
        var match = _urlRegex.Match(Url);
        return (
          match.Success
            ? match.Groups[6].Value + "_" + match.Groups[7].Value
            : MakeValidFolderName(Url.ToSnakeCase())
        ).ToSnakeCase();
      }
    }

    public RequiredAddon(
      string name,
      string configFilePath,
      string url,
      string checkout,
      string subfolder
    ) {
      Name = name;
      ConfigFilePath = configFilePath;
      Url = url;
      Checkout = checkout;
      Subfolder = Path.TrimEndingDirectorySeparator(subfolder).TrimEnd(
        Path.DirectorySeparatorChar,
        Path.AltDirectorySeparatorChar
      );
    }

    public override string ToString() => $"Addon \"{Name}\" from " +
      $"`{ConfigFilePath}` at `{Subfolder}/` on branch `{Checkout}` of `{Url}`";

    // Credit: https://stackoverflow.com/a/33353841
    private static string MakeValidFolderName(string value) {
      foreach (var c in System.IO.Path.GetInvalidFileNameChars()) {
        value = value.Replace(c.ToString(), string.Empty);
      }

      foreach (var c in System.IO.Path.GetInvalidPathChars()) {
        value = value.Replace(c.ToString(), string.Empty);
      }

      return value;
    }
  }
}
