namespace Chickensoft.GodotEnv.Tests.Features.Config.Commands.List;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Text.Json;
using System.Text.Json.Nodes;
using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Common.Utilities;
using Chickensoft.GodotEnv.Features.Godot.Domain;
using Chickensoft.GodotEnv.Features.Godot.Models;
using Chickensoft.GodotEnv.Features.Godot.Serializers;
using Moq;
using Shouldly;
using Xunit;

public class GodotVersionSpecifierRepositoryTest
{
  // all global.json first, all csproj second, all godotrc third
  // nearest ancestor to furthest ancestor
  [Fact]
  public void GetsVersionFilesInCorrectOrder()
  {
    var directoryNames = new List<string> {
      "/test/path/to/project",
      "/test/path/to",
      "/test/path",
      "/test",
      "/",
    };
    var directoryMocks = new List<Mock<IDirectoryInfo>> {
      new(),
      new(),
      new(),
      new(),
      new(),
    };
    for (var i = 0; i < directoryMocks.Count; ++i)
    {
      directoryMocks[i].Setup(dir => dir.FullName).Returns(directoryNames[i]);
      if (i < directoryMocks.Count - 1)
      {
        directoryMocks[i].Setup(dir => dir.Parent).Returns(directoryMocks[i + 1].Object);
      }
    }
    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(
        client => client.GetAncestorDirectories(directoryNames[0])
      ).Returns(Enumerate(directoryMocks, m => m.Object));
    fileClient.Setup(
        client => client.Combine(It.IsAny<string[]>())
      ).Returns((string[] s) => string.Join('/', s));
    fileClient.Setup(
        client => client.FileExists(It.IsAny<string>())
      ).Returns(true);
    fileClient.Setup(
      client => client.GetFiles(It.IsAny<string>(), "*.csproj")
    ).Returns((string dir, string pattern) => [$"{dir}/Project.csproj"]);

    var results = new List<IGodotVersionFile>();
    foreach (var dir in directoryNames)
    {
      results.Add(new GlobalJsonFile($"{dir}/global.json"));
    }
    foreach (var dir in directoryNames)
    {
      results.Add(new CsprojFile($"{dir}/Project.csproj"));
    }
    foreach (var dir in directoryNames)
    {
      results.Add(new GodotrcFile($"{dir}/.godotrc"));
    }

    var repo = new GodotVersionSpecifierRepository(directoryNames[0], fileClient.Object);

    repo.GetVersionFiles().ShouldBe(results);
  }

  [Fact]
  public void ValidatedGodotVersionReturnsFailureIfVersionIsFailure()
  {
    var fileClient = new Mock<IFileClient>();
    var file = new Mock<IGodotVersionFile>();
    var versionResult = new Result<SpecificDotnetStatusGodotVersion>(
      false,
      null,
      "Testing"
    );
    file.Setup(f => f.ParseGodotVersion(fileClient.Object)).Returns(versionResult);

    var repo = new GodotVersionSpecifierRepository("/test/working-dir", fileClient.Object);

    var version = repo.GetValidatedGodotVersion(file.Object);
    version.IsSuccess.ShouldBeFalse();
    version.Error.ShouldBe("Testing");
  }

  [Fact]
  public void ValidatedGodotVersionIsVersionIfVersionDeserializerReturns()
  {
    var version = new Result<SpecificDotnetStatusGodotVersion>(
      true,
      new(4, 4, 1, "stable", -1, true),
      string.Empty
    );
    var path = "/test/path";
    var fileClient = new Mock<IFileClient>();

    var file = new Mock<IGodotVersionFile>();
    file.Setup(f => f.FilePath).Returns(path);
    file.Setup(f => f.ParseGodotVersion(fileClient.Object)).Returns(version);

    var repo = new GodotVersionSpecifierRepository("test/working-dir", fileClient.Object);

    repo.GetValidatedGodotVersion(file.Object).ShouldBe(version);
  }

  [Fact]
  public void InfersVersionFromFirstValidVersionFile()
  {
    var fileClient = new Mock<IFileClient>();

    var versions = new List<Result<SpecificDotnetStatusGodotVersion>> {
      new(false, null, "Testing"),
      new(true, new(4, 3, 0, "stable", -1, true), string.Empty),
      new(true, new(4, 4, 1, "stable", -1, true), string.Empty),
    };
    var fileMocks = new List<Mock<IGodotVersionFile>> {
      new(),
      new(),
      new(),
    };
    for (var i = 0; i < versions.Count; ++i)
    {
      fileMocks[i].Setup(f => f.FilePath).Returns($"file{i}");
      fileMocks[i].Setup(f => f.ParseGodotVersion(fileClient.Object)).Returns(versions[i]);
    }

    var log = new Mock<ILog>();

    var repo = new GodotVersionSpecifierRepository("test/working-dir", fileClient.Object);

    repo.InferVersion(Enumerate(fileMocks, m => m.Object), log.Object).ShouldBe(versions[1]);
  }

