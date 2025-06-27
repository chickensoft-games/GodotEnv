namespace Chickensoft.GodotEnv.Tests;

using Chickensoft.GodotEnv.Common.Models;
using Shouldly;
using Xunit;

public class ConfigTest {
  [Fact]
#pragma warning disable CS0618 // for deprecated property
  public void UpgradeMovesGodotInstallationPathIfNotDefaultAndNewKeyIsNullOrEmpty() {
    var testPath = "/test/path";
    var config = new Config(
      new ConfigValues {
        GodotInstallationsPath = testPath,
        Godot = new GodotConfigSection { InstallationsPath = string.Empty }
      }
    );
    config.ConfigValues.GodotInstallationsPath.ShouldBe(testPath);
    string.IsNullOrEmpty(config.ConfigValues.Godot.InstallationsPath).ShouldBeTrue();

    config.Upgrade();

    config.ConfigValues.GodotInstallationsPath.ShouldBeNull();
    config.ConfigValues.Godot.InstallationsPath.ShouldBe(testPath);
  }
#pragma warning restore CS0618

  [Fact]
#pragma warning disable CS0618 // for deprecated property
  public void UpgradeMovesGodotInstallationPathIfNotDefaultAndNewKeyIsDefault() {
    var testPath = "/test/path";
    var config = new Config(
      new ConfigValues {
        GodotInstallationsPath = testPath,
      }
    );
    config.ConfigValues.GodotInstallationsPath.ShouldBe(testPath);
    config.ConfigValues.Godot.InstallationsPath.ShouldBe(Defaults.CONFIG_GODOT_INSTALLATIONS_PATH);

    config.Upgrade();

    config.ConfigValues.GodotInstallationsPath.ShouldBeNull();
    config.ConfigValues.Godot.InstallationsPath.ShouldBe(testPath);
  }
#pragma warning restore CS0618

  [Fact]
#pragma warning disable CS0618 // for deprecated property
  public void UpgradeRemovesGodotInstallationPathButDoesNotMoveIfDefaultAndNewKeyIsNotDefault() {
    var testPath = "/test/path";
    var config = new Config(
      new ConfigValues {
        GodotInstallationsPath = Defaults.CONFIG_GODOT_INSTALLATIONS_PATH,
        Godot = new() { InstallationsPath = testPath }
      }
    );
    config.ConfigValues.GodotInstallationsPath.ShouldBe(Defaults.CONFIG_GODOT_INSTALLATIONS_PATH);
    config.ConfigValues.Godot.InstallationsPath.ShouldBe(testPath);

    config.Upgrade();

    config.ConfigValues.GodotInstallationsPath.ShouldBeNull();
    config.ConfigValues.Godot.InstallationsPath.ShouldBe(testPath);
  }
#pragma warning restore CS0618

  [Fact]
#pragma warning disable CS0618 // for deprecated property
  public void UpgradeRemovesGodotInstallationPathButDoesNotMoveIfNotDefaultAndNewKeyIsNotDefault() {
    var testPath = "/test/path";
    var testPathNew = "/test/path/new";
    var config = new Config(
      new ConfigValues {
        GodotInstallationsPath = testPath,
        Godot = new() { InstallationsPath = testPathNew }
      }
    );
    config.ConfigValues.GodotInstallationsPath.ShouldBe(testPath);
    config.ConfigValues.Godot.InstallationsPath.ShouldBe(testPathNew);

    config.Upgrade();

    config.ConfigValues.GodotInstallationsPath.ShouldBeNull();
    config.ConfigValues.Godot.InstallationsPath.ShouldBe(testPathNew);
  }
#pragma warning restore CS0618

  [Fact]
  public void NewConfigHasKeyValuePairFromGodotInstallationsPath() {
    var testPath = "/test/path";
    var config = new Config(
      new ConfigValues {
        Godot = new() { InstallationsPath = testPath }
      }
    );
    config.Get("Godot:InstallationsPath").ShouldBe(testPath);
  }
}
