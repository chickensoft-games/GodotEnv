namespace Chickensoft.GodotEnv.Common.Clients;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Common.Utilities;

/// <summary>File client interface.</summary>
public interface IFileClient {
  public ISystemInfo SystemInfo { get; }

  /// <summary>Underlying file system interface used by the client.</summary>
  public IFileSystem Files { get; }

  /// <summary>Computer instance.</summary>
  public IComputer Computer { get; }

  /// <summary>
  /// Process runner. Used on Windows to perform elevated file operations.
  /// </summary>
  public IProcessRunner ProcessRunner { get; }

  /// <summary>Path directory separator.</summary>
  public char Separator { get; }

  /// <summary>User directory without trailing slashes.</summary>
  public string UserDirectory { get; }

  /// <summary>Application data directory without trailing slashes.</summary>
  public string AppDataDirectory { get; }

  /// <summary>
  /// Replaces invalid file system characters with underscores.
  /// </summary>
  /// <param name="path">File or folder path.</param>
  /// <returns>Sanitized path.</returns>
  public string Sanitize(string path);

  /// <summary>
  /// Combines the given path components into a single path.
  /// </summary>
  /// <param name="paths">Path components to combine.</param>
  /// <returns>Combined path.</returns>
  public string Combine(params string[] paths);

  /// <summary>
  /// Returns the target of the given symlink.
  /// </summary>
  /// <param name="symlinkPath">Path of the symlink / shortcut / alias.</param>
  /// <returns>Symlink target.</returns>
  public string DirectorySymlinkTarget(string symlinkPath);

  /// <summary>
  /// Returns the target of the given symlink.
  /// </summary>
  /// <param name="symlinkPath">Path of the symlink / shortcut / alias.</param>
  /// <returns>Symlink target.</returns>
  public string? FileSymlinkTarget(string symlinkPath);

  /// <summary>
  /// Creates all directories and subdirectories in the specified path unless
  /// they already exist.
  /// </summary>
  /// <param name="path">Directory path.</param>
  public void CreateDirectory(string path);

  /// <summary>
  /// Gets the path of the parent directory for the given path. If the path is
  /// itself a directory, this returns the parent directory, or the blank
  /// string if there is no parent directory.
  /// </summary>
  /// <param name="path">File or directory path.</param>
  public string GetParentDirectoryPath(string path);

  /// <summary>
  /// Gets the name of the parent directory for the given path (just the last
  /// directory component of the path).
  /// </summary>
  /// <param name="path">File or directory path.</param>
  /// <returns>Parent directory name.</returns>
  public string GetParentDirectoryName(string path);

  /// <summary>
  /// Creates a symbolic link identified by <paramref name="path" />
  /// that points to <paramref name="pathToTarget" />. If the target is a
  /// directory, a directory symlink will be created. Otherwise, a file symlink
  /// is created. If the symlink already exists, it is deleted and recreated.
  /// </summary>
  /// <param name="path">Path to the symbolic link.</param>
  /// <param name="pathToTarget">Path to the target of the symbolic
  /// link.</param>
  public Task CreateSymlink(string path, string pathToTarget);

  /// <summary>
  /// Creates symbolic links recursively on <paramref name="path" />,
  /// these links points to <paramref name="pathToTarget" /> by the same structure.
  /// Directories are not symlinked but created.
  /// </summary>
  /// <param name="path">Path to the symbolic link.</param>
  /// <param name="pathToTarget">Path to the target of the symbolic
  /// link.</param>
  public Task CreateSymlinkRecursively(string path, string pathToTarget);

  /// <summary>
  /// Determines if the given directory path is a symlink.
  /// </summary>
  /// <param name="path">Path in question.</param>
  /// <returns>True if the path is a symlink, false otherwise.</returns>
  public bool IsDirectorySymlink(string path);

  /// <summary>
  /// Determines if the given file path is a symlink.
  /// </summary>
  /// <param name="path">Path in question.</param>
  /// <returns>True if the path is a symlink, false otherwise.</returns>
  public bool IsFileSymlink(string path);

  /// <summary>
  /// Deletes a directory. The directory can be a symbolic link or an actual
  /// directory.<br />
  /// Tries to use cmd.exe with elevated privileges on windows to remove
  /// symlinks.
  /// </summary>
  /// <param name="path">Path of the directory to delete.</param>
  public Task DeleteDirectory(string path);

