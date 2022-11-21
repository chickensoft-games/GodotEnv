namespace Chickensoft.Chicken.Tests;

using Shouldly;
using Xunit;

public class StringExtensionsTest {
  [Theory]
  [InlineData(@"volume/foo", @"volume", true)]
  [InlineData(@"volume/foo", @"volume/", true)]
  [InlineData(@"volume/foo/bar/", @"volume/foo/", true)]
  [InlineData(@"volume/foo/bar", @"volume/foo/", true)]
  [InlineData(@"volume/foo/a.txt", @"volume/foo", true)]
  [InlineData(@"volume/foobar", @"volume/foo", false)]
  [InlineData(@"volume/foobar/a.txt", @"volume/foo", false)]
  [InlineData(@"volume/foobar/a.txt", @"volume/foo/", false)]
  [InlineData(@"volume/foo/a.txt", @"volume/foobar", false)]
  [InlineData(@"volume/foo/a.txt", @"volume/foobar/", false)]
  [InlineData(@"volume/foo/../bar/baz", @"volume/foo", false)]
  [InlineData(@"volume/foo/../bar/baz", @"volume/bar", true)]
  [InlineData(@"volume/foo/../bar/baz", @"volume/barr", false)]
  public void IsSubPathOfTest(string path, string baseDirPath, bool expected)
    => path.IsWithinPath(baseDirPath).ShouldBe(expected);

  [Theory]
  [InlineData("file.txt", "file.txt")]
  [InlineData("/file.txt", "file.txt")]
  [InlineData("././file.txt", "file.txt")]
  [InlineData(".././file.txt", "file.txt")]
  [InlineData("c:/.././file.txt", "c/file.txt")]
  [InlineData("\\file.txt", "file.txt")]
  [InlineData(".\\.\\file.txt", "file.txt")]
  [InlineData("..\\.\\file.txt", "file.txt")]
  [InlineData("c:\\..\\.\\file.txt", "c\\file.txt")]
  [InlineData("file.txt", "/file.txt", "/")]
  [InlineData("c:/file.txt", "/c/file.txt", "/")]
  [InlineData("~/file.txt", "/file.txt", "/")]
  public void SanitizePathTests(
    string path, string expected, string basePath = ""
  ) => path.SanitizePath(basePath).ShouldBe(expected);
}
