// Use concrete types when possible for improved performance
#pragma warning disable CA1859
namespace Chickensoft.GodotEnv.Tests;

using Chickensoft.GodotEnv.Common.Models;
using Shouldly;
using Xunit;

public class AssetTest {
  public class TestAsset(string url, string checkout, AssetSource source)
    : IAsset {
    public string Url { get; } = url;
    public string Checkout { get; } = checkout;
    public AssetSource Source { get; } = source;
  }

  public const string URL = "git@github.com:chickensoft-games/GodotEnv.git";
  public const string CHECKOUT = "main";

  [Fact]
  public void RemoteAsset() {
    IAsset asset = new TestAsset(URL, CHECKOUT, AssetSource.Remote);
    asset.Id.ShouldBe("chickensoft_games_godot_env");
    asset.IsLocal.ShouldBeFalse();
    asset.IsRemote.ShouldBeTrue();
    asset.IsSymlink.ShouldBeFalse();
  }

  [Fact]
  public void LocalAsset() {
    const string url = "a/b/c/ChickensoftGodotEnv";
    IAsset asset = new TestAsset(url, CHECKOUT, AssetSource.Local);
    asset.Id.ShouldBe("chickensoft_godot_env");
    asset.IsLocal.ShouldBeTrue();
    asset.IsRemote.ShouldBeFalse();
    asset.IsSymlink.ShouldBeFalse();
  }

  [Fact]
  public void SymlinkAsset() {
    const string url = "a/b/c/ChickensoftGodotEnv";
    IAsset asset = new TestAsset(url, CHECKOUT, AssetSource.Symlink);
    asset.Id.ShouldBe("chickensoft_godot_env");
    asset.IsLocal.ShouldBeFalse();
    asset.IsRemote.ShouldBeFalse();
    asset.IsSymlink.ShouldBeTrue();
  }
}
#pragma warning restore CA1859
