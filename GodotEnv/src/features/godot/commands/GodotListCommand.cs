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
    godotRepo.GetInstallationsList(
      out var installations,
      out var unrecognizedDirectories,
      out var failedInstallations
    );
    foreach (var installation in installations) {
      var activeTag = installation.IsActiveVersion ? " *" : "";
      log.Print(godotRepo.InstallationVersionName(installation) + activeTag);
    }
    foreach (var unrecognized in unrecognizedDirectories) {
      log.Warn("Unrecognized subfolder in Godot installation directory (may be a non-conforming version identifier):");
      log.Warn($"  {unrecognized}");
    }
    foreach (var failedInstallation in failedInstallations) {
      log.Err("Installation directory matches Godot version but failed to load:");
      log.Err($"  {failedInstallation}");
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
    var log = ExecutionContext.CreateLog(console);
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
