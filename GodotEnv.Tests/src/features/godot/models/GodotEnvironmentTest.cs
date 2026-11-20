namespace Chickensoft.GodotEnv.Tests.Features.Godot.Models;

using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Features.Godot.Models;
using Chickensoft.GodotEnv.Features.Godot.Serializers;
using Common.Clients;
using Common.Utilities;
using Moq;
using Shouldly;
using Xunit;

public class GodotEnvironmentTest
{
  private readonly Mock<IComputer> _computer = new();
  private readonly Mock<IFileClient> _fileClient = new();
  private readonly ReleaseVersionDeserializer _versionDeserializer = new();
  private readonly ReleaseVersionSerializer _versionSerializer = new();
  private readonly SpecificDotnetStatusGodotVersion _version4Dotnet = new(4, 1, 2, "stable", -1, true);
  private readonly SpecificDotnetStatusGodotVersion _version4NonDotnet = new(4, 1, 2, "stable", -1, false);
  private readonly SpecificDotnetStatusGodotVersion _version3Dotnet = new(3, 5, 3, "stable", -1, true);
  private readonly SpecificDotnetStatusGodotVersion _version3NonDotnet = new(3, 5, 3, "stable", -1, false);
  private readonly SpecificDotnetStatusGodotVersion _firstKnownWinArmVersionDotnet = new(4, 3, 0, "stable", -1, true);
  private readonly SpecificDotnetStatusGodotVersion _firstKnownWinArmVersionNonDotnet = new(4, 3, 0, "stable", -1, false);

  [Fact]
  public void GetsExpectedMacDownloadUrl()
  {
    var systemInfo = new MockSystemInfo(OSType.MacOS, CpuArch.Arm64);
    var platform = new MacOS(systemInfo, _fileClient.Object, _computer.Object, _versionDeserializer, _versionSerializer);

    var downloadUrl = platform.GetDownloadUrl(_version4NonDotnet, false);
    downloadUrl.ShouldBe(GetExpectedDownloadUrl(_version4NonDotnet, "_macos.universal"));

    downloadUrl = platform.GetDownloadUrl(_version3NonDotnet, false);
    downloadUrl.ShouldBe(GetExpectedDownloadUrl(_version3NonDotnet, "_osx.universal"));
  }

  [Fact]
  public void GetsExpectedMacMonoDownloadUrl()
  {
    var systemInfo = new MockSystemInfo(OSType.MacOS, CpuArch.Arm64);
    var platform = new MacOS(systemInfo, _fileClient.Object, _computer.Object, _versionDeserializer, _versionSerializer);

    var downloadUrl = platform.GetDownloadUrl(_version4Dotnet, false);
    downloadUrl.ShouldBe(GetExpectedDownloadUrl(_version4Dotnet, "_mono_macos.universal"));

    downloadUrl = platform.GetDownloadUrl(_version3Dotnet, false);
    downloadUrl.ShouldBe(GetExpectedDownloadUrl(_version3Dotnet, "_mono_osx.universal"));
  }

  [Theory]
  [InlineData(CpuArch.Arm64, false, "_macos.arm64")]
  [InlineData(CpuArch.Arm64, true, "_mono_macos.arm64")]
  [InlineData(CpuArch.X64, false, "_macos.x86_64")]
  [InlineData(CpuArch.X64, true, "_mono_macos.x86_64")]
  public void GetsExpectedMacSingleArchDownloadUrl(CpuArch arch, bool isDotnet, string suffix)
  {
    var systemInfo = new MockSystemInfo(OSType.MacOS, arch);
    var platform = new MacOS(systemInfo, _fileClient.Object, _computer.Object, _versionDeserializer, _versionSerializer);

    var beta = new SpecificDotnetStatusGodotVersion(4, 8, 0, "beta", 1, isDotnet);
    platform.GetDownloadUrl(beta, false).ShouldBe(GetExpectedDownloadUrl(beta, suffix));

    var stable = new SpecificDotnetStatusGodotVersion(4, 8, 0, "stable", -1, isDotnet);
    platform.GetDownloadUrl(stable, false).ShouldBe(GetExpectedDownloadUrl(stable, suffix));

    var later = new SpecificDotnetStatusGodotVersion(5, 0, 0, "dev", 1, isDotnet);
    platform.GetDownloadUrl(later, false).ShouldBe(GetExpectedDownloadUrl(later, suffix));
  }

