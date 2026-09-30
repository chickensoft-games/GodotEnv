namespace Chickensoft.GodotEnv.Tests;

using System;
using System.Diagnostics.CodeAnalysis;
using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Common.Utilities;
using Xunit.Sdk;

public class MockSystemInfo(OSType os, CpuArch cpuArch) : ISystemInfo
{
  public OSType OS { get; } = os;
  public CpuArch CpuArch { get; } = cpuArch;
}

public class MockSystemInfoSerializer : IXunitSerializer
{
  public bool IsSerializable(Type type, object? value, [NotNullWhen(false)] out string? failureReason)
  {
    if (value is MockSystemInfo)
    {
      failureReason = string.Empty;
      return true;
    }
    failureReason = $"{value?.GetType()} is not a {nameof(MockSystemInfo)}";
    return false;
  }

  public object Deserialize(Type type, string serializedValue)
  {
    if (type == typeof(ISystemInfo))
    {
      var data = SerializationHelper.Instance.Deserialize<(OSType, CpuArch)>(serializedValue);
      return new MockSystemInfo(data.Item1, data.Item2);
    }
    throw new ArgumentException($"Cannot deserialize non-ISystemInfo type {type}");
  }

  public string Serialize(object value)
  {
    if (value is MockSystemInfo mockSystemInfo)
    {
      return SerializationHelper.Instance.Serialize((mockSystemInfo.OS, mockSystemInfo.CpuArch));
    }
    throw new ArgumentException($"{value.GetType()} is not a {nameof(MockSystemInfo)}");
  }
}
