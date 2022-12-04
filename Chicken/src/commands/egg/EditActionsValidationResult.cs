namespace Chickensoft.Chicken;

using System;
using System.Collections.Generic;

public record EditActionsValidationResult(
  Dictionary<EditAction, List<Exception>> Warnings,
  List<IEditActionSpecific> Actions
) {
  public bool Success => Warnings.Count == 0;
}
