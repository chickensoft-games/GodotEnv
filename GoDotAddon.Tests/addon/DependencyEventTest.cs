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
      var similarDependencyWarning = new SimilarDependencyWarning(
        conflict: _addonA,
        addons: new List<RequiredAddon> { _addonB }
      );

      similarDependencyWarning.ToString().ShouldBe(
        "The addon \"A\" could conflict with a previously installed addon.\n\n"
        + $"  Attempted to install {_addonA}\n\n"
      );
    }

    [Fact]
    public void SimilarDependencyWarningDescribesDifferentSubfolders() {
      var similarDependencyWarning = new SimilarDependencyWarning(
        conflict: _addonA,
        addons: new List<RequiredAddon> { _addonASubfolder }
      );

      similarDependencyWarning.ToString().ShouldBe(
        "The addon \"A\" could conflict with a previously installed addon.\n\n"
        + $"  Attempted to install {_addonA}\n\n" +
        "Both \"A\" and \"A2\" install different subfolders " +
        "(`/`, `subfolder/`) on the same branch `main` from " +
        "`https://user/a.git`."
      );
    }

    [Fact]
    public void SimilarDependencyWarningDescribesDifferentCheckouts() {
      var similarDependencyWarning = new SimilarDependencyWarning(
        conflict: _addonA,
        addons: new List<RequiredAddon> { _addonABranch }
      );

      similarDependencyWarning.ToString().ShouldBe(
        "The addon \"A\" could conflict with a previously installed addon.\n\n"
        + $"  Attempted to install {_addonA}\n\n" +
        "Both \"A\" and \"A3\" install the same subfolder `/` of two " +
        "different branches (`main`, `dev`) from `https://user/a.git`."
      );
    }

    [Fact]
    public void SimilarDependencyWarningDescribesMultipleConflicts() {
      var similarDependencyWarning = new SimilarDependencyWarning(
        conflict: _addonA,
        addons: new List<RequiredAddon> { _addonASubfolder, _addonABranch }
      );

      similarDependencyWarning.ToString().ShouldBe(
        "The addon \"A\" could conflict with the previously installed addons." +
        "\n\n" +
        $"  Attempted to install {_addonA}\n\n" +
        "Both \"A\" and \"A2\" install different subfolders " +
        "(`/`, `subfolder/`) on the same branch `main` from " +
        "`https://user/a.git`.\n\n" +
        "Both \"A\" and \"A3\" install the same subfolder `/` of two " +
        "different branches (`main`, `dev`) from `https://user/a.git`."
      );
    }
  }
}
