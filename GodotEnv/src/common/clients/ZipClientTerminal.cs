namespace Chickensoft.GodotEnv.Common.Clients;

using System;
using System.Globalization;
using System.IO.Abstractions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Utilities;

public partial class ZipClientTerminal : IZipClient
{
  public IComputer Computer { get; }
  public IFileSystem Files { get; }

  public static readonly Regex NumFilesRegex = numFilesRegex();

  public ZipClientTerminal(IComputer computer, IFileSystem files)
  {
    Computer = computer;
    Files = files;
  }

  public async Task<int> ExtractToDirectory(
    string sourceArchiveFileName,
    string destinationDirectoryName,
    IProgress<double> progress
  )
  {
    var parentDir = Files.Path.GetDirectoryName(destinationDirectoryName)!;
    Files.Directory.CreateDirectory(parentDir);
    var shell = Computer.CreateShell(parentDir);

    // See how many files there are.
    var stdOut = await shell.Run("unzip", "-l", sourceArchiveFileName);
    var numFiles = int.Parse(
      NumFilesRegex.Match(stdOut.StandardOutput).Groups["numFiles"].Value,
      CultureInfo.InvariantCulture
    );

    var numEntries = 0;

    await shell.RunWithUpdates(
      "unzip",
      (stdOutLine) =>
      {
        if (stdOutLine.Contains("inflating:") || stdOutLine.Contains("creating:") || stdOutLine.Contains("extracting:"))
        {
          numEntries++;
          progress.Report((double)numEntries / numFiles);
        }
      },
      (stdErrLine) => { },
      "-o", sourceArchiveFileName, "-d", destinationDirectoryName
    );

    return numEntries;
  }

  [GeneratedRegex(@"\s*(?<numFiles>\d+)\s+files?")]
  private static partial Regex numFilesRegex();
}
