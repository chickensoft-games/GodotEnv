namespace Chickensoft.GodotEnv.Features.Godot.Commands;

using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Models;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using CliWrap;

[Command(
  "godot cache clear",
  Description = "Clear the Godot installation cache."
)]
public class GodotCacheClearCommand : ICommand, ICliCommand {
  public IExecutionContext ExecutionContext { get; set; } = default!;

  public GodotCacheClearCommand(IExecutionContext context) {
    ExecutionContext = context;
  }

  public async ValueTask ExecuteAsync(IConsole console) {
    var log = ExecutionContext.CreateLog(console);

    log.Print("");
    log.Info("Clearing Godot installation cache...");
    log.Print("");
    ExecutionContext.Godot.GodotRepo.ClearCache();
    log.Success("Godot installation cache cleared.");

    await Task.CompletedTask;
  }
}
