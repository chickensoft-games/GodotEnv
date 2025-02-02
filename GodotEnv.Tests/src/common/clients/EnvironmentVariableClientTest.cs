namespace Chickensoft.GodotEnv.Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Xml.Xsl;
using Chickensoft.GodotEnv.Common.Utilities;
using Common.Clients;
using Common.Models;
using global::GodotEnv.Common.Utilities;
using Moq;
using Shouldly;
using Xunit;

public class EnvironmentVariableClientTest {
  private static IEnumerable<object[]> GetSystemInfoForUnixOSes() {
    yield return [
      new MockSystemInfo(OSType.Linux, CPUArch.X64)
    ];
    yield return [
      new MockSystemInfo(OSType.MacOS, CPUArch.Arm64)
    ];
  }

  private static IEnumerable<object[]> GetSystemInfoForAllOSes() {
    var oSes = GetSystemInfoForUnixOSes();

    oSes = oSes.Append([
      new MockSystemInfo(OSType.Windows, CPUArch.X64)
    ]);

    foreach (var os in oSes) {
      yield return os;
    }
  }

  private static IEnumerable<object[]> CombineUnixOsesAndShell() {
    var oses = GetSystemInfoForUnixOSes();

    foreach (var obj in oses) {
      if (obj is not [var systemInfo]) {
        throw new InvalidOperationException();
      }

      foreach (var shell in EnvironmentVariableClient.SUPPORTED_UNIX_SHELLS) {
        yield return [systemInfo, shell];
      }
    }
  }

  [Theory]
  [MemberData(nameof(GetSystemInfoForAllOSes))]
  public async Task SetUserEnv(ISystemInfo systemInfo) {
    const string WORKING_DIR = ".";
    var env = "GODOT";
    var envValue = "godotenv/godot/bin/godot";

    // Given
    var processRunner = new Mock<IProcessRunner>();

    // GetUserDefaultShell()
    processRunner.Setup(
      pr => pr.Run(WORKING_DIR, "sh", It.Is<string[]>(
        value => value.SequenceEqual(new[] { "-c", EnvironmentVariableClient.USER_SHELL_COMMAND_MAC })
      ))
    ).Returns(Task.FromResult(new ProcessResult(0, "zsh")));
    processRunner.Setup(
      pr => pr.Run(WORKING_DIR, "sh", It.Is<string[]>(
        value => value.SequenceEqual(new[] { "-c", EnvironmentVariableClient.USER_SHELL_COMMAND_LINUX })
      ))
    ).Returns(Task.FromResult(new ProcessResult(0, "bash")));

    // GetUserEnv()
    processRunner.Setup(
      pr => pr.Run(WORKING_DIR, It.IsIn(EnvironmentVariableClient.SUPPORTED_UNIX_SHELLS), It.Is<string[]>(
        value => value.SequenceEqual(new[] { "-ic", $"echo ${env}" })
      ))).Returns(Task.FromResult(new ProcessResult(0, envValue)));

    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(fc => fc.AppDataDirectory).Returns(WORKING_DIR);

    var computer = new Mock<IComputer>();
    computer.Setup(c => c.CreateShell(WORKING_DIR)).Returns(new Shell(processRunner.Object, WORKING_DIR));

    var envClient = new Mock<IEnvironmentClient>();
    envClient.Setup(ec => ec.GetEnvironmentVariable(env, EnvironmentVariableTarget.User)).Returns(envValue);

    var envVarClient = new EnvironmentVariableClient(systemInfo, processRunner.Object, fileClient.Object,
      computer.Object, envClient.Object);

    // When
    envVarClient.SetUserEnv(env, envValue);

    // Then
    var userEnv = await envVarClient.GetUserEnv(env);
    userEnv.ShouldBe(envValue);
  }

