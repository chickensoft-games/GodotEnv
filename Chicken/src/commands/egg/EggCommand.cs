namespace Chickensoft.Chicken;
using System;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

[Command("egg", Description = "Generate folders from templates.")]
public class EggCommand : ICommand {
  public ValueTask ExecuteAsync(IConsole console) {
    var output = console.Output;
    console.ForegroundColor = ConsoleColor.Yellow;
    output.WriteLine("");
    output.Write("Please use a subcommand to generate a project or ");
    output.Write("feature from a template.");
    output.WriteLine("");
    console.ResetColor();
    output.WriteLine("For example:");
    output.WriteLine("");
    console.ForegroundColor = ConsoleColor.Green;
    output.WriteLine("    chicken egg crack MyGodot3Game \\");
    output.WriteLine("      --egg \"git@github.com:chickensoft-games/" +
      "godot_3_game.git\" \\"
    );
    output.WriteLine("      -- --title \"MyGodot3Game\"");
    output.WriteLine("");
    return new();
  }
}
