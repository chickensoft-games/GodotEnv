namespace Chickensoft.GodotEnv.Features.Godot.Domain;

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Common.Clients;
using Models;

public interface IGodotChecksumClient {
  /// <summary>
  /// Gets the checksum for a given archive, computes the checksum
  /// of the downloaded archive and compares them, raising an Exception
  /// if they do not match.
  /// </summary>
  /// <param name="archive">Downloaded archive to check</param>
  /// <returns>Nothing</returns>
  public Task VerifyArchiveChecksum(GodotCompressedArchive archive, string? proxyUrl = null);

  /// <summary>
  /// Gets the expected checksum for a given GodotCompressedArchive
  /// </summary>
  /// <param name="archive">Archive to get the checksum for</param>
  /// <returns>Checksum as hex-string</returns>
  public Task<string> GetExpectedChecksumForArchive(GodotCompressedArchive archive, string? proxyUrl = null);
  /// <summary>
  /// Computes the checksum of a given local GodotCompressedArchive
  /// </summary>
  /// <param name="archive">Archive to compute the checksum for</param>
  /// <returns>Checksum as hex-string</returns>
  public Task<string> ComputeChecksumOfArchive(GodotCompressedArchive archive);
}

/// <summary>
/// A GodotChecksumClient accessing the godotengine/godot-builds releases
/// repository as source for checksums.
/// </summary>
/// <param name="networkClient">Network client to use to make requests</param>
/// <param name="platform">Platform used to determine file names</param>
public class GodotChecksumClient(
    INetworkClient networkClient,
    IGodotEnvironment platform
  ) : IGodotChecksumClient {
  private INetworkClient NetworkClient { get; } = networkClient;
  private IGodotEnvironment Platform { get; } = platform;

  private static string GetChecksumFileUrl(GodotCompressedArchive archive) {
    // We need to be sure this will be a release-style version string to get the right URL
    var versionConverter = new ReleaseVersionStringConverter();
    var releaseFilename = $"godot-{versionConverter.VersionString(archive.Version)}.json";
    return $"https://raw.githubusercontent.com/godotengine/godot-builds/main/releases/{releaseFilename}";
  }

  public async Task VerifyArchiveChecksum(GodotCompressedArchive archive, string? proxyUrl = null) {
    var expected = await GetExpectedChecksumForArchive(archive, proxyUrl);
    var computed = await ComputeChecksumOfArchive(archive);

    if (computed != expected) {
      throw new ChecksumMismatchException($"Expected: {expected}, Actual: {computed}");
    }
  }

  public async Task<string> GetExpectedChecksumForArchive(GodotCompressedArchive archive, string? proxyUrl = null) {
    try {
      var checksumFileResponse = await NetworkClient.WebRequestGetAsync(GetChecksumFileUrl(archive), true, proxyUrl);
      checksumFileResponse.EnsureSuccessStatusCode();

      var metadata = await checksumFileResponse.Content.ReadFromJsonAsync<JsonReleaseMetadata>() ?? throw new KeyNotFoundException("Metadata object not found");
      var downloadFileName = Platform.GetInstallerFilename(archive.Version, archive.IsDotnetVersion);
      var fileData = metadata.GetChecksumForFile(downloadFileName) ?? throw new MissingChecksumException($"File checksum for {downloadFileName} not present");

      return fileData.Checksum ?? throw new MissingChecksumException($"File checksum for {downloadFileName} not present");
    }
    catch (HttpRequestException ex) {
      throw new MissingChecksumException($"Failed to connect to checksum server. If you are using a proxy, please ensure your proxy configuration is correct. URL: {GetChecksumFileUrl(archive)}", ex);
    }
  }

  private record JsonReleaseMetadata {
    [JsonPropertyName("files")]
    public List<FileChecksumData>? Files { get; init; }

    public FileChecksumData? GetChecksumForFile(string filename) => Files?.Find((x) => x.Filename == filename);
  }

  private record FileChecksumData {
    [JsonPropertyName("filename")]
    public string? Filename { get; init; }
    [JsonPropertyName("checksum")]
    public string? Checksum { get; init; }
  }

  public async Task<string> ComputeChecksumOfArchive(GodotCompressedArchive archive) {
    using var sha512 = SHA512.Create();
    await using var filestream = File.OpenRead(Path.Join(archive.Path, archive.Filename));
    return Convert.ToHexString(await sha512.ComputeHashAsync(filestream)).ToLowerInvariant();
  }
}

public class MissingChecksumException : Exception {
  public MissingChecksumException() { }
  public MissingChecksumException(string message) : base(message) { }
  public MissingChecksumException(string message, Exception innerException) : base(message, innerException) { }
}

public class ChecksumMismatchException : Exception {
  public ChecksumMismatchException() { }
  public ChecksumMismatchException(string message) : base(message) { }
  public ChecksumMismatchException(string message, Exception innerException) : base(message, innerException) { }
}
