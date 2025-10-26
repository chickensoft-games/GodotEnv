namespace Chickensoft.GodotEnv.Common.Utilities;

using System;
using System.Threading.Tasks;

public interface IShell
{
  IProcessRunner Runner { get; }
  string WorkingDir { get; }

  Task<ProcessResult> Run(string executable, params string[] args);
  Task<ProcessResult> RunUnchecked(string executable, params string[] args);
  Task<ProcessResult> RunWithUpdates(
      string executable,
      Action<string> onStdOut,
      Action<string> onStdError,
      params string[] args
    );
}

public class Shell(IProcessRunner runner, string workingDir) : IShell
{
  public IProcessRunner Runner { get; } = runner;
  public string WorkingDir { get; } = workingDir;

  public async Task<ProcessResult> Run(
    string executable, params string[] args
  )
  {
    var result = await Runner.Run(WorkingDir, executable, args);
    if (!result.Succeeded)
    {
      throw new InvalidOperationException(
        $"Failed to run `{executable}` in `{WorkingDir}`" +
        $" with args `{string.Join(" ", args)}`. Received exit " +
        $"code {result.ExitCode}."
      );
    }
    return result;
  }

  public async Task<ProcessResult> RunUnchecked(
    string executable, params string[] args
  ) => await Runner.Run(WorkingDir, executable, args);

  public async Task<ProcessResult> RunWithUpdates(
    string executable,
    Action<string> onStdOut,
    Action<string> onStdError,
    params string[] args
  )
  {
    var result = await Runner.RunWithUpdates(
      WorkingDir, executable, args, onStdOut, onStdError
    );
    if (!result.Succeeded)
    {
      throw new InvalidOperationException(
        $"Failed to run `{executable}` in `{WorkingDir}`" +
        $" with args `{string.Join(" ", args)}`. Received exit " +
        $"code {result.ExitCode}."
      );
    }
    return result;
  }
}
