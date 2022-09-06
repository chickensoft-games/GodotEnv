namespace Chickensoft.Chicken {
  using System;
  using System.Threading.Tasks;
  using CliFx;
  using CliFx.Attributes;
  using CliFx.Exceptions;
  using CliFx.Infrastructure;

  [Command(
    "addon symbolic",
    Description = "Determines if a directory is a symlink."
  )]
  public class AddonSymbolicCommand : ICommand {
    private readonly IApp _app;

    [CommandParameter(0, Description = "Directory to check.")]
    public string Path { get; init; } = "";

    public AddonSymbolicCommand() => _app = new App();

    public AddonSymbolicCommand(IApp app) => _app = app;

    public async ValueTask ExecuteAsync(IConsole console) {
      var startDir = Environment.CurrentDirectory;
      var output = console.Output;

      var addonRepo = _app.CreateAddonRepo();

      var isSymlink = addonRepo.IsDirectorySymlink(Path);

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
}
