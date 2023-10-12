namespace Chickensoft.GodotEnv.Tests.features.godot.models;

using Common.Clients;
using Common.Utilities;
using Features.Godot.Models;
using Moq;
using Shouldly;
using Xunit;

public class GodotEnvironmentTest {
  private readonly SemanticVersion version;
  private readonly Mock<IComputer> computer;
  private readonly Mock<IFileClient> fileClient;

  public GodotEnvironmentTest() {
    computer = new Mock<IComputer>();
    fileClient = new Mock<IFileClient>();
    version = new SemanticVersion("4", "1", "2", string.Empty);
  }

  [Fact]
  public void GetsExpectedMacDownloadUrl() {
    var platform = new MacOS(fileClient.Object, computer.Object);
    var downloadUrl = platform.GetDownloadUrl(version, false, false);

    downloadUrl.ShouldBe($"{TestConstants.GODOT_URL_PREFIX}4.1.2-stable/Godot_v4.1.2-stable_macos.universal.zip");
  }

  [Fact]
  public void GetsExpectedMacMonoDownloadUrl() {
    var platform = new MacOS(fileClient.Object, computer.Object);
    var downloadUrl = platform.GetDownloadUrl(version, true, false);

    downloadUrl.ShouldBe($"{TestConstants.GODOT_URL_PREFIX}4.1.2-stable/Godot_v4.1.2-stable_mono_macos.universal.zip");
  }

  [Fact]
  public void GetsExpectedWindowsDownloadUrl() {
    var platform = new Windows(fileClient.Object, computer.Object);
    var downloadUrl = platform.GetDownloadUrl(version, false, false);

    downloadUrl.ShouldBe($"{TestConstants.GODOT_URL_PREFIX}4.1.2-stable/Godot_v4.1.2-stable_win64.exe.zip");
  }

  [Fact]
  public void GetsExpectedWindowsMonoDownloadUrl() {
    var platform = new Windows(fileClient.Object, computer.Object);
    var downloadUrl = platform.GetDownloadUrl(version, true, false);

    downloadUrl.ShouldBe($"{TestConstants.GODOT_URL_PREFIX}4.1.2-stable/Godot_v4.1.2-stable_mono_win64.zip");
  }

  [Fact]
  public void GetsExpectedLinuxDownloadUrl() {
    var platform = new Linux(fileClient.Object, computer.Object);
    var downloadUrl = platform.GetDownloadUrl(version, false, false);

    downloadUrl.ShouldBe($"{TestConstants.GODOT_URL_PREFIX}4.1.2-stable/Godot_v4.1.2-stable_linux.x86_64.zip");
  }

  [Fact]
  public void GetsExpectedLinuxMonoDownloadUrl() {
    var platform = new Linux(fileClient.Object, computer.Object);
    var downloadUrl = platform.GetDownloadUrl(version, true, false);

    downloadUrl.ShouldBe($"{TestConstants.GODOT_URL_PREFIX}4.1.2-stable/Godot_v4.1.2-stable_mono_linux_x86_64.zip");
  }

  [Fact]
  public void GetsExpectedTemplatesDownloadUrl() {
    var platform = new MacOS(fileClient.Object, computer.Object);
    var downloadUrl = platform.GetDownloadUrl(version, false, true);

    downloadUrl.ShouldBe($"{TestConstants.GODOT_URL_PREFIX}4.1.2-stable/Godot_v4.1.2-stable_export_templates.tpz");
  }

  [Fact]
  public void GetsExpectedTemplatesMonoDownloadUrl() {
    var platform = new MacOS(fileClient.Object, computer.Object);
    var downloadUrl = platform.GetDownloadUrl(version, true, true);

    downloadUrl.ShouldBe($"{TestConstants.GODOT_URL_PREFIX}4.1.2-stable/Godot_v4.1.2-stable_mono_export_templates.tpz");
  }
}
