namespace Chickensoft.GodotEnv.Tests.Features.Godot.Models;

using System;
using System.Collections.Generic;
using Chickensoft.GodotEnv.Features.Godot.Models;
using Shouldly;
using Xunit;

public partial class GodotVersionTest {
  public static IEnumerable<object[]> RejectionOfInvalidPropertyValuesTestData() {
    yield return ["a", "1", "2", "stable", ""];
    yield return ["1", "a", "2", "stable", ""];
    yield return ["1", "1", "a", "stable", ""];
    yield return ["1", "1", "2", "", ""];
    yield return ["1", "1", "2", "label", ""];
    yield return ["1", "1", "2", "rc.1", ""];
    yield return ["1", "1", "2", "", "rc1"];
    yield return ["1", "1", "2", "", "stable"];
  }

  [Theory]
  [MemberData(nameof(RejectionOfInvalidPropertyValuesTestData))]
  public void RejectionOfInvalidPropertyValues(string major,
                                               string minor,
                                               string patch,
                                               string godotLabel,
                                               string godotSharpLabel) =>
    Assert.Throws<ArgumentException>(
      () =>
        new GodotVersion(major,
                         minor,
                         patch,
                         godotLabel,
                         godotSharpLabel));

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
  public void RejectionOfInvalidGodotVersionNumbers(string invalidVersionNumber) =>
    GodotVersion.ParseGodotVersion(invalidVersionNumber).ShouldBeNull();

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
  public void RejectionOfInvalidSharpVersionNumbers(string invalidVersionNumber) =>
    GodotVersion.ParseGodotSharpVersion(invalidVersionNumber).ShouldBeNull();

  public static IEnumerable<object[]> CorrectParsingOfValidGodotVersionsTestData() {
    yield return ["1.2.3-stable", new GodotVersion("1", "2", "3", "stable", "")];
    yield return ["0.2.3-stable", new GodotVersion("0", "2", "3", "stable", "")];
    yield return ["1.0-stable", new GodotVersion("1", "0", "0", "stable", "")];
    yield return ["1.0-label1", new GodotVersion("1", "0", "0", "label1", "label.1")];
    yield return ["1.0-label23", new GodotVersion("1", "0", "0", "label23", "label.23")];
    yield return ["1.0.1-label23", new GodotVersion("1", "0", "1", "label23", "label.23")];
  }

  [Theory]
  [MemberData(nameof(CorrectParsingOfValidGodotVersionsTestData))]
  public void CorrectParsingOfValidGodotVersions(string toParse, GodotVersion expected) {
    var parsed = GodotVersion.ParseGodotVersion(toParse);
    Assert.Equal(expected, parsed);
  }

  public static IEnumerable<object[]> CorrectParsingOfValidSharpVersionsTestData() {
    yield return ["1.2.3", new GodotVersion("1", "2", "3", "stable", "")];
    yield return ["0.2.3", new GodotVersion("0", "2", "3", "stable", "")];
    yield return ["1.0.0", new GodotVersion("1", "0", "0", "stable", "")];
    yield return ["1.0.0-label.1", new GodotVersion("1", "0", "0", "label1", "label.1")];
    yield return ["1.0.0-label.23", new GodotVersion("1", "0", "0", "label23", "label.23")];
    yield return ["1.0.1-label.23", new GodotVersion("1", "0", "1", "label23", "label.23")];
  }

  [Theory]
  [MemberData(nameof(CorrectParsingOfValidSharpVersionsTestData))]
  public void CorrectParsingOfValidSharpVersions(string toParse, GodotVersion expected) {
    var parsed = GodotVersion.ParseGodotSharpVersion(toParse);
    Assert.Equal(expected, parsed);
  }

  public static IEnumerable<object[]> CorrectGodotVersionStringFormattingTestData() {
    yield return [new GodotVersion("0", "0", "1", "stable", ""), "0.0.1-stable"];
    yield return [new GodotVersion("1", "2", "0", "stable", ""), "1.2-stable"];
    yield return [new GodotVersion("1", "2", "3", "stable", ""), "1.2.3-stable"];
    yield return [new GodotVersion("1", "2", "0", "label1", "label.1"), "1.2-label1"];
    yield return [new GodotVersion("1", "2", "3", "label23", "label.23"), "1.2.3-label23"];
  }

  [Theory]
  [MemberData(nameof(CorrectGodotVersionStringFormattingTestData))]
  public void CorrectGodotVersionStringFormatting(GodotVersion toFormat, string expected) =>
    Assert.Equal(expected, toFormat.GodotVersionString());

  public static IEnumerable<object[]> CorrectSharpVersionStringFormattingTestData() {
    yield return [new GodotVersion("0", "0", "1", "stable", ""), "0.0.1"];
    yield return [new GodotVersion("1", "2", "0", "stable", ""), "1.2.0"];
    yield return [new GodotVersion("1", "2", "3", "stable", ""), "1.2.3"];
    yield return [new GodotVersion("1", "2", "0", "label1", "label.1"), "1.2.0-label.1"];
    yield return [new GodotVersion("1", "2", "3", "label23", "label.23"), "1.2.3-label.23"];
  }

  [Theory]
  [MemberData(nameof(CorrectSharpVersionStringFormattingTestData))]
  public void CorrectVersionStringFormatting(GodotVersion toFormat, string expected) =>
    Assert.Equal(expected, toFormat.GodotSharpVersionString());
}
