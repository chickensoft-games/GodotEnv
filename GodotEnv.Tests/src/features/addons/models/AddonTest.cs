namespace Chickensoft.GodotEnv.Tests;

using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Features.Addons.Models;
using Shouldly;
using Xunit;

public class AddonTest {
  public const string NAME = "godotenv";
  public const string ADDONS_FILE_PATH = "godotenv";
  public const string URL = "git@github.com:chickensoft-games/GodotEnv.git";
  public const string SUBFOLDER = "GodotEnv.Tests";
  public const string CHECKOUT = "main";
  public const AssetSource SOURCE = AssetSource.Remote;

  [Fact]
  public void Initializes() {
    var addon = new Addon(
      name: NAME,
      addonsFilePath: ADDONS_FILE_PATH,
      url: URL,
      subfolder: SUBFOLDER,
      checkout: CHECKOUT,
      source: SOURCE
    );

    NAME.ShouldBe(addon.Name);
    ADDONS_FILE_PATH.ShouldBe(addon.AddonsFilePath);
    URL.ShouldBe(addon.Url);
    SUBFOLDER.ShouldBe(addon.Subfolder);
    CHECKOUT.ShouldBe(addon.Checkout);
    SOURCE.ShouldBe(addon.Source);
  }

  [Fact]
  public void DescribesItself() {
    var addon = new Addon(
      name: NAME,
      addonsFilePath: ADDONS_FILE_PATH,
      url: URL,
      subfolder: SUBFOLDER,
      checkout: CHECKOUT,
      source: SOURCE
    );

    addon.ToString().ShouldBe(
      $"Addon \"{NAME}\" from `{ADDONS_FILE_PATH}`" +
      $" at `{SUBFOLDER}/` on branch `{CHECKOUT}` of `{URL}`"
    );
  }
}
