namespace Chickensoft.GodotEnv.Common.Utilities;

public interface IComputer {
  /// <summary>
  /// Creates a new shell in the given working directory. A shell can be used
  /// to run terminal commands within the working directory.
  /// </summary>
  /// <param name="workingDir">Current working directory of the shell.</param>
  /// <returns>A new shell.</returns>
  IShell CreateShell(string workingDir);
}

public class Computer : IComputer {
  public IShell CreateShell(string workingDir) =>
    new Shell(new ProcessRunner(), workingDir);
}
