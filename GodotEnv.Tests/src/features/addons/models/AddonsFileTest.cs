namespace Chickensoft.GodotEnv.Tests;

using System.Collections.Generic;
using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Features.Addons.Models;
using Shouldly;
using Xunit;

public class AddonsFileTest
{
  public static readonly Dictionary<string, AddonsFileEntry> ENTRIES
    = new() {
      { "a", new AddonsFileEntry{ Url = "https://a.git" } },
      { "b", new AddonsFileEntry{ Url = "https://b.git" } },
    };
  public const string ADDONS_PATH = "my_addons";
  public const string CACHE_PATH = ".my_addons";

  [Fact]
  public void InitializesWithDefaults()
  {
    var addonsFile = new AddonsFile();
    addonsFile.Addons.ShouldBeEmpty();
    addonsFile.CacheRelativePath.ShouldBe(Defaults.CACHE_PATH);
    addonsFile.PathRelativePath.ShouldBe(Defaults.ADDONS_PATH);
  }

  [Fact]
  public void Initializes()
  {
    var addonsFile = new AddonsFile(
      ENTRIES,
      CACHE_PATH,
      ADDONS_PATH
    );

    addonsFile.Addons.ShouldBe(ENTRIES);
    addonsFile.CacheRelativePath.ShouldBe(CACHE_PATH);
    addonsFile.PathRelativePath.ShouldBe(ADDONS_PATH);
  }
}
