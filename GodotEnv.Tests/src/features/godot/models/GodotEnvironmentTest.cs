namespace Chickensoft.GodotEnv.Tests.Features.Godot.Models;

using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Features.Godot.Models;
using Common.Clients;
using Common.Utilities;
using global::GodotEnv.Common.Utilities;
using Moq;
using Shouldly;
using Xunit;

public class GodotEnvironmentTest {
  private readonly Mock<IComputer> _computer = new();
  private readonly Mock<IFileClient> _fileClient = new();
  private readonly GodotVersion _version4 = new("4", "1", "2", "stable", "");
  private readonly GodotVersion _version3 = new("3", "5", "3", "stable", "");

  [Fact]
  public void GetsExpectedMacDownloadUrl() {
    var systemInfo = new MockSystemInfo(OSType.MacOS, CPUArch.Arm64);
    var platform = new MacOS(systemInfo, _fileClient.Object, _computer.Object);

    var downloadUrl = platform.GetDownloadUrl(_version4, false, false);
    downloadUrl.ShouldBe(GetExpectedDownloadUrl(_version4, "_macos.universal"));

    downloadUrl = platform.GetDownloadUrl(_version3, false, false);
    downloadUrl.ShouldBe(GetExpectedDownloadUrl(_version3, "_osx.universal"));
  }

  [Fact]
  public void GetsExpectedMacMonoDownloadUrl() {
    var systemInfo = new MockSystemInfo(OSType.MacOS, CPUArch.Arm64);
    var platform = new MacOS(systemInfo, _fileClient.Object, _computer.Object);

    var downloadUrl = platform.GetDownloadUrl(_version4, true, false);
    downloadUrl.ShouldBe(GetExpectedDownloadUrl(_version4, "_mono_macos.universal"));

    downloadUrl = platform.GetDownloadUrl(_version3, true, false);
    downloadUrl.ShouldBe(GetExpectedDownloadUrl(_version3, "_mono_osx.universal"));
  }

  [Fact]
  public void GetsExpectedWindowsDownloadUrl() {
    var systemInfo = new MockSystemInfo(OSType.Windows, CPUArch.X64);
    var platform = new Windows(systemInfo, _fileClient.Object, _computer.Object);

    var downloadUrl = platform.GetDownloadUrl(_version4, false, false);
    downloadUrl.ShouldBe($"{GodotEnvironment.GODOT_URL_PREFIX}4.1.2-stable/Godot_v4.1.2-stable_win64.exe.zip");

    downloadUrl = platform.GetDownloadUrl(_version3, false, false);
    downloadUrl.ShouldBe($"{GodotEnvironment.GODOT_URL_PREFIX}{_version3.GodotVersionString()}/Godot_v{_version3.GodotVersionString()}_win64.exe.zip");
  }

  [Fact]
  public void GetsExpectedWindowsMonoDownloadUrl() {
    var systemInfo = new MockSystemInfo(OSType.Windows, CPUArch.X64);
    var platform = new Windows(systemInfo, _fileClient.Object, _computer.Object);

    var downloadUrl = platform.GetDownloadUrl(_version4, true, false);
    downloadUrl.ShouldBe(GetExpectedDownloadUrl(_version4, "_mono_win64"));

    downloadUrl = platform.GetDownloadUrl(_version3, true, false);
    downloadUrl.ShouldBe(GetExpectedDownloadUrl(_version3, "_mono_win64"));
  }

  [Fact]
  public void GetsExpectedWindowsArmDownloadUrlForOlderVersions() {
    var systemInfo = new MockSystemInfo(OSType.Windows, CPUArch.Arm64);
    var platform = new Windows(systemInfo, _fileClient.Object, _computer.Object);

    var downloadUrl = platform.GetDownloadUrl(_version4, false, false);
    downloadUrl.ShouldBe($"{GodotEnvironment.GODOT_URL_PREFIX}4.1.2-stable/Godot_v4.1.2-stable_win64.exe.zip");

    downloadUrl = platform.GetDownloadUrl(_version3, false, false);
    downloadUrl.ShouldBe($"{GodotEnvironment.GODOT_URL_PREFIX}{_version3.GodotVersionString()}/Godot_v{_version3.GodotVersionString()}_win64.exe.zip");
  }

