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
public class GodotListCommand : ICommand, ICliCommand
{
  public IExecutionContext ExecutionContext { get; set; } = default!;

  [CommandOption("remote", 'r',
    Description = "Specify to list all available versions of Godot")]
  public bool ListRemoteAvailable { get; set; }

  [CommandOption(
    "proxy", 'x',
    Description = "Specify a proxy server URL (e.g., http://127.0.0.1:1080)."
  )]
  public string? ProxyUrl { get; set; }

  public GodotListCommand(IExecutionContext context)
  {
    ExecutionContext = context;
  }

  private static void ListLocalVersions(ILog log, IGodotRepository godotRepo)
  {
    var installations = godotRepo.GetInstallationsList();
    foreach (var result in installations)
    {
      if (result.Value is { } installation)
      {
        var activeTag = installation.IsActiveVersion ? " *" : "";
        log.Print(godotRepo.InstallationVersionName(installation) + activeTag);
      }
      else
      {
        log.Warn(result.Error);
      }
    }
    if (!godotRepo.IsGodotSymlinkTargetAvailable)
    {
      log.Warn("Could not determine current target Godot version.");
    }
  }

  private static async Task ListRemoteVersions(ILog log, IGodotRepository godotRepo, string? proxyUrl = null)
  {
    log.Print("Retrieving available Godot versions...");

    try
    {
      var remoteVersions = await godotRepo.GetRemoteVersionsList(log, proxyUrl);
      foreach (var version in remoteVersions)
      {
        log.Print(version);
      }
    }
    catch (HttpRequestException)
    {
      log.Print("Unable to reach remote Godot versions list.");
    }
  }

  public async ValueTask ExecuteAsync(IConsole console)
  {
    var log = ExecutionContext.CreateLog(console);
    var godotRepo = ExecutionContext.Godot.GodotRepo;

    if (ListRemoteAvailable)
    {
      await ListRemoteVersions(log, godotRepo, ProxyUrl);
    }
    else
    {
      ListLocalVersions(log, godotRepo);
    }

    await Task.CompletedTask;
  }
}
