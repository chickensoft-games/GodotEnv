namespace Chickensoft.GodotEnv.Common.Models;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

public interface IConfig : IEnumerable<KeyValuePair<string, string?>> {
  public IReadOnlyConfigValues ConfigValues { get; }

  public string Get(string key);
  public void Set(string key, string value);
  public void Upgrade();
}

// Responsible for providing access to a ConfigValues (for strongly-typed
// config values that don't rely on string keys and can be serialized) and an
// IConfiguration (for user-facing key-value lookups and changes), and keeping
// the two in sync
public class Config : IConfig {
  private readonly IConfiguration _configuration;
  private readonly ConfigValues _configValues;
  public IReadOnlyConfigValues ConfigValues => _configValues;

  public Config() : this(new ConfigValues()) {
  }

  public Config(IConfiguration configuration) {
    _configuration = configuration;
    _configValues = new();
    _configuration.Bind(ConfigValues);
  }

  public Config(ConfigValues configValues) {
    _configValues = configValues;
    var json = JsonSerializer.Serialize(ConfigValues);
    using (var jsonStream = new MemoryStream()) {
      using (var writer = new StreamWriter(jsonStream, null, -1, true)) {
        writer.Write(json);
        writer.Flush();
      }
      jsonStream.Position = 0;
      _configuration = new ConfigurationBuilder()
        .AddJsonStream(jsonStream)
        .Build();
    }
  }

  public string Get(string key) =>
    _configuration.GetValue<string>(ConfigurationKey(key))
      ?? throw new ArgumentException($"Key \"{key}\" not valid");

  public void Set(string key, string value) {
    var configKey = ConfigurationKey(key);
    var oldValue = _configuration.GetValue<string>(configKey);
    if (string.IsNullOrEmpty(oldValue)) {
      throw new ArgumentException($"Key \"{key}\" not valid");
    }
    _configuration[configKey] = value;
    try {
      _configuration.Bind(ConfigValues);
    }
    catch (Exception) {
      _configuration[configKey] = oldValue;
      _configuration.Bind(ConfigValues);
      throw;
    }
  }

  public IEnumerator<KeyValuePair<string, string?>> GetEnumerator() {
    foreach (var keyValuePair in _configuration.AsEnumerable()) {
      yield return new(UserKey(keyValuePair.Key), keyValuePair.Value);
    }
  }

  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

  public static string ConfigurationKey(string userKey) =>
    userKey.Replace('.', ':');

  public static string UserKey(string configurationKey) =>
    configurationKey.Replace(':', '.');

  /// <summary>
  /// Upgrades the configuration structure to the latest specification, removing
  /// deprecated properties/keys and transferring their values to newer ones
  /// if necessary.
  /// </summary>
  // We need to use deprecated properties to update the values of the new properties
#pragma warning disable CS0618
  public void Upgrade() {
    if (ConfigValues.GodotInstallationsPath is not null) {
      if (
        ConfigValues.GodotInstallationsPath != Defaults.CONFIG_GODOT_INSTALLATIONS_PATH
        && (string.IsNullOrEmpty(ConfigValues.Godot.InstallationsPath)
          || ConfigValues.Godot.InstallationsPath == Defaults.CONFIG_GODOT_INSTALLATIONS_PATH)
      ) {
        _configValues.Godot.InstallationsPath = ConfigValues.GodotInstallationsPath;
        _configuration["Godot:InstallationsPath"] = ConfigValues.GodotInstallationsPath;
      }
      _configValues.GodotInstallationsPath = null;
      _configuration["Godot:InstallationsPath"] = null;
    }
  }
#pragma warning restore CS0618
}

public interface IReadOnlyConfigValues {
  [
    Obsolete(
      "GodotInstallationsPath is deprecated. Please use Godot.InstallationsPath"
    ),
    JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull),
    JsonPropertyName("godotInstallationsPath"),
  ]
  public string? GodotInstallationsPath { get; }
  public IReadOnlyGodotConfigSection Godot { get; }
  public IReadOnlyTerminalConfigSection Terminal { get; }
}

// required to be able to write out new values, since Extensions.Configuration
// doesn't directly support that
/// <summary>
/// Configuration values as strongly-typed, named C# properties. Access from
/// <see cref="Config"/>.
/// </summary>
public class ConfigValues : IReadOnlyConfigValues {
  /// <summary>
  /// The old location of Godot.InstallationsPath, kept for backwards
  /// compatibility with old installations of GodotEnv.
  /// </summary>
  [
    Obsolete(
      "GodotInstallationsPath is deprecated. Please use Godot.InstallationsPath"
    ),
    JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull),
    JsonPropertyName("godotInstallationsPath"),
  ]
  public string? GodotInstallationsPath { get; set; }

  public GodotConfigSection Godot { get; set; } = new();

  public TerminalConfigSection Terminal { get; set; } = new();

  IReadOnlyGodotConfigSection IReadOnlyConfigValues.Godot => Godot;

  IReadOnlyTerminalConfigSection IReadOnlyConfigValues.Terminal => Terminal;
}

public interface IReadOnlyGodotConfigSection {
  public string InstallationsPath { get; }
}

public class GodotConfigSection : IReadOnlyGodotConfigSection {
  public string InstallationsPath { get; set; }
    = Defaults.CONFIG_GODOT_INSTALLATIONS_PATH;
}

public interface IReadOnlyTerminalConfigSection {
  /// <summary>
  /// Whether terminal output includes emoji. Ignored on Windows, which never
  /// displays emoji in the terminal.
  /// </summary>
  public bool DisplayEmoji { get; }
}

public class TerminalConfigSection : IReadOnlyTerminalConfigSection {
  public bool DisplayEmoji { get; set; }
    = Defaults.CONFIG_TERMINAL_DISPLAY_EMOJI;
}
