namespace Chickensoft.GodotEnv.Tests.Features.Godot.Models;

using System;
using System.Collections.Generic;
using Chickensoft.GodotEnv.Features.Godot.Models;
using Xunit;

public partial class GodotVersionTest {
  public static IEnumerable<object[]> RejectionOfInvalidPropertyValuesTestData() {
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
  public void RejectionOfInvalidPropertyValues(int major,
                                               int minor,
                                               int patch,
                                               string label,
                                               int labelNum) =>
    Assert.Throws<ArgumentException>(
      () =>
        new GodotVersion(major,
                         minor,
                         patch,
                         label,
                         labelNum));
  //

}
