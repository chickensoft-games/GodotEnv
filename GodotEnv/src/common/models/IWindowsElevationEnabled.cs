namespace Chickensoft.GodotEnv.Common.Models;

/// <summary>
/// A CLI command that might need elevated administrator privileges on
/// Windows.
/// </summary>
public interface IWindowsElevationEnabled {
  /// <summary>
  /// True if this command will require elevated administrator privileges
  /// on Windows.
  /// </summary>
  bool IsWindowsElevationRequired { get; }
}