  /// <summary>
  /// Deletes a single file.<br />
  /// Tries to use cmd.exe with elevated privileges on windows to remove
  /// symlinks.
  /// </summary>
  /// <param name="path">Absolute path of file to delete.</param>
  public Task DeleteFile(string path);

  /// <summary>
  /// Copies files in bulk, excluding any `.git` folder inside the source
  /// path. Note: robocopy is used on Windows and rsync is used on Unix.
  /// </summary>
  /// <param name="shell">Shell to use for copy commands.</param>
  /// <param name="source">Source directory path to be copied.</param>
  /// <param name="destination">Destination directory path.</param>
  /// <returns>Task that completes when the copy processes complete.</returns>
  /// <exception cref="IOException" />
  public Task CopyBulk(IShell shell, string source, string destination);

  /// <summary>
  /// Given a list of possible file names and a project path, return the first
  /// file that exists.
  /// </summary>
  /// <param name="possibleFilenames">Possible file names.</param>
  /// <param name="basePath">Path to search.</param>
  /// <returns>The first found file matching
  /// <paramref name="possibleFilenames" />, or null.</returns>
  public string? FileThatExists(string[] possibleFilenames, string basePath);

  /// <summary>
  /// Computes the rooted path of <paramref name="url" /> using
  /// <paramref name="basePath" /> as the base path if <paramref name="url" />
  /// is not rooted.
  /// </summary>
  /// <param name="url">Path url.</param>
  /// <param name="basePath">Base path</param>
  /// <returns>Rooted path.</returns>
  public string GetRootedPath(string url, string basePath);

  /// <summary>
  /// Gets the full path of the given path (.NET's equivalent to path
  /// normalization).
  /// </summary>
  /// <param name="path">File path.</param>
  /// <returns>Full / "normalized" path.</returns>
  public string GetFullPath(string path);

  /// <summary>Determines whether the specified file exists.</summary>
  /// <param name="path">File path to check.</param>
  /// <returns>True if the file exists, false otherwise.</returns>
  public bool FileExists(string path);

  /// <summary>Determines whether the specified directory exists.</summary>
  /// <param name="path">Directory path to check.</param>
  /// <returns>True if the directory exists, false otherwise.</returns>
  public bool DirectoryExists(string path);

  /// <summary>
  /// Reads and deserializes a JSON file into the given type.
  /// </summary>
  /// <param name="path">Path to the json file.</param>
  /// <typeparam name="T">JSON model type.</typeparam>
  /// <exception cref="InvalidOperationException" />
  /// <returns>The deserialized JSON model from the file.</returns>
  public T ReadJsonFile<T>(string path) where T : notnull;

  /// <summary>
  /// Reads and deserializes a json file of type<typeparamref name= "T" />
  /// from a directory path and list of possible filenames to load from. The
  /// first file in the list that exists will be loaded. If no file exists, the
  /// default value will be returned.
  /// </summary>
  /// <typeparam name="T">Type of the json file.</typeparam>
  /// <param name="projectPath">Project path.</param>
  /// <param name="possibleFilenames">Possible file names for the file to load.
  /// The first file that exists will be loaded.
  /// </param>
  /// <param name="filename">Resolved filename that will be set to the name
  /// of the file that was loaded from the possible file names. If no file
  /// was found, it will be set to the first of the possible filenames.</param>
  /// <param name="defaultValue">Default value to return if none of the files
  /// can be found.</param>
  /// <returns>Loaded json model (or the default value).</returns>
  /// <exception cref="InvalidOperationException" />
  /// <exception cref="IOException" />
  public T ReadJsonFile<T>(
      string projectPath,
      string[] possibleFilenames,
      out string filename,
      T defaultValue
    ) where T : notnull;

  /// <summary>Get a <see cref="TextReader"/> for a given file path.</summary>
  /// <param name="path">File to read.</param>
  /// <returns>
  /// A reader to use for reading the file. This value must be disposed by
  /// the caller.
  /// </returns>
  public TextReader GetReader(string path);

