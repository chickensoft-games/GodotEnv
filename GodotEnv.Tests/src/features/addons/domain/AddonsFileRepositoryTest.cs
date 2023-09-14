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

    fileClient.Setup(client => client.ReadJsonFile(
      projectPath,
      It.Is<string[]>(
        value => value.SequenceEqual(
          new string[] { "addons.json", "addons.jsonc" }
        )
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
}
