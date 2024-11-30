namespace Chickensoft.GodotEnv.Common.Utilities;

using System.IO;

public static class StringExtensions {
  public static string SanitizeForFs(this string filename) =>
    string.Concat(
      filename.Split([
        .. Path.GetInvalidFileNameChars(),
        .. Path.GetInvalidPathChars(),
      ])
    );
}
