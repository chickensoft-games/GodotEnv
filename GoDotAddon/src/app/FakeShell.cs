namespace Chickensoft.GoDotAddon {
  using System.Threading.Tasks;
  using CliFx.Infrastructure;


  public class FakeShell : IShell {
    private readonly ConsoleWriter _output;
    private readonly string _workingDir = "";

    public FakeShell(ConsoleWriter output, string workingDir) {
      _output = output;
      _workingDir = workingDir;
    }

    public Task<ProcessResult> Run(
      string executable, params string[] args
    ) {
      _output.WriteLine(
        $"{_workingDir}: {executable} {string.Join(" ", args)}"
      );
      return Task.FromResult(new ProcessResult(0));
    }

    public Task<ProcessResult> RunUnchecked(
      string executable, params string[] args
    ) => Run(executable, args);
  }
}
