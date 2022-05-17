namespace Chickensoft.GoDotAddon.Tests {
  using global::GoDotAddon;
  using Shouldly;
  using Xunit;


  public class RequiredAddonTest {
    private const string ADDON_NAME = "GoDotAddon";
    private const string ADDON_URL
      = "git@github.com:chickensoft-games/GoDotAddon.git";
    private const string SUBFOLDER = "/";
    private const string CHECKOUT = "main";

    [Fact]
    public void ToStringReturnsDescription() {
      var addon = new RequiredAddon(
        Name: ADDON_NAME,
        Url: ADDON_URL,
        Subfolder: SUBFOLDER,
        Checkout: CHECKOUT
      );
      var description = addon.ToString();
      description.ShouldBe(
        $"Addon **{ADDON_NAME}** to subfolder `{SUBFOLDER}` of " +
        $"`{ADDON_URL}` from branch `{CHECKOUT}`"
      );
    }
  }
}
