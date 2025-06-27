namespace Chickensoft.GodotEnv.Common.Models;

using Chickensoft.GodotEnv.Common.Utilities;
using Chickensoft.GodotEnv.Features.Godot.Models;
using CliFx.Infrastructure;
using global::GodotEnv.Common.Utilities;

/// <summary>
/// Execution context created by the app before any commands are run. Execution
/// contexts can be used to create a command context, which supplies information
/// and services to a command while allowing commands to be tested.
/// </summary>
public interface IExecutionContext {
  /// <summary>Arguments passed to the app itself.</summary>
  public string[] CliArgs { get; }
  /// <summary>
  /// Dynamic arguments that follow `--` (if supplied by the user). Some
  /// commands, such as template execution, require dynamic arguments be given.
  /// </summary>
  public string[] CommandArgs { get; }
  /// <summary>App package version.</summary>
  public string Version { get; }
  /// <summary>Working directory that the app is running in.</summary>
  public string WorkingDir { get; }
  /// <summary>App configuration settings.</summary>
  public Config Config { get; }
  /// <summary>System information.</summary>
  public ISystemInfo SystemInfo { get; }
  /// <summary>Addons context.</summary>
  public IAddonsContext Addons { get; }
  /// <summary>Godot context.</summary>
  public IGodotContext Godot { get; }

  /// <summary>Creates a log using the specified console.</summary>
  /// <param name="console">Output console.</param>
  /// <returns>Log.</returns>
  public ILog CreateLog(IConsole console);
}

public record ExecutionContext(
  string[] CliArgs,
  string[] CommandArgs,
  string Version,
  string WorkingDir,
  Config Config,
  ISystemInfo SystemInfo,
  IAddonsContext Addons,
  IGodotContext Godot
) : IExecutionContext {
  public ILog CreateLog(IConsole console) => new Log(SystemInfo, console);
}
