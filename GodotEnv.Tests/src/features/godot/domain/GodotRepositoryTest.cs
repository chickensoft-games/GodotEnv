namespace Chickensoft.GodotEnv.Tests.Features.Godot.Domain;

using System.Threading.Tasks;
using Chickensoft.GodotEnv.Features.Godot.Domain;
using Chickensoft.GodotEnv.Features.Godot.Models;
using CliFx.Infrastructure;
using Common.Clients;
using Common.Models;
using Common.Utilities;
using Downloader;
using global::GodotEnv.Common.Utilities;
using Moq;
using Xunit;

public class GodotRepositoryTest {
  [Fact]
  public async Task AddOrUpdateGodotEnvVariable() {
    var workingDir = ".";

    var systemInfo = new MockSystemInfo(OSType.Linux, CPUArch.X64);
    var computer = new Mock<IComputer>();
    var processRunner = new Mock<IProcessRunner>();
    var fileClient = new Mock<IFileClient>();

    fileClient.Setup(fc => fc.AppDataDirectory).Returns(workingDir);

    // GodotBinPath
    fileClient.Setup(fc => fc.Combine(fileClient.Object.AppDataDirectory, Defaults.GODOT_PATH, Defaults.GODOT_BIN_NAME))
      .Returns("/godot/bin/");

    // GodotSymlinkPath
    fileClient.Setup(fc => fc.Combine(fileClient.Object.AppDataDirectory, Defaults.GODOT_PATH, Defaults.GODOT_BIN_PATH,
      Defaults.GODOT_BIN_NAME)).Returns("/godot/bin/godot");

    var networkClient = new Mock<NetworkClient>(new Mock<DownloadService>().Object, Defaults.DownloadConfiguration);
    var zipClient = new Mock<ZipClient>(fileClient.Object.Files);

    var environmentVariableClient = new Mock<IEnvironmentVariableClient>();
    environmentVariableClient.Setup(evc => evc.UpdateGodotEnvEnvironment(It.IsAny<string>(), It.IsAny<string>()))
      .Returns(Task.CompletedTask);

    var platform = new Mock<GodotEnvironment>(systemInfo, fileClient.Object, computer.Object);
    var checksumClient = new Mock<IGodotChecksumClient>();

    var godotRepo = new GodotRepository(
      systemInfo: systemInfo,
      config: new ConfigFile { GodotInstallationsPath = "INSTALLATION_PATH" },
      fileClient: fileClient.Object,
      networkClient: networkClient.Object,
      zipClient: zipClient.Object,
      platform: platform.Object,
      environmentVariableClient: environmentVariableClient.Object,
      processRunner: processRunner.Object,
      checksumClient: checksumClient.Object
    );

    var executionContext = new Mock<IExecutionContext>();
    var console = new FakeInMemoryConsole();
    var log = new Mock<ILog>(); // Use real log to test colors in output

    executionContext.Setup(context => context.CreateLog(console)).Returns(log.Object);

    await godotRepo.AddOrUpdateGodotEnvVariable(log.Object);
  }
}
