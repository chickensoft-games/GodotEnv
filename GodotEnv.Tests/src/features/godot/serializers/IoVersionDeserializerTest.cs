namespace Chickensoft.GodotEnv.Tests.Features.Godot.Models;

using System.Collections.Generic;
using Chickensoft.GodotEnv.Features.Godot.Models;
using Chickensoft.GodotEnv.Features.Godot.Serializers;
using Shouldly;
using Xunit;

using IoVersionDeserializerTestData =
  (string VersionString, Chickensoft.GodotEnv.Features.Godot.Models.GodotVersionNumber VersionNumber);

public class IoVersionDeserializerTest
{
  public static IEnumerable<TheoryDataRow<IoVersionDeserializerTestData>>
    CorrectDeserializationOfValidReleaseVersionsTestData()
  {
    IoVersionDeserializerTestData[] testData = [
        ("1.2.3-stable", new GodotVersionNumber(1, 2, 3, "stable", -1)),
        ("0.2.3-stable", new GodotVersionNumber(0, 2, 3, "stable", -1)),
        ("1.0-stable", new GodotVersionNumber(1, 0, 0, "stable", -1)),
        ("1.0-label1", new GodotVersionNumber(1, 0, 0, "label", 1)),
        ("1.0-label23", new GodotVersionNumber(1, 0, 0, "label", 23)),
        ("1.0.1-label23", new GodotVersionNumber(1, 0, 1, "label", 23))
    ];
    foreach (var testItem in testData)
    {
      yield return testItem;
      yield return ($"v{testItem.VersionString}", testItem.VersionNumber);
    }
  }

  [Theory]
  [MemberData(nameof(CorrectDeserializationOfValidReleaseVersionsTestData))]
  public void CorrectDeserializationOfValidReleaseVersions(IoVersionDeserializerTestData testData)
  {
    var deserializer = new IoVersionDeserializer();
    var parsedAgnostic = deserializer.Deserialize(testData.VersionString);
    parsedAgnostic.IsSuccess.ShouldBeTrue();
    parsedAgnostic.Value.Number.ShouldBe(testData.VersionNumber);
    var parsedDotnet = deserializer.Deserialize(testData.VersionString, true);
    parsedDotnet.IsSuccess.ShouldBeTrue();
    parsedDotnet.Value.Number.ShouldBe(testData.VersionNumber);
    parsedDotnet.Value.IsDotnetEnabled.ShouldBeTrue();
    var parsedNonDotnet = deserializer.Deserialize(testData.VersionString, false);
    parsedNonDotnet.IsSuccess.ShouldBeTrue();
    parsedNonDotnet.Value.Number.ShouldBe(testData.VersionNumber);
    parsedNonDotnet.Value.IsDotnetEnabled.ShouldBeFalse();
  }

  public static IEnumerable<TheoryDataRow<IoVersionDeserializerTestData>>
    CorrectDeserializationOfValidSharpVersionsTestData()
  {
    IoVersionDeserializerTestData[] testData = [
        ("1.2.3", new GodotVersionNumber(1, 2, 3, "stable", -1)),
        ("0.2.3", new GodotVersionNumber(0, 2, 3, "stable", -1)),
        ("1.0.0", new GodotVersionNumber(1, 0, 0, "stable", -1)),
        ("1.0.0-label.1", new GodotVersionNumber(1, 0, 0, "label", 1)),
        ("1.0.0-label.23", new GodotVersionNumber(1, 0, 0, "label", 23)),
        ("1.0.1-label.23", new GodotVersionNumber(1, 0, 1, "label", 23))
    ];
    foreach (var testItem in testData)
    {
      yield return testItem;
      yield return ($"v{testItem.VersionString}", testItem.VersionNumber);
    }
  }

  [Theory]
  [MemberData(nameof(CorrectDeserializationOfValidSharpVersionsTestData))]
  public void CorrectDeserializationOfValidSharpVersions(IoVersionDeserializerTestData testData)
  {
    var deserializer = new IoVersionDeserializer();
    var parsedAgnostic = deserializer.Deserialize(testData.VersionString);
    parsedAgnostic.IsSuccess.ShouldBeTrue();
    parsedAgnostic.Value.Number.ShouldBe(testData.VersionNumber);
    var parsedDotnet = deserializer.Deserialize(testData.VersionString, true);
    parsedDotnet.IsSuccess.ShouldBeTrue();
    parsedDotnet.Value.Number.ShouldBe(testData.VersionNumber);
    parsedDotnet.Value.IsDotnetEnabled.ShouldBeTrue();
    var parsedNonDotnet = deserializer.Deserialize(testData.VersionString, false);
    parsedNonDotnet.IsSuccess.ShouldBeTrue();
    parsedNonDotnet.Value.Number.ShouldBe(testData.VersionNumber);
    parsedNonDotnet.Value.IsDotnetEnabled.ShouldBeFalse();
  }
}
