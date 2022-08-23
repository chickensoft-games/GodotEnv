namespace Chickensoft.GoDotAddon {
  using System.IO;
  using System.Text.RegularExpressions;
  using CaseExtensions;

  public record RequiredAddon {
    // Based on: https://serverfault.com/a/917253
    private static readonly Regex _urlRegex = new(
      @"^((https?|ssh|git|ftps?|git\+ssh|git\+https):\/\/)?(([^\/@]+)@)?" +
      @"([^\/:]+)[\/:]([^\/:]+)\/(.+).git\/?$"
    );

    public string Name { get; init; }
    public string Url { get; init; }
    public string Checkout { get; init; }
    public string Subfolder { get; init; }
    public string ConfigFilePath { get; init; }

    // Deterministic id based on url, username, and repository name
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
