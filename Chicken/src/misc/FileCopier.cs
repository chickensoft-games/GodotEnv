namespace Chickensoft.Chicken;

using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CliFx.Exceptions;

public interface IFileCopier {
  Task Copy(IFileSystem fs, IShell shell, string source, string destination);

  List<string> CopyDotNet(
    IFileSystem fs,
    string source,
    string destination,
    ISet<string> exclusions
  );

  /// <summary>
  /// Recursively removes all directories with the given
  /// <paramref name="directoryName"/> from inside <paramref name="source"/>.
  /// </summary>
  /// <param name="fs">File system.</param>
  /// <param name="source">Source directory.</param>
  /// <param name="directoryName">Subdirectory name to remove recursively.
  /// </param>
  void RemoveDirectories(IFileSystem fs, string source, string directoryName);
}

public class FileCopier : IFileCopier {
  public FileCopier() { }

  public async Task Copy(
    IFileSystem fs, IShell shell, string source, string destination
  ) {
    if (fs.Path.DirectorySeparatorChar == '\\') {
      var result = await shell.RunUnchecked(
        "robocopy", source, destination, "/e", "/xd", ".git"
      );
      if (result.ExitCode >= 8) {
        // Robocopy has non-traditional exit codes
        throw new CommandException(
          $"Failed to copy `{source}` to `{destination}`"
        );
      }
    }
    else {
      await shell.Run(
        "rsync", "-av", source, destination, "--exclude", ".git"
      );
    }
  }

  public List<string> CopyDotNet(
    IFileSystem fs,
    string source,
    string destination,
    ISet<string> exclusions
  ) {
    var results = new List<string>();
    var sourceDir = fs.DirectoryInfo.FromDirectoryName(source);

    if (!sourceDir.Exists) {
      throw new DirectoryNotFoundException(
        $"Source directory not found: {sourceDir.FullName}"
      );
    }

    var dirs = sourceDir.GetDirectories();

    // Create the destination directory
    fs.Directory.CreateDirectory(destination);

    foreach (var file in sourceDir.GetFiles()) {
      var targetFilePath = Path.Combine(destination, file.Name);
      if (exclusions.Any(exclusion => MatchesGlob(file.Name, exclusion))) {
        continue;
      }
      results.Add(targetFilePath.TrimStart(Path.DirectorySeparatorChar));
      file.CopyTo(targetFilePath);
    }

    foreach (var subDir in dirs) {
      var newDestinationDir = Path.Combine(destination, subDir.Name);
      if (exclusions.Any(exclusion => MatchesGlob(subDir.Name, exclusion))) {
        continue;
      }
      results.Add(newDestinationDir.Trim(Path.DirectorySeparatorChar) + '/');
      results.AddRange(
        CopyDotNet(fs, subDir.FullName, newDestinationDir, exclusions)
      );
    }

    return results;
  }

  public void RemoveDirectories(
    IFileSystem fs, string source, string directoryName
  ) {
    var sourceDir = fs.DirectoryInfo.FromDirectoryName(source);

    if (!sourceDir.Exists) {
      throw new DirectoryNotFoundException(
        $"Source directory not found: {sourceDir.FullName}"
      );
    }

    var dirs = sourceDir.GetDirectories(
      directoryName, SearchOption.AllDirectories
    );

    var remaining = new List<IDirectoryInfo>();
    foreach (var dir in dirs) {
      if (dir.Name == directoryName) {
        dir.Delete(true);
      }
    }
  }

  /// <summary>
  /// Checks a string to see if it matches a glob pattern.
  /// Credit: https://stackoverflow.com/a/4146349
  /// </summary>
  /// <param name="str">The string in question.</param>
  /// <param name="pattern">The glob pattern.</param>
  /// <returns>True if the string satisfies the glob pattern.</returns>
  public static bool MatchesGlob(string str, string pattern) => new Regex(
    "^" +
    Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") +
    "$",
    RegexOptions.IgnoreCase | RegexOptions.Singleline
  ).IsMatch(str);
}
