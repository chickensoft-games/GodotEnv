namespace Chickensoft.Chicken {
  using System.Text;
  using System.Threading.Tasks;
  using CliWrap;

  public interface IProcessResult {
    int ExitCode { get; init; }
    string StandardOutput { get; init; }
    string StandardError { get; init; }
    bool Success { get; }
  }

  public record ProcessResult(
    int ExitCode,
    string StandardOutput = "",
    string StandardError = ""
  ) : IProcessResult {
    public bool Success => ExitCode == 0;
  }

  public interface IProcessRunner {
    Task<IProcessResult> Run(string workingDir, string exe, string[] args);
  }
  public class ProcessRunner : IProcessRunner {
    public async Task<IProcessResult> Run(string workingDir, string exe, string[] args) {
      var stdOutBuffer = new StringBuilder();
      var stdErrBuffer = new StringBuilder();
      var result = await Cli.Wrap(exe)
        .WithArguments(args)
        .WithValidation(CommandResultValidation.None)
        .WithWorkingDirectory(workingDir)
        .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
        .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
        .ExecuteAsync();
      return new ProcessResult(
        ExitCode: result.ExitCode,
        StandardOutput: stdOutBuffer.ToString(),
        StandardError: stdErrBuffer.ToString()
      );
    }
  }
}
