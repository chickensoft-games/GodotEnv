namespace Chickensoft.GodotEnv.Tests;

using System;
using System.Diagnostics.CodeAnalysis;
using Chickensoft.GodotEnv.Features.Godot.Models;
using Xunit.Sdk;

public class GodotVersionNumberSerializer : IXunitSerializer
{
  public bool IsSerializable(Type type, object? value, [NotNullWhen(false)] out string? failureReason)
  {
    if (value is GodotVersionNumber)
    {
      failureReason = string.Empty;
      return true;
    }
    failureReason = $"{value?.GetType()} is not {nameof(GodotVersionNumber)}";
    return false;
  }

  public object Deserialize(Type type, string serializedValue)
  {
    if (type == typeof(GodotVersionNumber))
    {
      var data = SerializationHelper.Instance.Deserialize<(int, int, int, string, int)>(serializedValue);
      return new GodotVersionNumber(data.Item1, data.Item2, data.Item3, data.Item4, data.Item5);
    }
    throw new ArgumentException($"Cannot deserialize non-{nameof(GodotVersionNumber)} type {type}");
  }

  public string Serialize(object value)
  {
    if (value is GodotVersionNumber number)
    {
      return SerializationHelper.Instance.Serialize(
        (number.Major, number.Minor, number.Patch, number.Label, number.LabelNumber)
      );
    }
    throw new ArgumentException($"{value?.GetType()} is not {nameof(GodotVersionNumber)}");
  }
}
