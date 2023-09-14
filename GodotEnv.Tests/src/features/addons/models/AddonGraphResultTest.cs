namespace Chickensoft.GodotEnv.Tests;
using System.Collections.Generic;
using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Common.Utilities;
using Chickensoft.GodotEnv.Features.Addons.Models;
using Moq;
using Shouldly;
using Xunit;

public class AddonGraphResultTest {
  private readonly Addon _canonical = new(
    name: "Canonical",
    addonsFilePath: "project/addons.json",
    url: "https://user/a.git",
    subfolder: "/",
    checkout: "main",
    source: AssetSource.Local
  );

  private readonly Addon _addonA = new(
    name: "A",
    addonsFilePath: "project/addons.json",
    url: "https://user/a.git",
    subfolder: "/",
    checkout: "main",
    source: AssetSource.Local
  );

  private readonly Addon _addonA2 = new(
    name: "A2",
    addonsFilePath: "project/addons.json",
    url: "https://user/a.git",
    subfolder: "/",
    checkout: "main",
    source: AssetSource.Local
  );

  private readonly Addon _addonAConflict = new(
    name: "A",
    addonsFilePath: "project/addons/other/addons.json",
    url: "https://user/a.git",
    subfolder: "/",
    checkout: "main",
    source: AssetSource.Local
  );

  private readonly Addon _addonASubfolder = new(
    name: "A2",
    addonsFilePath: "project/addons.json",
    url: "https://user/a.git",
    subfolder: "subfolder/",
    checkout: "main",
    source: AssetSource.Local
  );

  private readonly Addon _addonABranch = new(
    name: "A3",
    addonsFilePath: "project/addons.json",
    url: "https://user/a.git",
    subfolder: "/",
    checkout: "dev",
    source: AssetSource.Local
  );

  private readonly Addon _addonB = new(
    name: "B",
    addonsFilePath: "project/addons.json",
    url: "https://user/b.git",
    subfolder: "/",
    checkout: "main",
    source: AssetSource.Local
  );

  [Fact]
  public void AddonSimilarWarningDescribesUnrelatedDependencies() {
    var result = new AddonResolvedButMightConflict(
      Addon: _addonA,
      Conflicts: new List<IAddon> { _addonB },
      CanonicalAddon: _canonical
    );

    result.ToString().ShouldBe(
      "The addon \"A\" could conflict with a previously resolved addon.\n\n"
      + $"  Attempted to resolve {_addonA}\n\n"
    );

    var log = new Mock<ILog>();
    log.Setup(l => l.Warn(result.ToString()));
    result.Report(log.Object);
    log.VerifyAll();
  }

  [Fact]
  public void AddonSimilarWarningDescribesDifferentSubfolders() {
    var result = new AddonResolvedButMightConflict(
      Addon: _addonA,
      Conflicts: new List<IAddon> { _addonASubfolder },
      CanonicalAddon: _canonical
    );

    result.ToString().ShouldBe(
      "The addon \"A\" could conflict with a previously resolved addon.\n\n"
      + $"  Attempted to resolve {_addonA}\n\n" +
      "Both \"A\" and \"A2\" could potentially conflict with each other.\n" +
      "\n- Different subfolders from the same url are required.\n" +
      "    - \"A\" requires `/` from `https://user/a.git`\n" +
      "    - \"A2\" requires `subfolder/` from `https://user/a.git`"
    );

    var log = new Mock<ILog>();
    log.Setup(l => l.Warn(result.ToString()));
    result.Report(log.Object);
    log.VerifyAll();
  }

  [Fact]
  public void AddonSimilarWarningDescribesDifferentCheckouts() {
    var result = new AddonResolvedButMightConflict(
      Addon: _addonA,
      Conflicts: new List<IAddon> { _addonABranch },
      CanonicalAddon: _canonical
    );

    result.ToString().ShouldBe(
      "The addon \"A\" could conflict with a previously resolved addon.\n\n"
      + $"  Attempted to resolve {_addonA}\n\n" +
      "Both \"A\" and \"A3\" could potentially conflict with each other.\n" +
      "\n- Different checkouts from the same url are required.\n" +
      "    - \"A\" requires `main` from `https://user/a.git`\n" +
      "    - \"A3\" requires `dev` from `https://user/a.git`"
    );

    var log = new Mock<ILog>();
    log.Setup(l => l.Warn(result.ToString()));
    result.Report(log.Object);
    log.VerifyAll();
  }

  [Fact]
  public void AddonSimilarWarningDescribesMultipleConflicts() {
    var result = new AddonResolvedButMightConflict(
      Addon: _addonA,
      Conflicts: new List<IAddon> { _addonASubfolder, _addonABranch },
      CanonicalAddon: _canonical
    );

    result.ToString().ShouldBe(
      "The addon \"A\" could conflict with the previously resolved addons." +
      "\n\n" +
      $"  Attempted to resolve {_addonA}\n\n" +
      "Both \"A\" and \"A2\" could potentially conflict with each other.\n" +
      "\n- Different subfolders from the same url are required.\n" +
      "    - \"A\" requires `/` from `https://user/a.git`\n" +
      "    - \"A2\" requires `subfolder/` from `https://user/a.git`\n" +
      "\n" +
      "Both \"A\" and \"A3\" could potentially conflict with each other.\n" +
      "\n- Different checkouts from the same url are required.\n" +
      "    - \"A\" requires `main` from `https://user/a.git`\n" +
      "    - \"A3\" requires `dev` from `https://user/a.git`"
    );

    var log = new Mock<ILog>();
    log.Setup(l => l.Warn(result.ToString()));
    result.Report(log.Object);
    log.VerifyAll();
  }

  [Fact]
  public void AddonConflictingInstallPathResultDescribesConflict() {
    var result = new AddonCannotBeResolved(
      Addon: _addonAConflict,
      CanonicalAddon: _addonA
    );

    result.ToString().ShouldBe(
      "Cannot resolve \"A\" from `project/addons/other/addons.json` " +
      "because it would conflict with a previously resolved addon of the " +
      "same name from `project/addons.json`.\n\n" +
      "Both addons would be installed to the same path.\n\n" +
      $"  Attempted to resolve: {_addonAConflict}\n\n" +
      $"  Previously resolved: {_addonA}"
    );

    var log = new Mock<ILog>();
    log.Setup(l => l.Err(result.ToString()));
    result.Report(log.Object);
    log.VerifyAll();
  }

  [Fact]
  public void AddonAlreadyResolvedResultDescribesItself() {
    var result = new AddonAlreadyResolved(
      Addon: _addonA,
      CanonicalAddon: _addonA2
    );

    result.ToString().ShouldBe(
      $"The addon \"{_addonA.Name}\" is already resolved as " +
      $"\"{_addonA2.Name}.\"\n\n" +
      $"  Attempted to resolve: {_addonA}\n\n" +
      $"  Previously resolved: {_addonA2}"
    );

    var log = new Mock<ILog>();
    log.Setup(l => l.Warn(result.ToString()));
    result.Report(log.Object);
    log.VerifyAll();
  }

  [Fact]
  public void AddonSuccessResultDescribesItself() {
    var result = new AddonResolved(Addon: _addonA);

    result.ToString().ShouldBe(
      $"Discovered \"{_addonA.Name}.\"\n\n" +
      $"  Resolved: {_addonA}"
    );

    var log = new Mock<ILog>();
    log.Setup(l => l.Info(result.ToString()));
    result.Report(log.Object);
    log.VerifyAll();
  }
}
