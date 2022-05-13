namespace GoDotAddon {
  using System.Text.Json;
  using CliFx;
  using CliFx.Attributes;
  using CliFx.Exceptions;
  using CliFx.Infrastructure;

  [Command]
  public class GoDotAddonApp : ICommand {
    public ValueTask ExecuteAsync(IConsole console) {
      console.Output.WriteLine("Hello, world!");
      return default;
    }
  }

  [Command("install")]
  public class InstallCommand : ICommand {
    [CommandOption("clear-cache", 'c',
      Description = "Clears the cache and downloads packages again.")]
    public bool ClearCache { get; init; } = false;

    public async ValueTask ExecuteAsync(IConsole console) {
      // Get the directory that we were executed in, not the one our app is
      // installed in.
      var directory = Info.App.WorkingDir;

      console.Output.WriteLine(directory);

      // Look for an `addons.json` file.
      var addonsJsonPath = Path.Combine(directory, "addons.json");
      if (!Info.App.FS.File.Exists(addonsJsonPath)) {
        throw new CommandException("No addons.json file found.");
      }


      var json = Info.App.FS.File.ReadAllText(addonsJsonPath);
      // deserialize json
      var addons = JsonSerializer.Deserialize<AddonsJson>(json);

      if (addons == null) {
        throw new CommandException("Failed to deserialize addons.json.");
      }

      if (addons.Addons.Count < 1) {
        throw new CommandException("No addons found in addons.json.");
      }

      var cacheDir = Path.Combine(directory, addons.Cache);

      if (!Info.App.FS.Directory.Exists(cacheDir)) {
        throw new CommandException(
          $"Cache directory ${cacheDir} does not exist."
        );
      }

      var existingAddonsInCache = new Dictionary<string, Addon>();

      // remove unneeded folders from the cache
      foreach (var addon in addons.Addons) {
        if (!addon.IsValid) {
          throw new CommandException("All addons must have a name and url.");
        }
        var name = addon.Name!;
        var addonDir = Path.Combine(cacheDir, name);
        if (Info.App.FS.Directory.Exists(addonDir)) {
          existingAddonsInCache.Add(name, addon);
        }
      }

      foreach ((var name, var addon) in existingAddonsInCache) {
        var main = addon.Main ?? IApp.DEFAULT_MAIN_BRANCH;
        await Info.App.Shell("git", "reset", "--hard", main);
        await Info.App.Shell("git", "checkout", main);
        if (addon.Tag != null) {
          var tag = addon.Tag;
          await Info.App.Shell("git", "fetch", "--all", "--tags", "--prune");
          await Info.App.Shell("git", "checkout", $"tags/${tag}");
        }
        else {
          await Info.App.Shell("git", "checkout", main);
          await Info.App.Shell("git", "pull");
        }
      }
    }
  }

  internal class GoDotAddon {
    private static async Task<int> Main(string[] args) {
      var app = new CliApplicationBuilder()
          .AddCommandsFromThisAssembly()
          .Build();

      // Console.WriteLine($"Launched from {Environment.CurrentDirectory}");
      // Console.WriteLine($"Physical location {AppDomain.CurrentDomain.BaseDirectory}");
      // Console.WriteLine($"AppContext.BaseDir {AppContext.BaseDirectory}");
      // Console.WriteLine($"Runtime Call {Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)}")

      return await app.RunAsync(args);
    }
  }
}
