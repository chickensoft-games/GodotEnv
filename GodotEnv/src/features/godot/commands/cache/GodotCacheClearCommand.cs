namespace Chickensoft.GodotEnv.Features.Godot.Commands;

using System.Diagnostics;
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
    var godotRepo = ExecutionContext.Godot.GodotRepo;
    var platform = ExecutionContext.Godot.Platform;

    // The clear command must be run with the admin role on Windows
    // To be able to debug, godotenv is not elevated globally if a debugger is attached
    if (platform.FileClient.OS == OSType.Windows && !godotRepo.ProcessRunner.IsElevatedOnWindows() &&
        !Debugger.IsAttached)
    {
      await godotRepo.ProcessRunner.ElevateOnWindows();
      return;
    }

    var log = ExecutionContext.CreateLog(console);

    log.Print("");
    log.Info("Clearing Godot installation cache...");
    log.Print("");
    ExecutionContext.Godot.GodotRepo.ClearCache();
    log.Success("Godot installation cache cleared.");
    log.Print("");

    await Task.CompletedTask;
  }
}
