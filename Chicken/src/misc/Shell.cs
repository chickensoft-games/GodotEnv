namespace Chickensoft.Chicken;
using System.Threading.Tasks;
using CliFx.Exceptions;

public interface IShell {
  Task<IProcessResult> Run(string executable, params string[] args);
  Task<IProcessResult> RunUnchecked(string executable, params string[] args);
}

public class Shell : IShell {
  private readonly IProcessRunner _runner;
  private readonly string _workingDir;

  public Shell(IProcessRunner runner, string workingDir) {
    _runner = runner;
    _workingDir = workingDir;
  }

  public async Task<IProcessResult> Run(
    string executable, params string[] args
  ) {
    var result = await _runner.Run(_workingDir, executable, args);
    if (!result.Success) {
      throw new CommandException(
        $"Failed to run `{executable}` in `{_workingDir}`" +
        $" with args `{string.Join(" ", args)}`. Received exit " +
        $"code {result.ExitCode}."
      );
    }
    return result;
  }

  public async Task<IProcessResult> RunUnchecked(
    string executable, params string[] args
  ) => await _runner.Run(_workingDir, executable, args);
}