  /// <summary>
  /// Get a <see cref="TextWriter"/> for a given file path.
  /// </summary>
  /// <param name="path">File to write.</param>
  /// <returns>
  /// A writer to use for writing to the file. This value must be disposed by
  /// the caller.
  /// </returns>
  /// <remarks>
  /// Clears the file contents; if you wish to modify or append to them, you
  /// must read them in before calling this method.
  /// </remarks>
  public TextWriter GetWriter(string path);

  /// <summary>
  /// Get a <see cref="Stream"/> for reading a given file.
  /// </summary>
  /// <param name="path">File to read.</param>
  /// <returns>
  /// A stream to use for reading the file. This value must be disposed by the
  /// caller.
  /// </returns>
  public Stream GetReadStream(string path);

  /// <summary>Read text lines from a file.</summary>
  /// <param name="path">File to read.</param>
  /// <returns>List of lines.</returns>
  public List<string> ReadLines(string path);

  /// <summary>Overwrite a file with new lines of text.</summary>
  /// <param name="path">File to read.</param>
  /// <param name="lines">Lines to write.</param>
  public void WriteLines(string path, IEnumerable<string> lines);

  /// <summary>
  /// Writes a JSON model to the file system.
  /// </summary>
  /// <param name="filePath">Absolute path to the destination file.</param>
  /// <param name="data">Data to be converted to JSON.</param>
  /// <typeparam name="T">Type of the JSON model.</typeparam>
  public void WriteJsonFile<T>(string filePath, T data) where T : notnull;

  /// <summary>
  /// Creates a file with the given contents.
  /// </summary>
  /// <param name="filePath">Absolute path of file to create.</param>
  /// <param name="contents">File contents.</param>
  public void CreateFile(string filePath, string contents);

  /// <summary>
  /// Searches through a directory recursively, capturing file info for each
  /// selected file.
  /// </summary>
  /// <param name="dir">Directory to search recursively.</param>
  /// <param name="selector">Selector function. Return true to cause the file
  /// to be added to the list of returned files.</param>
  /// <param name="dirSelector">Directory selector. Return true to enter
  /// the proposed subdirectory, return false to prevent searching that
  /// subdirectory.</param>
  /// <param name="onDirectory">Callback invoked for each directory.</param>
  /// <param name="indent">Optional indent.</param>
  /// <returns>List of selected files.</returns>
  public Task<List<IFileInfo>> SearchRecursively(
    string dir,
    Func<IFileInfo, string, Task<bool>> selector,
    Func<IDirectoryInfo, Task<bool>> dirSelector,
    Action<IDirectoryInfo, string> onDirectory,
    string indent = ""
  );

  /// <summary>
  /// Adds lines to a file if the file does not contain the lines. This is
  /// case insensitive and trims whitespace.
  /// </summary>
  /// <param name="filePath">File path.</param>
  /// <param name="lines">Lines to add to the file. Any lines the file does
  /// not contain will be added to the file.</param>
  public void AddLinesToFileIfNotPresent(
    string filePath,
    params string[] lines
  );

  /// <summary>
  /// Returns the line in a file that begins with the specified prefix. Case-
  /// insensitive and trims whitespace.
  /// </summary>
  /// <param name="filePath">File path.</param>
  /// <param name="prefix">Line prefix.</param>
  /// <returns>First matching line, or the empty string.</returns>
  public string FindLineBeginningWithPrefix(string filePath, string prefix);

  /// <summary>
  /// Returns directory information for each subdirectory in the given
  /// directory.
  /// </summary>
  /// <param name="dir">Directory to examine.</param>
  /// <returns>Enumerable of directory info objects.</returns>
  public IEnumerable<IDirectoryInfo> GetSubdirectories(string dir);

  /// <summary>
  /// Returns directory information for each ancestor directory of the given
  /// directory.
  /// </summary>
  /// <param name="dir">Directory to examine.</param>
  /// <returns>Enumerable of directory info objects.</returns>
  public IEnumerable<IDirectoryInfo> GetAncestorDirectories(string dir);

  /// <summary>
  /// Returns the full names (including paths) of files within the given
  /// directory, optionally matching a provided wildcard pattern.
  /// </summary>
  /// <param name="dir">Directory to examine.</param>
  /// <param name="wildcardPattern">
  /// Wildcard pattern to match, or null to enumerate all files.
  /// </param>
  /// <returns>Enumerable of strings.</returns>
  public IEnumerable<string> GetFiles(string dir, string? wildcardPattern = null);
}

