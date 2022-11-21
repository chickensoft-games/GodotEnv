namespace Chickensoft.Chicken.Tests;
using System;
using Moq;
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
    var log = new Mock<ILog>();
    log.Setup(l => l.Success(e.ToString()));

    e.Log(log.Object);

    log.VerifyAll();
  }

  [Fact]
  public void AddonFailedToInstallEventInitializesCorrectly() {
    var e = new AddonFailedToInstallEvent(
      _addon, new InvalidOperationException("test")
    );

    var log = new Mock<ILog>();
    log.Setup(l => l.Err(e.ToString()));

    e.Log(log.Object);

    log.VerifyAll();
  }
}
