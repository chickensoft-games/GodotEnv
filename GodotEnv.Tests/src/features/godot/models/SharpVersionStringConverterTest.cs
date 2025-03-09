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
    yield return ["1.2.3", new GodotVersion(1, 2, 3, "stable", -1)];
    yield return ["0.2.3", new GodotVersion(0, 2, 3, "stable", -1)];
    yield return ["1.0.0", new GodotVersion(1, 0, 0, "stable", -1)];
    yield return ["1.0.0-label.1", new GodotVersion(1, 0, 0, "label", 1)];
    yield return ["1.0.0-label.23", new GodotVersion(1, 0, 0, "label", 23)];
    yield return ["1.0.1-label.23", new GodotVersion(1, 0, 1, "label", 23)];
  }

  [Theory]
  [MemberData(nameof(CorrectParsingOfValidSharpVersionsTestData))]
  public void CorrectParsingOfValidSharpVersions(string toParse, GodotVersion expected) {
    var converter = new SharpVersionStringConverter();
    var parsed = converter.ParseVersion(toParse);
    Assert.Equal(expected, parsed);
  }

  public static IEnumerable<object[]> CorrectSharpVersionStringFormattingTestData() {
    yield return [new GodotVersion(0, 0, 1, "stable", -1), "0.0.1"];
    yield return [new GodotVersion(1, 2, 0, "stable", -1), "1.2.0"];
    yield return [new GodotVersion(1, 2, 3, "stable", -1), "1.2.3"];
    yield return [new GodotVersion(1, 2, 0, "label", 1), "1.2.0-label.1"];
    yield return [new GodotVersion(1, 2, 3, "label", 23), "1.2.3-label.23"];
  }

  [Theory]
  [MemberData(nameof(CorrectSharpVersionStringFormattingTestData))]
  public void CorrectVersionStringFormatting(GodotVersion toFormat, string expected) {
    var converter = new SharpVersionStringConverter();
    Assert.Equal(expected, converter.VersionString(toFormat));
  }
}
