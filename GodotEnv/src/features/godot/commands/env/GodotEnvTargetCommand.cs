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
public class GodotEnvTargetCommand : ICommand, ICliCommand {
  public IExecutionContext ExecutionContext { get; set; } = default!;

  public GodotEnvTargetCommand(IExecutionContext context) {
    ExecutionContext = context;
  }

  public async ValueTask ExecuteAsync(IConsole console) {
    var systemInfo = ExecutionContext.Godot.Platform.SystemInfo;
    var log = ExecutionContext.CreateLog(systemInfo, console);
    var godotRepo = ExecutionContext.Godot.GodotRepo;

    log.Print(godotRepo.GodotSymlinkTarget);

    await Task.CompletedTask;
  }
}
