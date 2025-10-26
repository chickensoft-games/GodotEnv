namespace Chickensoft.GodotEnv.Tests;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Common.Clients;
using Common.Models;
using Common.Utilities;
using Moq;
using Shouldly;
using Xunit;

public class EnvironmentVariableClientTest
{
  private const string USER_DIR = "$HOME";
  private const string WORKING_DIR = $"{USER_DIR}/.config/godotenv";

  public static IEnumerable<object[]> GetSystemInfoForUnixOSes()
  {
    yield return [
      new MockSystemInfo(OSType.Linux, CPUArch.X64)
    ];
    yield return [
      new MockSystemInfo(OSType.MacOS, CPUArch.Arm64)
    ];
  }

  public static IEnumerable<object[]> GetSystemInfoForAllOSes()
  {
    var oSes = GetSystemInfoForUnixOSes();

    oSes = oSes.Append([
      new MockSystemInfo(OSType.Windows, CPUArch.X64)
    ]);

    foreach (var os in oSes)
    {
      yield return os;
    }
  }

  [Theory]
  [MemberData(nameof(GetSystemInfoForAllOSes))]
  public async Task GetUserEnv(ISystemInfo systemInfo)
  {
    var envFilePath = $"{WORKING_DIR}/env";
    var envValue = $"{WORKING_DIR}/godot/bin/godot";

    var processRunner = new Mock<IProcessRunner>();

    processRunner.Setup(
      pr => pr.Run(WORKING_DIR, "sh", It.Is<string[]>(
        value => value.Any(str => str.Contains($"echo ${Defaults.GODOT_ENV_VAR_NAME}"))
      ))).Returns(Task.FromResult(new ProcessResult(0, envValue)));

    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(fc => fc.AppDataDirectory).Returns(WORKING_DIR);
    fileClient.Setup(fc => fc.Combine(It.IsAny<string[]>())).Returns((string[] paths) => Path.Combine(paths).Replace('\\', '/'));
    fileClient.Setup(fc => fc.FileExists(It.IsAny<string>())).Returns(true);

    var computer = new Mock<IComputer>();
    computer.Setup(c => c.CreateShell(WORKING_DIR)).Returns(new Shell(processRunner.Object, WORKING_DIR));

    var envVarClient = new EnvironmentVariableClient(
      systemInfo,
      processRunner.Object,
      fileClient.Object,
      computer.Object
    )
    {
      GetEnvironmentVariableOnWindowsProxy = (name, _) =>
      {
        if (name == Defaults.GODOT_ENV_VAR_NAME)
        {
          return envValue;
        }

        return null;
      }
    };

    var userEnv = await envVarClient.GetUserEnv(Defaults.GODOT_ENV_VAR_NAME);
    userEnv.ShouldBe(envValue);
  }

  [Fact]
  public async Task UpdateGodotEnvEnvironmentOnWindows()
  {
    var binPath = $"{WORKING_DIR}/godot/bin";
    var symlinkPath = $"{binPath}/godot";

    var systemInfo = new MockSystemInfo(OSType.Windows, CPUArch.X64);
    var processRunner = new Mock<IProcessRunner>();

    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(fc => fc.AppDataDirectory).Returns(WORKING_DIR);

    var computer = new Mock<IComputer>();
    computer.Setup(c => c.CreateShell(WORKING_DIR)).Returns(new Shell(processRunner.Object, WORKING_DIR));

    var envVarClient = new EnvironmentVariableClient(systemInfo, processRunner.Object, fileClient.Object,
      computer.Object);

    var envs = new Dictionary<string, string>
    {
      [Defaults.PATH_ENV_VAR_NAME] = @"C:\Program Files\PowerShell\7;C:\Windows\System32\WindowsPowerShell\v1.0\",
    };
    envVarClient.GetEnvironmentVariableOnWindowsProxy = (name, _) => envs[name];

    envVarClient.SetEnvironmentVariableOnWindowsProxy = (name, val, _)
      => envs[name] = val;

    await envVarClient.UpdateGodotEnvEnvironment(symlinkPath, binPath);

    // Assert PATH update
    envs[Defaults.PATH_ENV_VAR_NAME].ShouldStartWith($"{binPath};");
    // Assert GODOT env-var creation
    envs[Defaults.GODOT_ENV_VAR_NAME].ShouldBe(symlinkPath);

    // Test idempotency of re-runs.
    var envsCopy = new Dictionary<string, string>();
    foreach (var (k, v) in envs)
    {
      envsCopy[k] = v;
    }

    // Call again.
    await envVarClient.UpdateGodotEnvEnvironment(symlinkPath, binPath);
    // Check if "ambient" continues the same.
    envs.ShouldBe(envsCopy);
  }

  [Theory]
  [MemberData(nameof(GetSystemInfoForUnixOSes))]
  public async Task UpdateGodotEnvEnvironmentOnUnix(ISystemInfo systemInfo)
  {
    var binPath = $"{WORKING_DIR}/godot/bin";
    var symlinkPath = $"{binPath}/godot";
    var envFilePath = $"{WORKING_DIR}/env";
    var envFileContent = """
      #!/bin/sh
      # godotenv shell setup (Updates PATH, and defines GODOT environment variable)

      # affix colons on either side of $PATH to simplify matching
      case ":${PATH}:" in
          *:"$HOME/.config/godotenv/godot/bin":*)
              ;;
          *)
              # Prepending path making it the highest in priority.
              export PATH="$HOME/.config/godotenv/godot/bin:$PATH"
              ;;
      esac

      if [ -z "${GODOT:-}" ]; then  # If variable not defined or empty.
          export GODOT="$HOME/.config/godotenv/godot/bin/godot"
      fi

      """;
    var shellFilesToUpdate = new[] {
      $"$HOME/.profile",
      $"$HOME/.bashrc",
      $"$HOME/.zshenv",
    };
    const string cmd = $". \"{WORKING_DIR}/env\" # Added by GodotEnv\n";

    var processRunner = new Mock<IProcessRunner>();

    var computer = new Mock<IComputer>();

    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(fc => fc.AppDataDirectory).Returns(WORKING_DIR);
    fileClient.Setup(fc => fc.UserDirectory).Returns(USER_DIR);
    fileClient.Setup(fc => fc.Combine(It.IsAny<string[]>())).Returns((string[] paths) => Path.Combine(paths).Replace('\\', '/'));
    fileClient.Setup(fc => fc.FileExists(It.IsAny<string>())).Returns(true);

    var envVarClient = new EnvironmentVariableClient(systemInfo, processRunner.Object, fileClient.Object,
      computer.Object);

    // Act
    await envVarClient.UpdateGodotEnvEnvironment(symlinkPath, binPath);

    // Assert shell initialization files patch.
    foreach (var file in shellFilesToUpdate)
    {
      fileClient.Verify(fc => fc.AddLinesToFileIfNotPresent(file, cmd), Times.Once);
    }
    // Assert env file creation.
    fileClient.Verify(fc => fc.CreateFile(envFilePath, envFileContent));
  }
}
