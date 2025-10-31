namespace Chickensoft.GodotEnv.Tests.Features.Godot.Models;

using System;
using System.Collections.Generic;
using Chickensoft.GodotEnv.Features.Godot.Models;
using Chickensoft.GodotEnv.Features.Godot.Serializers;
using Shouldly;
using Xunit;

public class SharpVersionDeserializerTest
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
  [InlineData("1.0.0-stable")]
  [InlineData("1.0-rc.1")]
  [InlineData("1.0.0-rc1")]
  public void RejectionOfInvalidSharpVersionNumbers(string invalidVersionNumber)
  {
    var deserializer = new SharpVersionDeserializer();
    var result = deserializer.Deserialize(invalidVersionNumber);
    result.IsSuccess.ShouldBeFalse();
    result.Error.ShouldBe($"Couldn't match \"{invalidVersionNumber}\" to known GodotSharp version patterns.");
  }

  public static IEnumerable<object[]> CorrectDeserializationOfValidSharpVersionsTestData()
  {
    yield return ["1.2.3", new GodotVersionNumber(1, 2, 3, "stable", -1)];
    yield return ["0.2.3", new GodotVersionNumber(0, 2, 3, "stable", -1)];
    yield return ["1.0.0", new GodotVersionNumber(1, 0, 0, "stable", -1)];
    yield return ["1.0.0-label.1", new GodotVersionNumber(1, 0, 0, "label", 1)];
    yield return ["1.0.0-label.23", new GodotVersionNumber(1, 0, 0, "label", 23)];
    yield return ["1.0.1-label.23", new GodotVersionNumber(1, 0, 1, "label", 23)];
  }

  [Theory]
  [MemberData(nameof(CorrectDeserializationOfValidSharpVersionsTestData))]
  public void CorrectDeserializationOfValidSharpVersions(string toParse, GodotVersionNumber expectedNumber)
  {
    var deserializer = new SharpVersionDeserializer();
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
