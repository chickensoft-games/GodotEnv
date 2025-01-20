namespace Chickensoft.GodotEnv.Tests.Features.Godot.Models;

using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Features.Godot.Models;
using Common.Clients;
using Common.Utilities;
using Moq;
using Shouldly;
using Xunit;

public class GodotEnvironmentTest {
  private readonly Mock<IComputer> _computer = new();
  private readonly Mock<IFileClient> _fileClient = new();
  private readonly SemanticVersion _version4 = new("4", "1", "2");
  private readonly SemanticVersion _version3 = new("3", "5", "3");

  [Fact]
  public void GetsExpectedMacDownloadUrl() {
    var platform = new MacOS(_fileClient.Object, _computer.Object);

    var downloadUrl = platform.GetDownloadUrl(_version4, false, false);
    downloadUrl.ShouldBe(GetExpectedDownloadUrl(_version4, "stable_macos.universal"));

    downloadUrl = platform.GetDownloadUrl(_version3, false, false);
    downloadUrl.ShouldBe(GetExpectedDownloadUrl(_version3, "stable_osx.universal"));
  }

  [Fact]
  public void GetsExpectedMacMonoDownloadUrl() {
    var platform = new MacOS(_fileClient.Object, _computer.Object);

    var downloadUrl = platform.GetDownloadUrl(_version4, true, false);
    downloadUrl.ShouldBe(GetExpectedDownloadUrl(_version4, "stable_mono_macos.universal"));

    downloadUrl = platform.GetDownloadUrl(_version3, true, false);
    downloadUrl.ShouldBe(GetExpectedDownloadUrl(_version3, "stable_mono_osx.universal"));
  }

  [Fact]
  public void GetsExpectedWindowsDownloadUrl() {
    _fileClient.Setup(f => f.Processor).Returns(CPUArch.other);

    var platform = new Windows(_fileClient.Object, _computer.Object);

    var downloadUrl = platform.GetDownloadUrl(_version4, false, false);
    downloadUrl.ShouldBe($"{GodotEnvironment.GODOT_URL_PREFIX}4.1.2-stable/Godot_v4.1.2-stable_win64.exe.zip");

    downloadUrl = platform.GetDownloadUrl(_version3, false, false);
    downloadUrl.ShouldBe($"{GodotEnvironment.GODOT_URL_PREFIX}{_version3.VersionString}-stable/Godot_v{_version3.VersionString}-stable_win64.exe.zip");
  }

  [Fact]
  public void GetsExpectedWindowsMonoDownloadUrl() {
    _fileClient.Setup(f => f.Processor).Returns(CPUArch.other);

    var platform = new Windows(_fileClient.Object, _computer.Object);

    var downloadUrl = platform.GetDownloadUrl(_version4, true, false);
    downloadUrl.ShouldBe(GetExpectedDownloadUrl(_version4, "stable_mono_win64"));

    downloadUrl = platform.GetDownloadUrl(_version3, true, false);
    downloadUrl.ShouldBe(GetExpectedDownloadUrl(_version3, "stable_mono_win64"));
  }

  [Fact]
  public void GetsExpectedWindowsArmDownloadUrlForOlderVersions() {
    _fileClient.Setup(f => f.Processor).Returns(CPUArch.arm64);

    var platform = new Windows(_fileClient.Object, _computer.Object);

    var downloadUrl = platform.GetDownloadUrl(_version4, false, false);
    downloadUrl.ShouldBe($"{GodotEnvironment.GODOT_URL_PREFIX}4.1.2-stable/Godot_v4.1.2-stable_win64.exe.zip");

    downloadUrl = platform.GetDownloadUrl(_version3, false, false);
    downloadUrl.ShouldBe($"{GodotEnvironment.GODOT_URL_PREFIX}{_version3.VersionString}-stable/Godot_v{_version3.VersionString}-stable_win64.exe.zip");
  }

