namespace Chickensoft.GoDotAddon {
  using System;
  using System.IO.Abstractions;
  using CliFx.Exceptions;
  using Newtonsoft.Json;

  public interface IApp {
    public const string DEFAULT_CACHE_PATH = ".addons";
    public const string DEFAULT_ADDONS_PATH = "addons";
    public const string DEFAULT_CHECKOUT = "main";
    public const string DEFAULT_SUBFOLDER = "/";
    public const string ADDONS_CONFIG_FILE = "addons.json";
    public const string ADDONS_LOCK_FILE = "addons.lock.json";

    string WorkingDir { get; }
    IFileSystem FS { get; }

    IShell CreateShell(string workingDir);
    T LoadFile<T>(string path);
    void SaveFile(string path, string contents);
  }

  public class App : IApp {
    public string WorkingDir { get; } = Environment.CurrentDirectory;
    public IFileSystem FS { get; } = new FileSystem();

    public App() { }

    public App(string workingDir, IFileSystem fs) {
      WorkingDir = workingDir;
      FS = fs;
    }

    public T LoadFile<T>(string path) {
      try {
        var contents = FS.File.ReadAllText(path);
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

    public void SaveFile(string path, string contents) {
      try {
        FS.File.WriteAllText(path, contents);
      }
      catch (Exception e) {
        throw new CommandException(
          $"Failed to write to {path}", innerException: e
        );
      }
    }

    public virtual IShell CreateShell(string workingDir)
      => new Shell(new ProcessRunner(), workingDir);
  }
}
