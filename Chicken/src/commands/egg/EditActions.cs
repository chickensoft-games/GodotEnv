namespace Chickensoft.Chicken;

using System.Collections.Generic;
using Newtonsoft.Json;

/// <summary>
/// Represents an EDIT_ACTIONS.jsonc file. An edit actions file is a JSON file
/// that contains a list of edit actions like editing text in a particular file,
/// renaming a file, deleting a file, etc.
/// </summary>
public record EditActions {
  [JsonProperty("inputs")]
  public List<EditActionInput> Inputs { get; init; }

  [JsonProperty("actions")]
  public List<EditAction> Actions { get; init; }

  [JsonConstructor]
  public EditActions(
    List<EditActionInput>? inputs,
    List<EditAction>? actions
  ) {
    Inputs = inputs ?? new();
    Actions = actions ?? new();
  }
}

public record EditActionInput {
  [JsonProperty("name")]
  public string Name { get; init; }

  [JsonProperty("type")]
  public string Type { get; init; }

  [JsonProperty("default")]
  public string? Default { get; init; }

  [JsonConstructor]
  public EditActionInput(
    string name,
    string type,
    string? @default
  ) {
    Name = name;
    Type = type;
    Default = @default;
  }
}

public record EditAction {
  [JsonProperty("type")]
  public string Type { get; init; }

  // Remaining json keys that are not mapped directly.
  // These are specific to the type of action.
  [JsonExtensionData]
  public Dictionary<string, object> Properties { get; init; }

  [JsonConstructor]
  public EditAction(
    string type,
    Dictionary<string, object>? properties
  ) {
    Type = type;
    Properties = properties ?? new();
  }
}
