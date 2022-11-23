namespace Chickensoft.Chicken;
using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;

[Command("addons link", Description = "Create a symbolic link to a directory.")]
public class AddonsLinkCommand : ICommand {
  public readonly IApp App;
  public readonly IFileSystem Fs;

  [CommandParameter(
    0,
    Description = "Directory source (original directory you are linking to)."
  )]
  public string Source { get; init; } = "";

  [CommandParameter(
    1,
    Description = "Directory target (directory which points to the source " +
    "directory)."
  )]
  public string Target { get; init; } = "";

  public AddonsLinkCommand() {
    App = new App();
    Fs = new FileSystem();
  }

  public AddonsLinkCommand(IApp app, IFileSystem fs) {
    App = app;
    Fs = fs;
  }

  public async ValueTask ExecuteAsync(IConsole console) {
    var startDir = App.WorkingDir;
    var output = console.Output;

    if (Fs.Directory.Exists(Target)) {
      throw new CommandException(
        $"Target directory `{Target}` already exists."
      );
    }

    if (!Fs.Directory.Exists(Source)) {
      throw new CommandException(
        $"Source directory `{Source}` does not exist."
      );
    }

    App.CreateSymlink(Fs, Target, Source);

    console.ForegroundColor = ConsoleColor.Green;
    console.Output
      .WriteLine($"Created symlink from `{Source}` to `{Target}`.");

    await ValueTask.CompletedTask;
  }
}
