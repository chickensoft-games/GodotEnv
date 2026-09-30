namespace Chickensoft.GodotEnv.Tests.Features.Godot.Models;

using System;
using System.Collections.Generic;
using Chickensoft.GodotEnv.Features.Godot.Models;
using Shouldly;
using Xunit;

using GodotVersionTestData = (int Major, int Minor, int Patch, string Label, int LabelNum);

public partial class GodotVersionTest
{
  public static IEnumerable<TheoryDataRow<GodotVersionTestData>> RejectionOfInvalidPropertyValuesTestData()
  {
    yield return (-1, 1, 2, "stable", -1);
    yield return (1, -1, 2, "stable", -1);
    yield return (1, 1, -2, "stable", -1);
    yield return (1, 1, 2, "", 3);
    yield return (1, 1, 2, "rc1", 2);
    yield return (1, 1, 2, "rc", -1);
    yield return (1, 1, 2, "stable", 3);
  }

  [Theory]
  [MemberData(nameof(RejectionOfInvalidPropertyValuesTestData))]
  public void VersionNumberRejectsInvalidPropertyValues(
    GodotVersionTestData testData
  ) =>
    Should.Throw<ArgumentException>(
      () =>
        new GodotVersionNumber(
          testData.Major,
          testData.Minor,
          testData.Patch,
          testData.Label,
          testData.LabelNum
      ));

  [Theory]
  [MemberData(nameof(RejectionOfInvalidPropertyValuesTestData))]
  public void DotnetAgnosticRejectsInvalidPropertyValues(
    GodotVersionTestData testData
  ) =>
    Should.Throw<ArgumentException>(
      () =>
        new AnyDotnetStatusGodotVersion(
          testData.Major,
          testData.Minor,
          testData.Patch,
          testData.Label,
          testData.LabelNum
      ));

  [Theory]
  [MemberData(nameof(RejectionOfInvalidPropertyValuesTestData))]
  public void DotnetSpecificRejectsInvalidPropertyValues(
    GodotVersionTestData testData
  )
  {
    Should.Throw<ArgumentException>(
      () =>
        new SpecificDotnetStatusGodotVersion(
          testData.Major,
          testData.Minor,
          testData.Patch,
          testData.Label,
          testData.LabelNum,
          false
      ));
    Should.Throw<ArgumentException>(
      () =>
        new SpecificDotnetStatusGodotVersion(
          testData.Major,
          testData.Minor,
          testData.Patch,
          testData.Label,
          testData.LabelNum,
          true
      ));
  }
}
