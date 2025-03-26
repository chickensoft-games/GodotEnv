namespace Chickensoft.GodotEnv.Tests.Features.Godot.Models;

using System;
using System.Collections.Generic;
using Chickensoft.GodotEnv.Features.Godot.Models;
using Xunit;

public partial class SharpVersionStringConverterTest {
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
  public void RejectionOfInvalidSharpVersionNumbers(string invalidVersionNumber) {
    var converter = new SharpVersionStringConverter();
    Assert.Throws<ArgumentException>(() => converter.ParseVersion(invalidVersionNumber));
  }

  public static IEnumerable<object[]> CorrectParsingOfValidSharpVersionsTestData() {
    yield return ["1.2.3", new GodotVersionNumber(1, 2, 3, "stable", -1)];
    yield return ["0.2.3", new GodotVersionNumber(0, 2, 3, "stable", -1)];
    yield return ["1.0.0", new GodotVersionNumber(1, 0, 0, "stable", -1)];
    yield return ["1.0.0-label.1", new GodotVersionNumber(1, 0, 0, "label", 1)];
    yield return ["1.0.0-label.23", new GodotVersionNumber(1, 0, 0, "label", 23)];
    yield return ["1.0.1-label.23", new GodotVersionNumber(1, 0, 1, "label", 23)];
  }

  [Theory]
  [MemberData(nameof(CorrectParsingOfValidSharpVersionsTestData))]
  public void CorrectParsingOfValidSharpVersions(string toParse, GodotVersionNumber expectedNumber) {
    var converter = new SharpVersionStringConverter();
    var parsedAgnostic = converter.ParseVersion(toParse);
    Assert.Equal(expectedNumber, parsedAgnostic.Number);
    var parsedDotnet = converter.ParseVersion(toParse, true);
    Assert.Equal(expectedNumber, parsedDotnet.Number);
    Assert.True(parsedDotnet.IsDotnetEnabled);
    var parsedNonDotnet = converter.ParseVersion(toParse, false);
    Assert.Equal(expectedNumber, parsedNonDotnet.Number);
    Assert.False(parsedNonDotnet.IsDotnetEnabled);
  }

  public static IEnumerable<object[]> CorrectSharpVersionStringFormattingTestData() {
    yield return [new GodotVersionNumber(0, 0, 1, "stable", -1), "0.0.1"];
    yield return [new GodotVersionNumber(1, 2, 0, "stable", -1), "1.2.0"];
    yield return [new GodotVersionNumber(1, 2, 3, "stable", -1), "1.2.3"];
    yield return [new GodotVersionNumber(1, 2, 0, "label", 1), "1.2.0-label.1"];
    yield return [new GodotVersionNumber(1, 2, 3, "label", 23), "1.2.3-label.23"];
  }

  [Theory]
  [MemberData(nameof(CorrectSharpVersionStringFormattingTestData))]
  public void CorrectVersionStringFormatting(GodotVersionNumber toFormat, string expected) {
    var converter = new SharpVersionStringConverter();
    Assert.Equal(expected, converter.VersionString(new AnyDotnetStatusGodotVersion(toFormat)));
    Assert.Equal(expected, converter.VersionString(new SpecificDotnetStatusGodotVersion(toFormat, true)));
    Assert.Equal(expected, converter.VersionString(new SpecificDotnetStatusGodotVersion(toFormat, false)));
  }
}
