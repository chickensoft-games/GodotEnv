namespace Chickensoft.GodotEnv.Tests.Features.Godot.Models;

using System.Collections.Generic;
using Chickensoft.GodotEnv.Features.Godot.Models;
using Chickensoft.GodotEnv.Features.Godot.Serializers;
using Shouldly;
using Xunit;

using OldDiskVersionSerializerTestData =
  (
    Chickensoft.GodotEnv.Features.Godot.Models.GodotVersionNumber VersionNumber,
    bool OutputLabelSeparator,
    bool OutputPatchNumber,
    bool OutputStable,
    string VersionString
  );

public class OldDiskVersionSerializerTest
{

  public static IEnumerable<TheoryDataRow<OldDiskVersionSerializerTestData>>
    CorrectOldDiskVersionSerializationTestData()
  {
    yield return (new GodotVersionNumber(0, 0, 1, "stable", -1), true, true, true, "0.0.1-stable");
    yield return (new GodotVersionNumber(0, 0, 1, "stable", -1), true, true, false, "0.0.1");
    yield return (new GodotVersionNumber(0, 0, 1, "stable", -1), true, false, true, "0.0.1-stable");
    yield return (new GodotVersionNumber(0, 0, 1, "stable", -1), true, false, false, "0.0.1");
    yield return (new GodotVersionNumber(0, 0, 1, "stable", -1), false, true, true, "0.0.1-stable");
    yield return (new GodotVersionNumber(0, 0, 1, "stable", -1), false, true, false, "0.0.1");
    yield return (new GodotVersionNumber(0, 0, 1, "stable", -1), false, false, true, "0.0.1-stable");
    yield return (new GodotVersionNumber(0, 0, 1, "stable", -1), false, false, false, "0.0.1");

    yield return (new GodotVersionNumber(1, 2, 0, "stable", -1), true, true, true, "1.2.0-stable");
    yield return (new GodotVersionNumber(1, 2, 0, "stable", -1), true, true, false, "1.2.0");
    yield return (new GodotVersionNumber(1, 2, 0, "stable", -1), true, false, true, "1.2-stable");
    yield return (new GodotVersionNumber(1, 2, 0, "stable", -1), true, false, false, "1.2");
    yield return (new GodotVersionNumber(1, 2, 0, "stable", -1), false, true, true, "1.2.0-stable");
    yield return (new GodotVersionNumber(1, 2, 0, "stable", -1), false, true, false, "1.2.0");
    yield return (new GodotVersionNumber(1, 2, 0, "stable", -1), false, false, true, "1.2-stable");
    yield return (new GodotVersionNumber(1, 2, 0, "stable", -1), false, false, false, "1.2");

    yield return (new GodotVersionNumber(1, 2, 3, "stable", -1), true, true, true, "1.2.3-stable");
    yield return (new GodotVersionNumber(1, 2, 3, "stable", -1), true, true, false, "1.2.3");
    yield return (new GodotVersionNumber(1, 2, 3, "stable", -1), true, false, true, "1.2.3-stable");
    yield return (new GodotVersionNumber(1, 2, 3, "stable", -1), true, false, false, "1.2.3");
    yield return (new GodotVersionNumber(1, 2, 3, "stable", -1), false, true, true, "1.2.3-stable");
    yield return (new GodotVersionNumber(1, 2, 3, "stable", -1), false, true, false, "1.2.3");
    yield return (new GodotVersionNumber(1, 2, 3, "stable", -1), false, false, true, "1.2.3-stable");
    yield return (new GodotVersionNumber(1, 2, 3, "stable", -1), false, false, false, "1.2.3");

    yield return (new GodotVersionNumber(1, 2, 0, "label", 1), true, true, true, "1.2.0-label.1");
    yield return (new GodotVersionNumber(1, 2, 0, "label", 1), true, true, false, "1.2.0-label.1");
    yield return (new GodotVersionNumber(1, 2, 0, "label", 1), true, false, true, "1.2-label.1");
    yield return (new GodotVersionNumber(1, 2, 0, "label", 1), true, false, false, "1.2-label.1");
    yield return (new GodotVersionNumber(1, 2, 0, "label", 1), false, true, true, "1.2.0-label1");
    yield return (new GodotVersionNumber(1, 2, 0, "label", 1), false, true, false, "1.2.0-label1");
    yield return (new GodotVersionNumber(1, 2, 0, "label", 1), false, false, true, "1.2-label1");
    yield return (new GodotVersionNumber(1, 2, 0, "label", 1), false, false, false, "1.2-label1");

    yield return (new GodotVersionNumber(1, 2, 3, "label", 23), true, true, true, "1.2.3-label.23");
    yield return (new GodotVersionNumber(1, 2, 3, "label", 23), true, true, false, "1.2.3-label.23");
    yield return (new GodotVersionNumber(1, 2, 3, "label", 23), true, false, true, "1.2.3-label.23");
    yield return (new GodotVersionNumber(1, 2, 3, "label", 23), true, false, false, "1.2.3-label.23");
    yield return (new GodotVersionNumber(1, 2, 3, "label", 23), false, true, true, "1.2.3-label23");
    yield return (new GodotVersionNumber(1, 2, 3, "label", 23), false, true, false, "1.2.3-label23");
    yield return (new GodotVersionNumber(1, 2, 3, "label", 23), false, false, true, "1.2.3-label23");
    yield return (new GodotVersionNumber(1, 2, 3, "label", 23), false, false, false, "1.2.3-label23");
  }

  [Theory]
  [MemberData(nameof(CorrectOldDiskVersionSerializationTestData))]
  public void CorrectOldDiskVersionSerialization(OldDiskVersionSerializerTestData testData)
  {
    var serializer = new OldDiskVersionSerializer(
      testData.OutputLabelSeparator,
      testData.OutputPatchNumber,
      testData.OutputStable
    );
    Assert.Equal(testData.VersionString, serializer.Serialize(new AnyDotnetStatusGodotVersion(testData.VersionNumber)));
    Assert.Equal(testData.VersionString, serializer.Serialize(new SpecificDotnetStatusGodotVersion(testData.VersionNumber, true)));
    Assert.Equal(testData.VersionString, serializer.Serialize(new SpecificDotnetStatusGodotVersion(testData.VersionNumber, false)));
  }

  [Fact]
  public void CorrectDotnetStatusSerialization()
  {
    var serializer = new OldDiskVersionSerializer();
    serializer.SerializeWithDotnetStatus(new SpecificDotnetStatusGodotVersion(4, 4, 1, "stable", -1, true))
      .ShouldBe("4.4.1 dotnet");
  }

  [Fact]
  public void CorrectNoDotnetStatusSerialization()
  {
    var serializer = new OldDiskVersionSerializer();
    serializer.SerializeWithDotnetStatus(new SpecificDotnetStatusGodotVersion(4, 4, 1, "stable", -1, false))
      .ShouldBe("4.4.1 no-dotnet");
  }
}
