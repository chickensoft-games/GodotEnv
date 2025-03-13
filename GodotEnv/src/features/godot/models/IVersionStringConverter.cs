namespace Chickensoft.GodotEnv.Features.Godot.Models;

public interface IVersionStringConverter {
  public DotnetAgnosticGodotVersion ParseVersion(string version);
  public DotnetSpecificGodotVersion ParseVersion(string version, bool isDotnet);
  public string VersionString(GodotVersion version);
}
