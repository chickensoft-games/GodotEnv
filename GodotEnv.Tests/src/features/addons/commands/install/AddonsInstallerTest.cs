namespace Chickensoft.GodotEnv.Tests;

using System;
using System.Threading;
using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Features.Addons.Commands;
using Chickensoft.GodotEnv.Features.Addons.Domain;
using Chickensoft.GodotEnv.Features.Addons.Models;
using Moq;
using Shouldly;
using Xunit;

public class AddonsInstallerTest {
  [Fact]
  public void CanGoOn() =>
    AddonsInstaller.CanGoOn(false, 1, 1, 1).ShouldBeFalse();

  [Fact]
  public async Task DoesNothingIfNothingToInstall() {
    var addonsRepo = new Mock<IAddonsRepository>();
    var addonsFileRepo = new Mock<IAddonsFileRepository>();
    var addonGraph = new Mock<IAddonGraph>();

    var projectPath = "/";

    var addonsFile = new AddonsFile(
      addons: [],
      cacheRelativePath: ".addons",
      pathRelativePath: "addons"
    );

    string addonsFilePath;
    addonsRepo.Setup(repo => repo.EnsureCacheAndAddonsDirectoriesExists());

    addonsFileRepo.Setup(repo => repo.LoadAddonsFile(
      projectPath, out addonsFilePath, null
    )).Returns(addonsFile);

    var installer = new AddonsInstaller(
      addonsFileRepo.Object, addonsRepo.Object, addonGraph.Object
    );

    var token = new CancellationToken();

    var result = await installer.Install(
      projectPath,
      null,
      (@event) => { },
      (addon, progress) => { },
      (addon, progress) => { },
      token
    );

    result.ShouldBe(AddonsInstaller.Result.NothingToInstall);

    addonsRepo.VerifyAll();
    addonsFileRepo.VerifyAll();
    addonGraph.VerifyAll();
  }

  [Fact]
  public async Task EndsInCannotBeResolvedIfFatalErrorEncountered() {
    var addonsRepo = new Mock<IAddonsRepository>();
    var addonsFileRepo = new Mock<IAddonsFileRepository>();
    var addonGraph = new Mock<IAddonGraph>();

    var projectPath = "/";

    var entry = new AddonsFileEntry(
      url: "https://github.com/chickensoft-games/addon"
    );

    var addonsFile = new AddonsFile(
      addons: new() {
        ["addon"] = entry
      },
      cacheRelativePath: ".addons",
      pathRelativePath: "addons"
    );

    var addonsFilePath = "";
    addonsRepo.Setup(repo => repo.EnsureCacheAndAddonsDirectoriesExists());

    addonsFileRepo.Setup(repo => repo.LoadAddonsFile(
      projectPath, out addonsFilePath, null
    )).Returns(addonsFile);

    addonsRepo.Setup(repo => repo.ResolveUrl(entry, addonsFilePath))
      .Returns(entry.Url);

    var addon = entry.ToAddon(
      name: "addon", resolvedUrl: entry.Url, addonsFilePath: addonsFilePath
    );

    var graphResult = new AddonCannotBeResolved(addon, addon);

    addonGraph.Setup(graph => graph.Add(addon))
      .Returns(graphResult);

    var installer = new AddonsInstaller(
      addonsFileRepo.Object, addonsRepo.Object, addonGraph.Object
    );

    var token = new CancellationToken();

    var result = await installer.Install(
      projectPath,
      null,
      (@event) => { },
      (addon, progress) => { },
      (addon, progress) => { },
      token
    );

    result.ShouldBe(AddonsInstaller.Result.CannotBeResolved);

    addonsRepo.VerifyAll();
    addonsFileRepo.VerifyAll();
    addonGraph.VerifyAll();
  }

