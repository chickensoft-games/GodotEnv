namespace Chickensoft.GodotEnv.Tests;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xunit;

public enum TestPlatform
{
  Windows,
  MacLinux,
  Mac,
  Linux
}

public sealed class PlatformFact : FactAttribute
{
  public PlatformFact(
    TestPlatform testPlatform,
    [CallerFilePath] string? sourceFilePath = null,
    [CallerLineNumber] int sourceLineNumber = -1
  )
    : base(sourceFilePath, sourceLineNumber)
  {
    Skip = testPlatform switch
    {
      TestPlatform.Windows when !RuntimeInformation.IsOSPlatform(OSPlatform.Windows) =>
        $"Skipped Windows specific test",
      TestPlatform.Mac when !RuntimeInformation.IsOSPlatform(OSPlatform.OSX) =>
        $"Skipped Mac specific test",
      TestPlatform.Linux when !RuntimeInformation.IsOSPlatform(OSPlatform.Linux) =>
        $"Skipped Linux specific test",
      TestPlatform.MacLinux when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) =>
        $"Skipped Mac/Linux specific test",
      _ => Skip
    };
  }
}