  [Fact]
  public void GetsExpectedWindowsArmMonoDownloadUrlForOlderVersions() {
    _fileClient.Setup(f => f.Processor).Returns(CPUArch.arm64);

    var platform = new Windows(_fileClient.Object, _computer.Object);

    var downloadUrl = platform.GetDownloadUrl(_version4, true, false);
    downloadUrl.ShouldBe(GetExpectedDownloadUrl(_version4, "stable_mono_win64"));

    downloadUrl = platform.GetDownloadUrl(_version3, true, false);
    downloadUrl.ShouldBe(GetExpectedDownloadUrl(_version3, "stable_mono_win64"));
  }

  [Fact]
  public void GetExpectedWindowsArmDownloadUrlForNewerVersions() {
    _fileClient.Setup(f => f.Processor).Returns(CPUArch.arm64);

    var platform = new Windows(_fileClient.Object, _computer.Object);
    var firstKnownWinArmVersion = new SemanticVersion("4", "3", "0");

    var downloadUrl = platform.GetDownloadUrl(firstKnownWinArmVersion, false, false);
    downloadUrl.ShouldBe($"{GodotEnvironment.GODOT_URL_PREFIX}4.3-stable/Godot_v4.3-stable_windows_arm64.exe.zip");
  }

  [Fact]
  public void GetExpectedWindowsArmMonoDownloadUrlForNewerVersions() {
    _fileClient.Setup(f => f.Processor).Returns(CPUArch.arm64);

    var platform = new Windows(_fileClient.Object, _computer.Object);
    var firstKnownWinArmVersion = new SemanticVersion("4", "3", "0");

    var downloadUrl = platform.GetDownloadUrl(firstKnownWinArmVersion, true, false);
    downloadUrl.ShouldBe($"{GodotEnvironment.GODOT_URL_PREFIX}4.3-stable/Godot_v4.3-stable_mono_windows_arm64.zip");
  }

  [Fact]
  public void GetsExpectedLinuxDownloadUrl() {
    var platform = new Linux(_fileClient.Object, _computer.Object);

    var downloadUrl = platform.GetDownloadUrl(_version4, false, false);
    downloadUrl.ShouldBe(GetExpectedDownloadUrl(_version4, "stable_linux.x86_64"));

    downloadUrl = platform.GetDownloadUrl(_version3, false, false);
    downloadUrl.ShouldBe(GetExpectedDownloadUrl(_version3, "stable_x11.64"));
  }

  [Fact]
  public void GetsExpectedLinuxMonoDownloadUrl() {
    var platform = new Linux(_fileClient.Object, _computer.Object);

    var downloadUrl = platform.GetDownloadUrl(_version4, true, false);
    downloadUrl.ShouldBe(GetExpectedDownloadUrl(_version4, "stable_mono_linux_x86_64"));

    downloadUrl = platform.GetDownloadUrl(_version3, true, false);
    downloadUrl.ShouldBe(GetExpectedDownloadUrl(_version3, "stable_mono_x11_64"));
  }

  [Fact]
  public void GetsExpectedTemplatesDownloadUrl() {
    var platform = new MacOS(_fileClient.Object, _computer.Object);
    var downloadUrl = platform.GetDownloadUrl(_version4, false, true);

    downloadUrl.ShouldBe($"{GodotEnvironment.GODOT_URL_PREFIX}4.1.2-stable/Godot_v4.1.2-stable_export_templates.tpz");
  }

  [Fact]
  public void GetsExpectedTemplatesMonoDownloadUrl() {
    var platform = new MacOS(_fileClient.Object, _computer.Object);
    var downloadUrl = platform.GetDownloadUrl(_version4, true, true);

    downloadUrl.ShouldBe($"{GodotEnvironment.GODOT_URL_PREFIX}4.1.2-stable/Godot_v4.1.2-stable_mono_export_templates.tpz");
  }

  private static string GetExpectedDownloadUrl(SemanticVersion version, string platformSuffix) =>
    $"{GodotEnvironment.GODOT_URL_PREFIX}{version.VersionString}-stable/Godot_v{version.VersionString}-{platformSuffix}.zip";
}
