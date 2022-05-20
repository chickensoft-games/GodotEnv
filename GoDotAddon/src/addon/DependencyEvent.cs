namespace GoDotAddon {
  using System.Text;

  public abstract record DependencyEvent { }
  public abstract record ReportableDependencyEvent : DependencyEvent { }
  public interface IDependencyNotInstalledEvent { }

  public record SimilarDependencyWarning : ReportableDependencyEvent {
    public RequiredAddon Conflict { get; init; }
    public List<RequiredAddon> Addons { get; init; }
    private readonly string _cachePath;

    public SimilarDependencyWarning(
      RequiredAddon conflict,
      List<RequiredAddon> addons,
      Config config
    ) {
      Conflict = conflict;
      Addons = addons;
      _cachePath = config.CachePath;
    }

    public override string ToString() {
      var s = Addons.Count == 1 ? " " : "s";
      var buffer = new StringBuilder();
      var destPath = Path.Combine(_cachePath, Conflict.Name);
      var desc = "";
      foreach (var addon in Addons) {
        var names = addon.Name == Conflict.Name
          ? $"\"{Conflict.Name}\" from `{Conflict.ConfigFilePath}` and " +
            $"\"{addon.Name}\" from `{addon.ConfigFilePath}`"
          : $"\"{Conflict.Name}\" and \"{addon.Name}\"";
        if (addon.Subfolder == Conflict.Subfolder) {
          desc = $"Both {names} install the subfolder " +
            $"`{addon.Subfolder}` from `{addon.Url}` to `{destPath}` on two " +
            $"different branches: `{Conflict.Checkout}`, `{addon.Checkout}`.";
          buffer.Append(desc + "\n\n");
        }
        else if (addon.Checkout == Conflict.Checkout) {
          desc = $"Both {names} install different subfolders " +
            $"(`{Conflict.Subfolder}`, `{addon.Subfolder}`) on the same " +
            $"branch `{addon.Checkout}` from `{addon.Url}` to `{destPath}`.";
          buffer.Append(desc + "\n\n");
        }
      }
      return
        $"The addon \"{Conflict.Name}\" could conflict with the previously " +
        $"installed addon{s}.\n\n" +
        $"  Attempted to install: {Conflict}" +
        buffer.ToString();
    }
  }

  public record ConflictingDestinationPathEvent
    : ReportableDependencyEvent, IDependencyNotInstalledEvent {
    public RequiredAddon Conflict { get; init; }
    public RequiredAddon Addon { get; init; }
    private readonly string _cachePath;

    public ConflictingDestinationPathEvent(
      RequiredAddon conflict,
      RequiredAddon addon,
      Config config
    ) {
      Conflict = conflict;
      Addon = addon;
      _cachePath = config.CachePath;
    }

    public override string ToString() {
      var path = Path.Combine(_cachePath, Conflict.Name);
      return $"Cannot install \"{Conflict.Name}\" because it would conflict " +
      $"with a previously installed addon of the same name.\n\n" +
      $"Both addons would be installed to the same path: `{path}`.\n\n" +
      $"  Attempted to install: {Conflict}\n\n" +
      $"  Previously installed: {Addon}";
    }
  }

  public record DependencyAlreadyInstalledEvent()
    : DependencyEvent, IDependencyNotInstalledEvent;
  public record DependencyInstalledEvent() : DependencyEvent;
}
