namespace Chickensoft.GodotEnv.Tests;

using Chickensoft.GodotEnv.Features.Addons.Models;
using Shouldly;
using Xunit;

public class AddonsConfigurationTest
{
  public const string PROJECT_PATH = "/a/b/c";
  public const string ADDONS_PATH = "/a/b/c/addons";
  public const string CACHE_PATH = "/a/b/c/.addons";

  [Fact]
  public void Initializes()
  {
    var config = new AddonsConfiguration(
      ProjectPath: PROJECT_PATH, AddonsPath: ADDONS_PATH, CachePath: CACHE_PATH
    );

    config.ProjectPath.ShouldBe(PROJECT_PATH);
    config.AddonsPath.ShouldBe(ADDONS_PATH);
    config.CachePath.ShouldBe(CACHE_PATH);
  }
}
