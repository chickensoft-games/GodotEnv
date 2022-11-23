namespace Chickensoft.Chicken;

using System.Collections.Generic;
using CliFx.Exceptions;

public interface IAdditionalArgParser {
  string[] Args { get; init; }
  string? Peek { get; }
  bool Ok { get; }

  string Advance();
  Dictionary<string, dynamic?> Parse();
}

public class AdditionalArgParser : IAdditionalArgParser {
  public string[] Args { get; init; }
  private int _index;

  public string? Peek => _index < Args.Length ? Args[_index] : null;
  public bool Ok => Peek != null;

  public AdditionalArgParser(string[] args) => Args = args;

  public string Advance() => Args[_index++];

  public Dictionary<string, dynamic?> Parse() {
    var args = new Dictionary<string, dynamic?>();
    while (Ok) {
      var arg = Advance();
      if (IsFlag(arg)) {
        var name = arg.TrimStart('-');
        if (Ok && !IsFlag(Peek!)) {
          // Argument has an associated value.
          var value = Advance();
          if (value == "true") {
            args[name] = true;
          }
          else if (value == "false") {
            args[name] = false;
          }
          else if (double.TryParse(value, out var number)) {
            args[name] = number;
          }
          else {
            args[name] = value;
          }
        }
        else {
          // Argument is a boolean flag whose presence implies "true"
          args[name] = true;
        }
      }
      else {
        throw new CommandException(
          $"Unrecognized additional argument: {arg}."
        );
      }
    }
    return args;
  }

  public static bool IsFlag(string arg)
    => arg.StartsWith("--", System.StringComparison.Ordinal);
}
