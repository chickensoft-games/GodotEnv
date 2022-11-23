namespace Chickensoft.Chicken;
using System;

public interface IAddonInstallationEvent : IReportableEvent { }
public record AddonInstalledEvent(RequiredAddon Addon)
  : IAddonInstallationEvent {
  public void Log(ILog log) => log.Success(ToString());
  public override string ToString() =>
    $"Installed \"{Addon.Name}\" from `{Addon.Url}`";
}
public record AddonFailedToInstallEvent(RequiredAddon Addon, Exception Error)
  : IAddonInstallationEvent {
  public void Log(ILog log) => log.Err(ToString());
  public override string ToString() =>
    $"Failed to install \"{Addon.Name}\": {Error.Message}";
}
