namespace Chickensoft.Chicken.Tests;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading.Tasks;
using CliFx.Infrastructure;
using Moq;
using Shouldly;
using Xunit;

public class AddonInstallCommandTest {
  [Fact]
  public void Initializes() {
    var command = new AddonsInstallCommand();
    command.ShouldBeOfType(typeof(AddonsInstallCommand));
  }

  [Fact]
  public async Task UsesAddonManagerToInstallAddons() {
    var maxDepth = 1;
    var app = new Mock<IApp>();
    var envDir = "/";
    var fs = new Mock<IFileSystem>();
    var console = new FakeInMemoryConsole();
    var addonManager = new Mock<IAddonManager>();
    var configFileLoader = new Mock<IConfigFileLoader>();
    var addonRepo = new Mock<IAddonRepo>();
    var log = new Mock<ILog>();
    var dependencyGraph = new Mock<IDependencyGraph>();
    var copier = new Mock<IFileCopier>();

    app.Setup(app => app.WorkingDir).Returns(envDir);

    app.Setup(a => a.CreateLog(console))
      .Returns(() => log.Object);

    configFileLoader.Setup(
      c => c.Load(envDir)
    ).Returns(new ConfigFile(
      addons: new Dictionary<string, AddonConfig>() { },
      cachePath: "/.addons",
      addonsPath: "/addons"
    ));

    app.Setup(a => a.CreateAddonManager(
      fs.Object,
      addonRepo.Object,
      configFileLoader.Object,
      log.Object,
      dependencyGraph.Object
    )).Returns(addonManager.Object);

    addonManager
      .Setup(am => am.InstallAddons(
        app.Object, envDir, copier.Object, maxDepth
      )).Returns(Task.CompletedTask);

    var command = new AddonsInstallCommand(
      app.Object,
      fs.Object,
      copier.Object,
      addonRepo.Object,
      configFileLoader.Object,
      dependencyGraph.Object
    ) { MaxDepth = maxDepth };

    await command.ExecuteAsync(console);

    app.VerifyAll();
    addonManager.VerifyAll();
  }
}
