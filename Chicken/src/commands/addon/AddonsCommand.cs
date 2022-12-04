namespace Chickensoft.Chicken;
using System;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

[Command("addons", Description = "Manage addons.")]
public class AddonsCommand : ICommand {
  public ValueTask ExecuteAsync(IConsole console) {
    var output = console.Output;
    console.ForegroundColor = ConsoleColor.Yellow;
    output.WriteLine("");
    output.WriteLine("Please use a subcommand to manage addons.");
    output.WriteLine("");
    console.ResetColor();
    output.WriteLine("For example:");
    output.WriteLine("");
    console.ForegroundColor = ConsoleColor.Green;
    output.WriteLine("    chicken addons install");
    output.WriteLine("");
    return new();
  }
}