  [Theory]
  [MemberData(nameof(GetSystemInfoForAllOSes))]
  public async Task AppendToUserEnv(ISystemInfo systemInfo) {
    var WORKING_DIR = ".";
    var env = Defaults.PATH_ENV_VAR_NAME;
    var envValue = "godotenv/godot/bin/godot";

    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(fc => fc.AppDataDirectory).Returns(WORKING_DIR);

    var processRunner = new Mock<IProcessRunner>();

    // GetDefaultShell()
    processRunner.Setup(
      pr => pr.Run(WORKING_DIR, "sh", It.Is<string[]>(
        value => value.SequenceEqual(new[] { "-c", EnvironmentVariableClient.USER_SHELL_COMMAND_MAC })
      ))
    ).Returns(Task.FromResult(new ProcessResult(0, "zsh")));
    processRunner.Setup(
      pr => pr.Run(WORKING_DIR, "sh", It.Is<string[]>(
        value => value.SequenceEqual(new[] { "-c", EnvironmentVariableClient.USER_SHELL_COMMAND_LINUX })
      ))
    ).Returns(Task.FromResult(new ProcessResult(0, "bash")));

    // GetUserEnv()
    processRunner.Setup(
      pr => pr.Run(WORKING_DIR, It.IsIn(EnvironmentVariableClient.SUPPORTED_UNIX_SHELLS), It.Is<string[]>(
        value => value.SequenceEqual(new[] { "-ic", $"echo ${env}" })
      ))).Returns(Task.FromResult(new ProcessResult(0, envValue)));

    var computer = new Mock<IComputer>();
    computer.Setup(c => c.CreateShell(WORKING_DIR)).Returns(new Shell(processRunner.Object, WORKING_DIR));

    var envClient = new Mock<IEnvironmentClient>();
    envClient.Setup(ec => ec.GetEnvironmentVariable(env, EnvironmentVariableTarget.User)).Returns(envValue);

    var envVarClient =
      new EnvironmentVariableClient(systemInfo, processRunner.Object, fileClient.Object, computer.Object,
        envClient.Object);

    await envVarClient.AppendToUserEnv(env, envValue);

    var userEnv = await envVarClient.GetUserEnv(env);
    userEnv.ShouldContain(envValue);
  }

  [PlatformFact(TestPlatform.Windows)]
  public async Task GetDefaultShellOnWindows() {
    var systemInfo = new MockSystemInfo(OSType.Windows, CPUArch.X64);
    var processRunner = new Mock<IProcessRunner>();
    var fileClient = new Mock<IFileClient>();
    // fileClient.Setup(fc => fc.OS).Returns(OSType.Windows);
    fileClient.Setup(fc => fc.AppDataDirectory).Returns(".");
    var computer = new Mock<IComputer>();
    var envClient = new EnvironmentVariableClient(systemInfo, processRunner.Object, fileClient.Object, computer.Object,
      new Mock<EnvironmentClient>().Object);

    var userDefaultShell = await envClient.GetUserDefaultShell();
    userDefaultShell.ShouldBe(string.Empty);
  }

  private static async Task GetDefaultShellUnixRoutine(ISystemInfo systemInfo, string[] shellArgs) {
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
    fileClient.Setup(fc => fc.AppDataDirectory).Returns(WORKING_DIR);

    var computer = new Mock<IComputer>();
    computer.Setup(c => c.CreateShell(WORKING_DIR)).Returns(new Shell(processRunner.Object, WORKING_DIR));

    var envVarClient = new EnvironmentVariableClient(systemInfo, processRunner.Object, fileClient.Object,
      computer.Object,
      new Mock<EnvironmentClient>().Object);

    var result = await envVarClient.GetUserDefaultShell();
    result.ShouldBe(stdOutput);
    processRunner.VerifyAll();
  }

  [Theory]
  [MemberData(nameof(GetSystemInfoForUnixOSes))]
  public async Task GetDefaultShellOnUnix(ISystemInfo systemInfo) => await GetDefaultShellUnixRoutine(systemInfo,
    systemInfo.OS == OSType.Linux
      ? ["-c", EnvironmentVariableClient.USER_SHELL_COMMAND_LINUX]
      : ["-c", EnvironmentVariableClient.USER_SHELL_COMMAND_MAC]);

  [Fact]
  public void IsSupportedShellOnWindows() {
    var systemInfo = new MockSystemInfo(OSType.Windows, CPUArch.X64);
    var processRunner = new Mock<IProcessRunner>();
    var fileClient = new Mock<IFileClient>();
    var computer = new Mock<IComputer>();
    var envClient = new EnvironmentVariableClient(systemInfo, processRunner.Object, fileClient.Object, computer.Object,
      new Mock<EnvironmentClient>().Object);

    envClient.IsShellSupported("any").ShouldBeTrue();
    envClient.IsShellSupported(string.Empty).ShouldBeTrue();
  }

  [Theory]
  [MemberData(nameof(GetSystemInfoForUnixOSes))]
  public void IsSupportedShellOnMacLinux(ISystemInfo systemInfo) {
    var processRunner = new Mock<IProcessRunner>();
    var fileClient = new Mock<IFileClient>();
    var computer = new Mock<IComputer>();
    var envClient = new EnvironmentVariableClient(systemInfo, processRunner.Object, fileClient.Object, computer.Object,
      new Mock<EnvironmentClient>().Object);

    envClient.IsShellSupported("zsh").ShouldBeTrue();
    envClient.IsShellSupported("bash").ShouldBeTrue();
    envClient.IsShellSupported("fish").ShouldBeFalse();
  }

