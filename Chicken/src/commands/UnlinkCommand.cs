namespace Chickensoft.Chicken {
  using System;
  using System.Threading.Tasks;
  using CliFx;
  using CliFx.Attributes;
  using CliFx.Exceptions;
  using CliFx.Infrastructure;

  [Command("egg unlink", Description = "Remove a symbolic link to a directory.")]
  public class UnlinkCommand : ICommand {
    private readonly IApp _app;

    [CommandParameter(0, Description = "Directory path symlink.")]
    public string Path { get; init; } = "";

    public UnlinkCommand() => _app = new App();

    public UnlinkCommand(IApp app) => _app = app;

    public async ValueTask ExecuteAsync(IConsole console) {
      var startDir = Environment.CurrentDirectory;
      var output = console.Output;

      var addonRepo = _app.CreateAddonRepo();

      if (!_app.FS.Directory.Exists(Path)) {
        throw new CommandException(
          $"Directory `{Path}` does not exist."
        );
      }

      if (!addonRepo.IsDirectorySymlink(Path)) {
        throw new CommandException(
          $"Directory `{Path}` is not a symlink."
        );
      }

      addonRepo.DeleteDirectory(Path);

      await ValueTask.CompletedTask;
    }
  }
}
