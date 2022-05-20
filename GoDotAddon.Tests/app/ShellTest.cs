namespace Chickensoft.GoDotAddon.Tests {
  using System;
  using System.Linq;
  using System.Threading.Tasks;
  using Chickensoft.GoDotAddon;
  using CliFx.Exceptions;
  using Moq;
  using Shouldly;
  using Xunit;

  public class ShellTest {
    private const string WORKING_DIR = ".";

    [Fact]
    public async Task ShellRunsProcess() {
      var exitCode = 0;
      var processRunner = new Mock<IProcessRunner>();
      var exe = "return";
      var arg = "0";
      processRunner.Setup(
        pr => pr.Run(WORKING_DIR, exe, It.Is<string[]>(
          value => value.SequenceEqual(new string[] { arg })
        ))
      ).Returns(Task.FromResult(new ProcessResult(exitCode)));
      var shell = new Shell(processRunner.Object, WORKING_DIR);
      var result = await shell.Run(exe, arg);
      result.ExitCode.ShouldBe(exitCode);
      result.Success.ShouldBe(true);
      processRunner.VerifyAll();
    }

    [Fact]
    public async Task ShellRunThrowsOnNonZeroExitCode() {
      var exitCode = 1;
      var processRunner = new Mock<IProcessRunner>();
      var exe = "return";
      var arg = "0";
      processRunner.Setup(
        pr => pr.Run(WORKING_DIR, exe, It.Is<string[]>(
          value => value.SequenceEqual(new string[] { arg })
        ))
      ).Returns(Task.FromResult(new ProcessResult(exitCode)));
      var shell = new Shell(processRunner.Object, WORKING_DIR);
      await Should.ThrowAsync<CommandException>(
        async () => await shell.Run(exe, arg)
      );
    }

    [Fact]
    public async Task ShellRunUncheckedReturnsResult() {
      var exitCode = 1;
      var processRunner = new Mock<IProcessRunner>();
      var exe = "return";
      var arg = "0";
      processRunner.Setup(
        pr => pr.Run(WORKING_DIR, exe, It.Is<string[]>(
          value => value.SequenceEqual(new string[] { arg })
        ))
      ).Returns(Task.FromResult(new ProcessResult(exitCode)));
      var shell = new Shell(processRunner.Object, WORKING_DIR);
      var result = await shell.RunUnchecked(exe, arg);
      result.ExitCode.ShouldBe(exitCode);
      result.Success.ShouldBe(false);
    }
  }
}
