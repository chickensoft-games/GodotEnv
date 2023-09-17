namespace Chickensoft.GodotEnv.Common.Clients;

using System;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Utilities;

/// <summary>
/// Zip client. Credit: https://stackoverflow.com/a/42436311
/// </summary>
public interface IZipClient {
  Task ExtractToDirectory(
    string sourceArchiveFileName,
    string destinationDirectoryName,
    IProgress<double> progress,
    ILog log
  );
}

/// <summary>
/// Zip client. Credit: https://stackoverflow.com/a/42436311
/// </summary>
public class ZipClient : IZipClient {
  public static Func<string, ZipArchive> ZipFileOpenReadDefault { get; } =
    (path) => ZipFile.Open(path, ZipArchiveMode.Read);
  public static Func<string, ZipArchive> ZipFileOpenRead { get; set; } =
    ZipFileOpenReadDefault;

  public IFileSystem Files { get; }

  public ZipClient(IFileSystem files) {
    Files = files;
  }

  public Task ExtractToDirectory(
    string sourceArchiveFileName,
    string destinationDirectoryName,
    IProgress<double> progress,
    ILog log
  ) {
    using var archive = ZipFileOpenRead(sourceArchiveFileName);
    var totalBytes = archive.Entries.Sum(e => e.Length);
    var currentBytes = 0d;

    foreach (var entry in archive.Entries) {
      var fileName = Files.Path.Combine(
        destinationDirectoryName, entry.FullName
      );

      var isDirectory = fileName.EndsWith('/') || fileName.EndsWith('\\');

      if (isDirectory) {
        Files.Directory.CreateDirectory(fileName);
        continue;
      }

      Files.Directory.CreateDirectory(Files.Path.GetDirectoryName(fileName));

      using (var inputStream = entry.Open())
      using (var outputStream = Files.File.OpenWrite(fileName)) {
        var progressStream = new ProgressStream(
          outputStream,
          new BasicProgress<int>((p) => { }),
          new BasicProgress<int>(i => {
            currentBytes += i;
            progress.Report(currentBytes / totalBytes);
          })
        );

        inputStream.CopyTo(progressStream);
      }

      Files.File.SetLastWriteTime(
        fileName, entry.LastWriteTime.LocalDateTime
      );
    }

    return Task.CompletedTask;
  }
}
