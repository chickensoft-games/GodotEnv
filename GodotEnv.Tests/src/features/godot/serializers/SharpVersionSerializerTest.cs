namespace Chickensoft.GodotEnv.Tests.Features.Godot.Models;

using System.Collections.Generic;
using Chickensoft.GodotEnv.Features.Godot.Models;
using Chickensoft.GodotEnv.Features.Godot.Serializers;
using Shouldly;
using Xunit;

public class SharpVersionSerializerTest
{
  public static IEnumerable<object[]> CorrectSharpSerializationTestData()
  {
    yield return [new GodotVersionNumber(0, 0, 1, "stable", -1), "0.0.1"];
    yield return [new GodotVersionNumber(1, 2, 0, "stable", -1), "1.2.0"];
    yield return [new GodotVersionNumber(1, 2, 3, "stable", -1), "1.2.3"];
    yield return [new GodotVersionNumber(1, 2, 0, "label", 1), "1.2.0-label.1"];
    yield return [new GodotVersionNumber(1, 2, 3, "label", 23), "1.2.3-label.23"];
  }

  [Theory]
  [MemberData(nameof(CorrectSharpSerializationTestData))]
  public void CorrectSerialization(GodotVersionNumber toFormat, string expected)
  {
    var converter = new SharpVersionSerializer();
    Assert.Equal(expected, converter.Serialize(new AnyDotnetStatusGodotVersion(toFormat)));
    Assert.Equal(expected, converter.Serialize(new SpecificDotnetStatusGodotVersion(toFormat, true)));
    Assert.Equal(expected, converter.Serialize(new SpecificDotnetStatusGodotVersion(toFormat, false)));
  }

  [Fact]
  public void CorrectDotnetStatusSerialization()
  {
    var serializer = new SharpVersionSerializer();
    serializer.SerializeWithDotnetStatus(new SpecificDotnetStatusGodotVersion(4, 4, 1, "stable", -1, true))
      .ShouldBe("4.4.1 dotnet");
  }

  [Fact]
  public void CorrectNoDotnetStatusSerialization()
  {
    var serializer = new SharpVersionSerializer();
    serializer.SerializeWithDotnetStatus(new SpecificDotnetStatusGodotVersion(4, 4, 1, "stable", -1, false))
      .ShouldBe("4.4.1 no-dotnet");
  }
}
