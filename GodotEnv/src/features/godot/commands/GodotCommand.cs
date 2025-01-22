namespace Chickensoft.GodotEnv.Features.Addons.Commands;

using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Models;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

[Command("godot", Description = "Manage Godot installations.")]
public class GodotCommand : ICommand, ICliCommand {
  public IExecutionContext ExecutionContext { get; set; } = default!;

  public GodotCommand(IExecutionContext context) {
    ExecutionContext = context;
  }

  public ValueTask ExecuteAsync(IConsole console) {
    var systemInfo = ExecutionContext.Godot.Platform.SystemInfo;

    var log = ExecutionContext.CreateLog(systemInfo, console);
    var output = console.Output;
    log.Print("");
    log.Warn("Please use a subcommand to manage Godot installations.");
    log.Print("");
    log.Print("To see a list of available subcommands:");
    log.Print("");
    log.Success("    godotenv godot --help");
    log.Print("");
    return new();
  }
}