  [Theory]
  [MemberData(nameof(GetSystemInfoForAllOSes))]
  public void IsDefaultShellSupportedWhenValidShell(ISystemInfo systemInfo) {
    const string WORKING_DIR = ".";
    const string linuxDefaultShell = "bash";
    const string macDefaultShell = "zsh";

    var processRunner = new Mock<IProcessRunner>();

    // GetuUserDefaultShell()
    processRunner.Setup(
      pr => pr.Run(WORKING_DIR, "sh", It.Is<string[]>(
        value => value.SequenceEqual(new[] { "-c", EnvironmentVariableClient.USER_SHELL_COMMAND_LINUX })
      ))
    ).Returns(Task.FromResult(new ProcessResult(0, linuxDefaultShell)));
    processRunner.Setup(
      pr => pr.Run(WORKING_DIR, "sh", It.Is<string[]>(
        value => value.SequenceEqual(new[] { "-c", EnvironmentVariableClient.USER_SHELL_COMMAND_MAC })
      ))
    ).Returns(Task.FromResult(new ProcessResult(0, macDefaultShell)));

    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(fc => fc.AppDataDirectory).Returns(WORKING_DIR);

    var computer = new Mock<IComputer>();
    computer.Setup(c => c.CreateShell(WORKING_DIR)).Returns(new Shell(processRunner.Object, WORKING_DIR));

    var envClient = new Mock<IEnvironmentClient>();

    var envVarClient =
      new EnvironmentVariableClient(systemInfo, processRunner.Object, fileClient.Object, computer.Object,
        envClient.Object);

    envVarClient.IsDefaultShellSupported.ShouldBeTrue();
  }

  [Theory]
  [MemberData(nameof(GetSystemInfoForUnixOSes))]
  public void IsDefaultShellSupportedWhenUnknownShellOnUnix(ISystemInfo systemInfo) {
    const string WORKING_DIR = ".";
    const string shellName = "fish";

    var processRunner = new Mock<IProcessRunner>();

    processRunner.Setup(
      pr => pr.Run(WORKING_DIR, "sh", It.Is<string[]>(
        value => value.Contains(EnvironmentVariableClient.USER_SHELL_COMMAND_LINUX) ||
                 value.Contains(EnvironmentVariableClient.USER_SHELL_COMMAND_MAC)
      ))
    ).Returns(Task.FromResult(new ProcessResult(0, shellName)));

    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(fc => fc.AppDataDirectory).Returns(WORKING_DIR);

    var computer = new Mock<IComputer>();
    computer.Setup(c => c.CreateShell(WORKING_DIR)).Returns(new Shell(processRunner.Object, WORKING_DIR));

    var envClient = new Mock<IEnvironmentClient>();

    var envVarClient =
      new EnvironmentVariableClient(systemInfo, processRunner.Object, fileClient.Object, computer.Object,
        envClient.Object);

    envVarClient.IsDefaultShellSupported.ShouldBeFalse();
  }

  [Fact]
  public void IsDefaultShellSupportedOnWindows() {
    const string WORKING_DIR = ".";

    var systemInfo = new MockSystemInfo(OSType.Windows, CPUArch.X64);
    var processRunner = new Mock<IProcessRunner>();

    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(fc => fc.AppDataDirectory).Returns(WORKING_DIR);

    var computer = new Mock<IComputer>();
    computer.Setup(c => c.CreateShell(WORKING_DIR)).Returns(new Shell(processRunner.Object, WORKING_DIR));

    var envClient = new Mock<IEnvironmentClient>();

    var envVarClient =
      new EnvironmentVariableClient(systemInfo, processRunner.Object, fileClient.Object, computer.Object,
        envClient.Object);

    envVarClient.IsDefaultShellSupported.ShouldBeTrue();
  }

  [Theory]
  [MemberData(nameof(CombineUnixOsesAndShell))]
  public void ValidUserShellOnUnix(ISystemInfo systemInfo, string shellName) {
    const string WORKING_DIR = ".";

    var processRunner = new Mock<IProcessRunner>();

    // GetuUserDefaultShell()
    processRunner.Setup(
      pr => pr.Run(WORKING_DIR, "sh", It.Is<string[]>(
        value => value.Contains(EnvironmentVariableClient.USER_SHELL_COMMAND_LINUX) ||
                 value.Contains(EnvironmentVariableClient.USER_SHELL_COMMAND_MAC)
      ))
    ).Returns(Task.FromResult(new ProcessResult(0, shellName)));

    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(fc => fc.AppDataDirectory).Returns(WORKING_DIR);

    var computer = new Mock<IComputer>();
    computer.Setup(c => c.CreateShell(WORKING_DIR)).Returns(new Shell(processRunner.Object, WORKING_DIR));

    var envClient = new Mock<IEnvironmentClient>();

    var envVarClient =
      new EnvironmentVariableClient(systemInfo, processRunner.Object, fileClient.Object, computer.Object,
        envClient.Object);

    envVarClient.UserShell.ShouldBe(shellName);
  }

