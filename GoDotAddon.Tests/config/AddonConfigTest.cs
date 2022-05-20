namespace Chickensoft.GoDotAddon.Tests {
  using Chickensoft.GoDotAddon;
  using Newtonsoft.Json;
  using Shouldly;
  using Xunit;

  public class AddonConfigTest {
    private const string URL
      = "https://github.com/chickensoft-games/GoDotAddon";
    private const string SUBFOLDER = "GoDotAddon";
    private const string CHECKOUT = "main";

    [Fact]
    public void Deserializes() {
      var json = "{" +
        $"  \"url\": \"{URL}\"," +
        $"  \"subfolder\": \"{SUBFOLDER}\"," +
        $"  \"checkout\": \"{CHECKOUT}\"" +
      "}";
      var config = JsonConvert.DeserializeObject<AddonConfig>(json);
      config.ShouldNotBeNull();
      config.Subfolder.ShouldBe(SUBFOLDER);
      config.Checkout.ShouldBe(CHECKOUT);
    }

    [Fact]
    public void DeserializesWithDefaults() {
      var json = "{" +
        $"  \"url\": \"{URL}\"," +
        $"  \"subfolder\": null," +
        $"  \"checkout\": null" +
      "}";
      var config = JsonConvert.DeserializeObject<AddonConfig>(json);
      config.ShouldNotBeNull();
      config.Subfolder.ShouldBe(IApp.DEFAULT_SUBFOLDER);
      config.Checkout.ShouldBe(IApp.DEFAULT_CHECKOUT);
    }

    [Fact]
    public void DeserializesWithMissingValues() {
      var json = "{" + $"\"url\": \"{URL}\"" + "}";
      var config = JsonConvert.DeserializeObject<AddonConfig>(json);
      config.ShouldNotBeNull();
      config.Subfolder.ShouldBe(IApp.DEFAULT_SUBFOLDER);
      config.Checkout.ShouldBe(IApp.DEFAULT_CHECKOUT);
    }
  }
}
