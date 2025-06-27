namespace Chickensoft.GodotEnv.Features.Config.Commands.List;

using System;
using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Models;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

[Command("config set", Description = "Set a godotenv configuration entry.")]
public class ConfigSetCommand : ICommand, ICliCommand {
  public IExecutionContext ExecutionContext { get; set; }

  [CommandParameter(0, IsRequired = true)]
  public string ConfigKey { get; set; } = string.Empty;

  [CommandParameter(1, IsRequired = true)]
  public string ConfigValue { get; set; } = string.Empty;

  public ConfigSetCommand(IExecutionContext executionContext) {
    ExecutionContext = executionContext;
  }

  public ValueTask ExecuteAsync(IConsole console) {
    var log = ExecutionContext.CreateLog(console);
    var config = ExecutionContext.Config;
    log.Print(string.Empty);
    try {
      config.Set(ConfigKey, ConfigValue);
    }
    catch (Exception) {
      log.Print($"""
      "{ConfigKey}" is not a valid configuration key, or "{ConfigValue}" is
      not a valid value for "{ConfigKey}". Try "godotenv config list" for a
      complete list of all entries.
      """);
    }
    return new();
  }
}
