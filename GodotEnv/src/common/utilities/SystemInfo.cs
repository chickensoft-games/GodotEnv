namespace GodotEnv.Common.Utilities;

using System;
using System.Runtime.InteropServices;
using Chickensoft.GodotEnv.Common.Models;

public interface ISystemInfo {
  public static OSFamily OSFamily { get; }
  public static OSType OS { get; }
  public static CPUArch CPUArch { get; }
}

public class SystemInfo : ISystemInfo {
  public static OSFamily OSFamily { get; }
  public static OSType OS { get; }
  public static CPUArch CPUArch { get; }

  static SystemInfo() {
    OS = IsOSPlatform(OSPlatform.OSX)
      ? OSType.MacOS
      : IsOSPlatform(OSPlatform.Linux)
        ? OSType.Linux
        : IsOSPlatform(OSPlatform.Windows)
          ? OSType.Windows
          : OSType.Unknown;

    // NOTE: 'Environment.OSVersion.Platform' treates MacOS not like Unix.

    OSFamily = OS == OSType.Windows ? OSFamily.Windows : OSFamily.Unix;

    CPUArch = RuntimeInformation.ProcessArchitecture switch {
      Architecture.X64 => CPUArch.X64,
      Architecture.X86 => CPUArch.X86,
      Architecture.Arm64 => CPUArch.Arm64,
      Architecture.Arm => CPUArch.Arm,
      _ => CPUArch.Other,
    };
  }

  private static Func<OSPlatform, bool> IsOSPlatformDefault { get; } =
RuntimeInformation.IsOSPlatform;

  private static Func<OSPlatform, bool> IsOSPlatform { get; set; } =
    IsOSPlatformDefault;
}
