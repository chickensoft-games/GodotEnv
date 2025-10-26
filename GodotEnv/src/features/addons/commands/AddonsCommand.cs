namespace Chickensoft.GodotEnv.Features.Addons.Commands;

using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Models;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

[Command("addons", Description = "Manage addons for a Godot project.")]
public class AddonsCommand : ICommand, ICliCommand
{
  public IExecutionContext ExecutionContext { get; set; } = default!;

  public AddonsCommand(IExecutionContext context)
  {
    ExecutionContext = context;
  }

  public ValueTask ExecuteAsync(IConsole console)
  {
    var log = ExecutionContext.CreateLog(console);
    log.Print("");
    log.Warn("Please use a subcommand to manage addons.");
    log.Print("");
    log.Print("To see a list of available subcommands:");
    log.Print("");
    log.Success("    godotenv addons --help");
    log.Print("");
    return new();
  }
}
