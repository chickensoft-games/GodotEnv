namespace Chickensoft.Chicken.Tests;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Shouldly;

public enum RunMode {
  Run,
  RunUnchecked,
}

// Test utility class that makes it easier to verify the order of
// shell commands that were executed.
public class ShellVerifier {
  private readonly Dictionary<string, Mock<IShell>> _shells = new();
  private readonly MockSequence _sequence = new();
  private int _calls;
  private int _added;

  public ShellVerifier() { }

  /// <summary>
  /// Creates a mock shell for the given working directory.
  /// </summary>
  /// <param name="workingDir">Directory in which the shell commands should
  /// be run from.</param>
  /// <returns>The mocked shell.</returns>
  public Mock<IShell> CreateShell(string workingDir) {
    var shell = new Mock<IShell>(MockBehavior.Strict);
    _shells.Add(workingDir, shell);
    return shell;
  }

  /// <summary>
  /// Adds a mock shell command to be verified later.
  /// </summary>
  /// <param name="workingDir">Directory in which the shell command should
  /// be run. Must have created a mock shell previously for this
  /// directory.</param>
  /// <param name="result">Execution result.</param>
  /// <param name="runMode">How the execution of the process runner is
  /// expected to be called from the system under test.</param>
  /// <param name="exe">Cli executable.</param>
  /// <param name="args">Executable args.</param>
  public void Setup(
    string workingDir,
    ProcessResult result,
    RunMode runMode,
    string exe,
    params string[] args
  ) {
    if (_shells.TryGetValue(workingDir, out var sh)) {
      MockSetup(sh, result, runMode, exe, args);
    }
    else {
      throw new InvalidOperationException($"Shell not found: {workingDir}");
    }
  }

  /// <summary>
  /// After creating mock shells and setting up verification calls, call
  /// this to verify that all of your mocked calls are actually run in the
  /// expected order by the system under test.
  /// </summary>
  public void VerifyAll() {
    foreach (var (_, value) in _shells) {
      value.VerifyAll();
    }
    if (_calls < _added) {
      throw new InvalidOperationException(
        $"{_calls} calls were made, but {_added} were added. " +
        $"Missing {_added - _calls} calls."
      );
    }
  }

  private void MockSetup(
    Mock<IShell> shell,
    IProcessResult result,
    RunMode runMode,
    string exe,
    string[] args
  ) {
    var call = _added++;
    if (runMode == RunMode.Run) {
      shell.InSequence(_sequence).Setup(shell => shell.Run(exe, args))
        .Returns(Task.FromResult(result))
        .Callback(() => _calls++.ShouldBe(call));
    }
    else {
      shell.InSequence(_sequence).Setup(
        shell => shell.RunUnchecked(exe, args)
      )
      .Returns(Task.FromResult(result))
      .Callback(() => _calls++.ShouldBe(call));
    }
  }
}
