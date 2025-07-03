namespace Chickensoft.GodotEnv.Tests.Features.Godot.Models;

using System;
using System.Collections.Generic;
using System.IO;
using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Features.Godot.Models;
using Moq;
using Shouldly;
using Xunit;

public class GodotRcFileTest {
  [Fact]
  public void NewGodotRcFileHasFilePath() {
    var path = "/test/path";
    var file = new GodotrcFile(path);
    file.FilePath.ShouldBe(path);
  }

  [Fact]
  public void ParsedVersionIsFirstLineOfFileContents() {
    var lines = new string[] { "4.2.0", "4.3.0", "4.4.1" };
    var lineQueue = new Queue<string>(lines);
    var path = "/test/path";
    var file = new GodotrcFile(path);
    var reader = new Mock<TextReader>();
    reader.SetupSequence(rdr => rdr.ReadLine()).Returns(lineQueue.Dequeue);
    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(client => client.GetReader(path)).Returns(reader.Object);
    var version = new SpecificDotnetStatusGodotVersion(4, 2, 0, "stable", -1, true);
    file.ParseGodotVersion(fileClient.Object).ShouldBe(version);
  }

  [Fact]
  public void ParsedVersionIsEmptyIfFileEmpty() {
    var path = "/test/path";
    var file = new GodotrcFile(path);
    var reader = new Mock<TextReader>();
    string? eof = null;
    reader.Setup(rdr => rdr.ReadLine()).Returns(eof);
    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(client => client.GetReader(path)).Returns(reader.Object);
    file.ParseGodotVersion(fileClient.Object).ShouldBe(null);
  }

  [Fact]
  public void ParsedVersionIsNotDotnetIfFollowedBySpaceSeparatedNoDotnet() {
    var path = "/test/path";
    var file = new GodotrcFile(path);
    var reader = new Mock<TextReader>();
    reader.Setup(rdr => rdr.ReadLine()).Returns("4.2.0 no-dotnet");
    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(client => client.GetReader(path)).Returns(reader.Object);
    var version = new SpecificDotnetStatusGodotVersion(4, 2, 0, "stable", -1, false);
    file.ParseGodotVersion(fileClient.Object).ShouldBe(version);
  }

  [Fact]
  public void ParsedVersionIsNotDotnetIfFollowedBySpaceSeparatedNotDotnet() {
    var path = "/test/path";
    var file = new GodotrcFile(path);
    var reader = new Mock<TextReader>();
    reader.Setup(rdr => rdr.ReadLine()).Returns("4.2.0 not-dotnet");
    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(client => client.GetReader(path)).Returns(reader.Object);
    var version = new SpecificDotnetStatusGodotVersion(4, 2, 0, "stable", -1, false);
    file.ParseGodotVersion(fileClient.Object).ShouldBe(version);
  }

  [Fact]
  public void ParsedVersionIsNotDotnetIfFollowedBySpaceSeparatedNonDotnet() {
    var path = "/test/path";
    var file = new GodotrcFile(path);
    var reader = new Mock<TextReader>();
    reader.Setup(rdr => rdr.ReadLine()).Returns("4.2.0 non-dotnet");
    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(client => client.GetReader(path)).Returns(reader.Object);
    var version = new SpecificDotnetStatusGodotVersion(4, 2, 0, "stable", -1, false);
    file.ParseGodotVersion(fileClient.Object).ShouldBe(version);
  }

  [Fact]
  public void ParseVersionThrowsIfFirstLineOfFileContentsIsNotValidVersion() {
    var path = "/test/path";
    var file = new GodotrcFile(path);
    var reader = new Mock<TextReader>();
    reader.Setup(rdr => rdr.ReadLine()).Returns("not.a.version");
    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(client => client.GetReader(path)).Returns(reader.Object);
    Should.Throw<ArgumentException>(() => file.ParseGodotVersion(fileClient.Object));
  }
}
