namespace Chickensoft.Chicken.Tests;

using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using CliFx.Exceptions;
using Moq;
using Shouldly;
using Xunit;

public class EditActionsRepoTest {
  [Fact]
  public void Initializes() {
    var app = new Mock<IApp>();
    var fs = new Mock<IFileSystem>();
    var file = new Mock<IFile>();

    var editActions = new EditActions(
      inputs: new() {
        new(name: "name", type: "string", @default: "alice")
      },
      actions: new()
    );

    var inputs = new Dictionary<string, dynamic?>();

    var repo = new EditActionsRepo(
      app: app.Object, fs.Object, "/", editActions, inputs
    );

    repo.EditActions.ShouldBe(editActions);
  }

  [Fact]
  public void InputVariableRendererTests() {
    var renderer = EditActionsRepo.InputVariableRenderer;

    // Should match
    renderer.IsMatch("hello, {name:snake_case}!").ShouldBeTrue();
    renderer.IsMatch("hello, {name:PascalCase}!").ShouldBeTrue();
    renderer.IsMatch("hello, {name:camelCase}!").ShouldBeTrue();
    renderer.IsMatch("hello, {name:lowercase}!").ShouldBeTrue();
    renderer.IsMatch("hello, {name:UPPERCASE}!").ShouldBeTrue();

    // Shouldn't match
    renderer.IsMatch("hello, {}!").ShouldBeFalse();
    renderer.IsMatch("hello, {name:}!").ShouldBeFalse();
    renderer.IsMatch("hello, {:lowercase}!").ShouldBeFalse();
  }

  [Fact]
  public void RenderTests() {
    var inputs = new Dictionary<string, dynamic?>() {
      ["name"] = "Microsoft Bob",
      ["other_type"] = new NotReallyStringConvertible(),
      ["stringified"] = new StringBuilder("Alice"),
      ["null"] = null!
    };

    var app = new Mock<IApp>();
    var fs = new Mock<IFileSystem>();

    var repo = new EditActionsRepo(
      app: app.Object, fs.Object, "/", new EditActions(null, null), inputs
    );

    // Renders variables with correct casing.
    repo.Render("hello, {name}!")
      .ShouldBe("hello, Microsoft Bob!");
    repo.Render("hello, {name:snake_case}!")
      .ShouldBe("hello, microsoft_bob!");
    repo.Render("hello, {name:pascalcase}!")
      .ShouldBe("hello, MicrosoftBob!");
    repo.Render("hello, {name:camelcase}!")
      .ShouldBe("hello, microsoftBob!");
    repo.Render("hello, {name:lowercase}!")
      .ShouldBe("hello, microsoft bob!");
    repo.Render("hello, {name:uppercase}!")
      .ShouldBe("hello, MICROSOFT BOB!");

    // Ignores unknown variables
    repo.Render("hello, {title}!")
      .ShouldBe("hello, {title}!");
    repo.Render("hello, {title:uppercase}!")
      .ShouldBe("hello, {title:uppercase}!");
    repo.Render("hello, {other_type}!")
      .ShouldBe("hello, !");
    repo.Render("hello, {stringified}!")
      .ShouldBe("hello, Alice!");
    repo.Render("hello, {null}!")
      .ShouldBe("hello, !");
  }

  [Fact]
  public void PropAddsToWarnings() {
    var editActionType = "edit";
    var index = 0;
    var editAction = new EditAction(editActionType, null);
    var inputs = new Dictionary<string, dynamic?>();

    var editActions = new EditActions(
      inputs: new() { },
      actions: new() { editAction }
    );

    var app = new Mock<IApp>();
    var fs = new Mock<IFileSystem>();

    var repo = new EditActionsRepo(
      app.Object, fs.Object, "/", editActions, inputs
    );

    repo.Prop(index, "file", editAction).ShouldBeNull();
    repo.Warnings[editAction].Single().ShouldBeOfType<CommandException>();
    repo.Prop(index, "find", editAction).ShouldBeNull();
    repo.Warnings[editAction][1].ShouldBeOfType<CommandException>();
  }

