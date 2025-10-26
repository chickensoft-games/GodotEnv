// Use concrete types when possible for improved performance
#pragma warning disable CA1859
namespace Chickensoft.GodotEnv.Tests;

using System.Text.Json;
using Chickensoft.GodotEnv.Common.Models;
using Shouldly;
using Xunit;

public class AssetTest
{
  public record TestAsset(string Url, string Checkout, AssetSource Source)
    : Asset(Url, Checkout, Source);

  public const string URL = "git@github.com:chickensoft-games/GodotEnv.git";
  public const string CHECKOUT = "main";

  [Fact]
  public void AssetSourceLocalSerializes()
  {
    var json = JsonSerializer.Serialize(AssetSource.Local);
    json.ShouldBe("\"local\"");
  }

  [Fact]
  public void AssetSourceLocalDeserializes()
  {
    var source = JsonSerializer.Deserialize<AssetSource>("\"local\"");
    source.ShouldBe(AssetSource.Local);
  }

  [Fact]
  public void AssetSourceRemoteSerializes()
  {
    var json = JsonSerializer.Serialize(AssetSource.Remote);
    json.ShouldBe("\"remote\"");
  }

  [Fact]
  public void AssetSourceRemoteDeserializes()
  {
    var source = JsonSerializer.Deserialize<AssetSource>("\"remote\"");
    source.ShouldBe(AssetSource.Remote);
  }

  [Fact]
  public void AssetSourceSymlinkSerializes()
  {
    var json = JsonSerializer.Serialize(AssetSource.Symlink);
    json.ShouldBe("\"symlink\"");
  }

  [Fact]
  public void AssetSourceSymlinkDeserializes()
  {
    var source = JsonSerializer.Deserialize<AssetSource>("\"symlink\"");
    source.ShouldBe(AssetSource.Symlink);
  }

  [Fact]
  public void AssetSourceZipSerializes()
  {
    var json = JsonSerializer.Serialize(AssetSource.Zip);
    json.ShouldBe("\"zip\"");
  }

  [Fact]
  public void AssetSourceZipDeserializes()
  {
    var source = JsonSerializer.Deserialize<AssetSource>("\"zip\"");
    source.ShouldBe(AssetSource.Zip);
  }

  [Fact]
  public void RemoteAsset()
  {
    IAsset asset = new TestAsset(URL, CHECKOUT, AssetSource.Remote);
    asset.Id.ShouldBe("chickensoft_games_godot_env");
    asset.IsLocal.ShouldBeFalse();
    asset.IsRemote.ShouldBeTrue();
    asset.IsSymlink.ShouldBeFalse();
  }

  [Fact]
  public void LocalAsset()
  {
    const string url = "a/b/c/ChickensoftGodotEnv";
    IAsset asset = new TestAsset(url, CHECKOUT, AssetSource.Local);
    asset.Id.ShouldBe("chickensoft_godot_env");
    asset.IsLocal.ShouldBeTrue();
    asset.IsRemote.ShouldBeFalse();
    asset.IsSymlink.ShouldBeFalse();
  }

  [Fact]
  public void SymlinkAsset()
  {
    const string url = "a/b/c/ChickensoftGodotEnv";
    IAsset asset = new TestAsset(url, CHECKOUT, AssetSource.Symlink);
    asset.Id.ShouldBe("chickensoft_godot_env");
    asset.IsLocal.ShouldBeFalse();
    asset.IsRemote.ShouldBeFalse();
    asset.IsSymlink.ShouldBeTrue();
  }
}
#pragma warning restore CA1859
