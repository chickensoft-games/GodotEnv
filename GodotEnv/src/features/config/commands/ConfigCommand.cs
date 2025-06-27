namespace Chickensoft.GodotEnv.Features.Config.Commands;

using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Models;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

[Command("config", Description = "Manage godotenv configuration.")]
public class ConfigCommand : ICommand, ICliCommand {
  public IExecutionContext ExecutionContext { get; set; }

  public ConfigCommand(IExecutionContext executionContext) {
    ExecutionContext = executionContext;
  }

  public ValueTask ExecuteAsync(IConsole console) {
    var log = ExecutionContext.CreateLog(console);
    var output = console.Output;
    log.Print(string.Empty);
    log.Warn("Please use a subcommand to manage configuration.");
    log.Print(string.Empty);
    log.Print("To see a list of available subcommands:");
    log.Print(string.Empty);
    log.Success("    godotenv config --help");
    log.Print(string.Empty);
    return new();
  }
}
