namespace GoDotAddon {
  using System;
  using System.IO.Abstractions;
  using System.Threading.Tasks;
  using CliFx.Exceptions;
  using Medallion.Shell;
  using Newtonsoft.Json;

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
    public const string DEFAULT_PATH_DIR = "addons";
    public const string DEFAULT_ADDONS_FILE = "addons.json";
    public const string DEFAULT_CHECKOUT = "main";
    public const string DEFAULT_SUBFOLDER = "/";
    public const string ADDONS_LOCK_FILE = "addons.lock.json";

    string WorkingDir { get; }
    IFileSystem FS { get; }

    IAppShell CreateShell(string directory);
    T LoadFile<T>(string path);
  }

  public class App : IApp {
    public string WorkingDir { get; } = Environment.CurrentDirectory;
    public IFileSystem FS { get; } = new FileSystem();

    public IAppShell CreateShell(string directory)
      => new AppShell(new Shell(
        options: (options) => options.WorkingDirectory(directory))
      );

    public T LoadFile<T>(string path) {
      var contents = FS.File.ReadAllText(path);
      try {
        var file = JsonConvert.DeserializeObject<T>(contents);
        if (file == null) {
          throw new InvalidOperationException(
            $"Couldn't load file `{path}`"
          );
        }
        return file;
      }
      catch (Exception e) {
        throw new CommandException(
          $"Failed to deserialize {path}", innerException: e
        );
      }
    }
  }

  public interface IAppShell {
    Task Run(string executable, params string[] args);
    Task<CommandResult> ManualRun(string executable, params string[] args);
    Task RunRegardless(string executable, params string[] args);
  }

  public class AppShell : IAppShell {
    private readonly Shell _shell;
    public AppShell(Shell shell) => _shell = shell;

    public async Task Run(string executable, params string[] args) {
      var result = await ManualRun(executable, args);
      if (result.Success) { return; }
      throw new CommandException(
        $"Shell `command {executable} {string.Join(" ", args)}` failed."
      );
    }

    public async Task<CommandResult> ManualRun(
      string executable, params string[] args
    ) => await _shell.Run(executable, args).Task;

    public async Task RunRegardless(string executable, params string[] args)
      => await _shell.Run(executable, args).Task;
  }
}
