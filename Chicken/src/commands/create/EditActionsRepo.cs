namespace Chickensoft.Chicken;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;
using CaseExtensions;
using CliFx.Exceptions;

public interface IEditActionsRepo {
  string RepoPath { get; }
  EditActions EditActions { get; }
  Dictionary<EditAction, List<Exception>> Warnings { get; }

  EditActionsValidationResult Validate();
  List<Exception> PerformEdits(IEnumerable<IEditActionSpecific> actions);
}

public class EditActionsRepo : IEditActionsRepo {
  private readonly IApp _app;
  private readonly IFileSystem _fs;

  public string RepoPath { get; }
  public IDictionary<string, dynamic?> Inputs { get; init; }
  public EditActions EditActions { get; init; }
  public Dictionary<EditAction, List<Exception>> Warnings {
    get; private set;
  }

  public EditActionsRepo(
    IApp app,
    IFileSystem fs,
    string repoPath,
    EditActions editActions,
    IDictionary<string, dynamic?> inputs
  ) {
    _app = app;
    _fs = fs;
    RepoPath = repoPath;
    Inputs = inputs;
    EditActions = editActions;
    Warnings = new();
  }

  /// <summary>
  /// Case-insensitive regular expression that matches instances of
  /// `{variableName:casing}` in a string, where casing is PascalCase,
  /// camelCase, lowercase, UPPERCASE, or snake_case.
  /// <br />
  /// Should match the following:
  /// <c>hello, {name:snake_case}!</c><br />
  /// <c>hello, {name:PascalCase}!</c><br />
  /// <c>hello, { name: camelCase}!</c><br />
  /// <c>hello, { name: lowercase}!</c><br />
  /// <c>hello, { name: UPPERCASE}!</c><br />
  /// Should not match the following:<br />
  /// <c>hello, {}!</c><br />
  /// <c>hello, {name:}!</c><br />
  /// <c>hello, {:lowercase}!</c><br />
  /// </summary>
  public static readonly Regex InputVariableRenderer = new(
    @"({(\w+)(?::(snake_case|pascalcase|camelcase|lowercase|uppercase))?})",
    RegexOptions.IgnoreCase
  );

  public List<Exception> PerformEdits(
    IEnumerable<IEditActionSpecific> actions
  ) {
    var problems = new List<Exception>();
    foreach (var action in actions) {
      try {
        action.Perform(_app, _fs, RepoPath);
      }
      catch (Exception e) {
        problems.Add(e);
      }
    }
    return problems;
  }

  /// <summary>
  /// Validates the edit actions and returns a result containing the specific
  /// actions to be performed and any warnings about malformed actions that were
  /// not able to be read.
  /// </summary>
  /// <param name="editActions">Edit actions to validate.</param>
  /// <returns>Edit actions validation result.</returns>
  public EditActionsValidationResult Validate() {
    Warnings = new();
    var actions = new List<IEditActionSpecific>();
    for (var i = 0; i < EditActions.Actions.Count; i++) {
      var editAction = EditActions.Actions[i];
      var type = editAction.Type;
      var file = CheckFile(i, "file", editAction);
      switch (type) {
        case "edit": {
            var find = Prop(i, "find", editAction);
            var replace = Prop(i, "replace", editAction);
            if (Warnings.ContainsKey(editAction)) { continue; }
            actions.Add(new Edit(file!, find!, replace!));
            break;
          }
        case "rename": {
            var to = GetFile(i, "to", editAction);
            if (Warnings.ContainsKey(editAction)) { continue; }
            actions.Add(new Rename(file!, to!));
            break;
          }
        case "guid": {
            var replace = Prop(i, "replace", editAction);
            if (Warnings.ContainsKey(editAction)) { continue; }
            actions.Add(new GooeyId(file!, replace!));
            break;
          }
        default:
          RegisterWarning(
            editAction,
            new CommandException($"Unknown edit action type '{type}'.")
          );
          break;
      }
    }
    return new(new Dictionary<EditAction, List<Exception>>(Warnings), actions);
  }

  /// <summary>
  /// Gets a property from an edit action, or adds a warning if it does not
  /// exist in the edit action's dictionary of extra properties.
  /// </summary>
  /// <param name="index">Index of the edit action.</param>
  /// <param name="propertyName">Name of the property to find.</param>
  /// <param name="editAction">Edit action.</param>
  /// <param name="warnings">List of warnings. Will be added to if the requested
  /// property cannot be found.</param>
  /// <returns></returns>
  internal string? Prop(
    int index,
    string propertyName,
    EditAction editAction
  ) {
    if (editAction.Properties.TryGetValue(propertyName, out var value)) {
      return Render((string)value);
    }

    RegisterWarning(
      editAction,
      new CommandException(
        $"Invalid edit action `{editAction.Type}` at index {index}. " +
        $"Missing property `{propertyName}`."
      )
    );
    return null;
  }

  // Gets a fully resolved, santized file path guaranteed to be within the
  // repo path we are working on. Registers a warning if the file doesn't exist.
  internal string? CheckFile(
    int index,
    string propertyName,
    EditAction editAction
  ) {
    var file = Prop(index, propertyName, editAction);
    if (file == null) { return null; }
    var path = file.SanitizePath(RepoPath);
    if (!_fs.File.Exists(path)) {
      RegisterWarning(
        editAction,
        new CommandException(
          $"Invalid edit action `{editAction.Type}` at index {index}. " +
          $"Cannot find file `{file}` at `{path}`."
        )
      );
    }
    return path;
  }

  // Gets a fully resolved, santized file path guaranteed to be within the
  // repo path we are working on.
  internal string? GetFile(
    int index, string propertyName, EditAction editAction
  ) {
    var file = Prop(index, propertyName, editAction);
    if (file == null) { return null; }
    var path = file.SanitizePath(RepoPath);
    return path;
  }

  // Registers a warning for an edit action. Requires the edit action, the
  // dictionary of warnings, and the exception to add.
  internal void RegisterWarning(EditAction editAction, Exception exception)
    => Warnings[editAction] = (
      Warnings.ContainsKey(editAction)
        ? Warnings[editAction]
        : new List<Exception>()
    ).Concat(new List<Exception>() { exception }).ToList();

  // Given a list of input variables, replaces every {variableName} in the given
  // string with the input variable's value. Casing can be specified, as well:
  // {variableName:casing}
  internal string Render(string text) {
    var matchCollection = InputVariableRenderer.Matches(text);
    var rendered = text;
    foreach (var match in matchCollection.Cast<Match>()) {
      var startIndex = match.Index;
      var length = match.Length;
      var variableName = match.Groups[2].Value;
      var casing = match.Groups[3].Value;
      if (!Inputs.TryGetValue(variableName, out var value)) {
        // Ignore unknown variables.
        continue;
      }
      string renderedValue;
      if (value is string str) {
        renderedValue = casing switch {
          "snake_case" => str.ToSnakeCase(),
          "pascalcase" => str.ToPascalCase(),
          "camelcase" => str.ToCamelCase(),
          "lowercase" => str.ToLower(CultureInfo.CurrentCulture),
          "uppercase" => str.ToUpper(CultureInfo.CurrentCulture),
          _ => value
        };
      }
      else {
        renderedValue = value?.ToString() ?? "";
      }

      rendered = rendered.Remove(startIndex, length);
      rendered = rendered.Insert(startIndex, renderedValue);
    }
    return rendered;
  }
}
