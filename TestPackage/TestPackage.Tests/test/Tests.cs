namespace TestPackage.Tests;

using System.Reflection;
using Chickensoft.GoDotTest;
using Godot;

public partial class Tests : Node2D {
  public override void _Ready()
    => _ = GoTest.RunTests(Assembly.GetExecutingAssembly(), this);
}
