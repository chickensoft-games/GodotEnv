namespace Chickensoft.GodotEnv.Tests.Features.Godot.Models;

using System.Collections.Generic;
using Chickensoft.GodotEnv.Features.Godot.Models;
using Chickensoft.GodotEnv.Features.Godot.Serializers;
using Shouldly;
using Xunit;

public class DiskVersionDeserializerTest
{
  [Theory]
  [InlineData("NotAVersion")]
  [InlineData("1")]
  [InlineData("1_")]
  [InlineData("1_2_")]
  [InlineData("1_2")]
  [InlineData("1_2_3_")]
  [InlineData("1_2_3_4_5")]
  [InlineData("1_a")]
  [InlineData("1_0_1_label")]
  public void RejectionOfInvalidOldDiskVersionNumbers(string invalidVersionNumber)
  {
    var deserializer = new DiskVersionDeserializer();
    var result = deserializer.Deserialize(invalidVersionNumber);
    result.IsSuccess.ShouldBeFalse();
    result.Error.ShouldBe($"Couldn't match \"{invalidVersionNumber}\" to known Godot version patterns.");
  }

  public static IEnumerable<object[]> CorrectDeserializationOfValidOldDiskVersionsTestData()
  {
    yield return ["1_2_3_stable", new GodotVersionNumber(1, 2, 3, "stable", -1)];
    yield return ["0_2_3_stable", new GodotVersionNumber(0, 2, 3, "stable", -1)];
    yield return ["1_0_stable", new GodotVersionNumber(1, 0, 0, "stable", -1)];
    yield return ["1_0_0_stable", new GodotVersionNumber(1, 0, 0, "stable", -1)];
    yield return ["1_0_label1", new GodotVersionNumber(1, 0, 0, "label", 1)];
    yield return ["1_0_label23", new GodotVersionNumber(1, 0, 0, "label", 23)];
    yield return ["1_0_label_1", new GodotVersionNumber(1, 0, 0, "label", 1)];
    yield return ["1_0_label_23", new GodotVersionNumber(1, 0, 0, "label", 23)];
    yield return ["1_0_0_label1", new GodotVersionNumber(1, 0, 0, "label", 1)];
    yield return ["1_0_0_label23", new GodotVersionNumber(1, 0, 0, "label", 23)];
    yield return ["1_0_0_label_1", new GodotVersionNumber(1, 0, 0, "label", 1)];
    yield return ["1_0_0_label_23", new GodotVersionNumber(1, 0, 0, "label", 23)];
    yield return ["1_0_1_label23", new GodotVersionNumber(1, 0, 1, "label", 23)];
    yield return ["1_0_1_label_23", new GodotVersionNumber(1, 0, 1, "label", 23)];
  }

  [Theory]
  [MemberData(nameof(CorrectDeserializationOfValidOldDiskVersionsTestData))]
  public void CorrectDeserializationOfValidOldDiskVersions(string toParse, GodotVersionNumber expectedNumber)
  {
    var deserializer = new DiskVersionDeserializer();
    var parsedAgnostic = deserializer.Deserialize(toParse);
    parsedAgnostic.IsSuccess.ShouldBe(true);
    parsedAgnostic.Value.ShouldNotBeNull();
    parsedAgnostic.Value.Number.ShouldBe(expectedNumber);
    var parsedDotnet = deserializer.Deserialize(toParse, true);
    parsedDotnet.IsSuccess.ShouldBe(true);
    parsedDotnet.Value.ShouldNotBeNull();
    parsedDotnet.Value.Number.ShouldBe(expectedNumber);
    parsedDotnet.Value.IsDotnetEnabled.ShouldBeTrue();
    var parsedNonDotnet = deserializer.Deserialize(toParse, false);
    parsedNonDotnet.IsSuccess.ShouldBeTrue();
    parsedNonDotnet.Value.ShouldNotBeNull();
    parsedNonDotnet.Value.Number.ShouldBe(expectedNumber);
    parsedNonDotnet.Value.IsDotnetEnabled.ShouldBeFalse();
  }
}