  [Fact]
  public void UserShellOnWindows() {
    const string WORKING_DIR = ".";

    var systemInfo = new MockSystemInfo(OSType.Windows, CPUArch.X64);
    var processRunner = new Mock<IProcessRunner>();

    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(fc => fc.AppDataDirectory).Returns(WORKING_DIR);

    var computer = new Mock<IComputer>();
    computer.Setup(c => c.CreateShell(WORKING_DIR)).Returns(new Shell(processRunner.Object, WORKING_DIR));

    var envClient = new Mock<IEnvironmentClient>();

    var envVarClient =
      new EnvironmentVariableClient(systemInfo, processRunner.Object, fileClient.Object, computer.Object,
        envClient.Object);

    envVarClient.UserShell.ShouldBe(string.Empty);
  }


  [Theory]
  [MemberData(nameof(GetSystemInfoForUnixOSes))]
  public void UserShellRcFilePathWhenValidShellOnUnix(ISystemInfo systemInfo) {
    const string WORKING_DIR = ".";
    const string shellName = "bash";
    string shellRcFilePath() => $"{WORKING_DIR}/.{shellName}rc";

    var processRunner = new Mock<IProcessRunner>();

    // GetuUserDefaultShell()
    processRunner.Setup(
      pr => pr.Run(WORKING_DIR, "sh", It.Is<string[]>(
        value => value.SequenceEqual(new[] { "-c", EnvironmentVariableClient.USER_SHELL_COMMAND_LINUX })
      ))
    ).Returns(Task.FromResult(new ProcessResult(0, shellName)));
    processRunner.Setup(
      pr => pr.Run(WORKING_DIR, "sh", It.Is<string[]>(
        value => value.SequenceEqual(new[] { "-c", EnvironmentVariableClient.USER_SHELL_COMMAND_MAC })
      ))
    ).Returns(Task.FromResult(new ProcessResult(0, shellName)));

    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(fc => fc.AppDataDirectory).Returns(WORKING_DIR);
    fileClient.Setup(fc => fc.UserDirectory).Returns(WORKING_DIR);
    fileClient.Setup(fc => fc.Combine(fileClient.Object.UserDirectory, It.Is<string>(s => s.EndsWith("rc"))))
      .Returns((string[] paths) => paths.Aggregate((a, b) => a + '/' + b));

    var computer = new Mock<IComputer>();
    computer.Setup(c => c.CreateShell(WORKING_DIR)).Returns(new Shell(processRunner.Object, WORKING_DIR));

    var envClient = new Mock<IEnvironmentClient>();

    var envVarClient =
      new EnvironmentVariableClient(systemInfo, processRunner.Object, fileClient.Object, computer.Object, envClient.Object);
    envVarClient.UserShellRcFilePath.ShouldBe(shellRcFilePath());
  }

  [Fact]
  public void UserShellRcFilePathWhenValidShellOnWindows() {
    const string WORKING_DIR = ".";

    var systemInfo = new MockSystemInfo(OSType.Windows, CPUArch.X64);
    var processRunner = new Mock<IProcessRunner>();

    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(fc => fc.AppDataDirectory).Returns(WORKING_DIR);
    fileClient.Setup(fc => fc.UserDirectory).Returns(WORKING_DIR);
    fileClient.Setup(fc => fc.Combine(fileClient.Object.UserDirectory, It.Is<string>(s => s.EndsWith("rc"))))
      .Returns((string[] paths) => paths.Aggregate((a, b) => a + '/' + b));

    var computer = new Mock<IComputer>();
    computer.Setup(c => c.CreateShell(WORKING_DIR)).Returns(new Shell(processRunner.Object, WORKING_DIR));

    var envClient = new Mock<IEnvironmentClient>();

    var envVarClient =
      new EnvironmentVariableClient(systemInfo, processRunner.Object, fileClient.Object, computer.Object,
        envClient.Object);

    envVarClient.UserShellRcFilePath.ShouldBe(string.Empty);
  }

