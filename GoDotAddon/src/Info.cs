namespace GoDotAddon {
  using System.IO.Abstractions;

  /// <summary>
  /// Contains information used by the GoDotAddon app. Static fields can be
  /// overridden for testing purposes — a makeshift sort of dependency injection
  /// for a simple CLI app.
  /// </summary>
  public static class Info {
    // These can be overridden for testing.
#pragma warning disable CA2211
    public static string EnvironmentDirectory = GetEnvironmentDirectory();
    public static IFileSystem FileSystem = new FileSystem();
#pragma warning restore CA2211

    public const string DEFAULT_CACHE_DIR = ".addons";

    private static string GetEnvironmentDirectory()
      => Environment.CurrentDirectory;
  }
}
