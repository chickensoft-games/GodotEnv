namespace Chickensoft.GodotEnv.Features.Godot.Models;

public interface IVersionStringConverter {
  public GodotVersion ParseVersion(string version);
  public string VersionString(GodotVersion version);
}
