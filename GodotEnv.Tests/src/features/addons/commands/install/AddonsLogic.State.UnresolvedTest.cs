namespace Chickensoft.GodotEnv.Tests;

using System.Collections.Generic;
using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Features.Addons.Commands;
using Chickensoft.GodotEnv.Features.Addons.Domain;
using Chickensoft.GodotEnv.Features.Addons.Models;
using Moq;
using Shouldly;
using Xunit;

public class UnresolvedTest {
  [Fact]
  public void CanGoOn() =>
    AddonsLogic.State.Unresolved.CanGoOn(false, 1, 1, 1).ShouldBeFalse();

  [Fact]
  public async Task DoesNothingIfNothingToInstall() {
    var addonsRepo = new Mock<IAddonsRepository>();
    var addonsFileRepo = new Mock<IAddonsFileRepository>();
    var addonGraph = new Mock<IAddonGraph>();

    var projectPath = "/";
    var input = new AddonsLogic.Input.Install(
      ProjectPath: projectPath, MaxDepth: null
    );

    var addonsFile = new AddonsFile(
      addons: new() { },
      cacheRelativePath: ".addons",
      pathRelativePath: "addons"
    );

    string addonsFilePath;
    addonsRepo.Setup(repo => repo.EnsureCacheAndAddonsDirectoriesExists());

    addonsFileRepo.Setup(repo => repo.LoadAddonsFile(
      projectPath, out addonsFilePath, null
    )).Returns(addonsFile);

    var state = new AddonsLogic.State.Unresolved();
    var context = state.CreateFakeContext();

    context.Set(addonsRepo.Object);
    context.Set(addonsFileRepo.Object);
    context.Set(addonGraph.Object);

    var nextState = await state.On(input);

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
    var input = new AddonsLogic.Input.Install(
      ProjectPath: projectPath, MaxDepth: null
    );

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

    var state = new AddonsLogic.State.Unresolved();
    var context = state.CreateFakeContext();
    context.Set(addonsRepo.Object);
    context.Set(addonsFileRepo.Object);
    context.Set(addonGraph.Object);

    var nextState = await state.On(input);

    nextState.ShouldBeOfType<AddonsLogic.State.CannotBeResolved>();

    addonsRepo.VerifyAll();
    addonsFileRepo.VerifyAll();
    addonGraph.VerifyAll();
  }

  [Fact]
  public async Task DeterminesCanonicalAddonCorrectly() {
    var addonsRepo = new Mock<IAddonsRepository>();
    var addonsFileRepo = new Mock<IAddonsFileRepository>();
    var addonGraph = new Mock<IAddonGraph>();

    var projectPath = "/";
    var input = new AddonsLogic.Input.Install(
      ProjectPath: projectPath, MaxDepth: null
    );

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
      Conflicts: new List<IAddon> { otherAddon },
      CanonicalAddon: otherAddon
    );

    addonGraph.Setup(graph => graph.Add(addon))
      .Returns(graphResult);

    var cacheName = otherAddon.Name;
    var pathToCachedAddon = "/.addons/other_addon";

    addonsRepo
      .Setup(repo => repo.CacheAddon(addon, cacheName))
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

    var state = new AddonsLogic.State.Unresolved();

    var context = state.CreateFakeContext();
    context.Set(addonsRepo.Object);
    context.Set(addonsFileRepo.Object);
    context.Set(addonGraph.Object);

    var nextState = await state.On(input);

    nextState.ShouldBeOfType<AddonsLogic.State.InstallationSucceeded>();

    addonsRepo.VerifyAll();
    addonsFileRepo.VerifyAll();
    addonGraph.VerifyAll();
  }

  [Fact]
  public async Task InstallsSymlinkAddon() {
    var addonsRepo = new Mock<IAddonsRepository>();
    var addonsFileRepo = new Mock<IAddonsFileRepository>();
    var addonGraph = new Mock<IAddonGraph>();

    var projectPath = "/";
    var input = new AddonsLogic.Input.Install(
      ProjectPath: projectPath, MaxDepth: null
    );

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
    addonsRepo.Setup(repo => repo.CacheAddon(addon, addon.Name))
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

    var state = new AddonsLogic.State.Unresolved();
    var context = state.CreateFakeContext();
    context.Set(addonsRepo.Object);
    context.Set(addonsFileRepo.Object);
    context.Set(addonGraph.Object);

    var nextState = await state.On(input);

    nextState.ShouldBeOfType<AddonsLogic.State.InstallationSucceeded>();

    addonsRepo.VerifyAll();
    addonsFileRepo.VerifyAll();
    addonGraph.VerifyAll();
  }
}
