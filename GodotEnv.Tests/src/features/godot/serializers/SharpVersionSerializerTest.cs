namespace Chickensoft.GodotEnv.Tests.Features.Godot.Models;

using Chickensoft.GodotEnv.Features.Godot.Models;
using Chickensoft.GodotEnv.Features.Godot.Serializers;
using Shouldly;
using Xunit;

public class SharpVersionSerializerTest
{
  public static TheoryData<(GodotVersionNumber, string)> CorrectSharpSerializationTestData()
  {
    return [
      (new GodotVersionNumber(0, 0, 1, "stable", -1), "0.0.1"),
      (new GodotVersionNumber(1, 2, 0, "stable", -1), "1.2.0"),
      (new GodotVersionNumber(1, 2, 3, "stable", -1), "1.2.3"),
      (new GodotVersionNumber(1, 2, 0, "label", 1), "1.2.0-label.1"),
      (new GodotVersionNumber(1, 2, 3, "label", 23), "1.2.3-label.23"),
    ];
  }

  [Theory]
  [MemberData(nameof(CorrectSharpSerializationTestData))]
  public void CorrectSerialization((GodotVersionNumber test, string expected) testAndExpected)
  {
    var converter = new SharpVersionSerializer();
    Assert.Equal(testAndExpected.expected, converter.Serialize(new AnyDotnetStatusGodotVersion(testAndExpected.test)));
    Assert.Equal(testAndExpected.expected, converter.Serialize(new SpecificDotnetStatusGodotVersion(testAndExpected.test, true)));
    Assert.Equal(testAndExpected.expected, converter.Serialize(new SpecificDotnetStatusGodotVersion(testAndExpected.test, false)));
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
