namespace Chickensoft.GodotEnv.Tests.Features.Godot.Models;

using System;
using System.Collections.Generic;
using System.IO;
using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Features.Godot.Models;
using Chickensoft.GodotEnv.Features.Godot.Serializers;
using Moq;
using Shouldly;
using Xunit;

public class GodotRcFileTest
{
  [Fact]
  public void NewGodotRcFileHasFilePath()
  {
    var path = "/test/path";
    var file = new GodotrcFile(path);
    file.FilePath.ShouldBe(path);
  }

  [Fact]
  public void ParsedVersionIsFirstLineOfFileContents()
  {
    var lines = new string[] { "4.2.0", "4.3.0", "4.4.1" };
    var lineQueue = new Queue<string>(lines);
    var path = "/test/path";
    var file = new GodotrcFile(path);
    var reader = new Mock<TextReader>();
    reader.SetupSequence(rdr => rdr.ReadLine()).Returns(lineQueue.Dequeue);
    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(client => client.GetReader(path)).Returns(reader.Object);
    var version = new SpecificDotnetStatusGodotVersion(4, 2, 0, "stable", -1, true);
    var parsedVersion = file.ParseGodotVersion(fileClient.Object);
    parsedVersion.IsSuccess.ShouldBeTrue();
    parsedVersion.Value.ShouldBe(version);
  }

  [Fact]
  public void ParsedVersionIsEmptyIfFileEmpty()
  {
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
  public void ParsedVersionIsNotDotnetIfFollowedBySpaceSeparatedNoDotnet()
  {
    var path = "/test/path";
    var file = new GodotrcFile(path);
    var reader = new Mock<TextReader>();
    reader.Setup(rdr => rdr.ReadLine()).Returns("4.2.0 no-dotnet");
    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(client => client.GetReader(path)).Returns(reader.Object);
    var version = new SpecificDotnetStatusGodotVersion(4, 2, 0, "stable", -1, false);
    var parsedVersion = file.ParseGodotVersion(fileClient.Object);
    parsedVersion.IsSuccess.ShouldBeTrue();
    parsedVersion.Value.ShouldBe(version);
  }

  [Fact]
  public void ParsedVersionIsNotDotnetIfFollowedBySpaceSeparatedNotDotnet()
  {
    var path = "/test/path";
    var file = new GodotrcFile(path);
    var reader = new Mock<TextReader>();
    reader.Setup(rdr => rdr.ReadLine()).Returns("4.2.0 not-dotnet");
    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(client => client.GetReader(path)).Returns(reader.Object);
    var version = new SpecificDotnetStatusGodotVersion(4, 2, 0, "stable", -1, false);
    var parsedVersion = file.ParseGodotVersion(fileClient.Object);
    parsedVersion.IsSuccess.ShouldBeTrue();
    parsedVersion.Value.ShouldBe(version);
  }

  [Fact]
  public void ParsedVersionIsNotDotnetIfFollowedBySpaceSeparatedNonDotnet()
  {
    var path = "/test/path";
    var file = new GodotrcFile(path);
    var reader = new Mock<TextReader>();
    reader.Setup(rdr => rdr.ReadLine()).Returns("4.2.0 non-dotnet");
    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(client => client.GetReader(path)).Returns(reader.Object);
    var version = new SpecificDotnetStatusGodotVersion(4, 2, 0, "stable", -1, false);
    var parsedVersion = file.ParseGodotVersion(fileClient.Object);
    parsedVersion.IsSuccess.ShouldBeTrue();
    parsedVersion.Value.ShouldBe(version);
  }

  [Fact]
  public void ParseVersionThrowsIfFirstLineOfFileContentsIsNotValidVersion()
  {
    var path = "/test/path";
    var file = new GodotrcFile(path);
    var reader = new Mock<TextReader>();
    reader.Setup(rdr => rdr.ReadLine()).Returns("not.a.version");
    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(client => client.GetReader(path)).Returns(reader.Object);
    Should.Throw<ArgumentException>(() => file.ParseGodotVersion(fileClient.Object));
  }

  [Fact]
  public void WriteGodotVersionOpensWriterToFileAndWritesVersionWithoutDotnetStatusIfDotnet()
  {
    var path = "/test/path/.godotrc";
    var version = new SpecificDotnetStatusGodotVersion(4, 4, 1, "stable", -1, true);
    var serializer = new IoVersionSerializer();
    var file = new GodotrcFile(path);
    var writer = new Mock<TextWriter>();
    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(client => client.GetWriter(path)).Returns(writer.Object);
    file.WriteGodotVersion(version, fileClient.Object);
    writer.Verify(wrt => wrt.WriteLine(serializer.Serialize(version)));
  }

  [Fact]
  public void WriteGodotVersionOpensWriterToFileAndWritesVersionWithDotnetStatusIfNotDotnet()
  {
    var path = "/test/path/.godotrc";
    var version = new SpecificDotnetStatusGodotVersion(4, 4, 1, "stable", -1, false);
    var serializer = new IoVersionSerializer();
    var file = new GodotrcFile(path);
    var writer = new Mock<TextWriter>();
    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(client => client.GetWriter(path)).Returns(writer.Object);
    file.WriteGodotVersion(version, fileClient.Object);
    writer.Verify(wrt => wrt.WriteLine(serializer.SerializeWithDotnetStatus(version)));
  }
}
