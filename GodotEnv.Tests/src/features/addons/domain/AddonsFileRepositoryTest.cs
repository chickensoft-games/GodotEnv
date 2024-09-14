namespace Chickensoft.GodotEnv.Tests;

using System.Linq;
using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Features.Addons.Domain;
using Chickensoft.GodotEnv.Features.Addons.Models;
using Moq;
using Shouldly;
using Xunit;

public partial class AddonsFileRepositoryTest {
  [Fact]
  public void LoadsAddonsFile() {
    var fileClient = new Mock<IFileClient>();
    var repo = new AddonsFileRepository(fileClient.Object);

    var projectPath = "/";
    string filename;
    var defaultValue = new AddonsFile();

    var value = new AddonsFile();

    var expectedArgs = new[] { "addons.json", "addons.jsonc" };
    fileClient.Setup(client => client.ReadJsonFile(
      projectPath,
      It.Is<string[]>(
        value => value.SequenceEqual(expectedArgs)
      ),
      out filename,
      It.IsAny<AddonsFile>()
    )).Returns(value);

    var result = repo.LoadAddonsFile(projectPath, out filename);

    fileClient.VerifyAll();

    result.ShouldBe(value);
  }

  [Fact]
  public void CreatesAddonsConfiguration() {
    var fileClient = new Mock<IFileClient>();
    var repo = new AddonsFileRepository(fileClient.Object);

    var projectPath = "/";
    var addonsFile = new AddonsFile();

    var result = repo.CreateAddonsConfiguration(projectPath, addonsFile);

    result.ProjectPath.ShouldBe(projectPath);
    result.AddonsPath.ShouldBe(
      fileClient.Object.Combine(projectPath, addonsFile.PathRelativePath)
    );
    result.CachePath.ShouldBe(
      fileClient.Object.Combine(projectPath, addonsFile.CacheRelativePath)
    );
  }

  [Fact]
  public void LoadsAddonsFileWhenCalledWithAddonsFileNameArgumentShouldLoadCorrectFile() {
    var fileClient = new Mock<IFileClient>();
    var repo = new AddonsFileRepository(fileClient.Object);

    var projectPath = "/";
    string filename;
    var addonsFileName = "foobar.json";

    var addonsFile = new AddonsFile();

    var expectedArgs = new[] { addonsFileName };
    fileClient.Setup(client => client.ReadJsonFile(
      projectPath,
      It.Is<string[]>(
        value => value.SequenceEqual(expectedArgs)
      ),
      out filename,
      It.IsAny<AddonsFile>()
    )).Returns(addonsFile);

    var result = repo.LoadAddonsFile(projectPath, out filename, addonsFileName);

    fileClient.VerifyAll();

    result.ShouldBe(addonsFile);
  }
}
