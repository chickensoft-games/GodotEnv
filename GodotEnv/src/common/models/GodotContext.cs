namespace Chickensoft.GodotEnv.Common.Models;

using Chickensoft.GodotEnv.Features.Godot.Domain;
using Chickensoft.GodotEnv.Features.Godot.Models;

public record GodotContext(
  IGodotEnvironment Platform,
  IGodotRepository GodotRepo,
  IGodotVersionSpecifierRepository VersionRepo
) : IGodotContext;

/// <summary>Godot feature dependencies.</summary>
public interface IGodotContext
{
  /// <summary>Godot environment.</summary>
  IGodotEnvironment Platform { get; }

  /// <summary>Godot installations repository.</summary>
  IGodotRepository GodotRepo { get; }

  /// <summary>Godot version-specifying repository.</summary>
  IGodotVersionSpecifierRepository VersionRepo { get; }
}
