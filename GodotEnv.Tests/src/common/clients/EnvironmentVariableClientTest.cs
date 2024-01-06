namespace Chickensoft.GodotEnv.Tests;

using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Utilities;
using Common.Clients;
using Common.Models;
using Moq;
using Shouldly;
using Xunit;

public class EnvironmentVariableClientTest {
  [Fact]
  public async void SetUserEnv() {
    const string WORKING_DIR = ".";
    var env = "GODOT";
    var envValue = "godotenv/godot/bin/godot";

    // Given
    var processRunner = new Mock<IProcessRunner>();

    // GetDefaultShell()
    processRunner.Setup(
      pr => pr.Run(WORKING_DIR, "sh", It.Is<string[]>(
        value => value.SequenceEqual(new[] { "-c", EnvironmentVariableClient.USER_SHELL_COMMAND_MAC })
      ))
    ).Returns(Task.FromResult(new ProcessResult(0, "bash")));
    processRunner.Setup(
      pr => pr.Run(WORKING_DIR, "sh", It.Is<string[]>(
        value => value.SequenceEqual(new[] { "-c", EnvironmentVariableClient.USER_SHELL_COMMAND_LINUX })
      ))
    ).Returns(Task.FromResult(new ProcessResult(0, "bash")));

    // GetUserEnv()
    processRunner.Setup(
      pr => pr.Run(WORKING_DIR, "bash", It.Is<string[]>(
        value => value.SequenceEqual(new[] { "-ic", $"echo ${env}" })
      ))).Returns(Task.FromResult(new ProcessResult(0, envValue)));

    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(fc => fc.OS).Returns(FileClient.IsOSPlatform(OSPlatform.OSX)
      ? OSType.MacOS
      : FileClient.IsOSPlatform(OSPlatform.Linux)
        ? OSType.Linux
        : FileClient.IsOSPlatform(OSPlatform.Windows)
          ? OSType.Windows
          : OSType.Unknown);
    fileClient.Setup(fc => fc.AppDataDirectory).Returns(WORKING_DIR);

    var computer = new Mock<IComputer>();
    computer.Setup(c => c.CreateShell(WORKING_DIR)).Returns(new Shell(processRunner.Object, WORKING_DIR));

    var envClient = new EnvironmentVariableClient(processRunner.Object, fileClient.Object, computer.Object, new Mock<EnvironmentClient>().Object);

    // var originalValue = envClient.GetUserEnv(env);

    // When
    await envClient.SetUserEnv(env, envValue);

    // Then
    envClient.GetUserEnv(env).ShouldBe(envValue);

    // Restoring original value
    // await envClient.SetUserEnv(env, originalValue);
    // envClient.GetUserEnv(env).ShouldBe(originalValue);
  }

  [Fact]
  public async void AppendToUserEnv() {
    var WORKING_DIR = ".";
    var env = Defaults.PATH_ENV_VAR_NAME;
    var envValue = "godotenv/godot/bin/godot";

    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(fc => fc.OS).Returns(FileClient.IsOSPlatform(OSPlatform.OSX)
      ? OSType.MacOS
      : FileClient.IsOSPlatform(OSPlatform.Linux)
        ? OSType.Linux
        : FileClient.IsOSPlatform(OSPlatform.Windows)
          ? OSType.Windows
          : OSType.Unknown);
    fileClient.Setup(fc => fc.AppDataDirectory).Returns(WORKING_DIR);

    var processRunner = new Mock<IProcessRunner>();

    // GetDefaultShell()
    processRunner.Setup(
          pr => pr.Run(WORKING_DIR, "sh", It.Is<string[]>(
            value => value.SequenceEqual(new[] { "-c", EnvironmentVariableClient.USER_SHELL_COMMAND_MAC })
          ))
        ).Returns(Task.FromResult(new ProcessResult(0, "bash")));
    processRunner.Setup(
      pr => pr.Run(WORKING_DIR, "sh", It.Is<string[]>(
        value => value.SequenceEqual(new[] { "-c", EnvironmentVariableClient.USER_SHELL_COMMAND_LINUX })
      ))
    ).Returns(Task.FromResult(new ProcessResult(0, "bash")));

    // GetUserEnv()
    processRunner.Setup(
      pr => pr.Run(WORKING_DIR, "bash", It.Is<string[]>(
        value => value.SequenceEqual(new[] { "-ic", $"echo ${env}" })
      ))).Returns(Task.FromResult(new ProcessResult(0, envValue)));

    var computer = new Mock<IComputer>();
    computer.Setup(c => c.CreateShell(WORKING_DIR)).Returns(new Shell(processRunner.Object, WORKING_DIR));

    var envClient = new Mock<IEnvironmentClient>();
    envClient.Setup(ec => ec.GetEnvironmentVariable(env, EnvironmentVariableTarget.User)).Returns(envValue);

    var envVarClient = new EnvironmentVariableClient(processRunner.Object, fileClient.Object, computer.Object, envClient.Object);

    await envVarClient.AppendToUserEnv(env, envValue);

    envVarClient.GetUserEnv(env).ShouldContain(envValue);
  }

