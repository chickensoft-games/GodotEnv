namespace Chickensoft.GodotEnv.Tests;

using CliFx.Infrastructure;

public class OutputTestFakeInMemoryConsole : FakeInMemoryConsole
{

  // Used for testing console output to ensure consistent line endings across platforms.
  public new string ReadOutputString() => Output.Encoding.GetString(ReadOutputBytes()).ReplaceLineEndings("\n");
}
