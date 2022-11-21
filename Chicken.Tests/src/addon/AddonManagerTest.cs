namespace Chickensoft.Chicken.Tests;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Moq;
using Shouldly;
using Xunit;

public class AddonManagerTest {
  [Fact]
  public async Task InstallsAddonsInProject() {
    var app = new Mock<IApp>(MockBehavior.Strict);
    var fs = new Mock<IFileSystem>(MockBehavior.Strict);
    var projectPath = "/";
    var addonRepo = new Mock<IAddonRepo>(MockBehavior.Strict);
    var configFileLoader = new Mock<IConfigFileLoader>(MockBehavior.Strict);
    var log = new Mock<ILog>(MockBehavior.Strict);
    var dependencyGraph = new Mock<IDependencyGraph>(MockBehavior.Strict);
    var copier = new Mock<IFileCopier>(MockBehavior.Strict);

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
      fs: fs.Object,
      addonRepo: addonRepo.Object,
      configFileLoader: configFileLoader.Object,
      log: log.Object,
      dependencyGraph: dependencyGraph.Object
    );

    var addon1Config = new AddonConfig(
      url: "http://example.com/addon1.git",
      checkout: "master",
      subfolder: "addon1"
    );

    var addon2Config = new AddonConfig(
      url: "http://example.com/addon2.git",
      checkout: "master",
      subfolder: "addon2"
    );

    var projectConfigFile = new ConfigFile(
      addons: new Dictionary<string, AddonConfig>() {
        { "addon1", addon1Config},
        { "addon2", addon2Config},
      },
      cachePath: ".addons",
      addonsPath: "addons"
    );

    var projectConfig = projectConfigFile.ToConfig(projectPath);

    configFileLoader.Setup(repo => repo.Load(projectPath))
      .Returns(projectConfigFile);

    addonRepo.Setup(repo => repo.LoadCache(projectConfig)).Returns(
      Task.FromResult(new Dictionary<string, string>() {
        { "http://example.com/addon1.git", "addon1" }
      })
    );
    app.Setup(
      app => app.FileThatExists(fs.Object, "/", App.ADDONS_CONFIG_FILES)
    ).Returns("/addons.json");

    // Addon 1 installation calls
    dependencyGraph.Setup(dg => dg.Add(addon1)).Returns(
      new DependencyCanBeInstalledEvent(addon1)
    );
    log.Setup(
      log => log.Info(new DependencyCanBeInstalledEvent(addon1).ToString())
    );
    log.Setup(
      log => log.Success(new AddonInstalledEvent(addon1).ToString())
    );
    addonRepo.Setup(ar => ar.CacheAddon(addon1, projectConfig)).Returns(
      Task.CompletedTask
    );
    addonRepo.Setup(ar => ar.DeleteAddon(addon1, projectConfig)).Returns(
      Task.CompletedTask
    );
    addonRepo.Setup(
      ar => ar.CopyAddonFromCache(addon1, projectConfig, copier.Object)
    ).Returns(Task.CompletedTask);
    app.Setup(
      app => app.FileThatExists(fs.Object, "/addons/addon1", App.ADDONS_CONFIG_FILES)
    ).Returns("/addons/addon1/addons.json");
    var addon1ConfigFile = new ConfigFile(
      addons: new Dictionary<string, AddonConfig>(),
      cachePath: null,
      addonsPath: null
    );
    app.Setup(app => app.ResolveUrl(fs.Object, addon1Config, projectPath))
      .Returns(addon1.Url);
    configFileLoader.Setup(
      repo => repo.Load("/addons/addon1")
    ).Returns(addon1ConfigFile);

    // Addon 2 installation calls
    dependencyGraph.Setup(dg => dg.Add(addon2)).Returns(
      new DependencyCanBeInstalledEvent(addon2)
    );
    log.Setup(
      log => log.Info(new DependencyCanBeInstalledEvent(addon2).ToString())
    );
    log.Setup(
      log => log.Success(new AddonInstalledEvent(addon2).ToString())
    );
    addonRepo.Setup(ar => ar.CacheAddon(addon2, projectConfig)).Returns(
      Task.CompletedTask
    );
    addonRepo.Setup(ar => ar.DeleteAddon(addon2, projectConfig)).Returns(
      Task.CompletedTask
    );
    addonRepo.Setup(
      ar => ar.CopyAddonFromCache(addon2, projectConfig, copier.Object)
    ).Returns(Task.CompletedTask);
    app.Setup(
      app => app.FileThatExists(
        fs.Object, "/addons/addon2", App.ADDONS_CONFIG_FILES
      )
    ).Returns("/addons/addon2/addons.json");
    var addon2ConfigFile = new ConfigFile(
      addons: new Dictionary<string, AddonConfig>(),
      cachePath: null,
      addonsPath: null
    );
    app.Setup(app => app.ResolveUrl(fs.Object, addon2Config, projectPath))
      .Returns(addon2.Url);
    configFileLoader.Setup(
      repo => repo.Load("/addons/addon2")
    ).Returns(addon2ConfigFile);

