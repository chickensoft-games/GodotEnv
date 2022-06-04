namespace Chickensoft.GoDotAddon.Tests {
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Moq;
  using Xunit;

  public class AddonManagerTest {
    [Fact]
    public async Task InstallsAddonsInProject() {
      var addonRepo = new Mock<IAddonRepo>(MockBehavior.Strict);
      var configFileRepo = new Mock<IConfigFileRepo>(MockBehavior.Strict);
      var reporter = new Mock<IReporter>(MockBehavior.Strict);
      var dependencyGraph = new Mock<IDependencyGraph>(MockBehavior.Strict);
      var manager = new AddonManager(
        addonRepo: addonRepo.Object,
        configFileRepo: configFileRepo.Object,
        reporter: reporter.Object,
        dependencyGraph: dependencyGraph.Object
      );

      var projectPath = "/";

      var projectConfigFile = new ConfigFile(
        addons: new Dictionary<string, AddonConfig>() {
          { "addon1", new AddonConfig(
            url: "http://example.com/addon1.git",
            checkout: "master",
            subfolder: "addon1"
          )},
          { "addon2", new AddonConfig(
            url: "http://example.com/addon2.git",
            checkout: "master",
            subfolder: "addon2"
          )},
        },
        cachePath: ".addons",
        addonsPath: "addons"
      );

      configFileRepo.Setup(repo => repo.LoadOrCreateConfigFile(projectPath))
        .Returns(projectConfigFile);
      await manager.InstallAddons("/path/to/project");
    }
  }
}
