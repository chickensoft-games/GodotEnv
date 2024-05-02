namespace Chickensoft.GodotEnv.Features.Godot.Domain;

using System;
using System.Collections.Generic;
using System.IO;
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
  public Task VerifyArchiveChecksum(GodotCompressedArchive archive);

  /// <summary>
  /// Gets the expected checksum for a given GodotCompressedArchive
  /// </summary>
  /// <param name="archive">Archive to get the checksum for</param>
  /// <returns>Checksum as hex-string</returns>
  public Task<string> GetExpectedChecksumForArchive(GodotCompressedArchive archive);
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
    var suffix = archive.Version.LabelNoDots == string.Empty ? "-stable" : string.Empty;
    var releaseFilename = $"godot-{archive.Version.Format(true, true)}{suffix}.json";
    return $"https://raw.githubusercontent.com/godotengine/godot-builds/main/releases/{releaseFilename}";
  }

  public async Task VerifyArchiveChecksum(GodotCompressedArchive archive) {
    var expected = await GetExpectedChecksumForArchive(archive);
    var computed = await ComputeChecksumOfArchive(archive);

    if (computed != expected) {
      throw new ChecksumMismatchException($"Expected: {expected}, Actual: {computed}");
    }
  }

  public async Task<string> GetExpectedChecksumForArchive(GodotCompressedArchive archive) {
    var checksumFileResponse = await NetworkClient.WebRequestGetAsync(GetChecksumFileUrl(archive));
    checksumFileResponse.EnsureSuccessStatusCode();

    var metadata = await checksumFileResponse.Content.ReadFromJsonAsync<JsonReleaseMetadata>() ?? throw new KeyNotFoundException("Metadata object not found");
    var downloadFileName = Platform.GetInstallerFilename(archive.Version, archive.IsDotnetVersion);
    var fileData = metadata.GetChecksumForFile(downloadFileName) ?? throw new MissingChecksumException($"File checksum for {downloadFileName} not present");

    return fileData.Checksum ?? throw new MissingChecksumException($"File checksum for {downloadFileName} not present");
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
    return Convert.ToHexString(await sha512.ComputeHashAsync(filestream)).ToLower();
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
