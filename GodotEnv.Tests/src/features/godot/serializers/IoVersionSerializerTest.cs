namespace Chickensoft.GodotEnv.Tests.Features.Godot.Models;

using System.Collections.Generic;
using Chickensoft.GodotEnv.Features.Godot.Models;
using Chickensoft.GodotEnv.Features.Godot.Serializers;
using Xunit;

public class IOVersionStringConverterTest {
  public static IEnumerable<object[]> CorrectReleaseVersionSerializationTestData() {
    yield return [new GodotVersionNumber(0, 0, 1, "stable", -1), "0.0.1-stable"];
    yield return [new GodotVersionNumber(1, 2, 0, "stable", -1), "1.2-stable"];
    yield return [new GodotVersionNumber(1, 2, 3, "stable", -1), "1.2.3-stable"];
    yield return [new GodotVersionNumber(1, 2, 0, "label", 1), "1.2-label1"];
    yield return [new GodotVersionNumber(1, 2, 3, "label", 23), "1.2.3-label23"];
  }

  [Theory]
  [MemberData(nameof(CorrectReleaseVersionSerializationTestData))]
  public void CorrectReleaseVersionSerialization(GodotVersionNumber toFormat, string expected) {
    var serializer = new IoVersionSerializer();
    Assert.Equal(expected, serializer.Serialize(new AnyDotnetStatusGodotVersion(toFormat)));
    Assert.Equal(expected, serializer.Serialize(new SpecificDotnetStatusGodotVersion(toFormat, true)));
    Assert.Equal(expected, serializer.Serialize(new SpecificDotnetStatusGodotVersion(toFormat, false)));
  }
}
