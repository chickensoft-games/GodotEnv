
namespace Chickensoft.Chicken;
using System.IO;
using System.Text.RegularExpressions;
using CaseExtensions;
using Newtonsoft.Json;

public enum RepositorySource {
  [JsonProperty("local")]
  Local,
  [JsonProperty("remote")]
  Remote,
  [JsonProperty("symlink")]
  Symlink
}

public interface ISourceRepository {
  string Url { get; init; }
  string Checkout { get; init; }
  RepositorySource Source { get; init; }
  public bool IsLocal => Source == RepositorySource.Local;
  public bool IsRemote => Source == RepositorySource.Remote;
  public bool IsSymlink => Source == RepositorySource.Symlink;

  // Git url regex.
  // Based on: https://serverfault.com/a/917253
  public static readonly Regex UrlRegex = new(
    @"^((https?|ssh|git|ftps?|git\+ssh|git\+https):\/\/)?(([^\/@]+)@)?" +
    @"([^\/:]+)[\/:]([^\/:]+)\/(.+).git\/?$"
  );

  /// <summary>Deterministic id based only on the url.</summary>
  public string Id {
    get {
      var match = UrlRegex.Match(Url);
      return (
        match.Success
          ? match.Groups[6].Value + "_" + match.Groups[7].Value
          : new DirectoryInfo(Url).Name
      ).ToSnakeCase();
    }
  }

  /// <summary>
  /// Returns the fully rooted file path if the source repository url
  /// represents a non-remote path, otherwise it just returns the url.
  /// </summary>
  /// <param name="app">Command line application context.</param>
  /// <returns>Resolved source repository path or url.</returns>
  public string SourcePath(IApp app) => IsLocal
      ? app.GetRootedPath(Url, app.WorkingDir)
      : Url;
}

// <summary>
/// Represents a git repository somewhere.
/// </summary>
/// <param name="Url">Git repository url.</param>
/// <param name="Checkout">Git checkout (branch, tag, or other valid reference).
/// </param>
/// <param name="Source">Source of the repository.</param>
public record SourceRepository : ISourceRepository {

  public string Url { get; init; }

  public string Checkout { get; init; }

  public RepositorySource Source { get; init; }

  public SourceRepository(string url, string? checkout) {
    Url = url;
    Checkout = checkout ?? App.DEFAULT_CHECKOUT;
    Source = ISourceRepository.UrlRegex.IsMatch(url)
      ? RepositorySource.Remote
      : RepositorySource.Local;
  }
}
