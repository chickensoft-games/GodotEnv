namespace Chickensoft.Chicken.Tests;
using Shouldly;
using Xunit;


public class RequiredAddonTest {
  private const string ADDON_NAME = "chicken";
  private const string CONFIG_FILE_PATH = "some/working/dir/addons.json";
  private const string ADDON_URL
    = "git@github.com:chickensoft-games/chicken.git";
  private const string SUBFOLDER = "Chicken.Tests";
  private const string CHECKOUT = "main";
  private const RepositorySource SOURCE = RepositorySource.Symlink;

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
      subfolder: SUBFOLDER + "/",
      source: SOURCE
    );
    addon.Name.ShouldBe(ADDON_NAME);
    addon.Url.ShouldBe(ADDON_URL);
    addon.Checkout.ShouldBe(CHECKOUT);
    addon.Subfolder.ShouldBe(SUBFOLDER);
    addon.Source.ShouldBe(SOURCE);
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
      $" at `{SUBFOLDER}/` on branch `{CHECKOUT}` of `{ADDON_URL}`"
    );
  }

  [Fact]
  public void ToStringTrimsSlashOnSubfolder() {
    var addon = new RequiredAddon(
      name: ADDON_NAME,
      configFilePath: CONFIG_FILE_PATH,
      url: ADDON_URL,
      checkout: CHECKOUT,
      subfolder: "/"
    );
    addon.ToString().ShouldBe(
      $"Addon \"{ADDON_NAME}\" from `{CONFIG_FILE_PATH}`" +
      $" at `/` on branch `{CHECKOUT}` of `{ADDON_URL}`"
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
    (addon as ISourceRepository).Id.ShouldBe("chickensoft_games_chicken");
  }

  [Fact]
  public void IdIsExpectedStringWhenRegexFails() {
    var testUrl = "Some/Folders/BobTheUrl";
    var addon = new RequiredAddon(
      name: ADDON_NAME,
      configFilePath: CONFIG_FILE_PATH,
      url: testUrl, checkout: CHECKOUT, subfolder: SUBFOLDER
    );
    (addon as ISourceRepository).Id.ShouldBe("bob_the_url");
  }

  [Fact]
  public void IsLocal() {
    var config = new RequiredAddon(
      name: ADDON_NAME,
      configFilePath: CONFIG_FILE_PATH,
      url: ADDON_URL,
      checkout: CHECKOUT,
      subfolder: SUBFOLDER,
      source: RepositorySource.Local
    );
    (config as ISourceRepository).IsLocal.ShouldBeTrue();
    (config as ISourceRepository).IsRemote.ShouldBeFalse();
    (config as ISourceRepository).IsSymlink.ShouldBeFalse();
  }

  [Fact]
  public void IsRemote() {
    var config = new RequiredAddon(
      name: ADDON_NAME,
      configFilePath: CONFIG_FILE_PATH,
      url: ADDON_URL,
      checkout: CHECKOUT,
      subfolder: SUBFOLDER,
      source: RepositorySource.Remote
    );
    (config as ISourceRepository).IsRemote.ShouldBeTrue();
    (config as ISourceRepository).IsLocal.ShouldBeFalse();
    (config as ISourceRepository).IsSymlink.ShouldBeFalse();
  }

  [Fact]
  public void IsSymlink() {
    var config = new RequiredAddon(
      name: ADDON_NAME,
      configFilePath: CONFIG_FILE_PATH,
      url: ADDON_URL,
      checkout: CHECKOUT,
      subfolder: SUBFOLDER,
      source: RepositorySource.Symlink
    );
    (config as ISourceRepository).IsSymlink.ShouldBeTrue();
    (config as ISourceRepository).IsRemote.ShouldBeFalse();
    (config as ISourceRepository).IsLocal.ShouldBeFalse();
  }
}
