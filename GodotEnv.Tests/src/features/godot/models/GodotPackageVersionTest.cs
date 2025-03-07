namespace Chickensoft.GodotEnv.Tests.Features.Godot.Models;

using System.Collections.Generic;
using Chickensoft.GodotEnv.Features.Godot.Models;
using Shouldly;
using Xunit;

public partial class GodotPackageVersionTest {
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
    GodotPackageVersion.Parse(invalidVersionNumber).ShouldBeNull();

  public static IEnumerable<object[]> CorrectParsingOfValidVersionsTestData() {
    yield return ["1.2.3-label", new GodotPackageVersion("1", "2", "3", "label")];
    yield return ["0.2.3-label", new GodotPackageVersion("0", "2", "3", "label")];
    yield return ["1.0-label", new GodotPackageVersion("1", "0", "", "label")];
    yield return ["1.0-label1", new GodotPackageVersion("1", "0", "", "label1")];
    yield return ["1.0-label23", new GodotPackageVersion("1", "0", "", "label23")];
    yield return ["1.0.1-label23", new GodotPackageVersion("1", "0", "1", "label23")];
  }

  [Theory]
  [MemberData(nameof(CorrectParsingOfValidVersionsTestData))]
  public void CorrectParsingOfValidVersions(string toParse, GodotPackageVersion expected) {
    var parsed = GodotPackageVersion.Parse(toParse);
    Assert.Equal(expected, parsed);
  }

  public static IEnumerable<object[]> CorrectVersionStringFormattingTestData() {
    yield return [new GodotPackageVersion("0", "0", "1", "label"), "0.0.1-label"];
    yield return [new GodotPackageVersion("1", "2", "0", "label"), "1.2.0-label"];
    yield return [new GodotPackageVersion("1", "2", "3", "label"), "1.2.3-label"];
    yield return [new GodotPackageVersion("1", "2", "", "label1"), "1.2-label1"];
    yield return [new GodotPackageVersion("1", "2", "3", "label23"), "1.2.3-label23"];
  }

  [Theory]
  [MemberData(nameof(CorrectVersionStringFormattingTestData))]
  public void CorrectVersionStringFormatting(GodotPackageVersion toFormat, string expected) =>
    Assert.Equal(expected, toFormat.VersionString());

  public static IEnumerable<object[]> CorrectGodotVersionFormattingTestData() {
    yield return [new GodotPackageVersion("0", "0", "1", "label"), "0.0.1-label"];
    yield return [new GodotPackageVersion("1", "2", "0", "label"), "1.2-label"];
    yield return [new GodotPackageVersion("1", "2", "3", "label"), "1.2.3-label"];
    yield return [new GodotPackageVersion("1", "2", "", "label1"), "1.2-label1"];
    yield return [new GodotPackageVersion("1", "2", "3", "label23"), "1.2.3-label23"];
  }

  [Theory]
  [MemberData(nameof(CorrectVersionStringFormattingTestData))]
  public void CorrectGodotVersionStringFormatting(GodotPackageVersion toFormat, string expected) =>
    Assert.Equal(expected, toFormat.VersionString());

  public static IEnumerable<object[]> CorrectSharpVersionConversionTestData() {
    yield return [new GodotSharpVersion("0", "1", "0", ""), new GodotPackageVersion("0", "1", "", "stable")];
    yield return [new GodotSharpVersion("0", "1", "1", ""), new GodotPackageVersion("0", "1", "1", "stable")];
    yield return [new GodotSharpVersion("0", "1", "0", "rc.2"), new GodotPackageVersion("0", "1", "", "rc2")];
    yield return [new GodotSharpVersion("0", "1", "1", "dev.6"), new GodotPackageVersion("0", "1", "1", "dev6")];
  }

  [Theory]
  [MemberData(nameof(CorrectSharpVersionConversionTestData))]
  public void CorrectSharpVersionConversion(GodotSharpVersion toConvert, GodotPackageVersion expected) =>
    Assert.Equal(expected, new GodotPackageVersion(toConvert));
}
