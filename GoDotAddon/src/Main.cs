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
    public ValueTask ExecuteAsync(IConsole console) {
      // Get the directory that we were executed in, not the one our app is
      // installed in.
      var directory = Info.EnvironmentDirectory;

      console.Output.WriteLine(directory);

      // Look for an `addons.json` file.
      var addonsJsonPath = Path.Combine(directory, "addons.json");
      if (!Info.FileSystem.File.Exists(addonsJsonPath)) {
        throw new CommandException("No addons.json file found.");
      }

      var json = Info.FileSystem.File.ReadAllText(addonsJsonPath);
      // deserialize json
      var addons = JsonSerializer.Deserialize<AddonsJson>(json);
      return default;
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
