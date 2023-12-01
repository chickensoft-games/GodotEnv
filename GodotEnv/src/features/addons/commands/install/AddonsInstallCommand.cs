namespace Chickensoft.GodotEnv.Features.Addons.Commands;

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Models;
using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;

[Command("addons install", Description = "Install addons in a Godot project.")]
public class AddonsInstallCommand : ICommand, ICliCommand {
  public IExecutionContext ExecutionContext { get; set; } = default!;

  [CommandOption(
    "max-depth",
    'd',
    Description = "The maximum depth to recurse while installing addons."
  )]
  public int? MaxDepth { get; init; }

  public AddonsInstallCommand(IExecutionContext context) {
    ExecutionContext = context;
  }

  public async ValueTask ExecuteAsync(IConsole console) {
    var log = ExecutionContext.CreateLog(console);
    var addonsFileRepo = ExecutionContext.Addons.AddonsFileRepo;
    var addonsRepo = ExecutionContext.Addons.AddonsRepo;
    var logic = ExecutionContext.Addons.AddonsLogic;

    // The install command should be run with admin role on Windows if the addons file contains addons with a symlink source
    // To be able to debug, godotenv is not elevated globally if a debugger is attached
    if (addonsFileRepo.AddonsFileContainsSymlinkAddons(ExecutionContext.WorkingDir) && addonsFileRepo.FileClient.OS == OSType.Windows && 
        !addonsRepo.ProcessRunner.IsElevatedOnWindows() && !Debugger.IsAttached) {
      await addonsRepo.ProcessRunner.ElevateOnWindows();
      return;
    }

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
