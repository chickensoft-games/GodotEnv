namespace Chickensoft.Chicken;
using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;

[Command(
  "addons symbolic",
  Description = "Determines if a directory is a symlink."
)]
public class AddonsSymbolicCommand : ICommand {
  public readonly IApp App;
  public readonly IFileSystem Fs;

  [CommandParameter(0, Description = "Directory to check.")]
  public string Path { get; init; } = "";

  public AddonsSymbolicCommand() {
    App = new App();
    Fs = new FileSystem();
  }

  public AddonsSymbolicCommand(IApp app, IFileSystem fs) {
    App = app;
    Fs = fs;
  }

  public async ValueTask ExecuteAsync(IConsole console) {
    var startDir = App.WorkingDir;
    var output = console.Output;

    var isSymlink = App.IsDirectorySymlink(Fs, Path);

    if (isSymlink) {
      console.ForegroundColor = ConsoleColor.Green;
      output.WriteLine($"YES: `{Path}` is a symlink");
    }
    else {
      throw new CommandException(
        $"NO: `{Path}` is not a symlink"
      );
    }

    await ValueTask.CompletedTask;
  }
}
