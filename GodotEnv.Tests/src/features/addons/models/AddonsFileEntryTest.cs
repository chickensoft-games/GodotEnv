namespace Chickensoft.GodotEnv.Tests;

using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Features.Addons.Models;
using Shouldly;
using Xunit;

public class AddonsFileEntryTest
{
  public const string URL = "git@github.com:chickensoft-games/GodotEnv.git";
  public const string CHECKOUT = "main";

  [Fact]
  public void InitializesWithDefaults()
  {
    var entry = new AddonsFileEntry { Url = URL };
    entry.Url.ShouldBe(URL);
    entry.Subfolder.ShouldBe(Defaults.SUBFOLDER);
    entry.Checkout.ShouldBe(Defaults.CHECKOUT);
    entry.Source.ShouldBe(Defaults.SOURCE);
  }

  [Fact]
  public void InitializesWithValues()
  {
    var entry = new AddonsFileEntry
    {
      Url = URL,
      Subfolder = "/",
      Checkout = CHECKOUT,
      Source = AssetSource.Remote,
    };
    entry.Url.ShouldBe(URL);
    entry.Subfolder.ShouldBe("/");
    entry.Checkout.ShouldBe(CHECKOUT);
    entry.Source.ShouldBe(AssetSource.Remote);
  }

  [Fact]
  public void ToAddonConvertsEntryToAddon()
  {
    var entry = new AddonsFileEntry { Url = URL };
    var addon = entry.ToAddon("godotenv", URL, "addons.json");
    addon.Name.ShouldBe("godotenv");
    addon.Url.ShouldBe(URL);
    addon.Subfolder.ShouldBe("");
    addon.Checkout.ShouldBe(Defaults.CHECKOUT);
    addon.Source.ShouldBe(Defaults.SOURCE);
    addon.AddonsFilePath.ShouldBe("addons.json");
  }
}
