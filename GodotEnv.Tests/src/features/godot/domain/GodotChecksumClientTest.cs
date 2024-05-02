namespace Chickensoft.GodotEnv.Tests.features.godot.domain;

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Chickensoft.GodotEnv.Features.Godot.Domain;
using Chickensoft.GodotEnv.Features.Godot.Models;
using Common.Clients;
using Moq;
using Xunit;

public class GodotChecksumClientTest {
  private static readonly string GODOT_4_3_DEV_5_MACOS_CHECKSUM = "0fdd44c725980c463d86b14aeb47fc41a35ff9005e9df9a9c821168b21d60f845d80313e93c892565daadef04d02c6f6fbb6a9d9a26374db9caa8cd4d9354d7c";

  private static readonly string GODOT_ENV_STRING_CHECKSUM =
    "3a2e1fa23f9e99ff976803d6fb1283707e015b7904040f604a6a10240c1eba138c6feed88e9ec5db72aee81c6f9b1eba99292e346eab054004e2427a4d4b39b8";

  private static string GetChecksumFileUrl(string version) =>
    $"https://raw.githubusercontent.com/godotengine/godot-builds/main/releases/godot-{version}.json";

  public static IEnumerable<object[]> CorrectChecksumUrlRequestedTestData() {
    yield return [new SemanticVersion("1", "2", "3"), false, GetChecksumFileUrl("1.2.3-stable")];
    yield return [new SemanticVersion("1", "0", "0"), false, GetChecksumFileUrl("1.0-stable")];
    yield return [new SemanticVersion("4", "0", "0", "alpha14"), false, GetChecksumFileUrl("4.0-alpha14")];
    yield return [new SemanticVersion("4", "2", "2", "rc1"), false, GetChecksumFileUrl("4.2.2-rc1")];
    // GodotSharp nuget packages use a dot in the label, and this has to be supported as well.
    yield return [new SemanticVersion("4", "3", "0", "dev.6"), false, GetChecksumFileUrl("4.3-dev6")];
  }

  [Theory]
  [MemberData(nameof(CorrectChecksumUrlRequestedTestData))]
  public async Task CorrectChecksumUrlRequested(
    SemanticVersion version,
    bool isDotNetVersion,
    string expectedChecksumUrl
  ) {
    var archive = new GodotCompressedArchive(
      string.Empty,
      string.Empty,
      version,
      isDotNetVersion,
      string.Empty
    );

    var networkClient = new Mock<INetworkClient>();
    networkClient.Setup(
      client => client.WebRequestGetAsync(
        It.IsAny<string>()
      ))
      .ThrowsAsync(new HttpRequestException());

    var platform = new Mock<IGodotEnvironment>();

    var checksumClient = new GodotChecksumClient(networkClient.Object, platform.Object);

    await Assert.ThrowsAsync<HttpRequestException>(async () => await checksumClient.GetExpectedChecksumForArchive(archive));

    networkClient.Verify(nc => nc.WebRequestGetAsync(expectedChecksumUrl), Times.Once);
  }

  public static IEnumerable<object[]> CorrectlyParsedJsonTestData() {
    yield return [
      false,
      "Godot_v4.3-dev5_macos.universal.zip",
      GODOT_4_3_DEV_5_MACOS_CHECKSUM
    ];

    yield return [
      true,
      "Godot_v4.3-dev5_mono_macos.universal.zip",
      "18790956c8c12be4458c47aa3b682ccfce4430fc43bd740372940cb5f294988035d2d709af8f05b59db6d0e8f9e36fb998e2b1105608f89d32b6cfef3f77ed36"
    ];

    yield return [
      true,
      "Godot_v4.3-dev5_mono_win64.zip",
      "c53b87f8f5369059fd729605a0e508123289fa02e1ffca2dc53fd97245bc78ade667346856505b821c75821d8720380fcf5e0d337a38bd030e8e05c6858305db"
    ];

    yield return [
      false,
      "Godot_v4.3-dev5_linux.x86_64.zip",
      "800e272ffb8ba92b535f6b17ffe7578273d9fd0b9e56d2b14d1db2eddbdffa3822be8e3f3e76775f1d9c940520a553a41ba1e2f3eb00e49992d03be090a7a022"
    ];
  }


  [Theory]
  [MemberData(nameof(CorrectlyParsedJsonTestData))]
  public async void CorrectlyParsedJson(
    bool isDotnetVersion,
    string filename,
    string expectedChecksum
    ) {
    var networkClient = await GetMockChecksumFileNetworkClient("godot-4.3-dev5.json");

    var archive = new GodotCompressedArchive(
      string.Empty,
      string.Empty,
      new SemanticVersion("4", "3", "0", "dev5"),
      isDotnetVersion,
      string.Empty
    );

    var platform = new Mock<IGodotEnvironment>();
    platform.Setup(
      platform => platform.GetInstallerFilename(
        It.IsAny<SemanticVersion>(),
        It.IsAny<bool>()
      )
    ).Returns(filename);

    var checksumClient = new GodotChecksumClient(networkClient.Object, platform.Object);

    var checksumFromClient = await checksumClient.GetExpectedChecksumForArchive(archive);

    Assert.Equal(expectedChecksum, checksumFromClient);
  }

