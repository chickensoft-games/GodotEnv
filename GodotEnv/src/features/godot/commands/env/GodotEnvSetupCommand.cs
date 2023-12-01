namespace Chickensoft.GodotEnv.Features.Godot.Commands;

using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Models;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using CliWrap;

[Command(
  "godot env setup",
  Description = "Set the system-wide GODOT environment variable to the symlink which always points to the active version of Godot."
)]
public class GodotEnvSetupCommand
  : ICommand, ICliCommand, IWindowsElevationEnabled {
  public IExecutionContext ExecutionContext { get; set; } = default!;

  public bool IsWindowsElevationRequired => true;

  public GodotEnvSetupCommand(IExecutionContext context) {
    ExecutionContext = context;
  }

  public async ValueTask ExecuteAsync(IConsole console) {
    var godotRepo = ExecutionContext.Godot.GodotRepo;
    var platform = ExecutionContext.Godot.Platform;

    var log = ExecutionContext.CreateLog(console);
    await godotRepo.AddOrUpdateGodotEnvVariable(log);
  }
}
