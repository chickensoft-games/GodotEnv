namespace Chickensoft.GodotEnv.Features.Godot.Models;

using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Common.Utilities;
using Chickensoft.GodotEnv.Features.Godot.Serializers;

public abstract class Unix : GodotEnvironment {
  protected Unix(
    ISystemInfo systemInfo,
    IFileClient fileClient,
    IComputer computer,
    IVersionDeserializer versionDeserializer,
    IVersionSerializer versionSerializer
  )
    : base(systemInfo, fileClient, computer, versionDeserializer, versionSerializer) { }
}
