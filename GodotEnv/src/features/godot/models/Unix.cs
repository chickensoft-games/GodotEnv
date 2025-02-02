namespace Chickensoft.GodotEnv.Features.Godot.Models;

using System.IO.Abstractions;
using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Common.Utilities;
using global::GodotEnv.Common.Utilities;

public abstract class Unix : GodotEnvironment {
  protected Unix(ISystemInfo systemInfo, IFileClient fileClient, IComputer computer)
    : base(systemInfo, fileClient, computer) { }

  public override async Task<bool> IsExecutable(IShell shell, IFileInfo file) {
    var result = await shell.RunUnchecked("test", "-x", file.FullName);
    return result.ExitCode == 0;
  }
}
