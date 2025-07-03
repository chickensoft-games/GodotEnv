namespace Chickensoft.GodotEnv.Tests.Features.Godot.Models;

using System;
using System.IO;
using System.Text.Json.Nodes;
using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Features.Godot.Models;
using Moq;
using Shouldly;
using Xunit;

public class GlobalJsonFileTest {
  [Fact]
  public void NewGlobalJsonFileHasFilePath() {
    var path = "/test/path";
    var file = new GlobalJsonFile(path);
    file.FilePath.ShouldBe(path);
  }

  [Fact]
  public void ParsedVersionIsGodotSdkIfPresent() {
    var contents =
        /*lang=json,strict*/
        """
        {
          "sdk": {
            "version": "8.0.410",
            "rollForward": "latestMinor"
          },
          "msbuild-sdks": {
            "Godot.NET.Sdk": "4.4.1"
          }
        }
        """;
    var path = "/test/path";
    var file = new GlobalJsonFile(path);
    using (var stream = new MemoryStream()) {
      using (var writer = new StreamWriter(stream, leaveOpen: true)) {
        writer.Write(contents);
        writer.Flush();
      }
      stream.Position = 0;
      var fileClient = new Mock<IFileClient>();
      fileClient.Setup(client => client.GetReadStream(path)).Returns(stream);
      var version = new SpecificDotnetStatusGodotVersion(4, 4, 1, "stable", -1, true);
      file.ParseGodotVersion(fileClient.Object).ShouldBe(version);
    }
  }

  [Fact]
  public void ParsedVersionIsNullIfGodotSdkNotPresent() {
    var contents =
        /*lang=json,strict*/
        """
        {
          "sdk": {
            "version": "8.0.410",
            "rollForward": "latestMinor"
          }
        }
        """;
    var path = "/test/path";
    var file = new GlobalJsonFile(path);
    using (var stream = new MemoryStream()) {
      using (var writer = new StreamWriter(stream, leaveOpen: true)) {
        writer.Write(contents);
        writer.Flush();
      }
      stream.Position = 0;
      var fileClient = new Mock<IFileClient>();
      fileClient.Setup(client => client.GetReadStream(path)).Returns(stream);
      file.ParseGodotVersion(fileClient.Object).ShouldBe(null);
    }
  }

  [Fact]
  public void ParseVersionThrowsIfGodotSdkVersionInvalid() {
    var contents =
        /*lang=json,strict*/
        """
        {
          "sdk": {
            "version": "8.0.410",
            "rollForward": "latestMinor"
          },
          "msbuild-sdks": {
            "Godot.NET.Sdk": "not.a.version"
          }
        }
        """;
    var path = "/test/path";
    var file = new GlobalJsonFile(path);
    using (var stream = new MemoryStream()) {
      using (var writer = new StreamWriter(stream, leaveOpen: true)) {
        writer.Write(contents);
        writer.Flush();
      }
      stream.Position = 0;
      var fileClient = new Mock<IFileClient>();
      fileClient.Setup(client => client.GetReadStream(path)).Returns(stream);
      Should.Throw<ArgumentException>(() => file.ParseGodotVersion(fileClient.Object));
    }
  }

  [Fact]
  public void WriteGodotVersionOpensWriterToNewFileAndWritesVersionWithDotnetStatus() {
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

    var path = "/test/path/global.json";
    var version = new SpecificDotnetStatusGodotVersion(4, 4, 1, "stable", -1, true);
    var file = new GlobalJsonFile(path);
    var writer = new Mock<TextWriter>();
    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(client => client.FileExists(path)).Returns(false);
    fileClient.Setup(client => client.GetWriter(path)).Returns(writer.Object);
    file.WriteGodotVersion(version, fileClient.Object);
    writer.Verify(wrt => wrt.WriteLine(expectedNode!.ToJsonString(file.JsonSerializerOptions)));
  }

  [Fact]
  public void WriteGodotVersionOpensWriterToExistingFileAndWritesNewMsbuildSdkSectionAndVersionWithDotnetStatus() {
    var existingJson =
      /*lang=json,strict*/
      """
      {
        "sdk": {
          "version": "8.0.410",
          "rollForward": "latestMinor"
        }
      }
      """;
    var expectedRawJson =
      /*lang=json,strict*/
      """
      {
        "sdk": {
          "version": "8.0.410",
          "rollForward": "latestMinor"
        },
        "msbuild-sdks": {
          "Godot.NET.Sdk": "4.4.1"
        }
      }
      """;
    var expectedNode = JsonNode.Parse(expectedRawJson);

    var path = "/test/path/global.json";
    var version = new SpecificDotnetStatusGodotVersion(4, 4, 1, "stable", -1, true);
    var file = new GlobalJsonFile(path);
    using (var stream = new MemoryStream()) {
      using (var existingWriter = new StreamWriter(stream, leaveOpen: true)) {
        existingWriter.Write(existingJson);
        existingWriter.Flush();
      }
      stream.Position = 0;
      var writer = new Mock<TextWriter>();
      var fileClient = new Mock<IFileClient>();
      fileClient.Setup(client => client.FileExists(path)).Returns(true);
      fileClient.Setup(client => client.GetReadStream(path)).Returns(stream);
      fileClient.Setup(client => client.GetWriter(path)).Returns(writer.Object);
      file.WriteGodotVersion(version, fileClient.Object);
      writer.Verify(wrt => wrt.WriteLine(expectedNode!.ToJsonString(file.JsonSerializerOptions)));
    }
  }

  [Fact]
  public void WriteGodotVersionOpensWriterToExistingFileAndOverwritesExistingVersionWithDotnetStatus() {
    var existingJson =
      /*lang=json,strict*/
      """
      {
        "sdk": {
          "version": "8.0.410",
          "rollForward": "latestMinor"
        },
        "msbuild-sdks": {
          "Godot.NET.Sdk": "4.3.0"
        }
      }
      """;
    var expectedRawJson =
      /*lang=json,strict*/
      """
      {
        "sdk": {
          "version": "8.0.410",
          "rollForward": "latestMinor"
        },
        "msbuild-sdks": {
          "Godot.NET.Sdk": "4.4.1"
        }
      }
      """;
    var expectedNode = JsonNode.Parse(expectedRawJson);

    var path = "/test/path/global.json";
    var version = new SpecificDotnetStatusGodotVersion(4, 4, 1, "stable", -1, true);
    var file = new GlobalJsonFile(path);
    using (var stream = new MemoryStream()) {
      using (var existingWriter = new StreamWriter(stream, leaveOpen: true)) {
        existingWriter.Write(existingJson);
        existingWriter.Flush();
      }
      stream.Position = 0;
      var writer = new Mock<TextWriter>();
      var fileClient = new Mock<IFileClient>();
      fileClient.Setup(client => client.FileExists(path)).Returns(true);
      fileClient.Setup(client => client.GetReadStream(path)).Returns(stream);
      fileClient.Setup(client => client.GetWriter(path)).Returns(writer.Object);
      file.WriteGodotVersion(version, fileClient.Object);
      writer.Verify(wrt => wrt.WriteLine(expectedNode!.ToJsonString(file.JsonSerializerOptions)));
    }
  }
}
