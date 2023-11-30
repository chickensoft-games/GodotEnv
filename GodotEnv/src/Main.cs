using System.Runtime.CompilerServices;

// IMPORTANT: Allow us to test internal methods in our test project.
[assembly: InternalsVisibleTo("Chickensoft.GodotEnv.Tests")]
namespace Chickensoft.GodotEnv;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
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

public static class GodotEnv {
  public static async Task<int> Main(string[] args) {
    // App-wide dependencies
    var computer = new Computer();
    var processRunner = new ProcessRunner();
    var fileClient = new FileClient(new FileSystem(), computer, processRunner);
    var configFileRepo = new ConfigFileRepository(fileClient);
    var config = configFileRepo.LoadConfigFile(out var _);
    var workingDir = Environment.CurrentDirectory;
    var networkClient = new NetworkClient(
      downloadService: new DownloadService(),
      downloadConfiguration: Defaults.DownloadConfiguration
    );
    IZipClient zipClient = (fileClient.OS == OSType.Windows)
      ? new ZipClient(fileClient.Files)
      : new ZipClientTerminal(computer, fileClient.Files);
    var systemEnvironmentVariableClient =
      new SystemEnvironmentVariableClient(processRunner, fileClient);

    // Addons feature dependencies

    var addonsFileRepo = new AddonsFileRepository(fileClient);
    // This loads the addons config from the addons.json or addons.jsonc file
    // (if it's in the working directory — otherwise creates a default config).
    var addonsConfig = addonsFileRepo.CreateAddonsConfiguration(
      projectPath: workingDir,
      addonsFile: addonsFileRepo.LoadAddonsFile(workingDir, out var _)
    );
    var addonsRepo = new AddonsRepository(
      fileClient: fileClient,
      computer: computer,
      config: addonsConfig
    );
    var addonGraph = new AddonGraph();
    var addonsLogic = new AddonsLogic(
      addonsFileRepo: addonsFileRepo,
      addonsRepo: addonsRepo,
      addonGraph: addonGraph
    );

    var addonsContext = new AddonsContext(
      AddonsFileRepo: addonsFileRepo,
      AddonsConfig: addonsConfig,
      AddonsRepo: addonsRepo,
      AddonGraph: addonGraph,
      AddonsLogic: addonsLogic
    );

    // Godot feature dependencies
    var platform = GodotEnvironment.Create(
      os: fileClient.OS, fileClient: fileClient, computer: computer
    );
    var godotRepo = new GodotRepository(
      config: config,
      fileClient: fileClient,
      networkClient: networkClient,
      zipClient: zipClient,
      platform: platform,
      systemEnvironmentVariableClient: systemEnvironmentVariableClient
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
      .UseTypeActivator(new GodotEnvActivator(context))
      .Build().RunAsync(context.CliArgs);

    // Save any changes made to our configuration file after running commands.
    configFileRepo.SaveConfig(config);

    return result;
  }

  internal static ExecutionContext CreateExecutionContext(
    string[] args,
    ConfigFile config,
    string workingDir,
    IAddonsContext addonsContext,
    IGodotContext godotContext
  ) {
    List<string> cliArgs = new();
    List<string> commandArgs = new();

    // VSCode doesn't break apart the prompt input string
    // into multiple arguments, so we'll do that if we're being
    // debugged from VSCode.
    var preprocessedArgs = args.ToList();
    if (args.Length == 2 && args[0] is "--debug") {
      preprocessedArgs = ReadArgs(args[1]).ToList();
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
      CliArgs: cliArgs.ToArray(),
      CommandArgs: commandArgs.ToArray(),
      Version: version,
      WorkingDir: workingDir,
      Config: config,
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
    return args.ToArray();
  }
}

/// <summary>
/// Custom type activator for CliFx. Creates commands by passing in the
/// execution context.
/// </summary>
/// <param name="context">Execution context.</param>
public class GodotEnvActivator(IExecutionContext context) : ITypeActivator {
  public IExecutionContext ExecutionContext { get; } = context;

  /// <summary>
  /// Use slow reflection to create a command. Commands must have a
  /// single-parameter constructor that receives the execution context.
  /// </summary>
  /// <param name="type">Command type to create.</param>
  public object CreateInstance(Type type)
    => Activator.CreateInstance(type, ExecutionContext)!;
}
