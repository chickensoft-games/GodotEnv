namespace Chickensoft.GodotEnv.Features.Addons.Domain;

using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Common.Models;
using Chickensoft.GodotEnv.Features.Addons.Models;

public interface IAddonsFileRepository {
  IFileClient FileClient { get; }

  /// <summary>
  /// Loads the addons file in the project path and returns the filename
  /// that was used to load it (relative to the project path).
  /// </summary>
  /// <param name="projectPath">Where to search for an addons file.</param>
  /// <param name="filename">Filename that was loaded, or the first of the
  /// possible addons file names.</param>
  /// <returns>Loaded addons file (or an empty one).</returns>
  AddonsFile LoadAddonsFile(string projectPath, out string filename);

  /// <summary>
  /// Creates an addons configuration object that represents the configuration
  /// for how addons should be managed.
  /// </summary>
  /// <param name="projectPath">Path containing the addons file.</param>
  /// <param name="addonsFile">Addons file.</param>
  /// <returns>Addons configuration.</returns>
  AddonsConfiguration CreateAddonsConfiguration(
      string projectPath, AddonsFile addonsFile
    );

  /// <summary>
  /// Creates a default addons file in the project path.
  /// </summary>
  /// <param name="projectPath">Project path.</param>
  string CreateAddonsConfigurationStartingFile(string projectPath);
}

public class AddonsFileRepository : IAddonsFileRepository {
  public IFileClient FileClient { get; }

  public AddonsFileRepository(IFileClient fileClient) {
    FileClient = fileClient;
  }

  public AddonsFile LoadAddonsFile(string projectPath, out string filename) =>
    FileClient.ReadJsonFile(
      projectPath: projectPath,
      possibleFilenames: new string[] { "addons.json", "addons.jsonc" },
      filename: out filename,
      defaultValue: new AddonsFile()
    );

  public AddonsConfiguration CreateAddonsConfiguration(
    string projectPath, AddonsFile addonsFile
  ) => new(
    ProjectPath: projectPath,
    AddonsPath: FileClient.Combine(projectPath, addonsFile.PathRelativePath),
    CachePath: FileClient.Combine(projectPath, addonsFile.CacheRelativePath)
  );

  /// <summary>
  /// This command does the following (non-destructively).
  /// <br />
  /// - If an addons.jsonc file does not exist, it creates an example one
  ///   in the project path.
  /// - If an addons/.editorconfig file does not exist, it creates one to
  ///   ignore C# scripts in the addons directory.
  /// - If the .gitignore file does not exist, it creates one.
  /// - If the .gitignore does not contain the entries for ignoring the addons
  ///   directory (except for the editorconfig), it adds them.
  /// </summary>
  /// <param name="projectPath">Godot project path.</param>
  /// <returns>Path to the addons file.</returns>
  public string CreateAddonsConfigurationStartingFile(string projectPath) {
    // This command does the following (non-destructively).
    //
    // - If an addons.jsonc file does not exist, it creates an example one
    //   in the project path.
    // - If an addons/.editorconfig file does not exist, it creates one to
    //   ignore C# scripts in the addons directory.
    // - If the .gitignore file does not exist, it creates one.
    // - If the .gitignore does not contain the entries for ignoring the addons
    //   directory (except for the editorconfig), it adds them.
    var addonsFilePath = FileClient.Combine(projectPath, "addons.jsonc");
    FileClient.CreateFile(addonsFilePath, Defaults.ADDONS_FILE);

    var addonsPath = FileClient.Combine(projectPath, Defaults.ADDONS_PATH);
    FileClient.CreateDirectory(addonsPath);

    var addonsEditorConfigFilePath = FileClient.Combine(
      addonsPath, ".editorconfig"
    );
    FileClient.CreateFile(
      addonsEditorConfigFilePath, Defaults.ADDONS_EDITOR_CONFIG_FILE
    );

    var gitIgnoreFilePath = FileClient.Combine(projectPath, ".gitignore");

    if (!FileClient.FileExists(gitIgnoreFilePath)) {
      FileClient.CreateFile(gitIgnoreFilePath, Defaults.ADDONS_GIT_IGNORE_FILE);
    }
    else {
      var addonsIgnoreEntry = $"{Defaults.ADDONS_PATH}/*";
      var addonsEditorConfigExceptionEntry =
        $"!{Defaults.ADDONS_PATH}/.editorconfig";

      FileClient.AddLinesToFileIfNotPresent(
        gitIgnoreFilePath,
        addonsIgnoreEntry, addonsEditorConfigExceptionEntry
      );
    }

    return addonsFilePath;
  }
}