  [PlatformFact(TestPlatform.Windows)]
  public void GetDefaultShellOnWindows() {
    var processRunner = new Mock<IProcessRunner>();
    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(fc => fc.OS).Returns(OSType.Windows);
    fileClient.Setup(fc => fc.AppDataDirectory).Returns(".");
    var computer = new Mock<IComputer>();
    var envClient = new EnvironmentVariableClient(processRunner.Object, fileClient.Object, computer.Object, new Mock<EnvironmentClient>().Object);

    envClient.GetUserDefaultShell().ShouldBe(string.Empty);
  }

  [PlatformFact(TestPlatform.Mac)]
  public void GetDefaultShellOnMac() => GetDefaultShellUnixRoutine(OSType.MacOS,
    ["-c", EnvironmentVariableClient.USER_SHELL_COMMAND_MAC]);

  [PlatformFact(TestPlatform.Linux)]
  public void GetDefaultShellOnLinux() =>
    GetDefaultShellUnixRoutine(OSType.Linux, ["-c", EnvironmentVariableClient.USER_SHELL_COMMAND_LINUX]);

  private static void GetDefaultShellUnixRoutine(OSType os, string[] shellArgs) {
    var processRunner = new Mock<IProcessRunner>();
    const string WORKING_DIR = ".";
    const int exitCode = 0;
    const string stdOutput = "bash";
    const string exe = "sh";

    var processResult = new ProcessResult(exitCode, stdOutput);
    processRunner.Setup(
      pr => pr.Run(WORKING_DIR, exe, It.Is<string[]>(
        value => value.SequenceEqual(shellArgs)
      ))
    ).Returns(Task.FromResult(processResult));

    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(fc => fc.OS).Returns(os);
    fileClient.Setup(fc => fc.AppDataDirectory).Returns(WORKING_DIR);

    var computer = new Mock<IComputer>();
    computer.Setup(c => c.CreateShell(WORKING_DIR)).Returns(new Shell(processRunner.Object, WORKING_DIR));

    var envVarClient = new EnvironmentVariableClient(processRunner.Object, fileClient.Object, computer.Object, new Mock<EnvironmentClient>().Object);

    var result = envVarClient.GetUserDefaultShell();

    result.ShouldBe(stdOutput);
    processRunner.VerifyAll();
  }

  [PlatformFact(TestPlatform.Windows)]
  public void CheckSupportedShellOnWindows() {
    var processRunner = new Mock<IProcessRunner>();
    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(fc => fc.OS).Returns(OSType.Windows);
    var computer = new Mock<IComputer>();
    var envClient = new EnvironmentVariableClient(processRunner.Object, fileClient.Object, computer.Object, new Mock<EnvironmentClient>().Object);

    envClient.IsShellSupported("any").ShouldBeTrue();
    envClient.IsShellSupported(string.Empty).ShouldBeTrue();
  }

  [PlatformFact(TestPlatform.MacLinux)]
  public void CheckSupportedShellOnMacLinux() {
    var processRunner = new Mock<IProcessRunner>();
    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(fc => fc.OS).Returns(FileClient.IsOSPlatform(OSPlatform.OSX)
      ? OSType.MacOS
      : FileClient.IsOSPlatform(OSPlatform.Linux)
        ? OSType.Linux
        : FileClient.IsOSPlatform(OSPlatform.Windows)
          ? OSType.Windows
          : OSType.Unknown);
    var computer = new Mock<IComputer>();
    var envClient = new EnvironmentVariableClient(processRunner.Object, fileClient.Object, computer.Object, new Mock<EnvironmentClient>().Object);

    envClient.IsShellSupported("zsh").ShouldBeTrue();
    envClient.IsShellSupported("bash").ShouldBeTrue();
    envClient.IsShellSupported("fish").ShouldBeFalse();
  }
}
