namespace Chickensoft.Chicken {
  using System.Collections.Generic;

  public interface IDependencyEvent { }

  public interface IReportableDependencyEvent : IDependencyEvent { }

  public interface IDependencyCannotBeInstalledEvent { }

  public record SimilarDependencyWarning : IReportableDependencyEvent {
    public RequiredAddon Conflict { get; init; }
    public List<RequiredAddon> Addons { get; init; }

    public SimilarDependencyWarning(
      RequiredAddon conflict,
      List<RequiredAddon> addons
    ) {
      Conflict = conflict;
      Addons = addons;
    }

    public override string ToString() {
      var article = Addons.Count == 1 ? "a" : "the";
      var s = Addons.Count == 1 ? "" : "s";
      var buffer = new List<string>();
      foreach (var addon in Addons) {
        if (addon.Url != Conflict.Url) { continue; }
        buffer.Add(
          $"\nBoth \"{Conflict.Name}\" and \"{addon.Name}\" could potentially " +
          "conflict with each other.\n"
        );
        if (Conflict.Subfolder != addon.Subfolder) {
          buffer.Add(
            "- Different subfolders from the same url are installed."
          );
          buffer.Add(
            $"    - \"{Conflict.Name}\" installs `{Conflict.Subfolder}/` " +
            $"from `{Conflict.Url}`"
          );
          buffer.Add(
            $"    - \"{addon.Name}\" installs `{addon.Subfolder}/` " +
            $"from `{addon.Url}`"
          );
        }
        else if (Conflict.Checkout != addon.Checkout) {
          buffer.Add(
            "- Different branches from the same url are installed."
          );
          buffer.Add(
            $"    - \"{Conflict.Name}\" installs `{Conflict.Checkout}` " +
            $"from `{Conflict.Url}`"
          );
          buffer.Add(
            $"    - \"{addon.Name}\" installs `{addon.Checkout}` " +
            $"from `{addon.Url}`"
          );
        }
      }
      return
        $"The addon \"{Conflict.Name}\" could conflict with {article} " +
        $"previously installed addon{s}.\n\n" +
        $"  Attempted to install {Conflict}\n\n" +
        string.Join("\n", buffer).Trim();
    }
  }

  public record ConflictingDestinationPathEvent
    : IReportableDependencyEvent, IDependencyCannotBeInstalledEvent {
    public RequiredAddon Conflict { get; init; }
    public RequiredAddon Addon { get; init; }

    public ConflictingDestinationPathEvent(
      RequiredAddon conflict,
      RequiredAddon addon
    ) {
      Conflict = conflict;
      Addon = addon;
    }

    public override string ToString() =>
      $"Cannot install \"{Conflict.Name}\" from " +
      $"`{Conflict.ConfigFilePath}` because it would conflict " +
      $"with a previously installed addon of the same name from " +
      $"`{Addon.ConfigFilePath}`.\n\n" +
      $"Both addons would be installed to the same path.\n\n" +
      $"  Attempted to install: {Conflict}\n\n" +
      $"  Previously installed: {Addon}";
  }

  public record DependencyAlreadyInstalledEvent
    : IDependencyEvent, IDependencyCannotBeInstalledEvent {
    public RequiredAddon Requested { get; init; }
    public RequiredAddon AlreadyInstalled { get; init; }

    public DependencyAlreadyInstalledEvent(RequiredAddon requested, RequiredAddon alreadyInstalled) {
      Requested = requested;
      AlreadyInstalled = alreadyInstalled;
    }

    public override string ToString() =>
      $"The addon \"{Requested.Name}\" is already installed as " +
      $"\"{AlreadyInstalled.Name}.\"\n\n" +
      $"  Attempted to install: {Requested}\n\n" +
      $"  Previously installed: {AlreadyInstalled}";
  }

  public record DependencyCanBeInstalledEvent
    : IDependencyEvent, IReportableDependencyEvent {
    public RequiredAddon Addon { get; init; }
    public DependencyCanBeInstalledEvent(RequiredAddon addon) => Addon = addon;
    public override string ToString() =>
        $"Attempting to install \"{Addon.Name}.\"\n\n" +
        $"  Installing: {Addon}";
  }
}
