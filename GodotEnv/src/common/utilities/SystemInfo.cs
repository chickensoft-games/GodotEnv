namespace GodotEnv.Common.Utilities;

using System;
using System.Runtime.InteropServices;
using Chickensoft.GodotEnv.Common.Models;

public interface ISystemInfo {
  public OSType OS { get; }

  public OSFamily OSFamily =>
    OS switch {
      OSType.Windows => OSFamily.Windows,
      OSType.Linux or OSType.MacOS => OSFamily.Unix,
      _ => OSFamily.Unknown
    };

  public CPUArch CPUArch { get; }
}

public class SystemInfo : ISystemInfo {
  public OSType OS {
    get =>
      IsOSPlatformProxy(OSPlatform.OSX)
        ? OSType.MacOS
        : IsOSPlatformProxy(OSPlatform.Linux)
          ? OSType.Linux
          : IsOSPlatformProxy(OSPlatform.Windows)
            ? OSType.Windows
            : OSType.Unknown;
  }

  public CPUArch CPUArch => CPUArchProxy switch {
    Architecture.X64 => CPUArch.X64,
    Architecture.X86 => CPUArch.X86,
    Architecture.Arm64 => CPUArch.Arm64,
    Architecture.Arm => CPUArch.Arm,
    _ => CPUArch.Other,
  };

  // Shims for testing.
  public static Func<OSPlatform, bool> IsOSPlatformDefault { get; } =
    RuntimeInformation.IsOSPlatform;

  public Func<OSPlatform, bool> IsOSPlatformProxy { get; set; } =
    IsOSPlatformDefault;

  public static Architecture CPUArchDefault { get; } =
    RuntimeInformation.ProcessArchitecture;

  public Architecture CPUArchProxy { get; set; } =
    CPUArchDefault;
}

public class MockSystemInfo(OSType os, CPUArch cpuArch) : ISystemInfo {
  public OSType OS { get; } = os;
  public CPUArch CPUArch { get; } = cpuArch;
}
