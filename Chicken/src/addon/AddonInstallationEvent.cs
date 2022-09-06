namespace Chickensoft.Chicken {
  using System;

  public interface IAddonInstallationEvent : IReportableEvent { }
  public record AddonInstalledEvent(RequiredAddon Addon)
    : IAddonInstallationEvent {
    public ConsoleColor Color => ConsoleColor.Green;
    public override string ToString() =>
      $"Installed \"{Addon.Name}\" from `{Addon.Url}`";
  }
  public record AddonFailedToInstallEvent(RequiredAddon Addon, Exception Error)
    : IAddonInstallationEvent {
    public ConsoleColor Color => ConsoleColor.Red;
    public override string ToString() =>
      $"Failed to install \"{Addon.Name}\": {Error.Message}";
  }
}
