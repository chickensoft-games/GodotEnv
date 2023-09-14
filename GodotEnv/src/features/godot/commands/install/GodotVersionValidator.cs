namespace Chickensoft.GodotEnv.Features.Godot.Commands;

using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Features.Godot.Models;
using CliFx.Extensibility;

/// <summary>
/// Validates a Godot version argument.
/// </summary>
public class GodotVersionValidator : BindingValidator<string> {
  public GodotVersionValidator(IExecutionContext _) { }

  public override BindingValidationError? Validate(string? value) {
    if (value is not string str) {
      return Error("Version cannot be null.");
    }
    else if (str.StartsWith('v')) {
      return Error("Version should not start with 'v'.");
    }
    else if (!SemanticVersion.IsValid(str)) {
      return Error($"Version '{str}' is not a valid semantic version.");
    }
    else {
      return Ok();
    }
  }
}
