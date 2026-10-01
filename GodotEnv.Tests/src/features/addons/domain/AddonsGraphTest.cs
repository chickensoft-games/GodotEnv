namespace Chickensoft.GodotEnv.Tests;

using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Features.Addons.Models;
using Shouldly;
using Xunit;

public class AddonsGraphTest
{
  private readonly Addon _addonA = new(
    name: "AddonA",
    addonsFilePath: "some/working/dir/addons.json",
    url: "https://user/repo.git",
    subfolder: "/",
    checkout: "main",
    source: AssetSource.Local
  );

  private readonly Addon _addonB = new(
    name: "AddonB",
    addonsFilePath: "some/working/dir/addons.json",
    url: "https://user/repo2.git",
    subfolder: "/",
    checkout: "main",
    source: AssetSource.Local
  );

  private readonly Addon _addonAEquivalent = new(
    name: "AlsoAddonA",
    addonsFilePath: "some/working/dir/addons.json",
    url: "https://user/repo.git",
    subfolder: "/",
    checkout: "main",
    source: AssetSource.Local
  );

  private readonly Addon _addonASubfolder = new(
    name: "AlsoAddonA",
    addonsFilePath: "some/working/dir/addons.json",
    url: "https://user/repo.git",
    subfolder: "different/",
    checkout: "main",
    source: AssetSource.Local
  );

  private readonly Addon _addonACheckout = new(
    name: "YetAnotherAddonA",
    addonsFilePath: "some/working/dir/addons.json",
    url: "https://user/repo.git",
    subfolder: "/",
    checkout: "dev",
    source: AssetSource.Local
  );

  private readonly Addon _addonAConflictingPath = new(
    name: "AddonA",
    addonsFilePath: "some/working/dir/addons.json",
    url: "https://user/a-different-repo.git",
    subfolder: "/",
    checkout: "main",
    source: AssetSource.Local
  );

  [Fact]
  public void IndicatesADependencyIsInstalled()
  {
    var graph = new AddonGraph();
    graph.Add(_addonA).ShouldBeOfType<AddonResolved>();
    graph.Addons.ShouldBe([_addonA]);
  }

  [Fact]
  public void IndicatesNonConflictingDependenciesAreInstalled()
  {
    var graph = new AddonGraph();
    graph.Add(_addonA).ShouldBeOfType<AddonResolved>();
    graph.Add(_addonB).ShouldBeOfType<AddonResolved>();
  }

  [Fact]
  public void IndicatesADependencyIsAlreadyInstalled()
  {
    var graph = new AddonGraph();
    graph.Add(_addonA).ShouldBeOfType<AddonResolved>();
    graph.Add(_addonA).ShouldBeOfType<AddonAlreadyResolved>();
  }

  [Fact]
  public void IndicatesAConflictingDestinationPath()
  {
    var graph = new AddonGraph();
    graph.Add(_addonA).ShouldBeOfType<AddonResolved>();
    graph.Add(_addonAConflictingPath).ShouldBeOfType<AddonCannotBeResolved>();
  }

  [Fact]
  public void IndicatesADependencyIsInstalledUnderDifferentName()
  {
    var graph = new AddonGraph();
    graph.Add(_addonA).ShouldBeOfType<AddonResolved>();
    graph.Add(_addonAEquivalent).ShouldBeOfType<AddonAlreadyResolved>();
  }

  [Fact]
  public void IssuesAddonSimilarWarningForSubfolderChange()
  {
    var graph = new AddonGraph();
    graph.Add(_addonA).ShouldBeOfType<AddonResolved>();
    var e = graph.Add(_addonASubfolder);
    e.ShouldBeOfType<AddonResolvedButMightConflict>();
    var warning = e.ShouldBeOfType<AddonResolvedButMightConflict>();
    warning.Addon.ShouldBe(_addonASubfolder);
    warning.Conflicts.ShouldContain(_addonA);
  }

  [Fact]
  public void IssuesAddonSimilarWarningForCheckoutChange()
  {
    var graph = new AddonGraph();
    graph.Add(_addonA).ShouldBeOfType<AddonResolved>();
    var e = graph.Add(_addonACheckout);
    e.ShouldBeOfType<AddonResolvedButMightConflict>();
    var warning = e.ShouldBeOfType<AddonResolvedButMightConflict>();
    warning.Addon.ShouldBe(_addonACheckout);
    warning.Conflicts.ShouldContain(_addonA);
  }

  [Fact]
  public void IssuesAddonSimilarWarningForMultiplePossibleConflicts()
  {
    var graph = new AddonGraph();
    graph.Add(_addonA).ShouldBeOfType<AddonResolved>();
    graph.Add(_addonB).ShouldBeOfType<AddonResolved>();
    graph.Add(_addonACheckout).ShouldBeOfType<AddonResolvedButMightConflict>();
    var e = graph.Add(_addonASubfolder);
    var warning = e.ShouldBeOfType<AddonResolvedButMightConflict>();
    warning.Addon.ShouldBe(_addonASubfolder);
    warning.Conflicts.ShouldContain(_addonA);
    warning.Conflicts.ShouldContain(_addonACheckout);
  }
}
