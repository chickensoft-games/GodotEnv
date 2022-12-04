namespace Chickensoft.Chicken.Tests;

using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using Moq;
using Shouldly;
using Xunit;

public class EditActionsLoaderTest {
  [Fact]
  public void Initializes() {
    var app = new Mock<IApp>();
    var fs = new MockFileSystem(new Dictionary<string, MockFileData>() { });
    var repo = new EditActionsLoader(app.Object, fs);
    repo.ShouldBeOfType(typeof(EditActionsLoader));
  }

  [Fact]
  public void Loads() {
    var app = new Mock<IApp>();
    var data = "{\"inputs\": [], \"actions\": [{\"type\": " +
      "\"rename\", \"file\": \"file.txt\", \"to\": \"file2.txt\"}]}";
    var fs = new MockFileSystem(new Dictionary<string, MockFileData>() {
      ["/EDIT_ACTIONS.json"] = new MockFileData(data)
    });
    var repo = new EditActionsLoader(app.Object, fs);
    var editActions = repo.Load("/");
    editActions.Actions.Count.ShouldBe(1);
  }
}
