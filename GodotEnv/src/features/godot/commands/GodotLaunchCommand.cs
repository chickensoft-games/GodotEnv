namespace Chickensoft.GodotEnv.Features.Godot.Commands;

using System;
using System.IO;
using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Common.Models;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using global::GodotEnv.Common.Utilities;

[Command("godot launch", Description = "Launches the currently active Godot version.")]
public class GodotLaunchCommand : ICommand, ICliCommand {
  public IExecutionContext ExecutionContext { get; set; } = default!;

  public GodotLaunchCommand(IExecutionContext context) {
    ExecutionContext = context;
  }

  private ISystemInfo SystemInfo => ExecutionContext.Godot.Platform.SystemInfo;


  public async ValueTask ExecuteAsync(IConsole console) {
    var log = ExecutionContext.CreateLog(console);

    string? godotPath = Environment.GetEnvironmentVariable("GODOT");

    if (string.IsNullOrWhiteSpace(godotPath)) {
      log.Err("❌ The GODOT environment variable is not set.");
      log.Print("To set it, use:\n    godotenv godot use <version>\n");
      return;
    }

    if (SystemInfo.OS == OSType.Windows && !godotPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) {
      godotPath += ".exe";
    }

    if (!File.Exists(godotPath)) {
      log.Err($"❌ The GODOT environment variable points to a missing file: {godotPath}");
      return;
    }

    log.Print($"🚀 Launching Godot from {godotPath}...");
    await ExecutionContext.Godot.GodotRepo.ProcessRunner.RunDetached(godotPath, []);
  }
}
