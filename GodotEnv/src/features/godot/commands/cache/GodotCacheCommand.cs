namespace Chickensoft.GodotEnv.Features.Godot.Commands;

using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Models;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using CliWrap;

[Command(
  "godot cache",
  Description = "Manage the Godot installation cache."
)]
public class GodotCacheCommand : ICommand, ICliCommand {
  public IExecutionContext ExecutionContext { get; set; } = default!;

  public GodotCacheCommand(IExecutionContext context) {
    ExecutionContext = context;
  }

  public async ValueTask ExecuteAsync(IConsole console) {
    var systemInfo = ExecutionContext.Godot.Platform.SystemInfo;

    var log = ExecutionContext.CreateLog(systemInfo, console);

    log.Print("");
    log.Warn(
      "Please use a subcommand to manage the Godot installations cache."
    );
    log.Print("");
    log.Print("To see a list of available subcommands:");
    log.Print("");
    log.Success("    godotenv godot cache --help");
    log.Print("");

    await Task.CompletedTask;
  }
}
