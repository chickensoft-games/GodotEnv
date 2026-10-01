namespace Chickensoft.GodotEnv.Tests;

using System;
using System.Diagnostics.CodeAnalysis;
using Chickensoft.GodotEnv.Features.Godot.Models;
using Xunit.Sdk;

public class SpecificDotnetStatusGodotVersionSerializer : IXunitSerializer
{
  public bool IsSerializable(Type type, object? value, [NotNullWhen(false)] out string? failureReason)
  {
    if (value is SpecificDotnetStatusGodotVersion)
    {
      failureReason = string.Empty;
      return true;
    }
    failureReason = $"{value?.GetType()} is not {nameof(SpecificDotnetStatusGodotVersion)}";
    return false;
  }

  public object Deserialize(Type type, string serializedValue)
  {
    if (type == typeof(SpecificDotnetStatusGodotVersion))
    {
      var data = SerializationHelper.Instance.Deserialize<(GodotVersionNumber, bool)>(serializedValue);
      return new SpecificDotnetStatusGodotVersion(data.Item1, data.Item2);
    }
    throw new ArgumentException($"Cannot deserialize non-{nameof(SpecificDotnetStatusGodotVersion)} type {type}");
  }

  public string Serialize(object value)
  {
    if (value is SpecificDotnetStatusGodotVersion version)
    {
      return SerializationHelper.Instance.Serialize((version.Number, version.IsDotnetEnabled));
    }
    throw new ArgumentException($"{value?.GetType()} is not {nameof(SpecificDotnetStatusGodotVersion)}");
  }
}
