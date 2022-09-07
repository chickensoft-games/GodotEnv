namespace Chickensoft.Chicken.Tests {
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Moq;
  using Shouldly;
  using Xunit;

  public class AddonManagerTest {
    [Fact]
    public async Task InstallsAddonsInProject() {
      var projectPath = "/";
      var addonRepo = new Mock<IAddonRepo>(MockBehavior.Strict);
      var configFileRepo = new Mock<IConfigFileRepo>(MockBehavior.Strict);
      var reporter = new Mock<IReporter>(MockBehavior.Strict);
      var dependencyGraph = new Mock<IDependencyGraph>(MockBehavior.Strict);

      var addon1 = new RequiredAddon(
          name: "addon1",
          configFilePath: "/addons.json",
          url: "http://example.com/addon1.git",
          checkout: "master",
          subfolder: "addon1"
        );

      var addon2 = new RequiredAddon(
          name: "addon2",
          configFilePath: "/addons.json",
          url: "http://example.com/addon2.git",
          checkout: "master",
          subfolder: "addon2"
        );

      var manager = new AddonManager(
        addonRepo: addonRepo.Object,
        configFileRepo: configFileRepo.Object,
        reporter: reporter.Object,
        dependencyGraph: dependencyGraph.Object
      );

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

      var projectConfig = projectConfigFile.ToConfig(projectPath);

      configFileRepo.Setup(repo => repo.LoadOrCreateConfigFile(projectPath))
        .Returns(projectConfigFile);

      addonRepo.Setup(repo => repo.LoadCache(projectConfig)).Returns(
        Task.FromResult(new Dictionary<string, string>() {
          { "http://example.com/addon1.git", "addon1" }
        })
      );

      // Addon 1 installation calls
      dependencyGraph.Setup(dg => dg.Add(addon1)).Returns(
        new DependencyCanBeInstalledEvent(addon1)
      );
      reporter.Setup(
        r => r.Handle(new DependencyCanBeInstalledEvent(addon1))
      );
      reporter.Setup(
        r => r.Handle(new AddonInstalledEvent(addon1))
      );
      addonRepo.Setup(ar => ar.CacheAddon(addon1, projectConfig)).Returns(
        Task.CompletedTask
      );
      addonRepo.Setup(ar => ar.DeleteAddon(addon1, projectConfig)).Returns(
        Task.CompletedTask
      );
      addonRepo.Setup(
        ar => ar.CopyAddonFromCache(addon1, projectConfig)
      ).Returns(Task.CompletedTask);
      var addon1ConfigFile = new ConfigFile(
        addons: new Dictionary<string, AddonConfig>(),
        cachePath: null,
        addonsPath: null
      );
      configFileRepo.Setup(
        repo => repo.LoadOrCreateConfigFile("/addons/addon1")
      ).Returns(addon1ConfigFile);

      // Addon 2 installation calls
      dependencyGraph.Setup(dg => dg.Add(addon2)).Returns(
        new DependencyCanBeInstalledEvent(addon2)
      );
      reporter.Setup(
        r => r.Handle(new DependencyCanBeInstalledEvent(addon2))
      );
      reporter.Setup(
        r => r.Handle(new AddonInstalledEvent(addon2))
      );
      addonRepo.Setup(ar => ar.CacheAddon(addon2, projectConfig)).Returns(
        Task.CompletedTask
      );
      addonRepo.Setup(ar => ar.DeleteAddon(addon2, projectConfig)).Returns(
        Task.CompletedTask
      );
      addonRepo.Setup(
        ar => ar.CopyAddonFromCache(addon2, projectConfig)
      ).Returns(Task.CompletedTask);
      var addon2ConfigFile = new ConfigFile(
        addons: new Dictionary<string, AddonConfig>(),
        cachePath: null,
        addonsPath: null
      );
      configFileRepo.Setup(
        repo => repo.LoadOrCreateConfigFile("/addons/addon2")
      ).Returns(addon2ConfigFile);

      await manager.InstallAddons(projectPath);
    }

    [Fact]
    public async Task InstallAddonsReportsFailedInstallationEvents() {
      var projectPath = "/";
      var addonRepo = new Mock<IAddonRepo>(MockBehavior.Strict);
      var configFileRepo = new Mock<IConfigFileRepo>(MockBehavior.Strict);
      var reporter = new Mock<IReporter>(MockBehavior.Strict);
      var dependencyGraph = new Mock<IDependencyGraph>(MockBehavior.Strict);

      var addon = new RequiredAddon(
          name: "addon1",
          configFilePath: "/addons.json",
          url: "/some/local/path",
          checkout: "main",
          subfolder: "",
          source: AddonSource.Local
        );

      var manager = new AddonManager(
        addonRepo: addonRepo.Object,
        configFileRepo: configFileRepo.Object,
        reporter: reporter.Object,
        dependencyGraph: dependencyGraph.Object
      );

      var projectConfigFile = new ConfigFile(
        addons: new Dictionary<string, AddonConfig>() {
          { addon.Name, new AddonConfig(
            url: addon.Url,
            checkout: addon.Checkout,
            subfolder: null,
            source: addon.Source
          )},
        },
        cachePath: ".addons",
        addonsPath: "addons"
      );

      var projectConfig = projectConfigFile.ToConfig(projectPath);

      configFileRepo.Setup(repo => repo.LoadOrCreateConfigFile(projectPath))
        .Returns(projectConfigFile);

      addonRepo.Setup(repo => repo.LoadCache(projectConfig)).Returns(
        Task.FromResult(new Dictionary<string, string>() { })
      );

      dependencyGraph.Setup(dg => dg.Add(addon)).Returns(
        new DependencyCanBeInstalledEvent(addon)
      );
      reporter.Setup(
        r => r.Handle(new DependencyCanBeInstalledEvent(addon))
      );
      reporter.Setup(
        r => r.Handle(
         It.IsAny<AddonFailedToInstallEvent>()
        )
      );
      addonRepo.Setup(ar => ar.CacheAddon(addon, projectConfig)).Returns(
        Task.CompletedTask
      );
      addonRepo.Setup(repo => repo.IsDirectorySymlink(projectPath)).Returns(false);
      addonRepo.Setup(ar => ar.DeleteAddon(addon, projectConfig)).Returns(
        Task.CompletedTask
      );

      // Make addon fail to install at last step
      addonRepo.Setup(
        ar => ar.CopyAddonFromCache(addon, projectConfig)
      ).Throws<InvalidOperationException>();

      var addonConfigFile = new ConfigFile(
        addons: new Dictionary<string, AddonConfig>(),
        cachePath: null,
        addonsPath: null
      );

      configFileRepo.Setup(
        repo => repo.LoadOrCreateConfigFile("/addons/addon1")
      ).Returns(addonConfigFile);


      await manager.InstallAddons(projectPath);
    }


    [Fact]
    public async Task InstallAddonKnowsHowToInstallSymlinkAddon() {
      var addonRepo = new Mock<IAddonRepo>();
      var configFileRepo = new Mock<IConfigFileRepo>();
      var reporter = new Mock<IReporter>();
      var dependencyGraph = new Mock<IDependencyGraph>();

      var manager = new AddonManager(
        addonRepo: addonRepo.Object,
        configFileRepo: configFileRepo.Object,
        reporter: reporter.Object,
        dependencyGraph: dependencyGraph.Object
      );

      var addon = new RequiredAddon(
        name: "addon1",
        configFilePath: "/addons.json",
        url: "http://example.com/addon1.git",
        checkout: "main",
        subfolder: "addon1",
        source: AddonSource.Symlink
      );

      var projectPath = "/";

      var projectConfigFile = new ConfigFile(
        addons: new Dictionary<string, AddonConfig>() {
          { "addon1", new AddonConfig(
            url: "http://example.com/addon1.git",
            checkout: "main",
            subfolder: "addon1",
            source: AddonSource.Symlink
          )}
        },
        cachePath: ".addons",
        addonsPath: "addons"
      );

      var projectConfig = projectConfigFile.ToConfig(projectPath);

      addonRepo.Setup(repo => repo.DeleteAddon(addon, projectConfig)).Returns(
        Task.CompletedTask
      );
      addonRepo.Setup(
        repo => repo.InstallAddonWithSymlink(addon, projectConfig)
      );

      await manager.InstallAddon(addon, projectConfig);

      addonRepo.VerifyAll();
    }

    [Fact]
    public void ResolveUrlDoesNothingForRemoteAddons() {
      var url = "http://example.com/addon1.git";
      var path = "/volume/directory";
      var addonConfig = new AddonConfig(
        url: url
      );

      var addonRepo = new Mock<IAddonRepo>();
      var configFileRepo = new Mock<IConfigFileRepo>();
      var reporter = new Mock<IReporter>();
      var dependencyGraph = new Mock<IDependencyGraph>();

      var addonManager = new AddonManager(
        addonRepo: addonRepo.Object,
        configFileRepo: configFileRepo.Object,
        reporter: reporter.Object,
        dependencyGraph: dependencyGraph.Object
      );
      addonManager.ResolveUrl(addonConfig, path).ShouldBe(url);
    }

    [Fact]
    public void ResolveUrlResolvesNonRootedPath() {
      var url = "../some/relative/path";
      var path = "/volume/old/directory";
      var resolved = "/volume/other/directory";
      var addonConfig = new AddonConfig(
        url: url,
        source: AddonSource.Local
      );

      var addonRepo = new Mock<IAddonRepo>(MockBehavior.Strict);
      var configFileRepo = new Mock<IConfigFileRepo>();
      var reporter = new Mock<IReporter>();
      var dependencyGraph = new Mock<IDependencyGraph>();

      addonRepo.Setup(repo => repo.IsDirectorySymlink(path)).Returns(true);
      addonRepo.Setup(repo => repo.DirectorySymlinkTarget(path))
        .Returns(resolved);

      var addonManager = new AddonManager(
        addonRepo: addonRepo.Object,
        configFileRepo: configFileRepo.Object,
        reporter: reporter.Object,
        dependencyGraph: dependencyGraph.Object
      );

      addonManager.ResolveUrl(addonConfig, path)
        .ShouldBe("/volume/other/some/relative/path");
    }

    [Fact]
    public void ResolveUrlDoesNotResolveRootedPath() {
      var url = "/volume2/some/path";
      var path = "/volume/directory";
      var addonConfig = new AddonConfig(
        url: url,
        source: AddonSource.Local
      );

      var addonRepo = new Mock<IAddonRepo>();
      var configFileRepo = new Mock<IConfigFileRepo>();
      var reporter = new Mock<IReporter>();
      var dependencyGraph = new Mock<IDependencyGraph>();

      addonRepo.Setup(repo => repo.IsDirectorySymlink(path)).Returns(false);
      addonRepo.Setup(repo => repo.DirectorySymlinkTarget(path)).Returns(path);

      var addonManager = new AddonManager(
        addonRepo: addonRepo.Object,
        configFileRepo: configFileRepo.Object,
        reporter: reporter.Object,
        dependencyGraph: dependencyGraph.Object
      );

      addonManager.ResolveUrl(addonConfig, path).ShouldBe(url);
    }

    [Fact]
    public void CanGoOn() {
      AddonManager.CanGoOn(0, 1, 10).ShouldBeFalse();
      AddonManager.CanGoOn(1, 1, 10).ShouldBeTrue();
      AddonManager.CanGoOn(1, 1, 1).ShouldBeFalse();
      AddonManager.CanGoOn(1, 1, 2).ShouldBeTrue();
      AddonManager.CanGoOn(1, 100, null).ShouldBeTrue();
      AddonManager.CanGoOn(0, 100, null).ShouldBeFalse();
    }
  }
}
