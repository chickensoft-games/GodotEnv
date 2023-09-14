namespace Chickensoft.GodotEnv.Tests;

using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Features.Addons.Models;
using Shouldly;
using Xunit;

public class ResolvedAddonTest {
  [Fact]
  public void Initializes() {
    var addon = new ResolvedAddon(
      new Addon(
        "addon_name", "addons.json", "url", "/", "main", AssetSource.Remote
      ),
      null
    );

    addon.CacheName.ShouldBe("addon_name");
  }

  [Fact]
  public void InitializesWithCanonicalAddon() {
    var addon = new ResolvedAddon(
      new Addon(
        "addon_name", "addons.json", "url", "/", "main", AssetSource.Remote
      ),
      new Addon(
        "canonical_addon", "addons.json", "url", "/", "main", AssetSource.Remote
      )
    );

    addon.CacheName.ShouldBe("canonical_addon");
  }
}
