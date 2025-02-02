namespace Chickensoft.GodotEnv.Features.Godot.Commands;

using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Models;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using CliWrap;

[Command(
  "godot env",
  Description = "Manage the GODOT environment variable and get the symlink " +
    "path to the active version of Godot."
)]
public class GodotEnvCommand : ICommand, ICliCommand {
  public IExecutionContext ExecutionContext { get; set; } = default!;

  public GodotEnvCommand(IExecutionContext context) {
    ExecutionContext = context;
  }

  public async ValueTask ExecuteAsync(IConsole console) {
    var log = ExecutionContext.CreateLog(console);

    log.Print("");
    log.Warn(
      "Please use a subcommand to manage the Godot environment."
    );
    log.Print("");
    log.Print("To see a list of available subcommands:");
    log.Print("");
    log.Success("    godotenv godot env --help");
    log.Print("");

    await Task.CompletedTask;
  }
}
