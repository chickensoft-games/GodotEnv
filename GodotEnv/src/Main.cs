namespace Chickensoft.GodotEnv;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Common.Domain;
using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Common.Utilities;
using Chickensoft.GodotEnv.Features.Addons.Commands;
using Chickensoft.GodotEnv.Features.Addons.Domain;
using Chickensoft.GodotEnv.Features.Godot.Domain;
using Chickensoft.GodotEnv.Features.Godot.Models;
using CliFx;
using CliFx.Infrastructure;
using Downloader;
using global::GodotEnv.Common.Utilities;

public static class GodotEnv {
  public static async Task<int> Main(string[] args) {
    // App-wide dependencies
    var systemInfo = new SystemInfo();

    var computer = new Computer();

    var processRunner = new ProcessRunner();

    var fileClient = new FileClient(systemInfo, new FileSystem(), computer, processRunner);

    var configFileRepo = new ConfigFileRepository(fileClient);

    var config = configFileRepo.LoadConfigFile(out var _);

    var workingDir = Environment.CurrentDirectory;

    var networkClient = new NetworkClient(
      downloadService: new DownloadService(),
      downloadConfiguration: Defaults.DownloadConfiguration
    );

    IZipClient zipClient = (systemInfo.OS == OSType.Windows)
      ? new ZipClient(fileClient.Files)
      : new ZipClientTerminal(computer, fileClient.Files);

    var environmentVariableClient =
      new EnvironmentVariableClient(
        systemInfo,
        processRunner,
        fileClient,
        computer
      );

    // Addons feature dependencies

    var addonsFileRepo = new AddonsFileRepository(fileClient);

    // This loads the addons config from the addons.json or addons.jsonc file
    // (if it's in the working directory — otherwise creates a default config).
    var mainAddonsFile = addonsFileRepo.LoadAddonsFile(workingDir, out var _);
    var addonsConfig = addonsFileRepo.CreateAddonsConfiguration(
      projectPath: workingDir,
      addonsFile: mainAddonsFile
    );
    var addonsRepo = new AddonsRepository(
      systemInfo: systemInfo,
      fileClient: fileClient,
      networkClient: networkClient,
      zipClient: zipClient,
      computer: computer,
      config: addonsConfig,
      processRunner: processRunner
    );
    var addonGraph = new AddonGraph();
    var addonsInstaller = new AddonsInstaller(
      addonsFileRepo: addonsFileRepo,
      addonsRepo: addonsRepo,
      addonGraph: addonGraph
    );

    var addonsContext = new AddonsContext(
      MainAddonsFile: mainAddonsFile,
      AddonsFileRepo: addonsFileRepo,
      AddonsConfig: addonsConfig,
      AddonsRepo: addonsRepo,
      AddonGraph: addonGraph,
      AddonsInstaller: addonsInstaller
    );

    // Godot feature dependencies
    var platform = GodotEnvironment.Create(systemInfo: systemInfo, fileClient: fileClient, computer: computer);

    var checksumRepository = new GodotChecksumClient(networkClient, platform);

    var godotRepo = new GodotRepository(
      systemInfo: systemInfo,
      config: config,
      fileClient: fileClient,
      networkClient: networkClient,
      zipClient: zipClient,
      platform: platform,
      environmentVariableClient: environmentVariableClient,
      processRunner: processRunner,
      checksumClient: checksumRepository
    );

    var godotContext = new GodotContext(
      Platform: platform,
      GodotRepo: godotRepo
    );

    // Create a context that contains all the information and dependencies that
    // commands need to execute.
    var context = CreateExecutionContext(
      args: args,
      config: config,
      workingDir: workingDir,
      systemInfo: systemInfo,
      addonsContext: addonsContext,
      godotContext: godotContext
    );

    configFileRepo.EnsureAppDataDirectoryExists();

    var result = await new CliApplicationBuilder()
      .SetExecutableName(Defaults.BIN_NAME)
      .SetTitle("GodotEnv")
      .SetVersion(context.Version)
      .SetDescription(
        """
        Manage your Godot environment from the command line on Windows, macOS,
          and Linux. Setup different versions of Godot and automatically update
          environment variables, as well as addons from local, remote, and even
          symlink'd paths using an addons.json file.
        """
      )
      .AddCommandsFromThisAssembly()
      .UseTypeActivator(
        new GodotEnvActivator(context, systemInfo.OS)
      )
      .Build().RunAsync(context.CliArgs);

    // Save any changes made to our configuration file after running commands.
    configFileRepo.SaveConfig(config);

    return result;
  }

