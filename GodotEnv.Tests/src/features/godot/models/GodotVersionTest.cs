namespace Chickensoft.GodotEnv.Tests.Features.Godot.Models;

using System;
using System.Collections.Generic;
using Chickensoft.GodotEnv.Features.Godot.Models;
using Shouldly;
using Xunit;

public partial class GodotVersionTest
{
  public static IEnumerable<object[]> RejectionOfInvalidPropertyValuesTestData()
  {
    yield return [-1, 1, 2, "stable", -1];
    yield return [1, -1, 2, "stable", -1];
    yield return [1, 1, -2, "stable", -1];
    yield return [1, 1, 2, "", 3];
    yield return [1, 1, 2, "rc1", 2];
    yield return [1, 1, 2, "rc", -1];
    yield return [1, 1, 2, "stable", 3];
  }

  [Theory]
  [MemberData(nameof(RejectionOfInvalidPropertyValuesTestData))]
  public void VersionNumberRejectsInvalidPropertyValues(
    int major,
    int minor,
    int patch,
    string label,
    int labelNum
  ) =>
    Should.Throw<ArgumentException>(
      () =>
        new GodotVersionNumber(
          major,
          minor,
          patch,
          label,
          labelNum
      ));

  [Theory]
  [MemberData(nameof(RejectionOfInvalidPropertyValuesTestData))]
  public void DotnetAgnosticRejectsInvalidPropertyValues(
    int major,
    int minor,
    int patch,
    string label,
    int labelNum
  ) =>
    Should.Throw<ArgumentException>(
      () =>
        new AnyDotnetStatusGodotVersion(
          major,
          minor,
          patch,
          label,
          labelNum
      ));

  [Theory]
  [MemberData(nameof(RejectionOfInvalidPropertyValuesTestData))]
  public void DotnetSpecificRejectsInvalidPropertyValues(
    int major,
    int minor,
    int patch,
    string label,
    int labelNum
  )
  {
    Should.Throw<ArgumentException>(
      () =>
        new SpecificDotnetStatusGodotVersion(
          major,
          minor,
          patch,
          label,
          labelNum,
          false
      ));
    Should.Throw<ArgumentException>(
      () =>
        new SpecificDotnetStatusGodotVersion(
          major,
          minor,
          patch,
          label,
          labelNum,
          true
      ));
  }
}
