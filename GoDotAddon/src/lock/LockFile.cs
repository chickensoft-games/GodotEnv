namespace Chickensoft.GoDotAddon {
  using System.Collections.Generic;
  using System.Text.Json.Serialization;

  public interface ILockFile {
    Dictionary<string, Dictionary<string, LockFileEntry>> Addons { get; init; }
  }

  public record LockFile : ILockFile {
    // [url][subfolder] = LockFileEntry
    [JsonPropertyName("addons")]
    public Dictionary<string, Dictionary<string, LockFileEntry>>
      Addons { get; init; } = new();
  }
}
