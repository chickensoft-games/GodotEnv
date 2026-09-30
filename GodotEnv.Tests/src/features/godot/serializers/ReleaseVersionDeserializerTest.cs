namespace Chickensoft.GodotEnv.Tests.Features.Godot.Models;

using System.Collections.Generic;
using Chickensoft.GodotEnv.Features.Godot.Models;
using Chickensoft.GodotEnv.Features.Godot.Serializers;
using Shouldly;
using Xunit;

using ReleaseVersionDeserializerTestData =
  (string VersionString, Chickensoft.GodotEnv.Features.Godot.Models.GodotVersionNumber VersionNumber);

public class ReleaseVersionDeserializerTest
{
  [Theory]
  [InlineData("NotAVersion")]
  [InlineData("1")]
  [InlineData("1.")]
  [InlineData("1.2.")]
  [InlineData("1.2")]
  [InlineData("1.2.3.")]
  [InlineData("1.2.3.4.5")]
  [InlineData("1.a")]
  [InlineData("1.0.1-label")]
  [InlineData("1.0-rc.1")]
  [InlineData("1.0.0-rc1")]
  public void RejectionOfInvalidReleaseVersionNumbers(string invalidVersionNumber)
  {
    var deserializer = new ReleaseVersionDeserializer();
    var result = deserializer.Deserialize(invalidVersionNumber);
    result.IsSuccess.ShouldBeFalse();
    result.Error.ShouldBe($"Couldn't match \"{invalidVersionNumber}\" to known Godot version patterns.");
  }

  public static IEnumerable<TheoryDataRow<ReleaseVersionDeserializerTestData>>
    CorrectDeserializationOfValidReleaseVersionsTestData()
  {
    yield return ("1.2.3-stable", new GodotVersionNumber(1, 2, 3, "stable", -1));
    yield return ("0.2.3-stable", new GodotVersionNumber(0, 2, 3, "stable", -1));
    yield return ("1.0-stable", new GodotVersionNumber(1, 0, 0, "stable", -1));
    yield return ("1.0-label1", new GodotVersionNumber(1, 0, 0, "label", 1));
    yield return ("1.0-label23", new GodotVersionNumber(1, 0, 0, "label", 23));
    yield return ("1.0.1-label23", new GodotVersionNumber(1, 0, 1, "label", 23));
  }

  [Theory]
  [MemberData(nameof(CorrectDeserializationOfValidReleaseVersionsTestData))]
  public void CorrectDeserializationOfValidReleaseVersions(ReleaseVersionDeserializerTestData testData)
  {
    var deserializer = new ReleaseVersionDeserializer();
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
