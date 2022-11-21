namespace Chickensoft.Chicken.Tests;

using System.IO.Abstractions;
using Moq;
using Xunit;

public class EditActionSpecific {
  [Fact]
  public void EditActionReplaces() {
    var repoPath = "/";
    var file = "file.txt";
    var find = "find";
    var contents = "find me";
    var replace = "replace";

    var app = new Mock<IApp>(MockBehavior.Strict);
    var fs = new Mock<IFileSystem>(MockBehavior.Strict);
    var fsFile = new Mock<IFile>(MockBehavior.Strict);

    fs.Setup(f => f.File).Returns(fsFile.Object);
    fsFile.Setup(f => f.ReadAllText(file)).Returns(contents);
    fsFile.Setup(f => f.WriteAllText(file, "replace me"));

    new Edit(file, find, replace).Perform(app.Object, fs.Object, repoPath);

    fsFile.VerifyAll();
  }

  [Fact]
  public void RenameActionRenames() {
    var repoPath = "/";
    var file = "file.txt";
    var to = "to.txt";

    var app = new Mock<IApp>(MockBehavior.Strict);
    var fs = new Mock<IFileSystem>(MockBehavior.Strict);
    var fsFile = new Mock<IFile>(MockBehavior.Strict);

    fs.Setup(f => f.File).Returns(fsFile.Object);
    fsFile.Setup(f => f.Move(file, to));

    new Rename(file, to).Perform(app.Object, fs.Object, repoPath);

    fsFile.VerifyAll();
  }

  [Fact]
  public void GooeyIdActionReplaces() {
    var repoPath = "/";
    var file = "file.txt";
    var placeholder = "GUID-GUID-GUID-GUID-GUID";
    var contents = $"{placeholder} me";
    var guid = "11111111-1111-1111-1111-111111111111";
    var app = new Mock<IApp>(MockBehavior.Strict);
    var fs = new Mock<IFileSystem>(MockBehavior.Strict);
    var fsFile = new Mock<IFile>(MockBehavior.Strict);

    app.Setup(a => a.GenerateGuid()).Returns(guid);
    fs.Setup(f => f.File).Returns(fsFile.Object);
    fsFile.Setup(f => f.ReadAllText(file)).Returns(contents);
    fsFile.Setup(f => f.WriteAllText(file, $"{guid} me"));

    new GooeyId(file, placeholder).Perform(app.Object, fs.Object, repoPath);

    fsFile.VerifyAll();
  }
}