  [Theory]
  [InlineData(CpuArch.Arm64)]
  [InlineData(CpuArch.X64)]
  public void GetsExpectedMacUniversalDownloadUrlBeforeSingleArchBuilds(CpuArch arch)
  {
    var systemInfo = new MockSystemInfo(OSType.MacOS, arch);
    var platform = new MacOS(systemInfo, _fileClient.Object, _computer.Object, _versionDeserializer, _versionSerializer);

    var dev = new SpecificDotnetStatusGodotVersion(4, 8, 0, "dev", 7, false);
    platform.GetDownloadUrl(dev, false).ShouldBe(GetExpectedDownloadUrl(dev, "_macos.universal"));

    var previous = new SpecificDotnetStatusGodotVersion(4, 7, 2, "stable", -1, true);
    platform.GetDownloadUrl(previous, false).ShouldBe(GetExpectedDownloadUrl(previous, "_mono_macos.universal"));
  }

  [Fact]
  public void GetsExpectedWindowsDownloadUrl()
  {
    var systemInfo = new MockSystemInfo(OSType.Windows, CpuArch.X64);
    var platform = new Windows(systemInfo, _fileClient.Object, _computer.Object, _versionDeserializer, _versionSerializer);

    var downloadUrl = platform.GetDownloadUrl(_version4NonDotnet, false);
    downloadUrl.ShouldBe($"{GodotEnvironment.GODOT_URL_PREFIX}4.1.2-stable/Godot_v4.1.2-stable_win64.exe.zip");

    downloadUrl = platform.GetDownloadUrl(_version3NonDotnet, false);
    downloadUrl.ShouldBe($"{GodotEnvironment.GODOT_URL_PREFIX}{GodotVersionString(_version3NonDotnet)}/Godot_v{GodotVersionString(_version3NonDotnet)}_win64.exe.zip");
  }

  [Fact]
  public void GetsExpectedWindowsMonoDownloadUrl()
  {
    var systemInfo = new MockSystemInfo(OSType.Windows, CpuArch.X64);
    var platform = new Windows(systemInfo, _fileClient.Object, _computer.Object, _versionDeserializer, _versionSerializer);

    var downloadUrl = platform.GetDownloadUrl(_version4Dotnet, false);
    downloadUrl.ShouldBe(GetExpectedDownloadUrl(_version4Dotnet, "_mono_win64"));

    downloadUrl = platform.GetDownloadUrl(_version3Dotnet, false);
    downloadUrl.ShouldBe(GetExpectedDownloadUrl(_version3Dotnet, "_mono_win64"));
  }

  [Fact]
  public void GetsExpectedWindowsArmDownloadUrlForOlderVersions()
  {
    var systemInfo = new MockSystemInfo(OSType.Windows, CpuArch.Arm64);
    var platform = new Windows(systemInfo, _fileClient.Object, _computer.Object, _versionDeserializer, _versionSerializer);

    var downloadUrl = platform.GetDownloadUrl(_version4NonDotnet, false);
    downloadUrl.ShouldBe($"{GodotEnvironment.GODOT_URL_PREFIX}4.1.2-stable/Godot_v4.1.2-stable_win64.exe.zip");

    downloadUrl = platform.GetDownloadUrl(_version3NonDotnet, false);
    downloadUrl.ShouldBe($"{GodotEnvironment.GODOT_URL_PREFIX}{GodotVersionString(_version3NonDotnet)}/Godot_v{GodotVersionString(_version3NonDotnet)}_win64.exe.zip");
  }

  [Fact]
  public void GetsExpectedWindowsArmMonoDownloadUrlForOlderVersions()
  {
    var systemInfo = new MockSystemInfo(OSType.Windows, CpuArch.Arm64);
    var platform = new Windows(systemInfo, _fileClient.Object, _computer.Object, _versionDeserializer, _versionSerializer);

    var downloadUrl = platform.GetDownloadUrl(_version4Dotnet, false);
    downloadUrl.ShouldBe(GetExpectedDownloadUrl(_version4Dotnet, "_mono_win64"));

    downloadUrl = platform.GetDownloadUrl(_version3Dotnet, false);
    downloadUrl.ShouldBe(GetExpectedDownloadUrl(_version3Dotnet, "_mono_win64"));
  }

