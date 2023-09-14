namespace Chickensoft.GodotEnv.Common.Clients;

using System;
using System.IO.Abstractions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Utilities;

public class ZipClientTerminal : IZipClient {
  public IComputer Computer { get; }
  public IFileSystem Files { get; }

  public static readonly Regex NumFilesRegex =
    new(@"\s*(?<numFiles>\d+)\s+files?");

  public ZipClientTerminal(IComputer computer, IFileSystem files) {
    Computer = computer;
    Files = files;
  }

  public async Task ExtractToDirectory(
    string sourceArchiveFileName,
    string destinationDirectoryName,
    IProgress<double> progress,
    ILog log
  ) {
    var parentDir = Files.Path.GetDirectoryName(destinationDirectoryName);
    Files.Directory.CreateDirectory(parentDir);
    var shell = Computer.CreateShell(parentDir);

    // See how many files there are.
    var stdOut = await shell.Run("unzip", "-l", sourceArchiveFileName);
    var numFiles = int.Parse(
      NumFilesRegex.Match(stdOut.StandardOutput).Groups["numFiles"].Value
    );

    var numEntries = 0d;

    await shell.RunWithUpdates(
      "unzip",
      (stdOutLine) => {
        if (stdOutLine.Contains("inflating:") || stdOutLine.Contains("creating:") || stdOutLine.Contains("extracting:")) {
          numEntries++;
          progress.Report(numEntries / numFiles);
        }
      },
      (stdErrLine) => { },
      "-o", sourceArchiveFileName, "-d", destinationDirectoryName
    );

    log.Print($"🗜 Extracted {numEntries} / {numFiles} files in {sourceArchiveFileName}.");
  }
}
