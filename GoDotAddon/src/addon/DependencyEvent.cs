namespace Chickensoft.GoDotAddon {
  using System.Collections.Generic;

  public abstract record DependencyEvent { }

  public abstract record ReportableDependencyEvent : DependencyEvent { }

  public interface IDependencyNotInstalledEvent { }

  public record SimilarDependencyWarning : ReportableDependencyEvent {
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
      var desc = "";
      foreach (var addon in Addons) {
        var names = $"\"{Conflict.Name}\" and \"{addon.Name}\"";
        if (addon.Url != Conflict.Url) { continue; }
        if (addon.Subfolder == Conflict.Subfolder) {
          desc = $"Both {names} install the same subfolder " +
            $"`{addon.Subfolder}/` of two different branches " +
            $"(`{Conflict.Checkout}`, `{addon.Checkout}`) from " +
            $"`{Conflict.Url}`.";
          buffer.Add(desc);
        }
        else if (addon.Checkout == Conflict.Checkout) {
          desc = $"Both {names} install different subfolders " +
            $"(`{Conflict.Subfolder}/`, `{addon.Subfolder}/`) on the same " +
            $"branch `{addon.Checkout}` from `{addon.Url}`.";
          buffer.Add(desc);
        }
      }
      return
        $"The addon \"{Conflict.Name}\" could conflict with {article} " +
        $"previously installed addon{s}.\n\n" +
        $"  Attempted to install {Conflict}\n\n" +
        string.Join("\n\n", buffer);
    }
  }

  public record ConflictingDestinationPathEvent
    : ReportableDependencyEvent, IDependencyNotInstalledEvent {
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

  public record DependencyAlreadyInstalledEvent()
    : DependencyEvent, IDependencyNotInstalledEvent;

  public record DependencyInstalledEvent() : DependencyEvent;
}
