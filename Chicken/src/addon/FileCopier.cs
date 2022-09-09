namespace Chickensoft.Chicken {
  using System.IO.Abstractions;
  using System.Threading.Tasks;
  using CliFx.Exceptions;

  public class FileCopier {
    public IShell Shell { get; init; }
    public IFileSystem FS { get; init; }

    public FileCopier(IShell shell, IFileSystem fs) {
      Shell = shell;
      FS = fs;
    }

    public async Task Copy(string source, string destination) {
      // If we're running on windows, use robocopy instead of rsync
      if (FS.Path.DirectorySeparatorChar == '\\') {
        var result = await Shell.RunUnchecked(
          "robocopy", source, destination, "/e", "/xd", ".git"
        );
        if (result.ExitCode >= 8) {
          // Robocopy has non-traditional exit codes
          throw new CommandException(
            $"Failed to copy `{source}` to `{destination}`"
          );
        }
      }
      else {
        await Shell.Run(
          "rsync", "-av", source, destination, "--exclude", ".git"
        );
      }
    }
  }
}
