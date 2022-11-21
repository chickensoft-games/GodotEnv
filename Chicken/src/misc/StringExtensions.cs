namespace Chickensoft.Chicken;

using System;
using System.IO;

/// <summary>
/// Helps determine if a file path is nested inside a directory. Based on
/// https://stackoverflow.com/a/31941159/20259396 (with modifications to behave
/// the way I need it to).
/// </summary>
public static class StringExtensions {
  /// <summary>
  /// Returns true if <paramref name="path"/> starts with the path
  /// <paramref name="baseDirPath"/>. The comparison is case-insensitive,
  /// handles / and \ slashes as folder separators and only matches if the base
  /// dir folder name is matched exactly ("c:\foobar\file.txt" is not a sub
  /// path of "c:\foo").
  /// </summary>
  public static bool IsWithinPath(this string path, string baseDirPath) {
    var normalizedPath = NormalizePath(path);
    var normalizedBaseDirPath
      = NormalizePath(baseDirPath) + Path.DirectorySeparatorChar;

    return normalizedPath.StartsWith(
      normalizedBaseDirPath, StringComparison.OrdinalIgnoreCase
    );
  }

  private static string NormalizePath(string path)
    => Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar);

  public static string SanitizePath(this string filePath, string baseDirPath) {
    filePath = filePath.Replace("~", "");
    filePath = filePath.Replace(":", "");
    filePath = filePath.Replace("../", "");
    filePath = filePath.Replace("..\\", "");
    filePath = filePath.Replace("./", "");
    filePath = filePath.Replace(".\\", "");
    filePath = filePath.TrimStart('/');
    filePath = filePath.TrimStart('\\');
    return Path.Join(baseDirPath, filePath);
  }
}
