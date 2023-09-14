namespace Chickensoft.GodotEnv.Features.Addons.Commands;

using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Models;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

[Command("godot list", Description = "List installed Godot versions.")]
public class GodotListCommand : ICommand, ICliCommand {
  public IExecutionContext ExecutionContext { get; set; } = default!;

  public GodotListCommand(IExecutionContext context) {
    ExecutionContext = context;
  }

  public async ValueTask ExecuteAsync(IConsole console) {
    var log = ExecutionContext.CreateLog(console);
    var godotRepo = ExecutionContext.Godot.GodotRepo;

    foreach (
      var installation in godotRepo.GetInstallationsList()
    ) {
      var activeTag = installation.IsActiveVersion ? " *" : "";
      log.Print(installation.VersionName + activeTag);
    }

    await Task.CompletedTask;
  }
}
