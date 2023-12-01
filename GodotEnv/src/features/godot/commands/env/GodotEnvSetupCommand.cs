namespace Chickensoft.GodotEnv.Features.Godot.Commands;

using System.Diagnostics;
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
public class GodotEnvSetupCommand : ICommand, ICliCommand {
  public IExecutionContext ExecutionContext { get; set; } = default!;

  public GodotEnvSetupCommand(IExecutionContext context) {
    ExecutionContext = context;
  }

  public async ValueTask ExecuteAsync(IConsole console) {
    var godotRepo = ExecutionContext.Godot.GodotRepo;
    var platform = ExecutionContext.Godot.Platform;

    // The setup command must be run with the admin role on Windows
    // To be able to debug, godotenv is not elevated globally if a debugger is attached
    if (platform.FileClient.OS == OSType.Windows && !godotRepo.ProcessRunner.IsElevatedOnWindows() &&
        !Debugger.IsAttached)
    {
      await godotRepo.ProcessRunner.ElevateOnWindows();
      return;
    }

    var log = ExecutionContext.CreateLog(console);
    await godotRepo.AddOrUpdateGodotEnvVariable(log);
  }
}
