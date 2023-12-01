namespace Chickensoft.GodotEnv.Common.Utilities;

using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.EventStream;

/// <summary>External shell process execution result.</summary>
/// <param name="ExitCode">Process exit code.</param>
/// <param name="StandardOutput">Standard output from the process.</param>
/// <param name="StandardError">Standard error output from the process.</param>
public record ProcessResult(
  int ExitCode,
  string StandardOutput = "",
  string StandardError = ""
) {
  /// <summary>
  /// True if the process succeeded, false otherwise.
  /// </summary>
  public bool Succeeded => ExitCode == 0;
}

/// <summary>Process runner interface.</summary>
public interface IProcessRunner {
  /// <summary>
  /// Run an external process.
  /// </summary>
  /// <param name="workingDir">Working directory to run the process from.
  /// </param>
  /// <param name="exe">Process to run (must be in the system shell's path).
  /// </param>
  /// <param name="args">Process arguments.</param>
  /// <returns>Process result task.</returns>
  Task<ProcessResult> Run(string workingDir, string exe, string[] args);

  /// <summary>
  /// Runs an external process with callbacks for stdout and stderr.
  /// </summary>
  /// <param name="workingDir">Working directory to run the process from.
  /// </param>
  /// <param name="exe">Process to run (must be in the system shell's path).
  /// </param>
  /// <param name="args">Process arguments.</param>
  /// <param name="onStdOut">Standard output callback.</param>
  /// <param name="onStdError">Standard error callback.</param>
  /// <returns>Process result task.</returns>
  Task<ProcessResult> RunWithUpdates(
      string workingDir,
      string exe,
      string[] args,
      Action<string> onStdOut,
      Action<string> onStdError
    );

  /// <summary>
  /// Attempts to run a shell command that requires an administrator role on Windows.
  /// </summary>
  /// <param name="exe">Process to run (must be in the system shell's path).
  /// </param>
  /// <param name="args">Process arguments.</param>
  /// <returns>Process result task.</returns>
  Task<ProcessResult> RunElevatedOnWindows(string exe, string args);
}

/// <summary>Process runner.</summary>
public class ProcessRunner : IProcessRunner {
  public async Task<ProcessResult> Run(
    string workingDir, string exe, string[] args
  ) {
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

  public async Task<ProcessResult> RunElevatedOnWindows(
    string exe, string args
  ) {
    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
      throw new InvalidOperationException(
        "RunElevatedOnWindows is only supported on Windows."
      );
    }

    // If a debugger is attached, the godotenv command is not elevated globally
    if (
      !Debugger.IsAttached &&
      !new WindowsPrincipal(WindowsIdentity.GetCurrent())
        .IsInRole(WindowsBuiltInRole.Administrator)
    ) {
      throw new InvalidOperationException(
        "RunElevatedOnWindows is only supported with admin role."
      );
    }

    // If a debugger is attached, the process is elevated as the godotenv
    // command is not elevated globally
    Process process = new() {
      StartInfo = new() {
        FileName = exe,
        Arguments = args,
        UseShellExecute = Debugger.IsAttached,
        Verb = Debugger.IsAttached ? "runas" : string.Empty,
        CreateNoWindow = !Debugger.IsAttached
      }
    };

    process.Start();

    await process.WaitForExitAsync();

    return new ProcessResult(
      ExitCode: process.ExitCode,
      StandardOutput: "",
      StandardError: ""
    );
  }

  public async Task<ProcessResult> RunWithUpdates(
    string workingDir,
    string exe,
    string[] args,
    Action<string> onStdOut,
    Action<string> onStdError
  ) {
    var stdOutBuffer = new StringBuilder();
    var stdErrBuffer = new StringBuilder();

    var cmd = Cli.Wrap(exe)
      .WithArguments(args)
      .WithValidation(CommandResultValidation.None)
      .WithWorkingDirectory(workingDir);

    var exitCode = 0;
    await cmd.Observe().ForEachAsync(@event => {
      switch (@event) {
        // case StartedCommandEvent started:
        case StandardOutputCommandEvent stdOut:
          onStdOut(stdOut.Text);
          stdOutBuffer.Append(stdOut.Text);
          break;
        case StandardErrorCommandEvent stdErr:
          onStdError(stdErr.Text);
          stdErrBuffer.Append(stdErr.Text);
          break;
        case ExitedCommandEvent exited:
          exitCode = exited.ExitCode;
          break;
        default:
          break;
      }
    });

    return new ProcessResult(
      ExitCode: exitCode,
      StandardOutput: stdOutBuffer.ToString(),
      StandardError: stdErrBuffer.ToString()
    );
  }
}