  [Fact]
  public async Task DeterminesCanonicalAddonCorrectly() {
    var token = new CancellationToken();
    var addonsRepo = new Mock<IAddonsRepository>();
    var addonsFileRepo = new Mock<IAddonsFileRepository>();
    var addonGraph = new Mock<IAddonGraph>();

    var projectPath = "/";

    var entry = new AddonsFileEntry(
      url: "https://github.com/chickensoft-games/addon"
    );

    var addonsFile = new AddonsFile(
      addons: new() {
        ["addon"] = entry
      },
      cacheRelativePath: ".addons",
      pathRelativePath: "addons"
    );

    var addonsFilePath = "";
    addonsRepo.Setup(repo => repo.EnsureCacheAndAddonsDirectoriesExists());

    addonsFileRepo.Setup(repo => repo.LoadAddonsFile(
      projectPath, out addonsFilePath, null
    )).Returns(addonsFile);

    addonsRepo.Setup(repo => repo.ResolveUrl(entry, addonsFilePath))
      .Returns(entry.Url);

    var addon = entry.ToAddon(
      name: "addon", resolvedUrl: entry.Url, addonsFilePath: addonsFilePath
    );

    var otherAddon = new Addon(
      name: "other_addon",
      addonsFilePath: "addons.json",
      url: "https://github.com/chickensoft-games/addon",
      subfolder: "other",
      checkout: "main",
      source: AssetSource.Remote
    );

    var otherAddonAddonsFile = new AddonsFile();

    var graphResult = new AddonResolvedButMightConflict(
      Addon: addon,
      Conflicts: [otherAddon],
      CanonicalAddon: otherAddon
    );

    addonGraph.Setup(graph => graph.Add(addon))
      .Returns(graphResult);

    var cacheName = otherAddon.Name;
    var pathToCachedAddon = "/.addons/other_addon";

    addonsRepo
      .Setup(
        repo => repo.CacheAddon(
          addon,
          cacheName,
          It.IsAny<IProgress<DownloadProgress>>(),
          It.IsAny<IProgress<double>>(),
          token
        )
      )
    .Returns(Task.FromResult(pathToCachedAddon));

    addonsRepo
      .Setup(repo => repo.PrepareCache(addon, cacheName))
      .Returns(Task.CompletedTask);

    addonsRepo
      .Setup(repo => repo.UpdateCache(addon, cacheName))
      .Returns(Task.CompletedTask);

    var otherAddonsFilePath = "";
    addonsFileRepo
      .Setup(repo => repo.LoadAddonsFile(
        pathToCachedAddon, out otherAddonsFilePath, null
      ))
      .Returns(otherAddonAddonsFile);

    var installer = new AddonsInstaller(
      addonsFileRepo.Object, addonsRepo.Object, addonGraph.Object
    );

    var result = await installer.Install(
      projectPath,
      null,
      (@event) => { },
      (addon, progress) => { },
      (addon, progress) => { },
      token
    );

    result.ShouldBe(AddonsInstaller.Result.Succeeded);

    addonsRepo.VerifyAll();
    addonsFileRepo.VerifyAll();
    addonGraph.VerifyAll();
  }

  [Fact]
  public async Task InstallsSymlinkAddon() {
    var token = new CancellationToken();
    var addonsRepo = new Mock<IAddonsRepository>();
    var addonsFileRepo = new Mock<IAddonsFileRepository>();
    var addonGraph = new Mock<IAddonGraph>();

    var projectPath = "/";


    var entry = new AddonsFileEntry(
      url: "/symlink_addon/addon",
      source: AssetSource.Symlink
    );

    var addonsFile = new AddonsFile(
      addons: new() {
        ["addon"] = entry
      },
      cacheRelativePath: ".addons",
      pathRelativePath: "addons"
    );

    var addonsFilePath = "";
    addonsRepo.Setup(repo => repo.EnsureCacheAndAddonsDirectoriesExists());

    addonsFileRepo.Setup(repo => repo.LoadAddonsFile(
      projectPath, out addonsFilePath, null
    )).Returns(addonsFile);

    addonsRepo.Setup(repo => repo.ResolveUrl(entry, addonsFilePath))
      .Returns(entry.Url);

    var addon = entry.ToAddon(
      name: "addon", resolvedUrl: entry.Url, addonsFilePath: addonsFilePath
    );

    var graphResult = new AddonResolved(addon);

    addonGraph.Setup(graph => graph.Add(addon))
      .Returns(graphResult);

    var cacheName = addon.Name;
    var pathToCachedAddon = "/.addons/addon";
    addonsRepo.Setup(
      repo => repo.CacheAddon(
        addon,
        addon.Name,
        It.IsAny<IProgress<DownloadProgress>>(),
        It.IsAny<IProgress<double>>(),
        token
      )
    )
    .Returns(Task.FromResult(pathToCachedAddon));

    var addonsAddonsFilePath = "";
    addonsFileRepo
      .Setup(repo => repo.LoadAddonsFile(
        pathToCachedAddon, out addonsAddonsFilePath, null
      )
    ).Returns(new AddonsFile());

    addonsRepo
      .Setup(repo => repo.DeleteAddon(addon))
      .Returns(Task.CompletedTask);

    addonsRepo
      .Setup(repo => repo.InstallAddonWithSymlink(It.IsAny<IAddon>()));

    var installer = new AddonsInstaller(
      addonsFileRepo.Object, addonsRepo.Object, addonGraph.Object
    );

    var result = await installer.Install(
      projectPath,
      null,
      (@event) => { },
      (addon, progress) => { },
      (addon, progress) => { },
      token
    );

    result.ShouldBe(AddonsInstaller.Result.Succeeded);

    addonsRepo.VerifyAll();
    addonsFileRepo.VerifyAll();
    addonGraph.VerifyAll();
  }
}
