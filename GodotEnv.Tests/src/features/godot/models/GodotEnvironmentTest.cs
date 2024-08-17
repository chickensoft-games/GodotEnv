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
    fileClient.Setup(f => f.Processor).Returns(ProcessorType.other);

    var platform = new Windows(_fileClient.Object, _computer.Object);

    var downloadUrl = platform.GetDownloadUrl(_version4, false, false);
    downloadUrl.ShouldBe($"{GodotEnvironment.GODOT_URL_PREFIX}4.1.2-stable/Godot_v4.1.2-stable_win64.exe.zip");

    downloadUrl = platform.GetDownloadUrl(_version3, false, false);
    downloadUrl.ShouldBe($"{GodotEnvironment.GODOT_URL_PREFIX}{_version3.VersionString}-stable/Godot_v{_version3.VersionString}-stable_win64.exe.zip");
  }

  [Fact]
  public void GetsExpectedWindowsMonoDownloadUrl() {
    fileClient.Setup(f => f.Processor).Returns(ProcessorType.other);

    var platform = new Windows(_fileClient.Object, _computer.Object);

    var downloadUrl = platform.GetDownloadUrl(_version4, true, false);
    downloadUrl.ShouldBe(GetExpectedDownloadUrl(_version4, "stable_mono_win64"));

    downloadUrl = platform.GetDownloadUrl(_version3, true, false);
    downloadUrl.ShouldBe(GetExpectedDownloadUrl(_version3, "stable_mono_win64"));
  }

  [Fact]
  public void GetsExpectedWindowsArmDownloadUrl() {
    fileClient.Setup(f => f.Processor).Returns(ProcessorType.arm64);

    var platform = new Windows(fileClient.Object, computer.Object);

    var downloadUrl = platform.GetDownloadUrl(version4, false, false);
    downloadUrl.ShouldBe($"{GodotEnvironment.GODOT_URL_PREFIX}4.1.2-stable/Godot_v4.1.2-stable_windows_arm64.exe.zip");

    downloadUrl = platform.GetDownloadUrl(version3, false, false);
    downloadUrl.ShouldBe($"{GodotEnvironment.GODOT_URL_PREFIX}{version3.VersionString}-stable/Godot_v{version3.VersionString}-stable_windows_arm64.exe.zip");
  }

  [Fact]
  public void GetsExpectedWindowsArmMonoDownloadUrl() {
    fileClient.Setup(f => f.Processor).Returns(ProcessorType.arm64);

    var platform = new Windows(fileClient.Object, computer.Object);

    var downloadUrl = platform.GetDownloadUrl(version4, true, false);
    downloadUrl.ShouldBe(GetExpectedDownloadUrl(version4, "stable_mono_windows_arm64"));

    downloadUrl = platform.GetDownloadUrl(version3, true, false);
    downloadUrl.ShouldBe(GetExpectedDownloadUrl(version3, "stable_mono_windows_arm64"));
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
