namespace Chickensoft.GodotEnv.Tests.Features.Godot.Domain;

using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Common.Utilities;
using Chickensoft.GodotEnv.Features.Godot.Domain;
using Chickensoft.GodotEnv.Features.Godot.Models;
using CliFx.Infrastructure;
using Downloader;
using Moq;
using Xunit;

public class GodotRepositoryTest {
  [Fact]
  public async Task AddOrUpdateGodotEnvVariable() {
    var WORKING_DIR = ".";
    var godotVar = "GODOT";

    var computer = new Mock<IComputer>();
    var processRunner = new Mock<IProcessRunner>();
    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(fc => fc.OS).Returns(FileClient.IsOSPlatform(OSPlatform.OSX)
      ? OSType.MacOS
      : FileClient.IsOSPlatform(OSPlatform.Linux)
        ? OSType.Linux
        : FileClient.IsOSPlatform(OSPlatform.Windows)
          ? OSType.Windows
          : OSType.Unknown);

    fileClient.Setup(fc => fc.AppDataDirectory).Returns(WORKING_DIR);

    // GodotBinPath
    fileClient.Setup(fc => fc.Combine(fileClient.Object.AppDataDirectory, Defaults.GODOT_PATH, Defaults.GODOT_BIN_NAME))
      .Returns("/godot/bin/");

    // GodotSymlinkPath
    fileClient.Setup(fc => fc.Combine(fileClient.Object.AppDataDirectory, Defaults.GODOT_PATH, Defaults.GODOT_BIN_PATH,
      Defaults.GODOT_BIN_NAME)).Returns("/godot/bin/godot");

    var networkClient = new Mock<NetworkClient>(new Mock<DownloadService>().Object, Defaults.DownloadConfiguration);
    var zipClient = new Mock<ZipClient>(fileClient.Object.Files);

    var environmentVariableClient = new Mock<IEnvironmentVariableClient>();
    environmentVariableClient.Setup(evc => evc.AppendToUserEnv(It.IsAny<string>(), It.IsAny<string>()))
      .Returns(Task.CompletedTask);

    var platform = new Mock<GodotEnvironment>(fileClient.Object, computer.Object);

    var godotRepo = new GodotRepository(
      config: new ConfigFile { GodotInstallationsPath = "INSTALLATION_PATH" },
      fileClient: fileClient.Object,
      networkClient: networkClient.Object,
      zipClient: zipClient.Object,
      platform: platform.Object,
      environmentVariableClient: environmentVariableClient.Object,
      processRunner: processRunner.Object
    );

    var executionContext = new Mock<IExecutionContext>();
    var console = new FakeInMemoryConsole();
    var log = new Mock<ILog>(); // Use real log to test colors in output

    executionContext.Setup(context => context.CreateLog(console)).Returns(log.Object);

    await godotRepo.AddOrUpdateGodotEnvVariable(log.Object);

    environmentVariableClient.Verify(mock => mock.SetUserEnv(godotVar, godotRepo.GodotSymlinkPath));
    environmentVariableClient.Verify(mock => mock.AppendToUserEnv(Defaults.PATH_ENV_VAR_NAME, godotRepo.GodotBinPath));
  }
}
