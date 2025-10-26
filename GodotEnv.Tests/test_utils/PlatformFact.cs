namespace Chickensoft.GodotEnv.Tests;

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
  public PlatformFact(TestPlatform testPlatform)
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
