namespace Chickensoft.GodotEnv.Tests;

using System;
using System.Linq;
using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Utilities;
using Moq;
using Shouldly;
using Xunit;

public class ShellTest
{
  private const string WORKING_DIR = ".";

  [Fact]
  public void Initializes()
  {
    var processRunner = new Mock<IProcessRunner>();
    var shell = new Shell(processRunner.Object, WORKING_DIR);

    shell.WorkingDir.ShouldBe(WORKING_DIR);
    shell.Runner.ShouldBe(processRunner.Object);
  }

  [Fact]
  public async Task ShellRunsProcess()
  {
    const int exitCode = 0;
    var processRunner = new Mock<IProcessRunner>();
    const string exe = "return";
    const string arg = "0";
    var processResult = new ProcessResult(exitCode);
    processRunner.Setup(
      pr => pr.Run(WORKING_DIR, exe, It.Is<string[]>(
        value => value.SequenceEqual(new string[] { arg })
      ))
    ).Returns(Task.FromResult(processResult));
    var shell = new Shell(processRunner.Object, WORKING_DIR);
    var result = await shell.Run(exe, arg);
    result.ExitCode.ShouldBe(exitCode);
    result.Succeeded.ShouldBe(true);
    processRunner.VerifyAll();
  }

  [Fact]
  public async Task ShellRunThrowsOnNonZeroExitCode()
  {
    const int exitCode = 1;
    var processRunner = new Mock<IProcessRunner>();
    const string exe = "return";
    const string arg = "0";
    var processResult = new ProcessResult(exitCode);
    processRunner.Setup(
      pr => pr.Run(WORKING_DIR, exe, It.Is<string[]>(
        value => value.SequenceEqual(new string[] { arg })
      ))
    ).Returns(Task.FromResult(processResult));
    var shell = new Shell(processRunner.Object, WORKING_DIR);
    await Should.ThrowAsync<InvalidOperationException>(
      async () => await shell.Run(exe, arg)
    );
  }

  [Fact]
  public async Task ShellRunUncheckedReturnsResult()
  {
    const int exitCode = 1;
    var processRunner = new Mock<IProcessRunner>();
    const string exe = "return";
    const string arg = "0";
    var processResult = new ProcessResult(exitCode);
    processRunner.Setup(
      pr => pr.Run(WORKING_DIR, exe, It.Is<string[]>(
        value => value.SequenceEqual(new string[] { arg })
      ))
    ).Returns(Task.FromResult(processResult));
    var shell = new Shell(processRunner.Object, WORKING_DIR);
    var result = await shell.RunUnchecked(exe, arg);
    result.ExitCode.ShouldBe(exitCode);
    result.Succeeded.ShouldBe(false);
  }
}
