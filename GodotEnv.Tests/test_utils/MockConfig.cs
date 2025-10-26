namespace Chickensoft.GodotEnv.Tests;

using Chickensoft.GodotEnv.Common.Models;
using Moq;

public static class MockConfig
{
  public static Mock<IConfig> Get() =>
    Get(
      Defaults.CONFIG_GODOT_INSTALLATIONS_PATH,
      Defaults.CONFIG_TERMINAL_DISPLAY_EMOJI
    );

  public static Mock<IConfig> Get(string godotInstallationsPath, bool displayEmoji)
  {
    var godotConfig = new Mock<IReadOnlyGodotConfigSection>();
    godotConfig.Setup(gdt => gdt.InstallationsPath).Returns(godotInstallationsPath);
    var terminalConfig = new Mock<IReadOnlyTerminalConfigSection>();
    terminalConfig.Setup(trm => trm.DisplayEmoji).Returns(displayEmoji);
    var configVals = new Mock<IReadOnlyConfigValues>();
    configVals.Setup(vals => vals.Terminal).Returns(terminalConfig.Object);
    var config = new Mock<IConfig>();
    config.Setup(cfg => cfg.ConfigValues).Returns(configVals.Object);
    return config;
  }
}
