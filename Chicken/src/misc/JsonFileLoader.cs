namespace Chickensoft.Chicken;

using System;
using System.IO;
using System.IO.Abstractions;
using CliFx.Exceptions;
using Newtonsoft.Json;

public interface IJsonFileLoader<T> {
  T Load(string projectPath, string[] possibleFilenames, T defaultValue);
}

/// <summary>
/// Repository which loads a json file of type <typeparamref name="T"/> from a
/// project path and list of possible filenames to load from. The first file
/// in the list that exists will be loaded. If no file exists, the default
/// value will be returned.
/// </summary>
/// <typeparam name="T">Type of json model to load.</typeparam>
public class JsonFileLoader<T> : IJsonFileLoader<T> {
  protected readonly IFileSystem _fs;
  protected readonly IApp _app;

  /// <summary>
  /// Create a new json file repo with the specified app.
  /// </summary>
  /// <param name="app">App to use for loading files.</param>
  public JsonFileLoader(IApp app, IFileSystem fs) {
    _app = app;
    _fs = fs;
  }

  /// <summary>
  /// Loads a json file of type<typeparamref name= "T" /> from a
  /// project path and list of possible filenames to load from. The first file
  /// in the list that exists will be loaded. If no file exists, the default
  /// value will be returned.
  /// </summary>
  /// <param name="projectPath">Project path.</param>
  /// <param name="possibleFilenames">Possible file names for the file to load.
  /// </param>
  /// <param name="defaultValue">Default value to return if none of the files
  /// can be found.</param>
  /// <returns>Loaded json model (or the default value).</returns>
  public T Load(
    string projectPath, string[] possibleFilenames, T defaultValue
  ) {
    foreach (var filename in possibleFilenames) {
      var path = Path.Combine(projectPath, filename);
      if (_fs.File.Exists(path)) {
        try {
          var contents = _fs.File.ReadAllText(path);
          var file = JsonConvert.DeserializeObject<T>(contents);
          if (file == null) {
            throw new InvalidOperationException(
              $"Couldn't load file `{path}`"
            );
          }
          return file;
        }
        catch (Exception e) {
          throw new CommandException(
            $"Failed to deserialize file `{path}`", innerException: e
          );
        }
      }
    }
    return defaultValue;
  }
}
