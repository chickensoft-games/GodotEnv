namespace Chickensoft.GodotEnv.Common.Models;

using Chickensoft.GodotEnv.Features.Addons.Commands;
using Chickensoft.GodotEnv.Features.Addons.Domain;
using Chickensoft.GodotEnv.Features.Addons.Models;

public record AddonsContext(
  AddonsFile MainAddonsFile,
  IAddonsFileRepository AddonsFileRepo,
  AddonsConfiguration AddonsConfig,
  IAddonsRepository AddonsRepo,
  IAddonGraph AddonGraph,
  IAddonsInstaller AddonsInstaller
) : IAddonsContext;

/// <summary>Addons feature dependencies.</summary>
public interface IAddonsContext {
  /// <summary>Main addons file.</summary>
  AddonsFile MainAddonsFile { get; }

  /// <summary>Addons file repository.</summary>
  IAddonsFileRepository AddonsFileRepo { get; }

  /// <summary>Addons configuration.</summary>
  AddonsConfiguration AddonsConfig { get; }

  /// <summary>Addons repository.</summary>
  IAddonsRepository AddonsRepo { get; }

  /// <summary>Addons dependency graph.</summary>
  IAddonGraph AddonGraph { get; }

  /// <summary>Addons state logic.</summary>
  IAddonsInstaller AddonsInstaller { get; }
}
