namespace Chickensoft.GodotEnv.Features.Godot.Models;

public interface IVersionStringConverter {
  public AnyDotnetStatusGodotVersion ParseVersion(string version);
  public SpecificDotnetStatusGodotVersion ParseVersion(string version, bool isDotnet);
  public string VersionString(GodotVersion version);
}
