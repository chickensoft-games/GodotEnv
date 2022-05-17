namespace GoDotAddon {
  using CliFx.Exceptions;

  public interface IShell {
    Task<ProcessResult> Run(string executable, params string[] args);
    Task<ProcessResult> RunUnchecked(string executable, params string[] args);
  }

  public class Shell : IShell {
    private readonly IProcessRunner _runner;
    private readonly string _workingDir;

    public Shell(IProcessRunner runner, string workingDir) {
      _runner = runner;
      _workingDir = workingDir;
    }

    public async Task<ProcessResult> Run(
      string executable, params string[] args
    ) {
      var result = await _runner.Run(_workingDir, executable, args);
      if (!result.Success) {
        throw new CommandException($"Failed to run `{executable}`");
      }
      return result;
    }

    public async Task<ProcessResult> RunUnchecked(
      string executable, params string[] args
    ) => await _runner.Run(_workingDir, executable, args);
  }
}
