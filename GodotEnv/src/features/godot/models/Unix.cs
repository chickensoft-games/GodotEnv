namespace Chickensoft.GodotEnv.Features.Godot.Models;

using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Common.Utilities;
using global::GodotEnv.Common.Utilities;

public abstract class Unix : GodotEnvironment {
  protected Unix(
    ISystemInfo systemInfo,
    IFileClient fileClient,
    IComputer computer,
    IVersionStringConverter versionStringConverter
  )
    : base(systemInfo, fileClient, computer, versionStringConverter) { }
}
