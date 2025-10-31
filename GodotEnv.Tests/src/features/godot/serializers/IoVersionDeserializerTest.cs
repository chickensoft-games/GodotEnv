namespace Chickensoft.GodotEnv.Tests.Features.Godot.Models;

using System.Collections.Generic;
using Chickensoft.GodotEnv.Features.Godot.Models;
using Chickensoft.GodotEnv.Features.Godot.Serializers;
using Shouldly;
using Xunit;

public class IoVersionDeserializerTest
{
  public static IEnumerable<object[]> CorrectDeserializationOfValidReleaseVersionsTestData()
  {
    object[][] testData = [
        ["1.2.3-stable", new GodotVersionNumber(1, 2, 3, "stable", -1)],
        ["0.2.3-stable", new GodotVersionNumber(0, 2, 3, "stable", -1)],
        ["1.0-stable", new GodotVersionNumber(1, 0, 0, "stable", -1)],
        ["1.0-label1", new GodotVersionNumber(1, 0, 0, "label", 1)],
        ["1.0-label23", new GodotVersionNumber(1, 0, 0, "label", 23)],
        ["1.0.1-label23", new GodotVersionNumber(1, 0, 1, "label", 23)]
    ];
    foreach (var testItem in testData)
    {
      yield return testItem;
      yield return [$"v{testItem[0]}", testItem[1]];
    }
  }

  [Theory]
  [MemberData(nameof(CorrectDeserializationOfValidReleaseVersionsTestData))]
  public void CorrectDeserializationOfValidReleaseVersions(string toParse, GodotVersionNumber expectedNumber)
  {
    var deserializer = new IoVersionDeserializer();
    var parsedAgnostic = deserializer.Deserialize(toParse);
    parsedAgnostic.IsSuccess.ShouldBeTrue();
    parsedAgnostic.Value.Number.ShouldBe(expectedNumber);
    var parsedDotnet = deserializer.Deserialize(toParse, true);
    parsedDotnet.IsSuccess.ShouldBeTrue();
    parsedDotnet.Value.Number.ShouldBe(expectedNumber);
    parsedDotnet.Value.IsDotnetEnabled.ShouldBeTrue();
    var parsedNonDotnet = deserializer.Deserialize(toParse, false);
    parsedNonDotnet.IsSuccess.ShouldBeTrue();
    parsedNonDotnet.Value.Number.ShouldBe(expectedNumber);
    parsedNonDotnet.Value.IsDotnetEnabled.ShouldBeFalse();
  }

  public static IEnumerable<object[]> CorrectDeserializationOfValidSharpVersionsTestData()
  {
    object[][] testData = [
        ["1.2.3", new GodotVersionNumber(1, 2, 3, "stable", -1)],
        ["0.2.3", new GodotVersionNumber(0, 2, 3, "stable", -1)],
        ["1.0.0", new GodotVersionNumber(1, 0, 0, "stable", -1)],
        ["1.0.0-label.1", new GodotVersionNumber(1, 0, 0, "label", 1)],
        ["1.0.0-label.23", new GodotVersionNumber(1, 0, 0, "label", 23)],
        ["1.0.1-label.23", new GodotVersionNumber(1, 0, 1, "label", 23)]
    ];
    foreach (var testItem in testData)
    {
      yield return testItem;
      yield return [$"v{testItem[0]}", testItem[1]];
    }
  }

  [Theory]
  [MemberData(nameof(CorrectDeserializationOfValidSharpVersionsTestData))]
  public void CorrectDeserializationOfValidSharpVersions(string toParse, GodotVersionNumber expectedNumber)
  {
    var deserializer = new IoVersionDeserializer();
    var parsedAgnostic = deserializer.Deserialize(toParse);
    parsedAgnostic.IsSuccess.ShouldBeTrue();
    parsedAgnostic.Value.Number.ShouldBe(expectedNumber);
    var parsedDotnet = deserializer.Deserialize(toParse, true);
    parsedDotnet.IsSuccess.ShouldBeTrue();
    parsedDotnet.Value.Number.ShouldBe(expectedNumber);
    parsedDotnet.Value.IsDotnetEnabled.ShouldBeTrue();
    var parsedNonDotnet = deserializer.Deserialize(toParse, false);
    parsedNonDotnet.IsSuccess.ShouldBeTrue();
    parsedNonDotnet.Value.Number.ShouldBe(expectedNumber);
    parsedNonDotnet.Value.IsDotnetEnabled.ShouldBeFalse();
  }
}
