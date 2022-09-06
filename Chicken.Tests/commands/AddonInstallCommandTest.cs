namespace Chickensoft.Chicken.Tests {
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using CliFx.Infrastructure;
  using Moq;
  using Shouldly;
  using Xunit;

  public class AddonInstallCommandTest {
    [Fact]
    public void Initializes() {
      var command = new InstallCommand();
      command.ShouldBeOfType(typeof(InstallCommand));
    }

    [Fact]
    public async Task UsesAddonManagerToInstallAddons() {
      var app = new Mock<IApp>();
      var console = new FakeInMemoryConsole();

      var addonManager = new Mock<IAddonManager>();
      var configFileRepo = new Mock<IConfigFileRepo>();

      configFileRepo.Setup(
        c => c.LoadOrCreateConfigFile(Environment.CurrentDirectory)
      ).Returns(new ConfigFile(
        addons: new Dictionary<string, AddonConfig>() { },
        cachePath: "/.addons",
        addonsPath: "/addons"
      ));

      addonManager.Setup(am => am.InstallAddons(Environment.CurrentDirectory))
        .Returns(Task.CompletedTask);

      app.Setup(a => a.CreateAddonRepo())
        .Returns(() => new Mock<IAddonRepo>().Object);

      app.Setup(a => a.CreateConfigFileRepo())
        .Returns(() => configFileRepo.Object);

      app.Setup(a => a.CreateReporter(console))
        .Returns(() => new Mock<IReporter>().Object);

      app.Setup(a => a.CreateAddonManager(
        It.IsAny<IAddonRepo>(),
        It.IsAny<IConfigFileRepo>(),
        It.IsAny<IReporter>(),
        It.IsAny<IDependencyGraph>()
      )).Returns(addonManager.Object);

      var command = new InstallCommand(app.Object);

      await command.ExecuteAsync(console);

      app.VerifyAll();
      addonManager.VerifyAll();
    }
  }
}
