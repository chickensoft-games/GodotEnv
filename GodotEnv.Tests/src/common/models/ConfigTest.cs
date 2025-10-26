namespace Chickensoft.GodotEnv.Tests;

using System.Collections.Generic;
using Chickensoft.GodotEnv.Common.Models;
using Microsoft.Extensions.Configuration;
using Shouldly;
using Xunit;

public class ConfigTest
{
  [Fact]
#pragma warning disable CS0618 // for deprecated property
  public void UpgradeMovesGodotInstallationPathIfNotDefaultAndNewKeyIsNullOrEmpty()
  {
    var testPath = "/test/path";
    var config = new Config(
      new ConfigValues
      {
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
  public void UpgradeMovesGodotInstallationPathIfNotDefaultAndNewKeyIsDefault()
  {
    var testPath = "/test/path";
    var config = new Config(
      new ConfigValues
      {
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
  public void UpgradeRemovesGodotInstallationPathButDoesNotMoveIfDefaultAndNewKeyIsNotDefault()
  {
    var testPath = "/test/path";
    var config = new Config(
      new ConfigValues
      {
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
  public void UpgradeRemovesGodotInstallationPathButDoesNotMoveIfNotDefaultAndNewKeyIsNotDefault()
  {
    var testPath = "/test/path";
    var testPathNew = "/test/path/new";
    var config = new Config(
      new ConfigValues
      {
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
  public void NewConfigHasKeyValuePairFromGodotInstallationsPath()
  {
    var testPath = "/test/path";
    var config = new Config(
      new ConfigValues
      {
        Godot = new() { InstallationsPath = testPath }
      }
    );
    config.Get("Godot.InstallationsPath").ShouldBe(testPath);
  }

  [Fact]
  public void NewConfigHasKeyValuePairFromTerminalDisplayEmoji()
  {
    var displayEmoji = !Defaults.CONFIG_TERMINAL_DISPLAY_EMOJI;
    var config = new Config(
      new ConfigValues
      {
        Terminal = new() { DisplayEmoji = displayEmoji }
      }
    );
    config.Get("Terminal.DisplayEmoji").ShouldBe(displayEmoji.ToString());
  }

  [Fact]
  public void NewConfigHasValueFromKeyValuePairGodotInstallationsPath()
  {
    var testPath = "/test/path";
    var configuration = new ConfigurationBuilder()
      .AddInMemoryCollection(initialData: [new("Godot:InstallationsPath", testPath)])
      .Build();
    var config = new Config(configuration);
    config.ConfigValues.Godot.InstallationsPath.ShouldBe(testPath);
  }

  [Fact]
  public void NewConfigHasValueFromKeyValuePairTerminalDisplayEmoji()
  {
    var displayEmoji = !Defaults.CONFIG_TERMINAL_DISPLAY_EMOJI;
    var configuration = new ConfigurationBuilder()
      .AddInMemoryCollection(initialData: [new("Terminal:DisplayEmoji", displayEmoji.ToString())])
      .Build();
    var config = new Config(configuration);
    config.ConfigValues.Terminal.DisplayEmoji.ShouldBe(displayEmoji);
  }

  [Fact]
  public void ConfigValueUpdatesWhenGodotInstallationsPathKeyValueSet()
  {
    var testPath = "/test/path";
    var config = new Config();
    config.ConfigValues.Godot.InstallationsPath.ShouldBe(Defaults.CONFIG_GODOT_INSTALLATIONS_PATH);
    config.Set("Godot.InstallationsPath", testPath);
    config.ConfigValues.Godot.InstallationsPath.ShouldBe(testPath);
  }

  [Fact]
  public void ConfigValueUpdatesWhenTerminalDisplayEmojiKeyValueSet()
  {
    var displayEmoji = !Defaults.CONFIG_TERMINAL_DISPLAY_EMOJI;
    var config = new Config();
    config.ConfigValues.Terminal.DisplayEmoji.ShouldBe(Defaults.CONFIG_TERMINAL_DISPLAY_EMOJI);
    config.Set("Terminal.DisplayEmoji", displayEmoji.ToString());
    config.ConfigValues.Terminal.DisplayEmoji.ShouldBe(displayEmoji);
  }

  [Fact]
  public void EnumeratesKeyValuePairs()
  {
    var config = new Config();
    var keyValuePairs = new KeyValuePair<string, string?>[4] {
      new("Godot.InstallationsPath", Defaults.CONFIG_GODOT_INSTALLATIONS_PATH),
      new("Terminal.DisplayEmoji", Defaults.CONFIG_TERMINAL_DISPLAY_EMOJI.ToString()),
      new("Terminal", null),
      new("Godot", null),
    };
    var pairCount = 0;
    foreach (var pair in config)
    {
      pair.ShouldBeOneOf(keyValuePairs);
      pairCount += 1;
    }
    pairCount.ShouldBe(4);
  }

  [Fact]
  public void ConvertsUserKeyToConfigurationKey() =>
    Config
      .ConfigurationKey("TestSection.TestKey")
      .ShouldBe("TestSection:TestKey");

  [Fact]
  public void ConvertsConfigurationKeyToUserKey() =>
    Config
      .UserKey("TestSection:TestKey")
      .ShouldBe("TestSection.TestKey");
}
