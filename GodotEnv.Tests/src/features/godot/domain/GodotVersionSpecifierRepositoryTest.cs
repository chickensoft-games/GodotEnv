namespace Chickensoft.GodotEnv.Tests.Features.Config.Commands.List;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Common.Utilities;
using Chickensoft.GodotEnv.Features.Godot.Domain;
using Chickensoft.GodotEnv.Features.Godot.Models;
using Moq;
using Shouldly;
using Xunit;

public class GodotVersionSpecifierRepositoryTest {
  // all global.json first, all csproj second, all godotrc third
  // nearest ancestor to furthest ancestor
  [Fact]
  public void GetsVersionFilesInCorrectOrder() {
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
    for (var i = 0; i < directoryMocks.Count; ++i) {
      directoryMocks[i].Setup(dir => dir.FullName).Returns(directoryNames[i]);
      if (i < directoryMocks.Count - 1) {
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
    foreach (var dir in directoryNames) {
      results.Add(new GlobalJsonFile($"{dir}/global.json"));
    }
    foreach (var dir in directoryNames) {
      results.Add(new CsprojFile($"{dir}/Project.csproj"));
    }
    foreach (var dir in directoryNames) {
      results.Add(new GodotrcFile($"{dir}/.godotrc"));
    }

    var repo = new GodotVersionSpecifierRepository(directoryNames[0], fileClient.Object);

    repo.GetVersionFiles().ShouldBe(results);
  }

  [Fact]
  public void ValidatedGodotVersionIsNullIfVersionIsNull() {
    var fileClient = new Mock<IFileClient>();
    var file = new Mock<IGodotVersionFile>();
    SpecificDotnetStatusGodotVersion? version = null;
    file.Setup(f => f.ParseGodotVersion(fileClient.Object)).Returns(version);

    var log = new Mock<ILog>();

    var repo = new GodotVersionSpecifierRepository("/test/working-dir", fileClient.Object);

    repo.GetValidatedGodotVersion(file.Object, log.Object).ShouldBe(null);
  }

  [Fact]
  public void ValidatedGodotVersionIsNullAndWarnsIfParsingThrows() {
    var deserializationError = "bad.version is not a recognized version string";
    var path = "/test/path";
    var fileClient = new Mock<IFileClient>();

    var file = new Mock<IGodotVersionFile>();
    file.Setup(f => f.FilePath).Returns(path);
    file.Setup(f => f.ParseGodotVersion(fileClient.Object))
      .Throws(new ArgumentException(deserializationError));

    var log = new Mock<ILog>();

    var repo = new GodotVersionSpecifierRepository("test/working-dir", fileClient.Object);

    repo.GetValidatedGodotVersion(file.Object, log.Object)
      .ShouldBe(null);
    log.Verify(
      log =>
        log.Warn($"{path} contains invalid version string; skipping")
    );
    log.Verify(
      log =>
        log.Warn(deserializationError)
    );
  }

  [Fact]
  public void ValidatedGodotVersionIsVersionIfVersionDeserializerReturns() {
    var version = new SpecificDotnetStatusGodotVersion(4, 4, 1, "stable", -1, true);
    var path = "/test/path";
    var fileClient = new Mock<IFileClient>();

    var file = new Mock<IGodotVersionFile>();
    file.Setup(f => f.FilePath).Returns(path);
    file.Setup(f => f.ParseGodotVersion(fileClient.Object)).Returns(version);

    var log = new Mock<ILog>();

    var repo = new GodotVersionSpecifierRepository("test/working-dir", fileClient.Object);

    repo.GetValidatedGodotVersion(file.Object, log.Object)
      .ShouldBe(version);
  }

  [Fact]
  public void InfersVersionFromFirstValidVersionFile() {
    var fileClient = new Mock<IFileClient>();

    var versions = new List<SpecificDotnetStatusGodotVersion?> {
      null,
      new(4, 3, 0, "stable", -1, true),
      new(4, 4, 1, "stable", -1, true),
    };
    var fileMocks = new List<Mock<IGodotVersionFile>> {
      new(),
      new(),
      new(),
    };
    for (var i = 0; i < versions.Count; ++i) {
      fileMocks[i].Setup(f => f.ParseGodotVersion(fileClient.Object)).Returns(versions[i]);
    }

    var log = new Mock<ILog>();

    var repo = new GodotVersionSpecifierRepository("test/working-dir", fileClient.Object);

    repo.InferVersion(Enumerate(fileMocks, m => m.Object), log.Object).ShouldBe(versions[1]);
  }

  [Fact]
  public void InfersNoVersionIfNoValidVersionFile() {
    var fileClient = new Mock<IFileClient>();

    var versions = new List<SpecificDotnetStatusGodotVersion?> {
      null,
      null,
      null,
    };
    var fileMocks = new List<Mock<IGodotVersionFile>> {
      new(),
      new(),
      new(),
    };
    for (var i = 0; i < versions.Count; ++i) {
      fileMocks[i].Setup(f => f.ParseGodotVersion(fileClient.Object)).Returns(versions[i]);
    }

    var log = new Mock<ILog>();

    var repo = new GodotVersionSpecifierRepository("test/working-dir", fileClient.Object);

    repo.InferVersion(Enumerate(fileMocks, m => m.Object), log.Object).ShouldBe(null);
  }

  public IEnumerable<TResult> Enumerate<TResult, TCollection>(
    IEnumerable<TCollection> collection,
    Func<TCollection, TResult> transform
  ) {
    foreach (var item in collection) {
      yield return transform(item);
    }
  }
}
