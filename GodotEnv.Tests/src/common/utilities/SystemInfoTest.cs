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
    ISystemInfo systemInfo = new SystemInfo { IsOSPlatformProxy = (platform) => platform == OSPlatform.Windows, CpuArchProxy = Architecture.Arm64 };
    systemInfo.OS.ShouldBe(OSType.Windows);
    systemInfo.OSFamily.ShouldBe(OSFamily.Windows);
    systemInfo.CpuArch.ShouldBe(CpuArch.Arm64);
  }

  [Fact]
  public void IsUnknownOS()
  {
    ISystemInfo systemInfo = new SystemInfo { IsOSPlatformProxy = (platform) => platform == OSPlatform.FreeBSD };
    systemInfo.OS.ShouldBe(OSType.Unknown);
    systemInfo.OSFamily.ShouldBe(OSFamily.Unknown);
  }

  [Fact]
  public void IsUnknownCpuArch()
  {
    ISystemInfo systemInfo = new SystemInfo { CpuArchProxy = Architecture.Ppc64le };
    systemInfo.CpuArch.ShouldBe(CpuArch.Other);
  }

  [Fact]
  public void IsCpuX64WhenRuntimeArchX64()
  {
    ISystemInfo systemInfo = new SystemInfo { CpuArchProxy = Architecture.X64 };
    systemInfo.CpuArch.ShouldBe(CpuArch.X64);
  }

  [Fact]
  public void IsCpuArm64WhenRuntimeArchArm64()
  {
    ISystemInfo systemInfo = new SystemInfo { CpuArchProxy = Architecture.Arm64 };
    systemInfo.CpuArch.ShouldBe(CpuArch.Arm64);
  }
}
