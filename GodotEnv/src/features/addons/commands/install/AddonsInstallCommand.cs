namespace Chickensoft.GodotEnv.Features.Addons.Commands;

using System;
using System.Linq;
using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Common.Utilities;
using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;

[Command("addons install", Description = "Install addons in a Godot project.")]
public class AddonsInstallCommand :
  ICommand, ICliCommand, IWindowsElevationEnabled {
  public IExecutionContext ExecutionContext { get; set; } = default!;

  [CommandOption(
    "max-depth",
    'd',
    Description = "The maximum depth to recurse while installing addons."
  )]
  public int? MaxDepth { get; init; }

  [CommandOption(
    "addons-file-name",
    'a',
    Description = "The file from to from which to install addons."
  )]
  public string? AddonsFileName { get; init; }

  // If we have any top-level addons that are symlinks, we know we're going
  // to need to elevate on Windows.
  public bool IsWindowsElevationRequired =>
    ExecutionContext.Addons.MainAddonsFile.Addons.Any(
      (addon) => addon.Value.Source == AssetSource.Symlink
    );

  public AddonsInstallCommand(IExecutionContext context) {
    ExecutionContext = context;
  }

  public async ValueTask ExecuteAsync(IConsole console) {
    var systemInfo = ExecutionContext.Godot.Platform.SystemInfo;

    var log = ExecutionContext.CreateLog(systemInfo, console);

    var addons = ExecutionContext.Addons;

    var addonsFileRepo = addons.AddonsFileRepo;
    var addonsRepo = addons.AddonsRepo;
    var addonsInstaller = addons.AddonsInstaller;

    var result = AddonsInstaller.Result.NotAttempted;

    var token = console.RegisterCancellationHandler();

    try {
      result = await addonsInstaller.Install(
        projectPath: ExecutionContext.WorkingDir,
        maxDepth: MaxDepth,
        onReport: (@event) => @event.Report(log),
        onDownload: (addon, progress) => log.PrintInPlace(
          $"Downloading {addon.Name}... {progress.Percent} @ {progress.Speed}"
        ),
        onExtract: (addon, progress) => log.PrintInPlace(
          $"Extracting {addon.Name}... {progress}"
        ),
        token,
        addonsFileName: AddonsFileName
      );
    }
    catch (Exception e) {
      log.Err(
        "An unknown error was encountered while attempting to install " +
        "addons."
      );
      log.Print("");
      log.Err(e.ToString());
      log.Print("");

      throw new CommandException(
        "Could not install addons. Please address any errors shown above " +
        "and try again.",
        1,
        innerException: e
      );
    }

    Finish(result, log);
  }

  internal static void Finish(AddonsInstaller.Result result, ILog log) {
    switch (result) {
      case AddonsInstaller.Result.Succeeded:
        log.Success("✅ Addons installed successfully.");
        break;
      case AddonsInstaller.Result.NothingToInstall:
        log.Success("✅ No addons to install.");
        break;
      case AddonsInstaller.Result.CannotBeResolved:
      case AddonsInstaller.Result.NotAttempted:
      default:
        throw new CommandException(
          "Could not install addons. Please address any errors shown above " +
          "and try again.",
          1
        );
    }
  }
}
