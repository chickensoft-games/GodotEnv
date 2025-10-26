namespace Chickensoft.GodotEnv.Tests;

using System.Runtime.InteropServices;
using Chickensoft.GodotEnv.Common.Utilities;
using Common.Models;
using Shouldly;
using Xunit;

public class SystemInfoTest
{
  [Fact]
  public void IsWindowsFamilyWhenWindowsOs()
  {
    ISystemInfo systemInfo = new SystemInfo { IsOSPlatformProxy = (platform) => platform == OSPlatform.Windows };
    systemInfo.OS.ShouldBe(OSType.Windows);
    systemInfo.OSFamily.ShouldBe(OSFamily.Windows);
  }

  [Fact]
  public void IsUnixFamilyWhenLinuxOs()
  {
    ISystemInfo systemInfo = new SystemInfo { IsOSPlatformProxy = (platform) => platform == OSPlatform.Linux };
    systemInfo.OS.ShouldBe(OSType.Linux);
    systemInfo.OSFamily.ShouldBe(OSFamily.Unix);
  }

  [Fact]
  public void IsUnixFamilyWhenMacOs()
  {
    ISystemInfo systemInfo = new SystemInfo { IsOSPlatformProxy = (platform) => platform == OSPlatform.OSX };
    systemInfo.OS.ShouldBe(OSType.MacOS);
    systemInfo.OSFamily.ShouldBe(OSFamily.Unix);
  }

  [Fact]
  public void IsWindowsOnArm()
  {
    ISystemInfo systemInfo = new SystemInfo { IsOSPlatformProxy = (platform) => platform == OSPlatform.Windows, CPUArchProxy = Architecture.Arm64 };
    systemInfo.OS.ShouldBe(OSType.Windows);
    systemInfo.OSFamily.ShouldBe(OSFamily.Windows);
    systemInfo.CPUArch.ShouldBe(CPUArch.Arm64);
  }

  [Fact]
  public void IsUnknownOS()
  {
    ISystemInfo systemInfo = new SystemInfo { IsOSPlatformProxy = (platform) => platform == OSPlatform.FreeBSD };
    systemInfo.OS.ShouldBe(OSType.Unknown);
    systemInfo.OSFamily.ShouldBe(OSFamily.Unknown);
  }

  [Fact]
  public void IsUnknownCPUArch()
  {
    ISystemInfo systemInfo = new SystemInfo { CPUArchProxy = Architecture.Ppc64le };
    systemInfo.CPUArch.ShouldBe(CPUArch.Other);
  }

  [Fact]
  public void IsCPUX64WhenRuntimeArchX64()
  {
    ISystemInfo systemInfo = new SystemInfo { CPUArchProxy = Architecture.X64 };
    systemInfo.CPUArch.ShouldBe(CPUArch.X64);
  }

  [Fact]
  public void IsCPUArm64WhenRuntimeArchArm64()
  {
    ISystemInfo systemInfo = new SystemInfo { CPUArchProxy = Architecture.Arm64 };
    systemInfo.CPUArch.ShouldBe(CPUArch.Arm64);
  }
}
