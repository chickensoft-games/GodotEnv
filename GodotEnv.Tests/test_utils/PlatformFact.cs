namespace Chickensoft.GodotEnv.Tests;

using System.Runtime.InteropServices;
using Xunit;

public enum TestPlatform {
  Windows,
  MacLinux,
}

public sealed class PlatformFact : FactAttribute {
  public PlatformFact(TestPlatform testPlatform) {
    Skip = testPlatform switch {
      TestPlatform.Windows when !RuntimeInformation.IsOSPlatform(OSPlatform.Windows) =>
        $"Skipped Windows specific test",
      TestPlatform.MacLinux when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) =>
        $"Skipped Mac/Linux specific test",
      _ => Skip
    };
  }
}