  [Fact]
  public void GetsExpectedWindowsArmMonoDownloadUrlForOlderVersions() {
    var systemInfo = new MockSystemInfo(OSType.Windows, CPUArch.Arm64);
    var platform = new Windows(systemInfo, _fileClient.Object, _computer.Object);

    var downloadUrl = platform.GetDownloadUrl(_version4, true, false);
    downloadUrl.ShouldBe(GetExpectedDownloadUrl(_version4, "_mono_win64"));

    downloadUrl = platform.GetDownloadUrl(_version3, true, false);
    downloadUrl.ShouldBe(GetExpectedDownloadUrl(_version3, "_mono_win64"));
  }

  [Fact]
  public void GetExpectedWindowsArmDownloadUrlForNewerVersions() {
    var systemInfo = new MockSystemInfo(OSType.Windows, CPUArch.Arm64);
    var platform = new Windows(systemInfo, _fileClient.Object, _computer.Object);
    var firstKnownWinArmVersion = new GodotVersion("4", "3", "0", "stable", "");

    var downloadUrl = platform.GetDownloadUrl(firstKnownWinArmVersion, false, false);
    downloadUrl.ShouldBe($"{GodotEnvironment.GODOT_URL_PREFIX}4.3-stable/Godot_v4.3-stable_windows_arm64.exe.zip");
  }

  [Fact]
  public void GetExpectedWindowsArmMonoDownloadUrlForNewerVersions() {
    var systemInfo = new MockSystemInfo(OSType.Windows, CPUArch.Arm64);
    var platform = new Windows(systemInfo, _fileClient.Object, _computer.Object);
    var firstKnownWinArmVersion = new GodotVersion("4", "3", "0", "stable", "");

    var downloadUrl = platform.GetDownloadUrl(firstKnownWinArmVersion, true, false);
    downloadUrl.ShouldBe($"{GodotEnvironment.GODOT_URL_PREFIX}4.3-stable/Godot_v4.3-stable_mono_windows_arm64.zip");
  }

  [Fact]
  public void GetsExpectedLinuxDownloadUrl() {
    var systemInfo = new MockSystemInfo(OSType.Linux, CPUArch.X64);
    var platform = new Linux(systemInfo, _fileClient.Object, _computer.Object);

    var downloadUrl = platform.GetDownloadUrl(_version4, false, false);
    downloadUrl.ShouldBe(GetExpectedDownloadUrl(_version4, "_linux.x86_64"));

    downloadUrl = platform.GetDownloadUrl(_version3, false, false);
    downloadUrl.ShouldBe(GetExpectedDownloadUrl(_version3, "_x11.64"));
  }

  [Fact]
  public void GetsExpectedLinuxMonoDownloadUrl() {
    var systemInfo = new MockSystemInfo(OSType.Linux, CPUArch.X64);
    var platform = new Linux(systemInfo, _fileClient.Object, _computer.Object);

    var downloadUrl = platform.GetDownloadUrl(_version4, true, false);
    downloadUrl.ShouldBe(GetExpectedDownloadUrl(_version4, "_mono_linux_x86_64"));

    downloadUrl = platform.GetDownloadUrl(_version3, true, false);
    downloadUrl.ShouldBe(GetExpectedDownloadUrl(_version3, "_mono_x11_64"));
  }

  [Fact]
  public void GetsExpectedTemplatesDownloadUrl() {
    var systemInfo = new MockSystemInfo(OSType.MacOS, CPUArch.Arm64);
    var platform = new MacOS(systemInfo, _fileClient.Object, _computer.Object);
    var downloadUrl = platform.GetDownloadUrl(_version4, false, true);

    downloadUrl.ShouldBe($"{GodotEnvironment.GODOT_URL_PREFIX}4.1.2-stable/Godot_v4.1.2-stable_export_templates.tpz");
  }

  [Fact]
  public void GetsExpectedTemplatesMonoDownloadUrl() {
    var systemInfo = new MockSystemInfo(OSType.MacOS, CPUArch.Arm64);
    var platform = new MacOS(systemInfo, _fileClient.Object, _computer.Object);
    var downloadUrl = platform.GetDownloadUrl(_version4, true, true);

    downloadUrl.ShouldBe($"{GodotEnvironment.GODOT_URL_PREFIX}4.1.2-stable/Godot_v4.1.2-stable_mono_export_templates.tpz");
  }

  private static string GetExpectedDownloadUrl(GodotVersion version, string platformSuffix) =>
    $"{GodotEnvironment.GODOT_URL_PREFIX}{version.GodotVersionString()}/Godot_v{version.GodotVersionString()}{platformSuffix}.zip";
}
