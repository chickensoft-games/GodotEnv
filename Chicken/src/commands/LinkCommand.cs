namespace Chickensoft.Chicken {
  using System;
  using System.Threading.Tasks;
  using CliFx;
  using CliFx.Attributes;
  using CliFx.Exceptions;
  using CliFx.Infrastructure;

  [Command("egg link", Description = "Create a symbolic link to a directory.")]
  public class LinkCommand : ICommand {
    private readonly IApp _app;

    [CommandParameter(0, Description = "Directory source (original directory you are linking to).")]
    public string Source { get; init; } = "";

    [CommandParameter(1, Description = "Directory target (directory which points to the source directory).")]
    public string Target { get; init; } = "";

    public LinkCommand() => _app = new App();

    public LinkCommand(IApp app) => _app = app;

    public async ValueTask ExecuteAsync(IConsole console) {
      var startDir = Environment.CurrentDirectory;
      var output = console.Output;

      var addonRepo = _app.CreateAddonRepo();

      if (_app.FS.Directory.Exists(Target)) {
        throw new CommandException(
          $"Target directory `{Target}` already exists."
        );
      }

      if (!_app.FS.Directory.Exists(Source)) {
        throw new CommandException(
          $"Source directory `{Source}` does not exist."
        );
      }

      addonRepo.CreateSymlink(Target, Source);

      await ValueTask.CompletedTask;
    }
  }
}