namespace Chickensoft.GoDotAddon.Tests {
  using global::GoDotAddon;
  using Newtonsoft.Json;
  using Shouldly;
  using Xunit;

  public class ConfigFileTest {
    private const string URL
  = "https://github.com/chickensoft-games/GoDotAddon";
    private const string SUBFOLDER = "GoDotAddon";
    private const string CHECKOUT = "main";
    private const string ADDON_NAME = "go_dot_addon";
    private const string ADDON_JSON = "{" +
      $"  \"url\": \"{URL}\"," +
      $"  \"subfolder\": \"{SUBFOLDER}\"," +
      $"  \"checkout\": \"{CHECKOUT}\"" +
    "}";

    private const string CACHE_DIR = ".addons";
    private const string PATH_DIR = "addons";

    [Fact]
    public void Deserializes() {
      var json = "{" +
        "  \"addons\": {" + ADDON_NAME + $": {ADDON_JSON}" + "}," +
        $"  \"cache\": \"{CACHE_DIR}\"," +
        $"  \"path\": \"{PATH_DIR}\"" +
      "}";
      var config = JsonConvert.DeserializeObject<ConfigFile>(json);
      config.ShouldNotBeNull();
      config.Addons.ShouldNotBeNull();
      config.Addons.ShouldNotBeEmpty();
      config.Addons.ShouldContainKey(ADDON_NAME);
      config.Addons[ADDON_NAME].ShouldBe(
        new AddonConfig(url: URL, subfolder: SUBFOLDER, checkout: CHECKOUT)
      );
      config.CachePath.ShouldBe(CACHE_DIR);
      config.AddonsPath.ShouldBe(PATH_DIR);
    }

    [Fact]
    public void DeserializesWithDefaults() {
      var json = "{" +
        "  \"addons\": null," +
        $"  \"cache\": null," +
        $"  \"path\": null" +
      "}";
      var config = JsonConvert.DeserializeObject<ConfigFile>(json);
      config.ShouldNotBeNull();
      config.Addons.ShouldNotBeNull();
      config.Addons.ShouldBeEmpty();
      config.CachePath.ShouldBe(IApp.DEFAULT_CACHE_DIR);
      config.AddonsPath.ShouldBe(IApp.DEFAULT_PATH_DIR);
    }

    [Fact]
    public void DeserializesWithMissingValues() {
      var json = "{}";
      var config = JsonConvert.DeserializeObject<ConfigFile>(json);
      config.ShouldNotBeNull();
      config.Addons.ShouldNotBeNull();
      config.Addons.ShouldBeEmpty();
      config.CachePath.ShouldBe(IApp.DEFAULT_CACHE_DIR);
      config.AddonsPath.ShouldBe(IApp.DEFAULT_PATH_DIR);
    }
  }
}
