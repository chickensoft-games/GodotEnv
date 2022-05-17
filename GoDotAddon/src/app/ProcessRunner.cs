namespace GoDotAddon {
  using System.Text;
  using System.Threading.Tasks;
  using CliWrap;

  public record ProcessResult(
    int ExitCode,
    string StandardOutput = "",
    string StandardError = ""
  ) {
    public bool Success => ExitCode == 0;
  }

  public interface IProcessRunner {
    Task<ProcessResult> Run(string workingDir, string exe, string[] args);
  }
  public class ProcessRunner : IProcessRunner {
    public async Task<ProcessResult> Run(string workingDir, string exe, string[] args) {
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
