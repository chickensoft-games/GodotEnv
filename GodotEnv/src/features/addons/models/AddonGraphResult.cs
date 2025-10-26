namespace Chickensoft.GodotEnv;

using System.Collections.Generic;
using Chickensoft.GodotEnv.Common.Utilities;
using Chickensoft.GodotEnv.Features.Addons.Models;

public interface IAddonGraphResult : IReportableEvent { }

public interface IAddonGraphWarningResult { }
public interface IAddonGraphFailureResult { }

/// <summary>
/// An addon was resolved but might conflict with other previously resolved
/// addons. Potential conflicts can  occur when an addon shares the same url
/// as other required addons but differs in checkout ref or subfolder path.
/// </summary>
/// <param name="Addon">Addon that was resolved.</param>
/// <param name="Conflicts">Previously resolved addon(s) that could
/// potentially conflict with the addon that was just resolved.</param>
/// <param name="CanonicalAddon">The first (canonical) addon resolved that
/// shares the same url. This is provided to help prevent addons using the
/// same source url to be cached more than once, which can be expensive.</param>
public record AddonResolvedButMightConflict(
  IAddon Addon, List<IAddon> Conflicts, IAddon CanonicalAddon
) : IAddonGraphResult, IAddonGraphWarningResult
{
  public void Report(ILog log) => log.Warn(ToString());

  public override string ToString()
  {
    var article = Conflicts.Count == 1 ? "a" : "the";
    var s = Conflicts.Count == 1 ? "" : "s";
    var buffer = new List<string>();
    foreach (var conflict in Conflicts)
    {
      if (conflict.Url != Addon.Url)
      { continue; }
      buffer.Add(
        $"\nBoth \"{Addon.Name}\" and \"{conflict.Name}\" could " +
        "potentially conflict with each other.\n"
      );
      if (Addon.Subfolder != conflict.Subfolder)
      {
        buffer.Add(
          "- Different subfolders from the same url are required."
        );
        buffer.Add(
          $"    - \"{Addon.Name}\" requires `{Addon.Subfolder}/` " +
          $"from `{Addon.Url}`"
        );
        buffer.Add(
          $"    - \"{conflict.Name}\" requires `{conflict.Subfolder}/` " +
          $"from `{conflict.Url}`"
        );
      }
      else if (Addon.Checkout != conflict.Checkout)
      {
        buffer.Add(
          "- Different checkouts from the same url are required."
        );
        buffer.Add(
          $"    - \"{Addon.Name}\" requires `{Addon.Checkout}` " +
          $"from `{Addon.Url}`"
        );
        buffer.Add(
          $"    - \"{conflict.Name}\" requires `{conflict.Checkout}` " +
          $"from `{conflict.Url}`"
        );
      }
    }
    return
      $"The addon \"{Addon.Name}\" could conflict with {article} " +
      $"previously resolved addon{s}.\n\n" +
      $"  Attempted to resolve {Addon}\n\n" +
      string.Join("\n", buffer).Trim();
  }
}

/// <summary>
/// An addon cannot be resolved because it shares the same name as a previously
/// resolved addon but differs in url, subfolder, or checkout ref.
/// </summary>
/// <param name="Addon">Addon attempted to be resolved.</param>
/// <param name="CanonicalAddon">Previously resolved addon.</param>
public record AddonCannotBeResolved(
  IAddon Addon, IAddon CanonicalAddon
) : IAddonGraphResult, IAddonGraphFailureResult
{
  public void Report(ILog log) => log.Err(ToString());

  public override string ToString() =>
    $"Cannot resolve \"{Addon.Name}\" from " +
    $"`{Addon.AddonsFilePath}` because it would conflict " +
    "with a previously resolved addon of the same name from " +
    $"`{CanonicalAddon.AddonsFilePath}`.\n\n" +
    "Both addons would be installed to the same path.\n\n" +
    $"  Attempted to resolve: {Addon}\n\n" +
    $"  Previously resolved: {CanonicalAddon}";
}

/// <summary>
/// An addon was already resolved under a different name. This occurs when two
/// addons share the same url, subfolder, and checkout ref, regardless of
/// whether the two addons have the same name.
/// </summary>
/// <param name="Addon">Addon attempted to be resolved.</param>
/// <param name="CanonicalAddon">Equivalent addon that was previously resolved.
/// </param>
/// <returns></returns>
public record AddonAlreadyResolved(
  IAddon Addon, IAddon CanonicalAddon
) : IAddonGraphResult, IAddonGraphWarningResult
{
  public void Report(ILog log) => log.Warn(ToString());

  public override string ToString() =>
    $"The addon \"{Addon.Name}\" is already resolved as " +
    $"\"{CanonicalAddon.Name}.\"\n\n" +
    $"  Attempted to resolve: {Addon}\n\n" +
    $"  Previously resolved: {CanonicalAddon}";
}

/// <summary>
/// An addon was successfully resolved.
/// </summary>
/// <param name="Addon">Resolved addon.</param>
public record AddonResolved(IAddon Addon) : IAddonGraphResult
{
  public void Report(ILog log) => log.Info(ToString());

  public override string ToString() =>
      $"Discovered \"{Addon.Name}.\"\n\n" +
      $"  Resolved: {Addon}";
}
