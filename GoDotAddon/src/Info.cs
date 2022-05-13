namespace GoDotAddon {
  using System;
  using System.IO.Abstractions;
  using System.Threading.Tasks;
  using Medallion.Shell;

  /// <summary>
  /// Contains information used by the GoDotAddon app. Static fields can be
  /// overridden for testing purposes — a makeshift sort of dependency injection
  /// for a simple CLI app.
  /// </summary>
  public static class Info {
    // These can be overridden for testing.
#pragma warning disable CA2211
    public static IApp App = new App();
#pragma warning restore CA2211
  }

  public interface IApp {
    public const string DEFAULT_CACHE_DIR = ".addons";
    public const string DEFAULT_ADDONS_DIR = "addons";
    public const string DEFAULT_MAIN_BRANCH = "main";

    string WorkingDir { get; }
    FileSystem FS { get; }

    Task<bool> Shell(string executable, params string[] args);
  }

  public class App : IApp {

    public string WorkingDir { get; } = Environment.CurrentDirectory;
    public FileSystem FS { get; } = new FileSystem();

    public async Task<bool> Shell(string executable, params string[] args) {
      var command = Command.Run(executable, args);
      var result = await command.Task;
      if (result.Success) { return true; }
      return false;
    }

  }
}
