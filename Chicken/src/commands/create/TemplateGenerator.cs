namespace Chickensoft.Chicken;

using CliFx.Exceptions;

public interface ITemplateGenerator {
  string ProjectName { get; init; }
  string ProjectPath { get; init; }
  string TemplateDescription { get; init; }
  IEditActionsRepo EditActionsRepo { get; init; }
  EditActions EditActions { get; init; }
  ILog Log { get; init; }

  void Generate();
}

public class TemplateGenerator : ITemplateGenerator {
  public string ProjectName { get; init; }
  public string ProjectPath { get; init; }
  public string TemplateDescription { get; init; }
  public IEditActionsRepo EditActionsRepo { get; init; }
  public EditActions EditActions { get; init; }
  public ILog Log { get; init; }

  public TemplateGenerator(
    string projectName,
    string projectPath,
    string templateDescription,
    IEditActionsRepo editActionsRepo,
    EditActions editActions,
    ILog log
  ) {
    ProjectName = projectName;
    ProjectPath = projectPath;
    TemplateDescription = templateDescription;
    EditActionsRepo = editActionsRepo;
    EditActions = editActions;
    Log = log;
  }

  public void Generate() {
    // validate edit actions and display warnings
    var validationResult = EditActionsRepo.Validate();
    for (var i = 0; i < EditActions.Actions.Count; i++) {
      var editAction = EditActions.Actions[i];
      if (!validationResult.Warnings.ContainsKey(editAction)) {
        continue;
      }
      foreach (var exception in validationResult.Warnings[editAction]) {
        Log.Err(
          $"Error in edit action {i} ({editAction.Type}): {exception.Message}"
        );
      }
    }

    if (validationResult.Warnings.Count == 0) {
      // perform edit actions if no warnings
      Log.Print("Generating template by performing the edit actions...");
      var editResult = EditActionsRepo.PerformEdits(validationResult.Actions);
      if (editResult.Count == 0) {
        Log.Success("Successfully generated template!");
        Log.Success(
          $"Created `{ProjectName}` from {TemplateDescription} at " +
          $"`{ProjectPath}`."
        );
      }
      else {
        // if we encountered problems while performing edit actions, log them
        foreach (var problem in editResult) {
          Log.Err($"Error: {problem.Message}");
        }
        Log.Print("");
        Log.Warn("Errors found while performing edit actions.");
        Log.Warn("Please repair the template's EDIT_ACTIONS.json file.");
      }
    }
    else {
      // inform user that we can't perform edit actions because they're invalid
      Log.Warn(
        "Errors found in edit actions. Skipping template generation."
      );
      throw new CommandException(
        "Please fix the template's edit actions and try again."
      );
    }
  }
}
