namespace Chickensoft.GodotEnv.Features.Addons.Models;

/// <summary>
/// Addons configuration loaded from an <see cref="AddonsFile"/>.
/// </summary>
/// <param name="ProjectPath">Project path containing the addons file.</param>
/// <param name="AddonsPath">Fully qualified addons installation path.</param>
/// <param name="CachePath">Fully qualified addons cache path.</param>
public record AddonsConfiguration(
  string ProjectPath,
  string AddonsPath,
  string CachePath
);