/// <summary>File system operations client.</summary>
public class FileClient : IFileClient {
  public ISystemInfo SystemInfo { get; }
  public IFileSystem Files { get; }
  public IComputer Computer { get; }
  public IProcessRunner ProcessRunner { get; }
  // public OSFamily OSFamily { get; }
  // public OSType OS { get; }
  // public ProcessorType Processor { get; }
  public char Separator { get; }
  public JsonSerializerOptions JsonSerializerOptions { get; }

  public string UserDirectory => Path.TrimEndingDirectorySeparator(
    Environment.GetFolderPath(
      Environment.SpecialFolder.UserProfile,
      Environment.SpecialFolderOption.DoNotVerify
    )
  );

  public string AppDataDirectory => Files.Path.Combine(
    Files.Path.TrimEndingDirectorySeparator(
      Environment.GetFolderPath(
        Environment.SpecialFolder.ApplicationData,
        Environment.SpecialFolderOption.Create
      )
    ),
    Defaults.BIN_NAME
  );

  public FileClient(
    ISystemInfo systemInfo,
    IFileSystem fs,
    IComputer computer,
    IProcessRunner processRunner
  ) {
    SystemInfo = systemInfo;
    Files = fs;
    Computer = computer;
    ProcessRunner = processRunner;
    Separator = Files.Path.DirectorySeparatorChar;
    JsonSerializerOptions = new JsonSerializerOptions {
      WriteIndented = true,
      ReadCommentHandling = JsonCommentHandling.Skip,
      AllowTrailingCommas = true,
    };
  }

  public string Sanitize(string path) =>
    Files.Path.GetInvalidFileNameChars()
      .Union(Files.Path.GetInvalidPathChars())
      .Aggregate(path, static (current, c) => current.Replace(c, '_')).Trim('_');

  /// <summary>
  /// Checks a string to see if it matches a glob pattern.
  /// Credit: https://stackoverflow.com/a/4146349
  /// </summary>
  /// <param name="str">The string in question.</param>
  /// <param name="pattern">The glob pattern.</param>
  /// <returns>True if the string satisfies the glob pattern.</returns>
  public static bool MatchesGlob(string str, string pattern) => new Regex(
    "^" +
    Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") +
    "$",
    RegexOptions.IgnoreCase | RegexOptions.Singleline
  ).IsMatch(str);

  public string Combine(params string[] paths) => Files.Path.Combine(paths);

  public string DirectorySymlinkTarget(string symlinkPath)
    => Files.DirectoryInfo.New(symlinkPath).LinkTarget!;

  public string? FileSymlinkTarget(string symlinkPath)
    => Files.FileInfo.New(symlinkPath).LinkTarget;

  public void CreateDirectory(string path)
    => Files.Directory.CreateDirectory(path);

  public string GetParentDirectoryPath(string path) =>
    Files.Path.GetDirectoryName(path)!;

  public string GetParentDirectoryName(string path) =>
    Files.DirectoryInfo.New(path).Name;

  public async Task CreateSymlink(string path, string pathToTarget) {
    // See what kind of symlink we're dealing with by checking the target.
    var isDirectory = Files.Directory.Exists(pathToTarget);

    // Remove any existing symlink
    if (isDirectory) {
      if (Files.Directory.Exists(path)) {
        await DeleteDirectory(path);
      }
    }
    else if (Files.File.Exists(path)) {
      await DeleteFile(path);
    }

    // On Windows, elevated privileges are required to manage symlinks
    if (SystemInfo.OS == OSType.Windows) {
      // Windows requires a flag if symlinking a directory
      var dirFlag = isDirectory ? "/D " : "";
      await ProcessRunner.RunElevatedOnWindows(
        "cmd.exe", $"/c mklink {dirFlag} \"{path}\" \"{pathToTarget}\""
      );
      return;
    }

    // Unix seems to manage symlinks fine.
    if (isDirectory) {
      var parentPath = GetParentDirectoryPath(path);
      Files.Directory.CreateDirectory(parentPath);
      Files.Directory.CreateSymbolicLink(path, pathToTarget);
    }
    else {
      Files.File.CreateSymbolicLink(path, pathToTarget);
    }
  }

