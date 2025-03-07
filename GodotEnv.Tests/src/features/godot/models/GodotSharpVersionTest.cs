namespace Chickensoft.GodotEnv.Tests.Features.Godot.Models;

using System.Collections.Generic;
using Chickensoft.GodotEnv.Features.Godot.Models;
using Shouldly;
using Xunit;

public partial class GodotSharpVersionTest {
  [Theory]
  [InlineData("NotAVersion")]
  [InlineData("1")]
  [InlineData("1.")]
  [InlineData("1.2.")]
  [InlineData("1.2")]
  [InlineData("1.2.3.")]
  [InlineData("1.2.3.4.5")]
  [InlineData("1.a")]
  [InlineData("1.0.0-label")]
  [InlineData("1.0-rc.1")]
  [InlineData("1.0.0-rc1")]
  public void RejectionOfInvalidVersionNumbers(string invalidVersionNumber) =>
    GodotSharpVersion.Parse(invalidVersionNumber).ShouldBeNull();

  public static IEnumerable<object[]> CorrectParsingOfValidVersionsTestData() {
    yield return ["1.2.3", new GodotSharpVersion("1", "2", "3", "")];
    yield return ["0.2.3", new GodotSharpVersion("0", "2", "3", "")];
    yield return ["1.0.0-label.1", new GodotSharpVersion("1", "0", "0", "label.1")];
    yield return ["1.0.0-label.23", new GodotSharpVersion("1", "0", "0", "label.23")];
  }

  [Theory]
  [MemberData(nameof(CorrectParsingOfValidVersionsTestData))]
  public void CorrectParsingOfValidVersions(string toParse, GodotSharpVersion expected) {
    var parsed = GodotSharpVersion.Parse(toParse);
    Assert.Equal(expected, parsed);
  }

  public static IEnumerable<object[]> CorrectVersionStringFormattingTestData() {
    yield return [new GodotSharpVersion("0", "0", "1", ""), "0.0.1"];
    yield return [new GodotSharpVersion("1", "2", "0", ""), "1.2.0"];
    yield return [new GodotSharpVersion("1", "2", "3", ""), "1.2.3"];
    yield return [new GodotSharpVersion("1", "2", "0", "label.1"), "1.2.0-label.1"];
    yield return [new GodotSharpVersion("1", "2", "3", "label.23"), "1.2.3-label.23"];
  }

  [Theory]
  [MemberData(nameof(CorrectVersionStringFormattingTestData))]
  public void CorrectVersionStringFormatting(GodotSharpVersion toFormat, string expected) =>
    Assert.Equal(expected, toFormat.VersionString());

  public static IEnumerable<object[]> CorrectGodotVersionFormattingTestData() {
    yield return [new GodotSharpVersion("0", "0", "1", ""), "0.0.1"];
    yield return [new GodotSharpVersion("1", "2", "0", ""), "1.2"];
    yield return [new GodotSharpVersion("1", "2", "3", ""), "1.2.3"];
    yield return [new GodotSharpVersion("1", "2", "0", "label.1"), "1.2-label.1"];
    yield return [new GodotSharpVersion("1", "2", "3", "label.23"), "1.2.3-label.23"];
  }

  [Theory]
  [MemberData(nameof(CorrectVersionStringFormattingTestData))]
  public void CorrectGodotVersionStringFormatting(GodotSharpVersion toFormat, string expected) =>
    Assert.Equal(expected, toFormat.VersionString());

  public static IEnumerable<object[]> CorrectPackageVersionConversionTestData() {
    yield return [new GodotPackageVersion("0", "1", "", "stable"), new GodotSharpVersion("0", "1", "0", "")];
    yield return [new GodotPackageVersion("0", "1", "1", "stable"), new GodotSharpVersion("0", "1", "1", "")];
    yield return [new GodotPackageVersion("0", "1", "", "rc2"), new GodotSharpVersion("0", "1", "0", "rc.2")];
    yield return [new GodotPackageVersion("0", "1", "1", "dev6"), new GodotSharpVersion("0", "1", "1", "dev.6")];
  }

  [Theory]
  [MemberData(nameof(CorrectPackageVersionConversionTestData))]
  public void CorrectPackageVersionConversion(GodotPackageVersion toConvert, GodotSharpVersion expected) =>
    Assert.Equal(expected, new GodotSharpVersion(toConvert));
}
