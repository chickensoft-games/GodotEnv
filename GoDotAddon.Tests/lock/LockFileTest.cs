namespace Chickensoft.GoDotAddon.Tests {
  using System.Collections.Generic;
  using global::GoDotAddon;
  using Newtonsoft.Json;
  using Shouldly;
  using Xunit;


  public class LockFileTest {
    [Fact]
    public void Initializes() {
      var addons = new Dictionary<string, Dictionary<string, LockFileEntry>>();
      var lockFile = new LockFile() {
        Addons = addons
      };
      lockFile.Addons.ShouldBe(addons);
    }

    [Fact]
    public void Deserializes() {
      var json = "{" +
        "\"addons\": {" +
          "\"url\": {" +
            "\"subfolder\": {" +
              "\"name\": \"addon\", " +
              "\"checkout\": \"main\"" +
            "}" +
          "}" +
        "}" +
      "}";
      var lockFile = JsonConvert.DeserializeObject<LockFile>(json);
      lockFile.ShouldNotBeNull();
      lockFile.Addons.ShouldNotBeEmpty();
      lockFile.Addons.ShouldContainKey("url");
      var addon = lockFile.Addons["url"];
      addon.ShouldNotBeEmpty();
      addon.ShouldContainKey("subfolder");
      var subfolder = addon["subfolder"];
      subfolder.Name.ShouldBe("addon");
      subfolder.Checkout.ShouldBe("main");
    }
  }
}
