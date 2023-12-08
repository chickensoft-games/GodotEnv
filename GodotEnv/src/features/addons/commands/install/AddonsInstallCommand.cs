namespace Chickensoft.GodotEnv.Features.Addons.Commands;

using System;
using System.Linq;
using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Models;
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
    var log = ExecutionContext.CreateLog(console);
    var addonsFileRepo = ExecutionContext.Addons.AddonsFileRepo;
    var addonsRepo = ExecutionContext.Addons.AddonsRepo;
    var logic = ExecutionContext.Addons.AddonsLogic;

    var binding = logic.Bind();

    binding
      .Handle<AddonsLogic.Output.Report>(output => {
        output.Event.Report(log);
        log.Print("");
      })
      .Catch<Exception>((e) => {
        log.Err("An error was encountered while attempting to install addons.");
        log.Print("");
        log.Err(e.ToString());
        log.Print("");
      });

    var state = await logic.Input(
      new AddonsLogic.Input.Install(
        ProjectPath: ExecutionContext.WorkingDir, MaxDepth: MaxDepth
      )
    );

    CheckSuccess(state);
  }

  internal void CheckSuccess(AddonsLogic.State state) {
    if (state is AddonsLogic.State.CannotBeResolved) {
      throw new CommandException(
        "Could not resolve addons. Please address any errors and try again.",
        1
      );
    }
    else if (state is AddonsLogic.State.Unresolved) {
      throw new CommandException(
        "Could not resolve addons. Please address any errors and try again.",
        2
      );
    }
  }
}
