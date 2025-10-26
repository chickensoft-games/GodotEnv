namespace Chickensoft.GodotEnv.Tests;

using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Features.Addons.Models;

public static class TestData
{
  public const string NAME = "godotenv";
  public const string ADDONS_FILE_PATH = "godotenv";
  public const string URL = "git@github.com:chickensoft-games/GodotEnv.git";
  public const string SUBFOLDER = "GodotEnv.Tests";
  public const string CHECKOUT = "main";
  public const AssetSource SOURCE = AssetSource.Remote;

  public static readonly Addon Addon = new(
    name: NAME,
    addonsFilePath: ADDONS_FILE_PATH,
    url: URL,
    subfolder: SUBFOLDER,
    checkout: CHECKOUT,
    source: SOURCE
  );

  public static readonly Addon SymlinkAddon = new(
    name: NAME,
    addonsFilePath: ADDONS_FILE_PATH,
    url: "/GodotEnv",
    subfolder: SUBFOLDER,
    checkout: CHECKOUT,
    source: AssetSource.Symlink
  );

  public static readonly Addon ZipAddon = new(
    name: NAME,
    addonsFilePath: ADDONS_FILE_PATH,
    url: URL,
    subfolder: SUBFOLDER,
    checkout: "",
    source: AssetSource.Zip
  );
}