  [Fact]
  public void InfersNoVersionAndLogsIfNoValidVersionFile()
  {
    var fileClient = new Mock<IFileClient>();

    var versions = new List<Result<SpecificDotnetStatusGodotVersion>> {
      new(false, null, "Testing-0"),
      new(false, null, "Testing-1"),
      new(false, null, "Testing-2"),
    };
    var fileMocks = new List<Mock<IGodotVersionFile>> {
      new(),
      new(),
      new(),
    };
    for (var i = 0; i < versions.Count; ++i)
    {
      fileMocks[i].Setup(f => f.FilePath).Returns($"file{i}");
      fileMocks[i].Setup(f => f.ParseGodotVersion(fileClient.Object)).Returns(versions[i]);
    }

    var log = new Mock<ILog>();

    var repo = new GodotVersionSpecifierRepository("test/working-dir", fileClient.Object);

    var result = repo.InferVersion(Enumerate(fileMocks, m => m.Object), log.Object);
    result.IsSuccess.ShouldBeFalse();
    result.Error.ShouldBe("No valid Godot version found in specifier files");

    for (var i = 0; i < versions.Count; ++i)
    {
      log.Verify(
        log =>
          log.Warn($"file{i} does not contain valid version string; skipping")
      );
      log.Verify(
        log =>
          log.Warn($"Testing-{i}")
      );
    }
  }

  [Fact]
  public void ProjectDirectoryIsHighestSolutionIfGodotProjectLower()
  {
    var directoryNames = new List<string> {
      "/test/path/to/project",
      "/test/path/to",
      "/test/path",
      "/test",
      "/",
    };
    var directoryMocks = new List<Mock<IDirectoryInfo>> {
      new(),
      new(),
      new(),
      new(),
      new(),
    };
    for (var i = 0; i < directoryMocks.Count; ++i)
    {
      directoryMocks[i].Setup(dir => dir.FullName).Returns(directoryNames[i]);
      if (i < directoryMocks.Count - 1)
      {
        directoryMocks[i].Setup(dir => dir.Parent).Returns(directoryMocks[i + 1].Object);
      }
    }
    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(
        client => client.GetAncestorDirectories(directoryNames[0])
      ).Returns(Enumerate(directoryMocks, m => m.Object));
    fileClient.Setup(
      client => client.GetFiles(directoryNames[0], "*.sln")
    ).Returns((string dir, string pattern) => [$"{dir}/Other.sln"]);
    fileClient.Setup(
      client => client.GetFiles(directoryNames[2], "*.sln")
    ).Returns((string dir, string pattern) => [$"{dir}/Project.sln"]);
    fileClient.Setup(
      client => client.GetFiles(directoryNames[1], "project.godot")
    ).Returns((string dir, string pattern) => [$"{dir}/project.godot"]);

    var repo = new GodotVersionSpecifierRepository(directoryNames[0], fileClient.Object);
    repo.GetProjectDefinitionDirectory().ShouldBe(directoryNames[2]);
  }

  [Fact]
  public void ProjectDirectoryIsHighestSolutionIfGodotProjectHigher()
  {
    var directoryNames = new List<string> {
      "/test/path/to/project",
      "/test/path/to",
      "/test/path",
      "/test",
      "/",
    };
    var directoryMocks = new List<Mock<IDirectoryInfo>> {
      new(),
      new(),
      new(),
      new(),
      new(),
    };
    for (var i = 0; i < directoryMocks.Count; ++i)
    {
      directoryMocks[i].Setup(dir => dir.FullName).Returns(directoryNames[i]);
      if (i < directoryMocks.Count - 1)
      {
        directoryMocks[i].Setup(dir => dir.Parent).Returns(directoryMocks[i + 1].Object);
      }
    }
    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(
        client => client.GetAncestorDirectories(directoryNames[0])
      ).Returns(Enumerate(directoryMocks, m => m.Object));
    fileClient.Setup(
      client => client.GetFiles(directoryNames[0], "*.sln")
    ).Returns((string dir, string pattern) => [$"{dir}/Other.sln"]);
    fileClient.Setup(
      client => client.GetFiles(directoryNames[2], "*.sln")
    ).Returns((string dir, string pattern) => [$"{dir}/Project.sln"]);
    fileClient.Setup(
      client => client.GetFiles(directoryNames[3], "project.godot")
    ).Returns((string dir, string pattern) => [$"{dir}/project.godot"]);

    var repo = new GodotVersionSpecifierRepository(directoryNames[0], fileClient.Object);
    repo.GetProjectDefinitionDirectory().ShouldBe(directoryNames[2]);
  }

