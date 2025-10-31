namespace Chickensoft.GodotEnv.Features.Godot.Commands;

using Chickensoft.GodotEnv.Common.Models;
using CliFx.Extensibility;

/// <summary>
/// Validates a Godot version argument.
/// </summary>
public class GodotVersionValidator : BindingValidator<string>
{
  public IExecutionContext ExecutionContext { get; }
  public GodotVersionValidator(IExecutionContext context)
  {
    ExecutionContext = context;
  }

  public override BindingValidationError? Validate(string? value)
  {
    if (value is not string str)
    {
      return Error("Version cannot be null.");
    }
    var result = ExecutionContext
      .Godot
      .GodotRepo
      .VersionDeserializer
      .Deserialize(str);
    return result.IsSuccess ?
      Ok() :
      Error($"Version '{str}' is invalid: {result.Error}.");
  }
}
