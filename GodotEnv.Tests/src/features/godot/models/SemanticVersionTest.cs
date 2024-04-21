namespace Chickensoft.GodotEnv.Tests.features.godot.models;

using System;
using System.Collections.Generic;
using Chickensoft.GodotEnv.Features.Godot.Models;
using Xunit;

public class SemanticVersionTest {
  [Theory]
  [InlineData("NotAVersion")]
  [InlineData("1")]
  [InlineData("1.")]
  [InlineData("1.2.")]
  [InlineData("1.2")]
  [InlineData("1.2.3.")]
  [InlineData("1.2.3.4.5")]
  [InlineData("1.a")]
  public void RejectionOfInvalidVersionNumbers(string invalidVersionNumber) {
    var ex = Assert.Throws<InvalidOperationException>(() => SemanticVersion.Parse(invalidVersionNumber));

    Assert.Equal(ex.Message, $"Invalid semantic version: {invalidVersionNumber}");
  }

  public static IEnumerable<object[]> CorrectParsingOfValidVersionsTestData() {
    yield return ["1.2.3", new SemanticVersion("1", "2", "3", "")];
    yield return ["0.2.3", new SemanticVersion("0", "2", "3", "")];
    yield return ["1.0.0-label", new SemanticVersion("1", "0", "0", "label")];
    yield return ["1.0.0-label+abc", new SemanticVersion("1", "0", "0", "label")];
  }

  [Theory]
  [MemberData(nameof(CorrectParsingOfValidVersionsTestData))]
  public void CorrectParsingOfValidVersions(string toParse, SemanticVersion expected) {
    var parsed = SemanticVersion.Parse(toParse);
    Assert.Equal(expected, parsed);
  }

  public static IEnumerable<object[]> CorrectVersionStringFormattingTestData() {
    yield return [new SemanticVersion("0", "0", "1"), "0.0.1"];
    yield return [new SemanticVersion("1", "2", "0"), "1.2.0"];
    yield return [new SemanticVersion("1", "2", "3"), "1.2.3"];
    yield return [new SemanticVersion("1", "2", "0", "label"), "1.2.0-label"];
    yield return [new SemanticVersion("1", "2", "3", "label"), "1.2.3-label"];
  }

  [Theory]
  [MemberData(nameof(CorrectVersionStringFormattingTestData))]
  public void CorrectVersionStringFormatting(SemanticVersion toFormat, string expected) {
    Assert.Equal(expected, toFormat.VersionString);
  }
}