  [Fact]
  public void PropFindsValue() {
    var editActionType = "edit";
    var index = 0;
    var editAction = new EditAction(editActionType, new() {
      ["file"] = "file"
    });
    var inputs = new Dictionary<string, dynamic?>();

    var app = new Mock<IApp>();
    var fs = new Mock<IFileSystem>();

    var editActions = new EditActions(
        inputs: null, actions: new List<EditAction>() { editAction }
      );

    var repoPath = "/";

    var repo = new EditActionsRepo(
      app.Object, fs.Object, repoPath, editActions, inputs
    );

    repo.Prop(index, "file", editAction).ShouldBe("file");
  }

  [Fact]
  public void ValidateValidatesCorrectEditActions() {
    var editActions = new EditActions(
      inputs: new() {
        new(name: "name", type: "string", @default: "alice")
      },
      actions: new() {
        new("edit", new() {
          ["file"] = "/file.txt",
          ["find"] = "find",
          ["replace"] = "replace"
        }),
        new("rename", new() {
          ["file"] = "/file.txt",
          ["to"] = "to",
        }),
        new("guid", new() {
          ["file"] = "/file.txt",
          ["replace"] = "replace",
        }),
      }
    );

    var inputs = new Dictionary<string, dynamic?>();

    var app = new Mock<IApp>();
    var fs = new Mock<IFileSystem>();

    var file = new Mock<IFile>();

    fs.Setup(fs => fs.File).Returns(file.Object);
    file.Setup(file => file.Exists("/file.txt")).Returns(true);

    var repoPath = "/";

    var repo = new EditActionsRepo(
      app.Object, fs.Object, repoPath, editActions, inputs
    );
    var result = repo.Validate();

    result.Warnings.ShouldBeEmpty();
    result.Actions.ShouldBe(
      new List<IEditActionSpecific>() {
        new Edit("/file.txt", "find", "replace"),
        new Rename("/file.txt", "/to"),
        new GooeyId("/file.txt", "replace"),
      }
    );

    result.Success.ShouldBeTrue();
  }

  [Fact]
  public void ValidateValidatesBrokenEditActions() {
    var editActions = new EditActions(
      inputs: new() {
        new(name: "name", type: "string", @default: "alice")
      },
      actions: new() {
        new("edit", new() {
          ["file"] = "/foo/file.txt",
          ["find"] = "find",
        }),
        new("rename", new() {
          ["file"] = "/file.txt",
        }),
        new("guid", new() {
          ["file"] = "/file.txt",
        }),
        new("unknown", null)
      }
    );

    var inputs = new Dictionary<string, dynamic?>();

    var app = new Mock<IApp>();
    var fs = new Mock<IFileSystem>();
    var file = new Mock<IFile>();

    fs.Setup(fs => fs.File).Returns(file.Object);
    file.Setup(file => file.Exists("/file.txt")).Returns(false);

    var repoPath = "/";

    var repo = new EditActionsRepo(
      app.Object, fs.Object, repoPath, editActions, inputs
    );
    var result = repo.Validate();

    var warnings = result.Warnings[editActions.Actions[0]];
    warnings.Count.ShouldBe(2);
    warnings[1].Message.ShouldContain("Missing property `replace`");
    warnings[0].Message.ShouldContain("Cannot find file `/foo/file.txt`");
    result.Actions.ShouldBeEmpty();

    result.Success.ShouldBeFalse();
  }

  [Fact]
  public void PerformEditsPerformsActionsAndHandlesErrors() {
    var app = new Mock<IApp>();
    var fs = new Mock<IFileSystem>();
    var repoPath = "/";

    var edit = new Mock<IEdit>();
    edit.Setup(a => a.Perform(app.Object, fs.Object, repoPath));

    var rename = new Mock<IRename>();
    rename.Setup(a => a.Perform(app.Object, fs.Object, repoPath))
      .Throws(new CommandException("fake exception"));

    var actions = new List<IEditActionSpecific>() {
      edit.Object, rename.Object
    };

    var editActions = new EditActions(inputs: null, actions: null);

    var inputs = new Dictionary<string, dynamic?>();

    var file = new Mock<IFile>();

    var repo = new EditActionsRepo(
      app.Object, fs.Object, repoPath, editActions, inputs
    );

    var problems = repo.PerformEdits(actions);

    problems.Count.ShouldBe(1);
    problems[0].Message.ShouldContain("fake exception");
  }
}

internal class NotReallyStringConvertible : object {
  public override string? ToString() => null;
}