  /// <summary>
  /// At the time of implementation (2024-04-29) there was no checksum data published
  /// for versions below 3.2.2-beta1. This test uses the empty release data for
  /// Godot v1.1 to verify that a correct exception is raised.
  /// </summary>
  [Fact]
  public async Task MissingVersionDataRaisesMissingChecksumException() {
    const string testDataFilename = "godot-1.1-stable.json";
    const string downloadFileName = "Godot_v1.1_stable_win64.exe.zip";
    var networkClient = await GetMockChecksumFileNetworkClient(testDataFilename);

    var archive = new GodotCompressedArchive(
      string.Empty,
      downloadFileName,
      new SemanticVersion("1", "1", "0"),
      false,
      string.Empty
    );

    var platform = new Mock<IGodotEnvironment>();
    platform.Setup(
      platform => platform.GetInstallerFilename(
        It.IsAny<SemanticVersion>(),
        It.IsAny<bool>()
      )
    ).Returns(downloadFileName);

    var checksumClient = new GodotChecksumClient(networkClient.Object, platform.Object);

    var ex = await Assert.ThrowsAsync<MissingChecksumException>(
      async () => await checksumClient.GetExpectedChecksumForArchive(archive)
    );

    var ex2 = await Assert.ThrowsAsync<MissingChecksumException>(
      async () => await checksumClient.VerifyArchiveChecksum(archive)
    );

    Assert.Equal(ex.Message, $"File checksum for {downloadFileName} not present");
    Assert.Equal(ex2.Message, $"File checksum for {downloadFileName} not present");

  }

  /// <summary>
  /// Creates a new Mock instance of INetworkClient that returns a JSON response
  /// with the contents of a given embedded resource in the data directory to any
  /// request.
  ///
  /// If you want to add another resource, you will have to configure it to be an
  /// embedded resource.
  /// </summary>
  /// <param name="responseFilename">Filename from data directory whose </param>
  /// <returns>A INetworkClient returning the JSON contents to any request.</returns>
  /// <exception cref="FileNotFoundException">Thrown if the embedded resource cannot be found.</exception>
  private static async Task<Mock<INetworkClient>> GetMockChecksumFileNetworkClient(string responseFilename) {
    var resourceName = $"Chickensoft.GodotEnv.Tests.src.features.godot.domain.data.{responseFilename}";
    string godotReleaseJson;
    await using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)
                              ?? throw new FileNotFoundException("Failed to get test release JSON file.")) {
      using (var reader = new StreamReader(stream))
      {
        godotReleaseJson = await reader.ReadToEndAsync();
      }
    }

    var networkClient = new Mock<INetworkClient>();
    networkClient.Setup(
        client => client.WebRequestGetAsync(
          It.IsAny<string>()
        ))
      .ReturnsAsync(() => {
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        response.Content = new StringContent(godotReleaseJson);
        return response;
      });
    return networkClient;
  }

  [Fact]
  public async void VerifyChecksumComputation() {
    var tempFileName = Path.GetTempFileName();

    try {
      await using (var writer = File.CreateText(tempFileName)) {
        await writer.WriteAsync("GodotEnv");
      }

      var dummyArchive = new GodotCompressedArchive(
        "TestFilename",
        Path.GetFileName(tempFileName),
        new SemanticVersion("1", "0", "0"),
        true,
        Path.GetDirectoryName(tempFileName) ?? "/"
      );

      var networkClient = new Mock<INetworkClient>();
      var platform = new Mock<IGodotEnvironment>();

      var checksumClient = new GodotChecksumClient(networkClient.Object, platform.Object);

      var computed = await checksumClient.ComputeChecksumOfArchive(dummyArchive);

      var expected =
        "3a2e1fa23f9e99ff976803d6fb1283707e015b7904040f604a6a10240c1eba138c6feed88e9ec5db72aee81c6f9b1eba99292e346eab054004e2427a4d4b39b8";

      Assert.Equal(expected, computed);
    }
    finally {
      File.Delete(tempFileName);
    }
  }

  [Fact]
  public async void IncorrectChecksumThrowsChecksumMismatchException() {
    var archiveDirectory = Path.Join(Path.GetTempPath(), "GodotEnvTest" + Guid.NewGuid());
    Directory.CreateDirectory(archiveDirectory);

    var archiveFileName = "Godot_v4.3-dev5_macos.universal.zip";
    var archivePath = Path.Join(archiveDirectory, archiveFileName);

    try {
      await using (var writer = File.CreateText(archivePath)) {
        await writer.WriteAsync("GodotEnv");
      }

      var networkClient = await GetMockChecksumFileNetworkClient("godot-4.3-dev5.json");

      var archive = new GodotCompressedArchive(
        string.Empty,
        archiveFileName,
        new SemanticVersion("4", "3", "0", "dev5"),
        false,
        Path.GetDirectoryName(archivePath) ?? "/"
      );

      var platform = new Mock<IGodotEnvironment>();
      platform.Setup(
        platform => platform.GetInstallerFilename(
          It.IsAny<SemanticVersion>(),
          It.IsAny<bool>()
        )
      ).Returns(archiveFileName);

      var checksumClient = new GodotChecksumClient(networkClient.Object, platform.Object);

      var ex = await Assert.ThrowsAsync<ChecksumMismatchException>(
        async () => await checksumClient.VerifyArchiveChecksum(archive)
      );

      Assert.Equal(ex.Message, $"Expected: {GODOT_4_3_DEV_5_MACOS_CHECKSUM}, Actual: {GODOT_ENV_STRING_CHECKSUM}");
    }
    finally {
      File.Delete(archivePath);
    }
  }
}