  public async Task CreateSymlinkRecursively(string path, string pathToTarget) {
    CreateDirectory(path);
    foreach (var sub in Files.Directory.GetDirectories(pathToTarget)) {
      await CreateSymlinkRecursively(Files.Path.Join(path, Files.Path.GetFileName(sub)), sub);
    }

    foreach (var file in Files.Directory.GetFiles(pathToTarget)) {
      await CreateSymlink(Files.Path.Join(path, Files.Path.GetFileName(file)), file);
    }
  }

  public bool IsDirectorySymlink(string path)
    => Files.DirectoryInfo.New(path).LinkTarget != null;

  public bool IsFileSymlink(string path)
    => Files.FileInfo.New(path).LinkTarget != null;

  public async Task DeleteDirectory(string path) {
    if (!DirectoryExists(path)) { return; }

    if (IsDirectorySymlink(path)) {
      if (SystemInfo.OS == OSType.Windows) {
        await ProcessRunner.RunElevatedOnWindows(
          "cmd.exe", $"/c rmdir \"{path}\""
        );
        return;
      }
      Files.Directory.Delete(path);
      return;
    }

    if (SystemInfo.OS == OSType.Windows) {
      var parentDirectory = GetParentDirectoryPath(path);
      var directoryInfo = Files.DirectoryInfo.New(path);

      // Delete files inside directory using command prompt to prevent
      // C# permissions issues.
      var dirShell = Computer.CreateShell(path);
      await dirShell.Run("cmd.exe", "/c", "erase", "/s", "/f", "/q", "*");

      // Delete (now empty) directory itself.
      var parentShell = Computer.CreateShell(parentDirectory);
      await parentShell.RunUnchecked(
        "cmd.exe", "/c", "rmdir", directoryInfo.Name, "/s", "/q"
      );

      return;
    }

    // Linux and macOS seem to delete non-empty directories just fine.
    Files.Directory.Delete(path, recursive: true);
  }

  public async Task DeleteFile(string path) {
    if (IsFileSymlink(path) && SystemInfo.OS == OSType.Windows) {
      await ProcessRunner.RunElevatedOnWindows("cmd.exe", $"/c del \"{path}\"");
      return;
    }
    Files.File.Delete(path);
  }

  public async Task CopyBulk(IShell shell, string source, string destination) {
    if (SystemInfo.OSFamily == OSFamily.Windows) {
      var result = await shell.RunUnchecked(
        "robocopy", source, destination, "/e", "/xd", ".git"
      );
      if (result.ExitCode >= 8) {
        // Robocopy has non-traditional exit codes
        throw new IOException(
          $"Failed to copy `{source}` to `{destination}`"
        );
      }
    }
    else {
      await shell.Run(
        "rsync", "-av", source, destination, "--exclude", ".git"
      );
    }
  }

  public string? FileThatExists(string[] possibleFilenames, string basePath)
    => possibleFilenames
      .Select(filename => Files.Path.Combine(basePath, filename))
      .FirstOrDefault(Files.File.Exists);

  public string GetRootedPath(string url, string basePath) {
    if (Files.Path.IsPathRooted(url)) { return url; }
    // Why we use GetFullPath: https://stackoverflow.com/a/1299356
    return Files.Path.GetFullPath(Files.Path.Combine(basePath, url));
  }

  public string GetFullPath(string path) => Files.Path.GetFullPath(path);

  public bool FileExists(string path) => Files.File.Exists(path);

  public bool DirectoryExists(string path) => Files.Directory.Exists(path);

  public IEnumerable<IDirectoryInfo> GetSubdirectories(string dir) =>
    Files.DirectoryInfo.New(dir).EnumerateDirectories();

  /// <inheritdoc/>
  public IEnumerable<IDirectoryInfo> GetAncestorDirectories(string dir) {
    var directoryInfo = Files.DirectoryInfo.New(dir);
    while (directoryInfo is not null) {
      yield return directoryInfo;
      directoryInfo = directoryInfo.Parent;
    }
  }

