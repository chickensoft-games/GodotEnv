namespace Chickensoft.Chicken;

using System.IO.Abstractions;

public interface IEditActionSpecific {
  public const string INFO = "Check your template's EDIT_ACTIONS.json file " +
    "to make sure the template is setup correctly.";
  string File { get; }

  void Perform(IApp app, IFileSystem fs, string repoPath);
}

public interface IEdit : IEditActionSpecific {
  string Find { get; init; }
  string Replace { get; init; }
}

public record Edit(string File, string Find, string Replace) : IEdit {
  public void Perform(IApp app, IFileSystem fs, string repoPath) {
    var content = fs.File.ReadAllText(File).Replace(Find, Replace);
    fs.File.WriteAllText(File, content);
  }
}

public interface IRename : IEditActionSpecific {
#pragma warning disable CA1716
  string To { get; }
#pragma warning restore CA1716
}

public record Rename(string File, string To) : IRename {
  public void Perform(IApp app, IFileSystem fs, string repoPath)
    => fs.File.Move(File, To);
}

public interface IGooeyId : IEditActionSpecific {
  string Replace { get; }
}

public record GooeyId(string File, string Replace) : IGooeyId {
  public void Perform(IApp app, IFileSystem fs, string repoPath) {
    var content = fs.File.ReadAllText(File).Replace(
      Replace, app.GenerateGuid()
    );
    fs.File.WriteAllText(File, content);
  }
}
