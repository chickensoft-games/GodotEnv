using System.Runtime.CompilerServices;

// IMPORTANT: Allow us to test internal methods in our test project.
[assembly: InternalsVisibleTo("Chickensoft.Chicken.Tests")]
namespace Chickensoft.Chicken;

using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx;

public class Chicken {
  public static Task<int> Main(string[] args) {
    List<string> cliArgs = new();
    List<string> commandArgs = new();

    var inCommandArgs = false;

    foreach (var arg in args) {
      if (arg == "--") {
        inCommandArgs = true;
        continue;
      }

      if (inCommandArgs) {
        commandArgs.Add(arg);
        continue;
      }

      cliArgs.Add(arg);
    }

    App.CommandArgs = commandArgs.ToArray();

    return new CliApplicationBuilder()
      .AddCommandsFromThisAssembly()
      .Build().RunAsync(cliArgs).AsTask();
  }
}
