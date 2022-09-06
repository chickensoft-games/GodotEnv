namespace Chickensoft.Chicken {
  using System;
  using System.Threading.Tasks;
  using CliFx;
  using CliFx.Attributes;
  using CliFx.Exceptions;
  using CliFx.Infrastructure;

  [Command("addon link", Description = "Create a symbolic link to a directory.")]
  public class AddonLinkCommand : ICommand {
    private readonly IApp _app;

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

    public AddonLinkCommand() => _app = new App();

    public AddonLinkCommand(IApp app) => _app = app;

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

      console.ForegroundColor = ConsoleColor.Green;
      console.Output
        .WriteLine($"Created symlink from `{Source}` to `{Target}`.");

      await ValueTask.CompletedTask;
    }
  }
}
