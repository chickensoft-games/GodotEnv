namespace Chickensoft.Chicken.Tests {
  using System;
  using Shouldly;
  using Xunit;


  public class AddonInstallationEventTest {

    private readonly RequiredAddon _addon = new(
      name: "A",
      configFilePath: "project/addons.json",
      url: "https://user/a.git",
      checkout: "main",
      subfolder: "/"
    );

    [Fact]
    public void AddonInstalledEventInitializesCorrectly() {
      var e = new AddonInstalledEvent(_addon);

      e.Color.ShouldBeOfType<ConsoleColor>();
      e.Addon.ShouldBe(_addon);
      e.ToString().ShouldContain(_addon.Name);
    }

    [Fact]
    public void AddonFailedToInstallEventInitializesCorrectly() {
      var e = new AddonFailedToInstallEvent(
        _addon, new InvalidOperationException("test")
      );

      e.Color.ShouldBeOfType<ConsoleColor>();
      e.Addon.ShouldBe(_addon);
      e.ToString().ShouldContain("failed");
      e.ToString().ShouldContain(_addon.Name);
    }
  }
}
