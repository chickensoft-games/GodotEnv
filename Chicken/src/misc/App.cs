namespace Chickensoft.Chicken;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using CliFx.Infrastructure;

public interface IApp {
  string WorkingDir { get; }

  IShell CreateShell(string workingDir);
  IAddonManager CreateAddonManager(
    IFileSystem fs,
    IAddonRepo addonRepo,
    IConfigFileLoader configFileLoader,
    ILog log,
    IDependencyGraph dependencyGraph
  );
  ILog CreateLog(IConsole console);
  IAddonRepo CreateAddonRepo(IFileSystem fs);
  IConfigFileLoader CreateConfigFileRepo(IFileSystem fs);
  IDependencyGraph CreateDependencyGraph();
  IEditActionsLoader CreateEditActionsLoader(IFileSystem fs);
  IEditActionsRepo CreateEditActionsRepo(
    IFileSystem fs,
    string repoPath,
    EditActions editActions,
    IDictionary<string, dynamic?> inputs
  );
  ITemplateGenerator CreateTemplateGenerator(
    string projectName,
    string projectPath,
    string templateDescription,
    IEditActionsRepo editActionsRepo,
    EditActions editActions,
    ILog log
  );
  string GenerateGuid();
  bool IsDirectorySymlink(IFileSystem fs, string path);
  string DirectorySymlinkTarget(IFileSystem fs, string symlinkPath);
  void CreateSymlink(IFileSystem fs, string path, string pathToTarget);
  void DeleteDirectory(IFileSystem fs, string path);
  string FileThatExists(
    IFileSystem fs, string projectPath, string[] possibleFilenames
  );
  string ResolveUrl(IFileSystem fs, ISourceRepository sourceRepo, string path);
  string GetRootedPath(string url, string path);
}

public class App : IApp {
  public const string DEFAULT_CACHE_PATH = ".addons";
  public const string DEFAULT_ADDONS_PATH = "addons";
  public const string DEFAULT_CHECKOUT = "main";
  public const string DEFAULT_SUBFOLDER = "/";
  public static readonly string[] ADDONS_CONFIG_FILES
    = new string[] { "addons.json", "addons.jsonc" };
  public const string ADDONS_LOCK_FILE = "addons.lock.json";
  public static readonly string[] EDIT_ACTIONS_FILES
    = new string[] { "EDIT_ACTIONS.json", "EDIT_ACTIONS.jsonc" };
  public string WorkingDir { get; } = Environment.CurrentDirectory;
  public static readonly HashSet<string> DEFAULT_EXCLUSIONS
    = new() { ".git", ".DS_Store", "thumbs.db" };

  public static string[] CommandArgs { get; set; } = Array.Empty<string>();

  public App() { }

  public App(string workingDir) => WorkingDir = workingDir;

  public IAddonManager CreateAddonManager(
    IFileSystem fs,
    IAddonRepo addonRepo,
    IConfigFileLoader configFileLoader,
    ILog log,
    IDependencyGraph dependencyGraph
  ) => new AddonManager(
    fs: fs,
    addonRepo: addonRepo,
    configFileLoader: configFileLoader,
    log: log,
    dependencyGraph: dependencyGraph
  );

  public ILog CreateLog(IConsole console)
    => new Log(console);
  public IAddonRepo CreateAddonRepo(IFileSystem fs) => new AddonRepo(this, fs);
  public IConfigFileLoader CreateConfigFileRepo(IFileSystem fs)
    => new ConfigFileLoader(this, fs);
  public IDependencyGraph CreateDependencyGraph() => new DependencyGraph();
  public IEditActionsLoader CreateEditActionsLoader(IFileSystem fs)
    => new EditActionsLoader(this, fs);
  public IEditActionsRepo CreateEditActionsRepo(
    IFileSystem fs,
    string repoPath,
    EditActions editActions,
    IDictionary<string, dynamic?> inputs
  ) => new EditActionsRepo(this, fs, repoPath, editActions, inputs);
  public ITemplateGenerator CreateTemplateGenerator(
    string projectName,
    string projectPath,
    string templateDescription,
    IEditActionsRepo editActionsRepo,
    EditActions editActions,
    ILog log
  ) => new TemplateGenerator(
    projectName,
    projectPath,
    templateDescription,
    editActionsRepo,
    editActions,
    log
  );

  public IShell CreateShell(string workingDir)
    => new Shell(new ProcessRunner(), workingDir);

  public string GenerateGuid()
    => Guid.NewGuid().ToString().ToUpper(CultureInfo.CurrentCulture);

  public bool IsDirectorySymlink(IFileSystem fs, string path)
  => fs.DirectoryInfo.FromDirectoryName(path).LinkTarget != null;

  public string DirectorySymlinkTarget(IFileSystem fs, string symlinkPath) =>
    fs.DirectoryInfo.FromDirectoryName(symlinkPath).LinkTarget;

  public void CreateSymlink(IFileSystem fs, string path, string pathToTarget)
    => fs.Directory.CreateSymbolicLink(path, pathToTarget);

  // Deletes a directory or a symlink directory correctly.
  public void DeleteDirectory(IFileSystem fs, string path) {
    if (IsDirectorySymlink(fs, path)) { fs.Directory.Delete(path); return; }
    fs.Directory.Delete(path, recursive: true);
  }

  // Given a list of possible filenames in a directory, returns the first
  // file name that exists in that directory as an immediate child, or the first
  // possible file name if no files exist.
  public string FileThatExists(
    IFileSystem fs, string projectPath, string[] possibleFilenames
  )
    => possibleFilenames.AsEnumerable()
      .Select(filename => Path.Combine(projectPath, filename))
      .FirstOrDefault((path) => fs.File.Exists(path)) ??
      possibleFilenames.First();

  /// <summary>
  /// Given an addon config and the path where the addon config resides,
  /// compute the actual addon's source url.
  /// <br />
  /// For addons sourced on the local machine, this will convert relative
  // paths into absolute paths.
  /// </summary>
  /// <param name="addonConfig">Addon config.</param>
  /// <param name="path">Path containing the addons.json the addon was
  /// required from.</param>
  /// <returns>Resolved addon source.</returns>
  public string ResolveUrl(
    IFileSystem fs, ISourceRepository sourceRepo, string path
  ) {
    var url = sourceRepo.Url;
    if (sourceRepo.IsRemote) { return url; }
    // If the path containing the addons.json is a symlink, determine the
    // actual path containing the addons.json file. This allows addons
    // that have their own addons with relative paths to be relative to
    // where the addon is actually stored, which is more intuitive.
    if (IsDirectorySymlink(fs, path)) {
      path = DirectorySymlinkTarget(fs, path);
    }
    // Locally sourced addons with relative paths are relative to the
    // addons.json file that defines them.
    return GetRootedPath(url, path);
  }

  public string GetRootedPath(string url, string path) {
    if (Path.IsPathRooted(url)) { return url; }
    // Why we use GetFullPath: https://stackoverflow.com/a/1299356
    return Path.GetFullPath(
      Path.TrimEndingDirectorySeparator(path) +
      Path.DirectorySeparatorChar +
      url
    );
  }
}
