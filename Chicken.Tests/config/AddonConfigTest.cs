namespace Chickensoft.Chicken.Tests {
  using Newtonsoft.Json;
  using Shouldly;
  using Xunit;

  public class AddonConfigTest {
    private const string URL
      = "https://github.com/chickensoft-games/Chicken";
    private const string SUBFOLDER = "Chicken";
    private const string CHECKOUT = "main";
    private const bool SYMLINK = true;

    [Fact]
    public void Deserializes() {
      var json = "{" +
        $"  \"url\": \"{URL}\"," +
        $"  \"subfolder\": \"{SUBFOLDER}\"," +
        $"  \"checkout\": \"{CHECKOUT}\"," +
        $"  \"symlink\": \"{SYMLINK}\"" +
      "}";
      var config = JsonConvert.DeserializeObject<AddonConfig>(json);
      config.ShouldNotBeNull();
      config.Subfolder.ShouldBe(SUBFOLDER);
      config.Checkout.ShouldBe(CHECKOUT);
      config.Symlink.ShouldBe(SYMLINK);
    }

    [Fact]
    public void DeserializesWithDefaults() {
      var json = "{" +
        $"  \"url\": \"{URL}\"," +
        $"  \"subfolder\": null," +
        $"  \"checkout\": null," +
        $"  \"symlink\": true" +
      "}";
      var config = JsonConvert.DeserializeObject<AddonConfig>(json);
      config.ShouldNotBeNull();
      config.Subfolder.ShouldBe(IApp.DEFAULT_SUBFOLDER);
      config.Checkout.ShouldBe(IApp.DEFAULT_CHECKOUT);
      config.Symlink.ShouldBe(true);
    }

    [Fact]
    public void DeserializesWithMissingValues() {
      var json = "{" + $"\"url\": \"{URL}\"" + "}";
      var config = JsonConvert.DeserializeObject<AddonConfig>(json);
      config.ShouldNotBeNull();
      config.Subfolder.ShouldBe(IApp.DEFAULT_SUBFOLDER);
      config.Checkout.ShouldBe(IApp.DEFAULT_CHECKOUT);
      config.Symlink.ShouldBe(false);
    }
  }
}
