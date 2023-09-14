namespace Chickensoft.GodotEnv.Common.Models;

public interface ICliCommand {
  /// <summary>
  /// Execution context. The execution context contains information
  /// known as soon as the app is executed (like command line arguments).
  /// Commands can reference this information to determine how to execute, if
  /// needed.
  /// </summary>
  IExecutionContext ExecutionContext { get; }
}