  public IEnumerable<string> GetFiles(string dir, string? wildcardPattern = null) =>
    (wildcardPattern is null)
      ? Files.Directory.EnumerateFiles(dir)
      : Files.Directory.EnumerateFiles(dir, wildcardPattern);

  public T ReadJsonFile<T>(string path) where T : notnull {
    var contents = Files.File.ReadAllText(path);
    return JsonSerializer.Deserialize<T>(contents, JsonSerializerOptions)
      ?? throw new InvalidOperationException(
        $"Failed to load file `{path}.`"
      );
  }

  public T ReadJsonFile<T>(
    string projectPath,
    string[] possibleFilenames,
    out string filename,
    T defaultValue
  ) where T : notnull {
    foreach (var file in possibleFilenames) {
      var path = Path.Combine(projectPath, file);
      if (Files.File.Exists(path)) {
        try {
          var contents = Files.File.ReadAllText(path);
          filename = file;
          var data = JsonSerializer.Deserialize<T>(
            contents,
            JsonSerializerOptions
          );
          return data ??
            throw new InvalidOperationException(
              $"Couldn't load file `{path}`"
            );
        }
        catch (Exception e) {
          throw new IOException(
            $"Failed to deserialize file `{path}`", innerException: e
          );
        }
      }
    }
    filename = possibleFilenames[0];
    return defaultValue;
  }

  public void WriteJsonFile<T>(string filePath, T data) where T : notnull {
    var contents = JsonSerializer.Serialize(data, JsonSerializerOptions);
    Files.File.WriteAllText(filePath, contents);
  }

  /// <inheritdoc/>
  public TextReader GetReader(string path) => new StreamReader(path);

  /// <inheritdoc/>
  public TextWriter GetWriter(string path) => new StreamWriter(path);

  /// <inheritdoc/>
  public Stream GetReadStream(string path) => File.OpenRead(path);

  public List<string> ReadLines(string path) =>
    [.. Files.File.ReadAllLines(path)];

  public void WriteLines(string path, IEnumerable<string> lines) =>
    Files.File.WriteAllLines(path, lines);

  public void CreateFile(string filePath, string contents) {
    // Don't overwrite file if it exists.
    if (Files.File.Exists(filePath)) { return; }
    Files.File.WriteAllText(filePath, contents);
  }

  public async Task<List<IFileInfo>> SearchRecursively(
    string dir,
    Func<IFileInfo, string, Task<bool>> selector,
    Func<IDirectoryInfo, Task<bool>> dirSelector,
    Action<IDirectoryInfo, string> onDirectory,
    string indent = ""
  ) {
    var dirInfo = Files.DirectoryInfo.New(dir);
    if (!dirInfo.Exists) {
      throw new DirectoryNotFoundException(
        $"Directory not found: {dirInfo.FullName}"
      );
    }

    onDirectory(dirInfo, indent);

    var files = new List<IFileInfo>();

    foreach (var file in dirInfo.GetFiles()) {
      if (await selector(file, indent)) {
        files.Add(file);
      }
    }

    foreach (var subDir in dirInfo.GetDirectories()) {
      if (await dirSelector(subDir)) {
        files.AddRange(
          await SearchRecursively(
            subDir.FullName, selector, dirSelector, onDirectory, indent + "  "
          )
        );
      }
    }

    return files;
  }

  public void AddLinesToFileIfNotPresent(
    string filePath, params string[] lines
  ) {
    if (!Files.File.Exists(filePath)) {
      Files.File.WriteAllLines(filePath, lines);
      return;
    }

    var existingLines = ReadLines(filePath);

    var compareLines = existingLines
      .Select(line => line.ToLowerInvariant().Trim()).ToHashSet();

    var newLines = lines.Where(
      line => !compareLines.Contains(line.ToLowerInvariant().Trim())
    );

    WriteLines(filePath, existingLines.Concat(newLines));
  }

  public string FindLineBeginningWithPrefix(string filePath, string prefix) {
    if (!Files.File.Exists(filePath)) { return ""; }

    foreach (var line in ReadLines(filePath)) {
      if (line.Trim().StartsWith(prefix.Trim(), StringComparison.InvariantCultureIgnoreCase)) {
        return line;
      }
    }

    return "";
  }
}
