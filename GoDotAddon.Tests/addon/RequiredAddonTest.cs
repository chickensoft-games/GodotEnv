namespace Chickensoft.GoDotAddon.Tests {
  using Chickensoft.GoDotAddon;
  using Shouldly;
  using Xunit;


  public class RequiredAddonTest {
    private const string ADDON_NAME = "go_dot_addon";
    private const string CONFIG_FILE_PATH = "some/working/dir/addons.json";
    private const string ADDON_URL
      = "git@github.com:chickensoft-games/GoDotAddon.git";
    private const string SUBFOLDER = "GoDotAddon.Tests";
    private const string CHECKOUT = "main";

    private readonly string[] _testUrls = {
        "git://github.com/some-user/my-repo.git",
        "git@github.com:some-user/my-repo.git",
        "https://github.com/some-user/my-repo.git",
        "ssh://git@github.com/some-user/my-repo.git/",
        "ftps://github.com/some-user/my-repo.git",
        "git+ssh://git@github.com:some-user/my-repo.git",
        "git+https://something@github.com/some-user/my-repo.git",
        "git://github.com/some-user/my-repo.git",
      };

    [Fact]
    public void Instantiates() {
      var addon = new RequiredAddon(
        name: ADDON_NAME,
        configFilePath: CONFIG_FILE_PATH,
        url: ADDON_URL,
        checkout: CHECKOUT,
        subfolder: SUBFOLDER + "/"
      );
      addon.Name.ShouldBe(ADDON_NAME);
      addon.Url.ShouldBe(ADDON_URL);
      addon.Checkout.ShouldBe(CHECKOUT);
      addon.Subfolder.ShouldBe(SUBFOLDER);
    }

    [Fact]
    public void ToStringReturnsDescription() {
      var addon = new RequiredAddon(
        name: ADDON_NAME,
        configFilePath: CONFIG_FILE_PATH,
        url: ADDON_URL,
        checkout: CHECKOUT,
        subfolder: SUBFOLDER
      );
      var description = addon.ToString();
      description.ShouldBe(
        $"Addon \"{ADDON_NAME}\" from `{CONFIG_FILE_PATH}`" +
        $" to `{SUBFOLDER}/` on branch `{CHECKOUT}` of `{ADDON_URL}`"
      );
    }


    [Fact]
    public void IdIsExpectedString() {
      var addon = new RequiredAddon(
        name: ADDON_NAME,
        configFilePath: CONFIG_FILE_PATH,
        url: ADDON_URL,
        checkout: CHECKOUT,
        subfolder: SUBFOLDER + "/"
      );
      addon.Id.ShouldBe("chickensoft_games_go_dot_addon");
    }

    [Fact]
    public void IdIsExpectedStringWhenRegexFails() {
      var testUrl = "BobTheUrl";
      var addon = new RequiredAddon(
        name: ADDON_NAME,
        configFilePath: CONFIG_FILE_PATH,
        url: testUrl, checkout: CHECKOUT, subfolder: SUBFOLDER
      );
      addon.Id.ShouldBe("bob_the_url");
    }
  }
}
