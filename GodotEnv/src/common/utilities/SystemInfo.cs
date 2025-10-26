namespace Chickensoft.GodotEnv.Common.Utilities;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Chickensoft.GodotEnv.Common.Models;

/// <summary>
/// Immutable system information (OS, CPU architecture, ...).
/// </summary>
public interface ISystemInfo
{
  OSType OS { get; }

  [
    SuppressMessage(
      "Style",
      "IDE0072",
      Justification = "Missing cases handled by default"
    )
  ]
  OSFamily OSFamily =>
    OS switch
    {
      OSType.Windows => OSFamily.Windows,
      OSType.Linux or OSType.MacOS => OSFamily.Unix,
      _ => OSFamily.Unknown
    };

  CpuArch CpuArch { get; }
}

public class SystemInfo : ISystemInfo
{
  public OSType OS =>
    IsOSPlatformProxy(OSPlatform.OSX)
      ? OSType.MacOS
      : IsOSPlatformProxy(OSPlatform.Linux)
        ? OSType.Linux
        : IsOSPlatformProxy(OSPlatform.Windows)
          ? OSType.Windows
          : OSType.Unknown;

  [
    SuppressMessage(
      "Style",
      "IDE0072",
      Justification = "Missing cases handled by default"
    )
  ]
  public CpuArch CpuArch => CpuArchProxy switch
  {
    Architecture.X64 => CpuArch.X64,
    Architecture.X86 => CpuArch.X86,
    Architecture.Arm64 => CpuArch.Arm64,
    Architecture.Arm => CpuArch.Arm,
    _ => CpuArch.Other,
  };

  // Shims for testing.
  public static Func<OSPlatform, bool> IsOSPlatformDefault { get; } =
    RuntimeInformation.IsOSPlatform;

  public Func<OSPlatform, bool> IsOSPlatformProxy { get; set; } =
    IsOSPlatformDefault;

  public static Architecture CpuArchDefault { get; } =
    RuntimeInformation.ProcessArchitecture;

  public Architecture CpuArchProxy { get; set; } =
    CpuArchDefault;
}

public class MockSystemInfo(OSType os, CpuArch cpuArch) : ISystemInfo
{
  public OSType OS { get; } = os;
  public CpuArch CpuArch { get; } = cpuArch;
}