  [Fact]
  public void GetExpectedWindowsArmDownloadUrlForNewerVersions()
  {
    var systemInfo = new MockSystemInfo(OSType.Windows, CpuArch.Arm64);
    var platform = new Windows(systemInfo, _fileClient.Object, _computer.Object, _versionDeserializer, _versionSerializer);

    var downloadUrl = platform.GetDownloadUrl(_firstKnownWinArmVersionNonDotnet, false);
    downloadUrl.ShouldBe($"{GodotEnvironment.GODOT_URL_PREFIX}4.3-stable/Godot_v4.3-stable_windows_arm64.exe.zip");
  }

  [Fact]
  public void GetExpectedWindowsArmMonoDownloadUrlForNewerVersions()
  {
    var systemInfo = new MockSystemInfo(OSType.Windows, CpuArch.Arm64);
    var platform = new Windows(systemInfo, _fileClient.Object, _computer.Object, _versionDeserializer, _versionSerializer);

    var downloadUrl = platform.GetDownloadUrl(_firstKnownWinArmVersionDotnet, false);
    downloadUrl.ShouldBe($"{GodotEnvironment.GODOT_URL_PREFIX}4.3-stable/Godot_v4.3-stable_mono_windows_arm64.zip");
  }

  [Fact]
  public void GetsExpectedLinuxDownloadUrl()
  {
    var systemInfo = new MockSystemInfo(OSType.Linux, CpuArch.X64);
    var platform = new Linux(systemInfo, _fileClient.Object, _computer.Object, _versionDeserializer, _versionSerializer);

    var downloadUrl = platform.GetDownloadUrl(_version4NonDotnet, false);
    downloadUrl.ShouldBe(GetExpectedDownloadUrl(_version4NonDotnet, "_linux.x86_64"));

    downloadUrl = platform.GetDownloadUrl(_version3NonDotnet, false);
    downloadUrl.ShouldBe(GetExpectedDownloadUrl(_version3NonDotnet, "_x11.64"));
  }

  [Fact]
  public void GetsExpectedLinuxMonoDownloadUrl()
  {
    var systemInfo = new MockSystemInfo(OSType.Linux, CpuArch.X64);
    var platform = new Linux(systemInfo, _fileClient.Object, _computer.Object, _versionDeserializer, _versionSerializer);

    var downloadUrl = platform.GetDownloadUrl(_version4Dotnet, false);
    downloadUrl.ShouldBe(GetExpectedDownloadUrl(_version4Dotnet, "_mono_linux_x86_64"));

    downloadUrl = platform.GetDownloadUrl(_version3Dotnet, false);
    downloadUrl.ShouldBe(GetExpectedDownloadUrl(_version3Dotnet, "_mono_x11_64"));
  }

  [Fact]
  public void GetsExpectedTemplatesDownloadUrl()
  {
    var systemInfo = new MockSystemInfo(OSType.MacOS, CpuArch.Arm64);
    var platform = new MacOS(systemInfo, _fileClient.Object, _computer.Object, _versionDeserializer, _versionSerializer);
    var downloadUrl = platform.GetDownloadUrl(_version4NonDotnet, true);

    downloadUrl.ShouldBe($"{GodotEnvironment.GODOT_URL_PREFIX}4.1.2-stable/Godot_v4.1.2-stable_export_templates.tpz");
  }

  [Fact]
  public void GetsExpectedTemplatesMonoDownloadUrl()
  {
    var systemInfo = new MockSystemInfo(OSType.MacOS, CpuArch.Arm64);
    var platform = new MacOS(systemInfo, _fileClient.Object, _computer.Object, _versionDeserializer, _versionSerializer);
    var downloadUrl = platform.GetDownloadUrl(_version4Dotnet, true);

    downloadUrl.ShouldBe($"{GodotEnvironment.GODOT_URL_PREFIX}4.1.2-stable/Godot_v4.1.2-stable_mono_export_templates.tpz");
  }

  private string GetExpectedDownloadUrl(GodotVersion version, string platformSuffix) =>
    $"{GodotEnvironment.GODOT_URL_PREFIX}{GodotVersionString(version)}/Godot_v{GodotVersionString(version)}{platformSuffix}.zip";

  private string GodotVersionString(GodotVersion version)
    => _versionSerializer.Serialize(version);
}
