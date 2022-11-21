namespace Chickensoft.Chicken.Tests;

using System;
using System.Collections.Generic;
using CliFx.Exceptions;
using Moq;
using Shouldly;
using Xunit;

public class TemplateGeneratorTest {
  private readonly string _projectName = "MyProject";
  private readonly string _projectPath = "/MyProject";
  private readonly string _templateDescription = "MyTemplate";

  [Fact]
  public void Initializes() {
    var inputs = new Dictionary<string, dynamic?>() { };
    var editActionsRepo = new Mock<IEditActionsRepo>();
    var editActions = new EditActions(null, null);
    var log = new Mock<ILog>();

    var generator = new TemplateGenerator(
      _projectName,
      _projectPath,
      _templateDescription,
      editActionsRepo.Object,
      editActions,
      log.Object
    );

    generator.ProjectName.ShouldBeSameAs(_projectName);
    generator.ProjectPath.ShouldBeSameAs(_projectPath);
    generator.TemplateDescription.ShouldBeSameAs(_templateDescription);
    generator.EditActionsRepo.ShouldBeSameAs(editActionsRepo.Object);
    generator.EditActions.ShouldBeSameAs(editActions);
    generator.Log.ShouldBeSameAs(log.Object);
  }

  [Fact]
  public void GenerateLogsValidationExceptions() {
    var actions = new List<EditAction>() {
      new EditAction(
        type: "edit",
        properties: new Dictionary<string, object>() {
          ["file"] = "project.godot",
          // ["find"] = "MyAssembly",
          ["replace"] = "{title:PascalCase}"
        }
      ),
      new EditAction(
        type: "rename",
        properties: new Dictionary<string, object>() {
          ["file"] = "MyGame.sln",
          ["to"] = "{title:PascalCase}.sln",
        }
      )
    };
    var validationResult = new EditActionsValidationResult(
      Warnings: new Dictionary<EditAction, List<Exception>>() {
        [actions[0]] = new List<Exception>() {
          new InvalidOperationException("Missing 'find' property.")
        }
      },
      Actions: new List<IEditActionSpecific>()
    );

    var editActionsRepo = new Mock<IEditActionsRepo>();
    var editActions = new EditActions(null, actions);
    var log = new Mock<ILog>();

    editActionsRepo.Setup(repo => repo.Validate()).Returns(validationResult);

    log.Setup(log => log.Err(
      "Error in edit action 0 (edit): Missing 'find' property."
    ));
    log.Setup(log => log.Warn(
      "Errors found in edit actions. Skipping template generation."
    ));

    var generator = new TemplateGenerator(
      _projectName,
      _projectPath,
      _templateDescription,
      editActionsRepo.Object,
      editActions,
      log.Object
    );

    Should.Throw<CommandException>(() => generator.Generate());
  }

  [Fact]
  public void GenerateLogsErrorsWhenPerformingEdits() {
    var actions = new List<EditAction>() {
      new EditAction(
        type: "edit",
        properties: new Dictionary<string, object>() {
          ["file"] = "project.godot",
          ["find"] = "MyAssembly",
          ["replace"] = "{title:PascalCase}"
        }
      ),
      new EditAction(
        type: "rename",
        properties: new Dictionary<string, object>() {
          ["file"] = "MyGame.sln",
          ["to"] = "{title:PascalCase}.sln",
        }
      )
    };

    var validationResult = new EditActionsValidationResult(
      Warnings: new Dictionary<EditAction, List<Exception>>(),
      Actions: new List<IEditActionSpecific>() {
        new Edit(
          File: (string)actions[0].Properties["file"],
          Find: (string)actions[0].Properties["find"],
          Replace: (string)actions[0].Properties["replace"]
        ),
        new Rename(
          File: (string)actions[1].Properties["file"],
          To: (string)actions[1].Properties["to"]
        )
      }
    );

    var editActionsRepo = new Mock<IEditActionsRepo>();
    var editActions = new EditActions(null, actions);
    var log = new Mock<ILog>();

    editActionsRepo.Setup(repo => repo.Validate()).Returns(validationResult);
    log.Setup(log => log.Print(
      "Generating template by performing the edit actions..."
    ));

    var exceptions = new List<Exception>() {
      new InvalidOperationException("Couldn't edit file"),
      new InvalidOperationException("Couldn't rename file")
    };

    editActionsRepo.Setup(repo => repo.PerformEdits(validationResult.Actions))
      .Returns(exceptions);

    log.Setup(log => log.Err($"Error: {exceptions[0].Message}"));
    log.Setup(log => log.Err($"Error: {exceptions[1].Message}"));
    log.Setup(log => log.Print(""));
    log.Setup(log => log.Warn("Errors found while performing edit actions."));
    log.Setup(log => log.Warn(
      "Please repair the template's EDIT_ACTIONS.json file."
    ));

    var generator = new TemplateGenerator(
      _projectName,
      _projectPath,
      _templateDescription,
      editActionsRepo.Object,
      editActions,
      log.Object
    );

    generator.Generate();

    log.VerifyAll();
  }

  [Fact]
  public void GenerateGeneratesProjectSuccessfully() {
    var actions = new List<EditAction>() {
      new EditAction(
        type: "edit",
        properties: new Dictionary<string, object>() {
          ["file"] = "project.godot",
          ["find"] = "MyAssembly",
          ["replace"] = "{title:PascalCase}"
        }
      ),
      new EditAction(
        type: "rename",
        properties: new Dictionary<string, object>() {
          ["file"] = "MyGame.sln",
          ["to"] = "{title:PascalCase}.sln",
        }
      )
    };

    var validationResult = new EditActionsValidationResult(
      Warnings: new Dictionary<EditAction, List<Exception>>(),
      Actions: new List<IEditActionSpecific>() {
        new Edit(
          File: (string)actions[0].Properties["file"],
          Find: (string)actions[0].Properties["find"],
          Replace: (string)actions[0].Properties["replace"]
        ),
        new Rename(
          File: (string)actions[1].Properties["file"],
          To: (string)actions[1].Properties["to"]
        )
      }
    );

    var editActionsRepo = new Mock<IEditActionsRepo>();
    var editActions = new EditActions(null, actions);
    var log = new Mock<ILog>();

    editActionsRepo.Setup(repo => repo.Validate()).Returns(validationResult);
    log.Setup(log => log.Print(
      "Generating template by performing the edit actions..."
    ));

    var exceptions = new List<Exception>() { };

    editActionsRepo.Setup(repo => repo.PerformEdits(validationResult.Actions))
      .Returns(exceptions);

    log.Setup(log => log.Success("Successfully generated template!"));
    log.Setup(log => log.Success(
      $"Created `{_projectName}` from {_templateDescription} at " +
      $"`{_projectPath}`."
    ));


    var generator = new TemplateGenerator(
      _projectName,
      _projectPath,
      _templateDescription,
      editActionsRepo.Object,
      editActions,
      log.Object
    );

    generator.Generate();

    log.VerifyAll();
  }
}
