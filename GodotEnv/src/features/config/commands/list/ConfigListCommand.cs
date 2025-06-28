namespace Chickensoft.GodotEnv.Features.Config.Commands.List;

using System;
using System.Linq;
using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Models;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

[Command("config list", Description = "List godotenv configuration entries.")]
public class ConfigListCommand : ICommand, ICliCommand {
  public IExecutionContext ExecutionContext { get; set; }

  [CommandParameter(0, IsRequired = false)]
  public string? ConfigKey { get; set; }

  public ConfigListCommand(IExecutionContext executionContext) {
    ExecutionContext = executionContext;
  }

  public ValueTask ExecuteAsync(IConsole console) {
    var log = ExecutionContext.CreateLog(console);
    var config = ExecutionContext.Config;
    log.Print(string.Empty);
    if (!string.IsNullOrEmpty(ConfigKey)) {
      try {
        log.Print($"{ConfigKey} = {config.Get(ConfigKey)}");
      }
      catch (Exception) {
        log.Print($"""
          "{ConfigKey}" is not a valid configuration key. Try
          "godotenv config list" for a complete list of all entries.
          """);
      }
    }
    else {
      foreach (
        var keyValuePair in config.AsEnumerable().OrderBy(
          (kvp) => kvp.Key,
          StringComparer.InvariantCulture
        )
      ) {
        if (!string.IsNullOrEmpty(keyValuePair.Value)) {
          log.Print($"{keyValuePair.Key} = {keyValuePair.Value}");
        }
      }
    }
    return new();
  }
}