  internal static ExecutionContext CreateExecutionContext(
    string[] args,
    ConfigFile config,
    string workingDir,
    ISystemInfo systemInfo,
    IAddonsContext addonsContext,
    IGodotContext godotContext
  ) {
    List<string> cliArgs = [];
    List<string> commandArgs = [];

    // VSCode doesn't break apart the prompt input string
    // into multiple arguments, so we'll do that if we're being
    // debugged from VSCode.
    var preprocessedArgs = args.ToList();
    if (args.Length == 2 && args[0] is "--debug") {
      preprocessedArgs = [.. ReadArgs(args[1])];
    }
    // ------------------------------------------------------ //

    var inCommandArgs = false;
    foreach (var arg in preprocessedArgs) {
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

    var version = Assembly
      .GetEntryAssembly()!
      .GetCustomAttribute<AssemblyInformationalVersionAttribute>()!
      .InformationalVersion;

    return new ExecutionContext(
      CliArgs: [.. cliArgs],
      CommandArgs: [.. commandArgs],
      Version: version,
      WorkingDir: workingDir,
      Config: config,
      SystemInfo: systemInfo,
      Addons: addonsContext,
      Godot: godotContext
    );
  }

  /// <summary>
  /// Reads command line arguments from a single string.
  /// Credit: https://stackoverflow.com/a/23961658
  /// </summary>
  /// <param name="argsString">The string that contains the entire command line.
  /// </param>
  /// <returns>An array of the parsed arguments.</returns>
  public static string[] ReadArgs(string argsString) {
    // Collects the split argument strings
    var args = new List<string>();
    // Builds the current argument
    var currentArg = new StringBuilder();
    // Indicates whether the last character was a backslash escape character
    var escape = false;
    // Indicates whether we're in a quoted range
    var inQuote = false;
    // Indicates whether there were quotes in the current arguments
    var hadQuote = false;
    // Remembers the previous character
    var prevCh = '\0';
    // Iterate all characters from the input string
    for (var i = 0; i < argsString.Length; i++) {
      var ch = argsString[i];
      if (ch == '\\' && !escape) {
        // Beginning of a backslash-escape sequence
        escape = true;
      }
      else if (ch == '\\' && escape) {
        // Double backslash, keep one
        currentArg.Append(ch);
        escape = false;
      }
      else if (ch == '"' && !escape) {
        // Toggle quoted range
        inQuote = !inQuote;
        hadQuote = true;
        if (inQuote && prevCh == '"') {
          // Doubled quote within a quoted range is like escaping
          currentArg.Append(ch);
        }
      }
      else if (ch == '"' && escape) {
        // Backslash-escaped quote, keep it
        currentArg.Append(ch);
        escape = false;
      }
      else if (char.IsWhiteSpace(ch) && !inQuote) {
        if (escape) {
          // Add pending escape char
          currentArg.Append('\\');
          escape = false;
        }
        // Accept empty arguments only if they are quoted
        if (currentArg.Length > 0 || hadQuote) {
          args.Add(currentArg.ToString());
        }
        // Reset for next argument
        currentArg.Clear();
        hadQuote = false;
      }
      else {
        if (escape) {
          // Add pending escape char
          currentArg.Append('\\');
          escape = false;
        }
        // Copy character from input, no special meaning
        currentArg.Append(ch);
      }
      prevCh = ch;
    }
    // Save last argument
    if (currentArg.Length > 0 || hadQuote) {
      args.Add(currentArg.ToString());
    }
    return [.. args];
  }
}

/// <summary>
/// Custom type activator for CliFx. Creates commands by passing in the
/// execution context.
/// </summary>
/// <param name="context">Execution context.</param>
/// <param name="processRunner">Process runner.</param>
public class GodotEnvActivator : ITypeActivator {
  public IExecutionContext ExecutionContext { get; }
  public OSType OS { get; }

  public GodotEnvActivator(
    IExecutionContext context,
    OSType os
  ) {
    ExecutionContext = context;
    OS = os;
  }

  /// <summary>
  /// Use slow reflection to create a command. Commands must have a
  /// single-parameter constructor that receives the execution context.
  /// </summary>
  /// <param name="type">Command type to create.</param>
  public object CreateInstance(Type type) {
    var command = Activator.CreateInstance(type, ExecutionContext)!;

    if (ShouldElevateOnWindows(OS, command)) {
      var elevateTask = ElevateOnWindows();
      elevateTask.Wait();

      // Prevent the command to be run without elevation
      Environment.Exit(elevateTask.Result.ExitCode);
    }

    return command;
  }

  public static bool ShouldElevateOnWindows(OSType os, object command) =>
    os == OSType.Windows &&
    !Debugger.IsAttached &&
    !UACHelper.UACHelper.IsElevated &&
    UACHelper.UACHelper.IsAdministrator &&
    command is IWindowsElevationEnabled windowsElevationEnabledCommand &&
    windowsElevationEnabledCommand.IsWindowsElevationRequired;

  public static async Task<ProcessResult> ElevateOnWindows() {
    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
      throw new InvalidOperationException(
        "ElevateOnWindows is only supported on Windows."
      );
    }

    var argsList = Environment.GetCommandLineArgs();

    // Regardless of the call context, the executable returned by
    // GetCommandLineArgs is always a dll.
    // It can be executed with the dotnet command.
    var exe = argsList?.FirstOrDefault() ?? string.Empty;
    if (exe.EndsWith(".exe")) { exe = $"\"{exe}\""; }

    if (exe.EndsWith(".dll")) { exe = $"dotnet \"{exe}\""; }

    var args = string.Join(
      " ",
      argsList?.Skip(1)?.Select(
        arg => (arg?.Contains(' ') ?? false) ? $"\"{arg}\"" : arg
      )?.ToList() ?? []
    );

    // Rerun the godotenv command with elevation in a new window
    var process = UACHelper.UACHelper.StartElevated(new ProcessStartInfo() {
      FileName = "cmd",
      Arguments = $"/s /c \"cd /d \"{Environment.CurrentDirectory}\" & {exe} {args} & pause\"",
      UseShellExecute = true,
      Verb = "runas",
    });

    await process.WaitForExitAsync();

    return new ProcessResult(
      ExitCode: process.ExitCode,
      StandardOutput: "",
      StandardError: ""
    );
  }
}