    await manager.InstallAddons(app.Object, projectPath, copier.Object);
  }

  [Fact]
  public async Task InstallAddonsReportsFailedInstallationEvents() {
    var app = new Mock<IApp>(MockBehavior.Strict);
    var fs = new Mock<IFileSystem>(MockBehavior.Strict);
    var projectPath = "/";
    var addonRepo = new Mock<IAddonRepo>(MockBehavior.Strict);
    var configFileLoader = new Mock<IConfigFileLoader>(MockBehavior.Strict);
    var log = new Mock<ILog>(MockBehavior.Strict);
    var dependencyGraph = new Mock<IDependencyGraph>(MockBehavior.Strict);
    var copier = new Mock<IFileCopier>(MockBehavior.Strict);

    var addon = new RequiredAddon(
        name: "addon1",
        configFilePath: "/addons.json",
        url: "/some/local/path",
        checkout: "main",
        subfolder: "",
        source: RepositorySource.Local
      );

    var manager = new AddonManager(
      fs: fs.Object,
      addonRepo: addonRepo.Object,
      configFileLoader: configFileLoader.Object,
      log: log.Object,
      dependencyGraph: dependencyGraph.Object
    );

    var addonConfig = new AddonConfig(
      url: addon.Url,
      checkout: addon.Checkout,
      subfolder: null,
      source: addon.Source
    );

    var projectConfigFile = new ConfigFile(
      addons: new Dictionary<string, AddonConfig>() {
        { addon.Name, addonConfig},
      },
      cachePath: ".addons",
      addonsPath: "addons"
    );

    var projectConfig = projectConfigFile.ToConfig(projectPath);

    configFileLoader.Setup(repo => repo.Load(projectPath))
      .Returns(projectConfigFile);

    addonRepo.Setup(repo => repo.LoadCache(projectConfig)).Returns(
      Task.FromResult(new Dictionary<string, string>() { })
    );
    app.Setup(
      app => app.FileThatExists(fs.Object, "/", App.ADDONS_CONFIG_FILES)
    ).Returns("/addons.json");

    dependencyGraph.Setup(dg => dg.Add(addon)).Returns(
      new DependencyCanBeInstalledEvent(addon)
    );
    log.Setup(
      log => log.Info(new DependencyCanBeInstalledEvent(addon).ToString())
    );
    log.Setup(log => log.Err(It.IsAny<string>()));
    app.Setup(
      app => app.FileThatExists(
        fs.Object, "/addons/addon1", App.ADDONS_CONFIG_FILES
      )
    ).Returns("/addons/addon1/addons.json");
    addonRepo.Setup(ar => ar.CacheAddon(addon, projectConfig)).Returns(
      Task.CompletedTask
    );
    app.Setup(app => app.IsDirectorySymlink(
      fs.Object, projectPath
    )).Returns(false);
    addonRepo.Setup(ar => ar.DeleteAddon(addon, projectConfig)).Returns(
      Task.CompletedTask
    );

    // Make addon fail to install at last step
    addonRepo.Setup(
      ar => ar.CopyAddonFromCache(addon, projectConfig, copier.Object)
    ).Throws<InvalidOperationException>();

    var addonConfigFile = new ConfigFile(
      addons: new Dictionary<string, AddonConfig>(),
      cachePath: null,
      addonsPath: null
    );

    configFileLoader.Setup(
      repo => repo.Load("/addons/addon1")
    ).Returns(addonConfigFile);

    app.Setup(app => app.ResolveUrl(fs.Object, addonConfig, projectPath))
      .Returns(addon.Url);

    await manager.InstallAddons(app.Object, projectPath, copier.Object);
  }

  [Fact]
  public async Task InstallAddonKnowsHowToInstallSymlinkAddon() {
    var addonRepo = new Mock<IAddonRepo>();
    var fs = new Mock<IFileSystem>();
    var configFileLoader = new Mock<IConfigFileLoader>();
    var log = new Mock<ILog>();
    var dependencyGraph = new Mock<IDependencyGraph>();
    var copier = new Mock<IFileCopier>();

    var manager = new AddonManager(
      fs.Object,
      addonRepo: addonRepo.Object,
      configFileLoader: configFileLoader.Object,
      log: log.Object,
      dependencyGraph: dependencyGraph.Object
    );

    var addon = new RequiredAddon(
      name: "addon1",
      configFilePath: "/addons.json",
      url: "http://example.com/addon1.git",
      checkout: "main",
      subfolder: "addon1",
      source: RepositorySource.Symlink
    );

    var projectPath = "/";

    var projectConfigFile = new ConfigFile(
      addons: new Dictionary<string, AddonConfig>() {
        { "addon1", new AddonConfig(
          url: "http://example.com/addon1.git",
          checkout: "main",
          subfolder: "addon1",
          source: RepositorySource.Symlink
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

    await manager.InstallAddon(addon, projectConfig, copier.Object);

    addonRepo.VerifyAll();
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
