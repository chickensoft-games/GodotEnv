namespace Chickensoft.Chicken.Tests;

using System.Collections.Generic;
using Shouldly;
using Xunit;

public class EditActionsTest {
  private readonly EditActionInput _input = new(
    name: "name", type: "string", @default: "alice"
  );

  private readonly EditAction _action = new(
    type: "edit", properties: new() {
      ["file"] = "file.txt",
      ["find"] = "alice",
      ["replace"] = "bob",
    }
  );

  [Fact]
  public void InitializesWithNulls() {
    var editActions = new EditActions(null, null);
    editActions.Inputs.ShouldBeEmpty();
    editActions.Actions.ShouldBeEmpty();

    var input = new EditActionInput("", "", null);
    input.Name.ShouldBe("");
    input.Type.ShouldBe("");
    input.Default.ShouldBeNull();

    var action = new EditAction("", null);
    action.Type.ShouldBe("");
    action.Properties.ShouldBeEmpty();
  }

  [Fact]
  public void InitializesWithValues() {
    var editActions = new EditActions(
      new() { _input },
      new() { _action }
    );
    editActions.Inputs.ShouldBe(new List<EditActionInput> { _input });
    editActions.Actions.ShouldBe(new List<EditAction> { _action });
  }
}
