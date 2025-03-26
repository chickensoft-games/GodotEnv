namespace Chickensoft.GodotEnv.Features.Godot.Commands;

using System;
using Chickensoft.GodotEnv.Common.Models;
using CliFx.Extensibility;

/// <summary>
/// Validates a Godot version argument.
/// </summary>
public class GodotVersionValidator : BindingValidator<string> {
  public IExecutionContext ExecutionContext { get; }
  public GodotVersionValidator(IExecutionContext context) {
    ExecutionContext = context;
  }

  public override BindingValidationError? Validate(string? value) {
    if (value is not string str) {
      return Error("Version cannot be null.");
    }
    try {
      ExecutionContext.Godot.GodotRepo.VersionDeserializer.Deserialize(str);
    }
    catch (Exception ex) {
      return Error($"Version '{str}' is invalid: {ex.Message}.");
    }
    return Ok();
  }
}
