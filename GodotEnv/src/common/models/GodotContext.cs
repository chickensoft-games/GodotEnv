namespace Chickensoft.GodotEnv.Common.Models;

using Chickensoft.GodotEnv.Features.Godot.Domain;
using Chickensoft.GodotEnv.Features.Godot.Models;

public record GodotContext(
  IGodotEnvironment Platform,
  IGodotRepository GodotRepo,
  IGodotVersionSpecifierRepository VersionRepo
) : IGodotContext;

/// <summary>Godot feature dependencies.</summary>
public interface IGodotContext {
  /// <summary>Godot environment.</summary>
  public IGodotEnvironment Platform { get; }

  /// <summary>Godot installations repository.</summary>
  public IGodotRepository GodotRepo { get; }

  /// <summary>Godot version-specifying repository.</summary>
  public IGodotVersionSpecifierRepository VersionRepo { get; }
}
