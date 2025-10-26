namespace Chickensoft.GodotEnv.Features.Godot.Commands;

using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Models;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using CliWrap;

[Command(
  "godot env target",
  Description = "Get the path to the active version of Godot."
)]
public class GodotEnvTargetCommand : ICommand, ICliCommand
{
  public IExecutionContext ExecutionContext { get; set; } = default!;

  public GodotEnvTargetCommand(IExecutionContext context)
  {
    ExecutionContext = context;
  }

  public async ValueTask ExecuteAsync(IConsole console)
  {
    var log = ExecutionContext.CreateLog(console);
    var godotRepo = ExecutionContext.Godot.GodotRepo;

    if (godotRepo.IsGodotSymlinkTargetAvailable)
    {
      log.Print(godotRepo.GodotSymlinkTarget);
    }
    else
    {
      log.Warn("Could not determine current target Godot version.");
    }

    await Task.CompletedTask;
  }
}
