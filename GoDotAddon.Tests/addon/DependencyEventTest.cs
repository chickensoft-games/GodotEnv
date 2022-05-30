namespace Chickensoft.GoDotAddon.Tests {
  using System.Collections.Generic;
  using Chickensoft.GoDotAddon;
  using Shouldly;
  using Xunit;


  public class DependencyEventTest {

    private readonly RequiredAddon _addonA = new(
      name: "A",
      configFilePath: "project/addons.json",
      url: "https://user/a.git",
      checkout: "main",
      subfolder: "/"
    );

    private readonly RequiredAddon _addonAConflict = new(
      name: "A",
      configFilePath: "project/addons/other/addons.json",
      url: "https://user/a.git",
      checkout: "main",
      subfolder: "/"
    );

    private readonly RequiredAddon _addonASubfolder = new(
      name: "A2",
      configFilePath: "project/addons.json",
      url: "https://user/a.git",
      checkout: "main",
      subfolder: "subfolder/"
    );

    private readonly RequiredAddon _addonABranch = new(
      name: "A3",
      configFilePath: "project/addons.json",
      url: "https://user/a.git",
      checkout: "dev",
      subfolder: "/"
    );

    private readonly RequiredAddon _addonB = new(
      name: "B",
      configFilePath: "project/addons.json",
      url: "https://user/b.git",
      checkout: "main",
      subfolder: "/"
    );

    [Fact]
    public void SimilarDependencyWarningDescribesUnrelatedDependencies() {
      var e = new SimilarDependencyWarning(
        conflict: _addonA,
        addons: new List<RequiredAddon> { _addonB }
      );

      e.ToString().ShouldBe(
        "The addon \"A\" could conflict with a previously installed addon.\n\n"
        + $"  Attempted to install {_addonA}\n\n"
      );
    }

    [Fact]
    public void SimilarDependencyWarningDescribesDifferentSubfolders() {
      var e = new SimilarDependencyWarning(
        conflict: _addonA,
        addons: new List<RequiredAddon> { _addonASubfolder }
      );

      e.ToString().ShouldBe(
        "The addon \"A\" could conflict with a previously installed addon.\n\n"
        + $"  Attempted to install {_addonA}\n\n" +
        "Both \"A\" and \"A2\" could potentially conflict with each other.\n" +
        "\n- Different subfolders from the same url are installed.\n" +
        "    - \"A\" installs `/` from `https://user/a.git`\n" +
        "    - \"A2\" installs `subfolder/` from `https://user/a.git`"
      );
    }

    [Fact]
    public void SimilarDependencyWarningDescribesDifferentCheckouts() {
      var e = new SimilarDependencyWarning(
        conflict: _addonA,
        addons: new List<RequiredAddon> { _addonABranch }
      );

      e.ToString().ShouldBe(
        "The addon \"A\" could conflict with a previously installed addon.\n\n"
        + $"  Attempted to install {_addonA}\n\n" +
        "Both \"A\" and \"A3\" could potentially conflict with each other.\n" +
        "\n- Different branches from the same url are installed.\n" +
        "    - \"A\" installs `main` from `https://user/a.git`\n" +
        "    - \"A3\" installs `dev` from `https://user/a.git`"
      );
    }

    [Fact]
    public void SimilarDependencyWarningDescribesMultipleConflicts() {
      var e = new SimilarDependencyWarning(
        conflict: _addonA,
        addons: new List<RequiredAddon> { _addonASubfolder, _addonABranch }
      );

      e.ToString().ShouldBe(
        "The addon \"A\" could conflict with the previously installed addons." +
        "\n\n" +
        $"  Attempted to install {_addonA}\n\n" +
        "Both \"A\" and \"A2\" could potentially conflict with each other.\n" +
        "\n- Different subfolders from the same url are installed.\n" +
        "    - \"A\" installs `/` from `https://user/a.git`\n" +
        "    - \"A2\" installs `subfolder/` from `https://user/a.git`\n" +
        "\n" +
        "Both \"A\" and \"A3\" could potentially conflict with each other.\n" +
        "\n- Different branches from the same url are installed.\n" +
        "    - \"A\" installs `main` from `https://user/a.git`\n" +
        "    - \"A3\" installs `dev` from `https://user/a.git`"
      );
    }

    [Fact]
    public void ConflictingDestinationPathEventDescribesConflict() {
      var e = new ConflictingDestinationPathEvent(
        conflict: _addonAConflict,
        addon: _addonA
      );

      e.ToString().ShouldBe(
        "Cannot install \"A\" from `project/addons/other/addons.json` " +
        "because it would conflict with a previously installed addon of the " +
        "same name from `project/addons.json`.\n\n" +
        "Both addons would be installed to the same path.\n\n" +
        $"  Attempted to install: {_addonAConflict}\n\n" +
        $"  Previously installed: {_addonA}"
      );
    }

    [Fact]
    public void InstantiatesAlreadyInstalledEvent() {
      var e = new DependencyAlreadyInstalledEvent();
      e.ShouldBeOfType(typeof(DependencyAlreadyInstalledEvent));
    }

    [Fact]
    public void InstantiatesInstalledEvent() {
      var e = new DependencyInstalledEvent();
      e.ShouldBeOfType(typeof(DependencyInstalledEvent));
    }
  }
}
