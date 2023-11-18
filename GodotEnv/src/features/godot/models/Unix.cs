namespace Chickensoft.GodotEnv.Features.Godot.Models;

using System.IO.Abstractions;
using System.Threading.Tasks;
using Common.Clients;
using Common.Utilities;

public abstract class Unix : GodotEnvironment {
  protected Unix(IFileClient fileClient, IComputer computer)
    : base(fileClient, computer) { }

  public override async Task<bool> IsExecutable(IShell shell, IFileInfo file) {
    var result = await shell.RunUnchecked("test", "-x", file.FullName);
    return result.ExitCode == 0;
  }
}
