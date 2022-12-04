namespace Chickensoft.Chicken;
using System.IO.Abstractions;

public interface IEditActionsLoader {
  public EditActions Load(string repoPath);
}

public class EditActionsLoader : JsonFileLoader<EditActions>, IEditActionsLoader {
  public EditActionsLoader(IApp app, IFileSystem fs) : base(app, fs) { }

  public EditActions Load(string repoPath) => base.Load(
      projectPath: repoPath,
      possibleFilenames: App.EDIT_ACTIONS_FILES,
      defaultValue: new EditActions(
        inputs: new(),
        actions: new()
      )
    );
}
