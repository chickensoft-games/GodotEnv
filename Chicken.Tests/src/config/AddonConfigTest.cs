namespace Chickensoft.Chicken.Tests;
using Newtonsoft.Json;
using Shouldly;
using Xunit;

public class AddonConfigTest {
  private const string URL
    = "https://github.com/chickensoft-games/Chicken";
  private const string SUBFOLDER = "Chicken";
  private const string CHECKOUT = "main";
  private const RepositorySource SOURCE = RepositorySource.Local;

  [Fact]
  public void Deserializes() {
    var json = "{" +
      $"  \"url\": \"{URL}\"," +
      $"  \"subfolder\": \"{SUBFOLDER}\"," +
      $"  \"checkout\": \"{CHECKOUT}\"," +
      $"  \"source\": \"{SOURCE}\"" +
    "}";
    var config = JsonConvert.DeserializeObject<AddonConfig>(json);
    config.ShouldNotBeNull();
    config.Subfolder.ShouldBe(SUBFOLDER);
    config.Checkout.ShouldBe(CHECKOUT);
    config.Source.ShouldBe(SOURCE);
  }

  [Fact]
  public void DeserializesWithAllPresent() {
    var json = "{" +
      $"  \"url\": \"{URL}\"," +
      $"  \"subfolder\": null," +
      $"  \"checkout\": null," +
      $"  \"source\": \"symlink\"" +
    "}";
    var config = JsonConvert.DeserializeObject<AddonConfig>(json);
    config.ShouldNotBeNull();
    config.Subfolder.ShouldBe(App.DEFAULT_SUBFOLDER);
    config.Checkout.ShouldBe(App.DEFAULT_CHECKOUT);
    config.Source.ShouldBe(RepositorySource.Symlink);
  }

  [Fact]
  public void DeserializesWithMissingValues() {
    var json = "{" + $"\"url\": \"{URL}\"" + "}";
    var config = JsonConvert.DeserializeObject<AddonConfig>(json);
    config.ShouldNotBeNull();
    config.Subfolder.ShouldBe(App.DEFAULT_SUBFOLDER);
    config.Checkout.ShouldBe(App.DEFAULT_CHECKOUT);
    config.Source.ShouldBe(RepositorySource.Remote);
  }

  [Fact]
  public void IsLocal() {
    var config = new AddonConfig(
      url: URL,
      subfolder: SUBFOLDER,
      checkout: CHECKOUT,
      source: RepositorySource.Local
    );
    config.IsLocal.ShouldBeTrue();
    config.IsRemote.ShouldBeFalse();
    config.IsSymlink.ShouldBeFalse();
  }

  [Fact]
  public void IsRemote() {
    var config = new AddonConfig(
      url: URL,
      subfolder: SUBFOLDER,
      checkout: CHECKOUT,
      source: RepositorySource.Remote
    );
    config.IsRemote.ShouldBeTrue();
    config.IsLocal.ShouldBeFalse();
    config.IsSymlink.ShouldBeFalse();
  }

  [Fact]
  public void IsSymlink() {
    var config = new AddonConfig(
      url: URL,
      subfolder: SUBFOLDER,
      checkout: CHECKOUT,
      source: RepositorySource.Symlink
    );
    config.IsSymlink.ShouldBeTrue();
    config.IsRemote.ShouldBeFalse();
    config.IsLocal.ShouldBeFalse();
  }
}
