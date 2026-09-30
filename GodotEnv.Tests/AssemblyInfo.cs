using Chickensoft.GodotEnv.Common.Utilities;
using Chickensoft.GodotEnv.Features.Godot.Models;
using Chickensoft.GodotEnv.Tests;
using Xunit.Sdk;

[assembly: RegisterXunitSerializer(typeof(MockSystemInfoSerializer), typeof(ISystemInfo))]
[assembly: RegisterXunitSerializer(typeof(GodotVersionNumberSerializer), typeof(GodotVersionNumber))]
[assembly:
  RegisterXunitSerializer(
    typeof(SpecificDotnetStatusGodotVersionSerializer),
    typeof(SpecificDotnetStatusGodotVersion)
  )
]