  [Fact]
  public void ProjectDirectoryIsHighestGodotProjectIfNoSln()
  {
    var directoryNames = new List<string> {
      "/test/path/to/project",
      "/test/path/to",
      "/test/path",
      "/test",
      "/",
    };
    var directoryMocks = new List<Mock<IDirectoryInfo>> {
      new(),
      new(),
      new(),
      new(),
      new(),
    };
    for (var i = 0; i < directoryMocks.Count; ++i)
    {
      directoryMocks[i].Setup(dir => dir.FullName).Returns(directoryNames[i]);
      if (i < directoryMocks.Count - 1)
      {
        directoryMocks[i].Setup(dir => dir.Parent).Returns(directoryMocks[i + 1].Object);
      }
    }
    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(
        client => client.GetAncestorDirectories(directoryNames[0])
      ).Returns(Enumerate(directoryMocks, m => m.Object));
    fileClient.Setup(
      client => client.GetFiles(directoryNames[2], "project.godot")
    ).Returns((string dir, string pattern) => [$"{dir}/project.godot"]);
    fileClient.Setup(
      client => client.GetFiles(directoryNames[3], "project.godot")
    ).Returns((string dir, string pattern) => [$"{dir}/project.godot"]);

    var repo = new GodotVersionSpecifierRepository(directoryNames[0], fileClient.Object);
    repo.GetProjectDefinitionDirectory().ShouldBe(directoryNames[3]);
  }

  [Fact]
  public void PinVersionWritesGlobalJsonForDotnetVersion()
  {
    var projectDir = "/test/path";
    var expectedRawJson =
      /*lang=json,strict*/
      """
      {
        "msbuild-sdks": {
          "Godot.NET.Sdk": "4.4.1"
        }
      }
      """;
    var expectedNode = JsonNode.Parse(expectedRawJson);
    var serializerOptions = new JsonSerializerOptions() { WriteIndented = true };

    var path = "/test/path/global.json";
    var version = new SpecificDotnetStatusGodotVersion(4, 4, 1, "stable", -1, true);
    var writer = new Mock<TextWriter>();
    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(client => client.Combine(projectDir, "global.json")).Returns(path);
    fileClient.Setup(client => client.FileExists(path)).Returns(false);
    fileClient.Setup(client => client.GetWriter(path)).Returns(writer.Object);
    var log = new Mock<ILog>();
    var repo = new GodotVersionSpecifierRepository(projectDir, fileClient.Object);

    repo.PinVersion(version, projectDir, log.Object);

    writer.Verify(wrt => wrt.WriteLine(expectedNode!.ToJsonString(serializerOptions)));
    log.Verify(log => log.Info($"Writing Godot version \"{new IoVersionSerializer().SerializeWithDotnetStatus(version)}\" to {path}"));
  }

  [Fact]
  public void PinVersionWritesGodotrcForNonDotnetVersion()
  {
    var projectDir = "/test/path";
    var path = "/test/path/.godotrc";
    var version = new SpecificDotnetStatusGodotVersion(4, 4, 1, "stable", -1, false);
    var serializer = new IoVersionSerializer();
    var versionString = serializer.SerializeWithDotnetStatus(version);
    var writer = new Mock<TextWriter>();
    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(client => client.Combine(projectDir, ".godotrc")).Returns(path);
    fileClient.Setup(client => client.FileExists(path)).Returns(false);
    fileClient.Setup(client => client.GetWriter(path)).Returns(writer.Object);
    var log = new Mock<ILog>();
    var repo = new GodotVersionSpecifierRepository(projectDir, fileClient.Object);

    repo.PinVersion(version, projectDir, log.Object);

    writer.Verify(wrt => wrt.WriteLine(versionString));
    log.Verify(log => log.Info($"Writing Godot version \"{versionString}\" to {path}"));
  }

  public IEnumerable<TResult> Enumerate<TResult, TCollection>(
    IEnumerable<TCollection> collection,
    Func<TCollection, TResult> transform
  )
  {
    foreach (var item in collection)
    {
      yield return transform(item);
    }
  }
}
