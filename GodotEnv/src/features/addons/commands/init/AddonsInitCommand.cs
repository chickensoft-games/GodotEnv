namespace Chickensoft.GodotEnv.Features.Addons.Commands;

using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Models;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

[Command(
  "addons init",
  Description =
    "Create a new addons configuration file (if not present), create/adjust " +
    ".gitignore to prevent addons from entering source control, and add a " +
    ".editorconfig file in the addons directory to prevent addons from " +
    "causing unnecessary IDE analyzer warnings. Runs non-destructively."
)]
public class AddonsInitCommand : ICommand, ICliCommand
{
  public IExecutionContext ExecutionContext { get; set; } = default!;

  public AddonsInitCommand(IExecutionContext context)
  {
    ExecutionContext = context;
  }

  public async ValueTask ExecuteAsync(IConsole console)
  {
    var log = ExecutionContext.CreateLog(console);
    var repo = ExecutionContext.Addons.AddonsFileRepo;

    log.Print("");
    log.Info("Initializing Godot addons (if needed)...");
    log.Print("");

    var path = repo.CreateAddonsConfigurationStartingFile(
      projectPath: ExecutionContext.WorkingDir
    );

    log.Print("");
    log.Success("Done!");
    log.Print("");
    log.Success(path);
    log.Print("");

    await Task.CompletedTask;
  }
}
