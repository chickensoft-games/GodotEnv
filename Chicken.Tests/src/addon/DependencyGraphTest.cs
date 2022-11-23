namespace Chickensoft.Chicken.Tests;
using Chickensoft.Chicken;
using Shouldly;
using Xunit;


public class DependencyGraphTest {

  private readonly RequiredAddon _addonA = new(
    name: "AddonA",
    configFilePath: "some/working/dir/addons.json",
    url: "https://user/repo.git",
    checkout: "main",
    subfolder: "/"
  );

  private readonly RequiredAddon _addonB = new(
    name: "AddonB",
    configFilePath: "some/working/dir/addons.json",
    url: "https://user/repo2.git",
    checkout: "main",
    subfolder: "/"
  );

  private readonly RequiredAddon _addonAEquivalent = new(
    name: "AlsoAddonA",
    configFilePath: "some/working/dir/addons.json",
    url: "https://user/repo.git",
    checkout: "main",
    subfolder: "/"
  );

  private readonly RequiredAddon _addonASubfolder = new(
    name: "AlsoAddonA",
    configFilePath: "some/working/dir/addons.json",
    url: "https://user/repo.git",
    checkout: "main",
    subfolder: "different/"
  );

  private readonly RequiredAddon _addonACheckout = new(
    name: "YetAnotherAddonA",
    configFilePath: "some/working/dir/addons.json",
    url: "https://user/repo.git",
    checkout: "dev",
    subfolder: "/"
  );

  private readonly RequiredAddon _addonAConflictingPath = new(
    name: "AddonA",
    configFilePath: "some/working/dir/addons.json",
    url: "https://user/a-different-repo.git",
    checkout: "main",
    subfolder: "/"
  );

  [Fact]
  public void IndicatesADependencyIsInstalled() {
    var graph = new DependencyGraph();
    graph.Add(_addonA).ShouldBeOfType(typeof(DependencyCanBeInstalledEvent));
  }

  [Fact]
  public void IndicatesNonConflictingDependenciesAreInstalled() {
    var graph = new DependencyGraph();
    graph.Add(_addonA).ShouldBeOfType(typeof(DependencyCanBeInstalledEvent));
    graph.Add(_addonB).ShouldBeOfType(typeof(DependencyCanBeInstalledEvent));
  }

  [Fact]
  public void IndicatesADependencyIsAlreadyInstalled() {
    var graph = new DependencyGraph();
    graph.Add(_addonA).ShouldBeOfType(typeof(DependencyCanBeInstalledEvent));
    graph.Add(_addonA).ShouldBeOfType(typeof(DependencyAlreadyInstalledEvent));
  }

  [Fact]
  public void IndicatesAConflictingDestinationPath() {
    var graph = new DependencyGraph();
    graph.Add(_addonA).ShouldBeOfType(typeof(DependencyCanBeInstalledEvent));
    graph.Add(_addonAConflictingPath).ShouldBeOfType(
      typeof(ConflictingDestinationPathEvent)
    );
  }

  [Fact]
  public void IndicatesADependencyIsInstalledUnderDifferentName() {
    var graph = new DependencyGraph();
    graph.Add(_addonA).ShouldBeOfType(typeof(DependencyCanBeInstalledEvent));
    graph.Add(_addonAEquivalent).ShouldBeOfType(
      typeof(DependencyAlreadyInstalledEvent)
    );
  }

  [Fact]
  public void IssuesSimilarDependencyWarningForSubfolderChange() {
    var graph = new DependencyGraph();
    graph.Add(_addonA).ShouldBeOfType(typeof(DependencyCanBeInstalledEvent));
    var e = graph.Add(_addonASubfolder);
    e.ShouldBeOfType(
      typeof(SimilarDependencyWarning)
    );
    var warning = e.ShouldBeOfType<SimilarDependencyWarning>();
    warning.Conflict.ShouldBe(_addonASubfolder);
    warning.Addons.ShouldContain(_addonA);
  }

  [Fact]
  public void IssuesSimilarDependencyWarningForCheckoutChange() {
    var graph = new DependencyGraph();
    graph.Add(_addonA).ShouldBeOfType(typeof(DependencyCanBeInstalledEvent));
    var e = graph.Add(_addonACheckout);
    e.ShouldBeOfType(
      typeof(SimilarDependencyWarning)
    );
    var warning = e.ShouldBeOfType<SimilarDependencyWarning>();
    warning.Conflict.ShouldBe(_addonACheckout);
    warning.Addons.ShouldContain(_addonA);
  }

  [Fact]
  public void IssuesSimilarDependencyWarningForMultiplePossibleConflicts() {
    var graph = new DependencyGraph();
    graph.Add(_addonA).ShouldBeOfType(typeof(DependencyCanBeInstalledEvent));
    graph.Add(_addonB).ShouldBeOfType(typeof(DependencyCanBeInstalledEvent));
    graph.Add(_addonACheckout).ShouldBeOfType(
      typeof(SimilarDependencyWarning)
    );
    var e = graph.Add(_addonASubfolder);
    var warning = e.ShouldBeOfType<SimilarDependencyWarning>();
    warning.Conflict.ShouldBe(_addonASubfolder);
    warning.Addons.ShouldContain(_addonA);
    warning.Addons.ShouldContain(_addonACheckout);
  }
}
