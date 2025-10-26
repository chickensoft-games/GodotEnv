namespace Chickensoft.GodotEnv.Features.Godot.Commands;

using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Models;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using CliWrap;

[Command(
  "godot env get",
  Description = "Show the contents of the GODOT user environment variable."
)]
public class GodotEnvGetCommand : ICommand, ICliCommand
{
  public IExecutionContext ExecutionContext { get; set; } = default!;

  public GodotEnvGetCommand(IExecutionContext context)
  {
    ExecutionContext = context;
  }

  public async ValueTask ExecuteAsync(IConsole console)
  {
    var log = ExecutionContext.CreateLog(console);
    var godotRepo = ExecutionContext.Godot.GodotRepo;

    var godotEnvVar = await godotRepo.GetGodotEnvVariable();

    log.Print(godotEnvVar);
  }
}
