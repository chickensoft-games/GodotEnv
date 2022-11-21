namespace Chickensoft.Chicken;
using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;

[Command("addons unlink", Description = "Remove a symbolic link to a directory.")]
public class AddonsUnlinkCommand : ICommand {
  public readonly IApp App;
  public readonly IFileSystem Fs;

  [CommandParameter(0, Description = "Directory path symlink.")]
  public string Path { get; init; } = "";

  public AddonsUnlinkCommand() {
    App = new App();
    Fs = new FileSystem();
  }

  public AddonsUnlinkCommand(IApp app, IFileSystem fs) {
    App = app;
    Fs = fs;
  }

  public async ValueTask ExecuteAsync(IConsole console) {
    var startDir = App.WorkingDir;
    var output = console.Output;

    if (!Fs.Directory.Exists(Path)) {
      throw new CommandException(
        $"Directory `{Path}` does not exist."
      );
    }

    if (!App.IsDirectorySymlink(Fs, Path)) {
      throw new CommandException(
        $"Directory `{Path}` is not a symlink."
      );
    }

    App.DeleteDirectory(Fs, Path);

    console.ForegroundColor = ConsoleColor.Green;
    console.Output
      .WriteLine($"Successfully removed symlink at `{Path}`.");

    await ValueTask.CompletedTask;
  }
}
