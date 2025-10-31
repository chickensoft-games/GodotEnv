namespace Chickensoft.GodotEnv.Tests.Features.Godot.Domain;

using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Chickensoft.GodotEnv.Features.Godot.Domain;
using Chickensoft.GodotEnv.Features.Godot.Models;
using Chickensoft.GodotEnv.Features.Godot.Serializers;
using CliFx.Infrastructure;
using Common.Clients;
using Common.Models;
using Common.Utilities;
using Downloader;
using Moq;
using Shouldly;
using Xunit;

public class GodotRepositoryTest
{
  [Fact]
  public async Task AddOrUpdateGodotEnvVariable()
  {
    var workingDir = ".";

    var systemInfo = new MockSystemInfo(OSType.Linux, CpuArch.X64);
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

    var networkClient = new Mock<NetworkClient>(new Mock<IDownloadService>().Object, Defaults.DownloadConfiguration);
    var zipClient = new Mock<ZipClient>(fileClient.Object.Files);

    var environmentVariableClient = new Mock<IEnvironmentVariableClient>();
    environmentVariableClient.Setup(evc => evc.UpdateGodotEnvEnvironment(It.IsAny<string>(), It.IsAny<string>()))
      .Returns(Task.CompletedTask);

    var platformVersionDeserializer = new Mock<IVersionDeserializer>();
    var platformVersionSerializer = new Mock<IVersionSerializer>();
    var platform = new Mock<GodotEnvironment>(
      systemInfo,
      fileClient.Object,
      computer.Object,
      platformVersionDeserializer.Object,
      platformVersionSerializer.Object
    );
    var checksumClient = new Mock<IGodotChecksumClient>();

    var versionDeserializer = new Mock<IVersionDeserializer>();
    var versionSerializer = new Mock<IVersionSerializer>();

    var godotRepo = new GodotRepository(
      systemInfo: systemInfo,
      config: new Config(
        new ConfigValues
        {
          Godot = new GodotConfigSection
          {
            InstallationsPath = "INSTALLATION_PATH"
          },
        }
      ),
      fileClient: fileClient.Object,
      networkClient: networkClient.Object,
      zipClient: zipClient.Object,
      platform: platform.Object,
      environmentVariableClient: environmentVariableClient.Object,
      processRunner: processRunner.Object,
      checksumClient: checksumClient.Object,
      versionDeserializer: versionDeserializer.Object,
      versionSerializer: versionSerializer.Object
    );

    var executionContext = new Mock<IExecutionContext>();
    var console = new FakeInMemoryConsole();
    var log = new Mock<ILog>(); // Use real log to test colors in output

    executionContext.Setup(context => context.CreateLog(console)).Returns(log.Object);

    await godotRepo.AddOrUpdateGodotEnvVariable(log.Object);
  }

  [Theory]
  [InlineData("4.0-rc1")]
  [InlineData("4.0.1-stable")]
  [InlineData("3.5.4-dev6")]
  [InlineData("4.5.0-rc.1")]
  public void DirectoryToVersionUndoesVersionFsName(string godotVersionString)
  {
    var systemInfo = new MockSystemInfo(OSType.Linux, CpuArch.X64);
    var computer = new Mock<IComputer>();
    var processRunner = new Mock<IProcessRunner>();

    var fs = new FileSystem();
    var fileClient = new Mock<IFileClient>();
    fileClient.Setup(fc => fc.Sanitize(It.IsAny<string>()))
      .Returns((string path) =>
                fs.Path.GetInvalidFileNameChars()
                  .Union(fs.Path.GetInvalidPathChars())
                  .Aggregate(path, (current, c) => current.Replace(c, '_')).Trim('_')
    );

    var networkClient = new Mock<NetworkClient>(new Mock<IDownloadService>().Object, Defaults.DownloadConfiguration);
    var zipClient = new Mock<ZipClient>(fileClient.Object.Files);
    var environmentVariableClient = new Mock<IEnvironmentVariableClient>();

    var fileVersionDeserializer = new ReleaseVersionDeserializer();
    var fileVersionSerializer = new ReleaseVersionSerializer();
    var platform = new Mock<GodotEnvironment>(
      systemInfo, fileClient.Object, computer.Object, fileVersionDeserializer, fileVersionSerializer
    );

    var checksumClient = new Mock<IGodotChecksumClient>();
    var versionDeserializer = new Mock<IVersionDeserializer>();
    var versionSerializer = new Mock<IVersionSerializer>();

    var godotRepo = new GodotRepository(
      systemInfo: systemInfo,
      config: new Config(
        new ConfigValues
        {
          Godot = new GodotConfigSection
          {
            InstallationsPath = "INSTALLATION_PATH"
          },
        }
      ),
      fileClient: fileClient.Object,
      networkClient: networkClient.Object,
      zipClient: zipClient.Object,
      platform: platform.Object,
      environmentVariableClient: environmentVariableClient.Object,
      processRunner: processRunner.Object,
      checksumClient: checksumClient.Object,
      versionDeserializer: versionDeserializer.Object,
      versionSerializer: versionSerializer.Object
    );

    var dotnetVersion = fileVersionDeserializer.Deserialize(godotVersionString, true);
    dotnetVersion.IsSuccess.ShouldBeTrue();
    var reconstructedDotnetVersion = godotRepo.DirectoryToVersion(
      godotRepo.GetVersionFsName(fileVersionSerializer, dotnetVersion.Value)
    );
    reconstructedDotnetVersion.ShouldBe(dotnetVersion.Value);
    var nonDotnetVersion = fileVersionDeserializer.Deserialize(godotVersionString, false);
    nonDotnetVersion.IsSuccess.ShouldBeTrue();
    var reconstructedNonDotnetVersion = godotRepo.DirectoryToVersion(
      godotRepo.GetVersionFsName(fileVersionSerializer, nonDotnetVersion.Value)
    );
    reconstructedNonDotnetVersion.ShouldBe(nonDotnetVersion.Value);
  }
}
