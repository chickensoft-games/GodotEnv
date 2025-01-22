namespace Chickensoft.GodotEnv.Features.Addons.Commands;

using System.Net.Http;
using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Models;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Common.Utilities;
using Godot.Domain;

[Command("godot list", Description = "List installed Godot versions.")]
public class GodotListCommand : ICommand, ICliCommand {
  public IExecutionContext ExecutionContext { get; set; } = default!;

  [CommandOption("remote", 'r',
    Description = "Specify to list all available versions of Godot")]
  public bool ListRemoteAvailable { get; set; }

  public GodotListCommand(IExecutionContext context) {
    ExecutionContext = context;
  }

  private static void ListLocalVersions(ILog log, IGodotRepository godotRepo) {
    foreach (var installation in godotRepo.GetInstallationsList()) {
      var activeTag = installation.IsActiveVersion ? " *" : "";
      log.Print(installation.VersionName + activeTag);
    }
  }

  private static async Task ListRemoteVersions(ILog log, IGodotRepository godotRepo) {
    log.Print("Retrieving available Godot versions...");

    try {
      var remoteVersions = await godotRepo.GetRemoteVersionsList();
      foreach (var version in remoteVersions) {
        log.Print(version);
      }
    }
    catch (HttpRequestException) {
      log.Print("Unable to reach remote Godot versions list.");
    }
  }

  public async ValueTask ExecuteAsync(IConsole console) {
    var systemInfo = ExecutionContext.Godot.Platform.SystemInfo;
    var log = ExecutionContext.CreateLog(systemInfo, console);
    var godotRepo = ExecutionContext.Godot.GodotRepo;

    if (ListRemoteAvailable) {
      await ListRemoteVersions(log, godotRepo);
    }
    else {
      ListLocalVersions(log, godotRepo);
    }

    await Task.CompletedTask;
  }
}