  [Fact]
  public void UserShellRcFilePathWhenInValidShellOnLinux() {
    const string WORKING_DIR = ".";
    const string shellName = "fish";
    string shellRcFilePath(string sl) => $"{WORKING_DIR}/.{sl}rc";

    var systemInfo = new MockSystemInfo(OSType.Linux, CPUArch.X64);
    var processRunner = new Mock<IProcessRunner>();

    // GetuUserDefaultShell()
    processRunner.Setup(
      pr => pr.Run(WORKING_DIR, "sh", It.Is<string[]>(
        value => value.SequenceEqual(new[] { "-c", EnvironmentVariableClient.USER_SHELL_COMMAND_LINUX })
      ))
    ).Returns(Task.FromResult(new ProcessResult(0, shellName)));

    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(fc => fc.AppDataDirectory).Returns(WORKING_DIR);
    fileClient.Setup(fc => fc.UserDirectory).Returns(WORKING_DIR);
    fileClient.Setup(fc => fc.Combine(fileClient.Object.UserDirectory, It.Is<string>(s => s.EndsWith("rc"))))
      .Returns((string[] paths) => paths.Aggregate((a, b) => a + '/' + b));

    var computer = new Mock<IComputer>();
    computer.Setup(c => c.CreateShell(WORKING_DIR)).Returns(new Shell(processRunner.Object, WORKING_DIR));

    var envClient = new Mock<IEnvironmentClient>();

    var envVarClient =
      new EnvironmentVariableClient(systemInfo, processRunner.Object, fileClient.Object, computer.Object, envClient.Object);
    // Linux
    envVarClient.UserShell.ShouldBe("bash");
    envVarClient.UserShellRcFilePath.ShouldBe(shellRcFilePath("bash"));
  }

  [Fact]
  public void UserShellRcFilePathWhenInValidShellOnMac() {
    const string WORKING_DIR = ".";
    const string shellName = "fish";
    string shellRcFilePath(string sl) => $"{WORKING_DIR}/.{sl}rc";

    var systemInfo = new MockSystemInfo(OSType.MacOS, CPUArch.Arm64);
    var processRunner = new Mock<IProcessRunner>();

    // GetuUserDefaultShell()
    processRunner.Setup(
      pr => pr.Run(WORKING_DIR, "sh", It.Is<string[]>(
        value => value.SequenceEqual(new[] { "-c", EnvironmentVariableClient.USER_SHELL_COMMAND_MAC })
      ))
    ).Returns(Task.FromResult(new ProcessResult(0, shellName)));

    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(fc => fc.AppDataDirectory).Returns(WORKING_DIR);
    fileClient.Setup(fc => fc.UserDirectory).Returns(WORKING_DIR);
    fileClient.Setup(fc => fc.Combine(fileClient.Object.UserDirectory, It.Is<string>(s => s.EndsWith("rc"))))
      .Returns((string[] paths) => paths.Aggregate((a, b) => a + '/' + b));

    var computer = new Mock<IComputer>();
    computer.Setup(c => c.CreateShell(WORKING_DIR)).Returns(new Shell(processRunner.Object, WORKING_DIR));

    var envClient = new Mock<IEnvironmentClient>();

    var envVarClient =
      new EnvironmentVariableClient(systemInfo, processRunner.Object, fileClient.Object, computer.Object, envClient.Object);
    envVarClient.UserShell.ShouldBe("zsh");
    envVarClient.UserShellRcFilePath.ShouldBe(shellRcFilePath("zsh"));
  }

  [Fact]
  public void UserShellRcFilePathWhenInValidShellOnWindows() {
    const string WORKING_DIR = ".";

    var systemInfo = new MockSystemInfo(OSType.Windows, CPUArch.X64);
    var processRunner = new Mock<IProcessRunner>();

    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(fc => fc.AppDataDirectory).Returns(WORKING_DIR);
    fileClient.Setup(fc => fc.UserDirectory).Returns(WORKING_DIR);
    fileClient.Setup(fc => fc.Combine(fileClient.Object.UserDirectory, It.Is<string>(s => s.EndsWith("rc"))))
      .Returns((string[] paths) => paths.Aggregate((a, b) => a + '/' + b));

    var computer = new Mock<IComputer>();
    computer.Setup(c => c.CreateShell(WORKING_DIR)).Returns(new Shell(processRunner.Object, WORKING_DIR));

    var envClient = new Mock<IEnvironmentClient>();

    var envVarClient =
      new EnvironmentVariableClient(systemInfo, processRunner.Object, fileClient.Object, computer.Object, envClient.Object);
    envVarClient.UserShell.ShouldBe(string.Empty);
    envVarClient.UserShellRcFilePath.ShouldBe(string.Empty);
  }
}
