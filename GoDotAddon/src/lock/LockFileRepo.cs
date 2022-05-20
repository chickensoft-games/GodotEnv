namespace Chickensoft.GoDotAddon {
  using System.IO.Abstractions;
  using Newtonsoft.Json;

  public interface ILockFileRepo {
    LockFile LoadOrCreateLockFile(string lockFilePath);
    void SaveLockFile(string lockFilePath, LockFile lockFile);
  }

  public class LockFileRepo : ILockFileRepo {
    private readonly IFileSystem _fs;
    private readonly IApp _app;

    public LockFileRepo(IApp app) {
      _app = app;
      _fs = app.FS;
    }

    public LockFile LoadOrCreateLockFile(string lockFilePath) {
      if (_fs.File.Exists(lockFilePath)) {
        return LoadLockFile(lockFilePath);
      }
      else {
        return CreateLockFile();
      }
    }

    public void SaveLockFile(string lockFilePath, LockFile lockFile) {
      var json = JsonConvert.SerializeObject(lockFile, Formatting.Indented);
      _fs.File.WriteAllText(lockFilePath, json);
    }

    private LockFile LoadLockFile(string lockFilePath)
      => _app.LoadFile<LockFile>(lockFilePath);

    private static LockFile CreateLockFile() {
      var lockFile = new LockFile();
      return lockFile;
    }
  }
}
