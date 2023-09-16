namespace TestPackage.Tests;

using Godot;
using GoDotTest;
using Shouldly;

public class PackageTest : TestClass {
  public PackageTest(Node testScene) : base(testScene) { }

  [Test]
  public void Initializes() {
    var package = new Package();
    package.ShouldBeAssignableTo<Package>();
  }

  [Test]
  public void MethodReturnsString() {
    var package = new Package();
    package.Method().ShouldBe("Hello, world!");
  }
}
