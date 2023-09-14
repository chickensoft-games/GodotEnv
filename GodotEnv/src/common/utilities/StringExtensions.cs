namespace Chickensoft.GodotEnv.Common.Utilities;

using System.IO;
using System.Linq;

public static class StringExtensions {
  public static string SanitizeForFs(this string filename) =>
    string.Concat(
      filename.Split(
        Path
          .GetInvalidFileNameChars()
          .Concat(Path.GetInvalidPathChars())
          .ToArray()
      )
    );
}
