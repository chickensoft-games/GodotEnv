namespace Chickensoft.GodotEnv.Tests.Features.Godot.Models;

using System.Collections.Generic;
using Chickensoft.GodotEnv.Features.Godot.Models;
using Xunit;

public partial class IOVersionStringConverterTest {
  public static IEnumerable<object[]> CorrectParsingOfValidReleaseVersionsTestData() {
    object[][] testData = [
        ["1.2.3-stable", new GodotVersion(1, 2, 3, "stable", -1)],
        ["0.2.3-stable", new GodotVersion(0, 2, 3, "stable", -1)],
        ["1.0-stable", new GodotVersion(1, 0, 0, "stable", -1)],
        ["1.0-label1", new GodotVersion(1, 0, 0, "label", 1)],
        ["1.0-label23", new GodotVersion(1, 0, 0, "label", 23)],
        ["1.0.1-label23", new GodotVersion(1, 0, 1, "label", 23)]
    ];
    foreach (var testItem in testData) {
      yield return testItem;
      yield return [$"v{testItem[0]}", testItem[1]];
    }
  }

  [Theory]
  [MemberData(nameof(CorrectParsingOfValidReleaseVersionsTestData))]
  public void CorrectParsingOfValidReleaseVersions(string toParse, GodotVersion expected) {
    var converter = new IOVersionStringConverter();
    var parsed = converter.ParseVersion(toParse);
    Assert.Equal(expected, parsed);
  }

  public static IEnumerable<object[]> CorrectParsingOfValidSharpVersionsTestData() {
    object[][] testData = [
        ["1.2.3", new GodotVersion(1, 2, 3, "stable", -1)],
        ["0.2.3", new GodotVersion(0, 2, 3, "stable", -1)],
        ["1.0.0", new GodotVersion(1, 0, 0, "stable", -1)],
        ["1.0.0-label.1", new GodotVersion(1, 0, 0, "label", 1)],
        ["1.0.0-label.23", new GodotVersion(1, 0, 0, "label", 23)],
        ["1.0.1-label.23", new GodotVersion(1, 0, 1, "label", 23)]
    ];
    foreach (var testItem in testData) {
      yield return testItem;
      yield return [$"v{testItem[0]}", testItem[1]];
    }
  }

  [Theory]
  [MemberData(nameof(CorrectParsingOfValidSharpVersionsTestData))]
  public void CorrectParsingOfValidSharpVersions(string toParse, GodotVersion expected) {
    var converter = new IOVersionStringConverter();
    var parsed = converter.ParseVersion(toParse);
    Assert.Equal(expected, parsed);
  }

  public static IEnumerable<object[]> CorrectReleaseVersionStringFormattingTestData() {
    yield return [new GodotVersion(0, 0, 1, "stable", -1), "0.0.1-stable"];
    yield return [new GodotVersion(1, 2, 0, "stable", -1), "1.2-stable"];
    yield return [new GodotVersion(1, 2, 3, "stable", -1), "1.2.3-stable"];
    yield return [new GodotVersion(1, 2, 0, "label", 1), "1.2-label1"];
    yield return [new GodotVersion(1, 2, 3, "label", 23), "1.2.3-label23"];
  }

  [Theory]
  [MemberData(nameof(CorrectReleaseVersionStringFormattingTestData))]
  public void CorrectReleaseVersionStringFormatting(GodotVersion toFormat, string expected) {
    var converter = new IOVersionStringConverter();
    Assert.Equal(expected, converter.VersionString(toFormat));
  }
}