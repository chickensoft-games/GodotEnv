namespace Chickensoft.GodotEnv.Common.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

// Responsible for providing access to a GodotEnvConfig (for strongly-typed
// config values that don't rely on string keys and can be serialized) and an
// IConfiguration (for user-facing key-value lookups and changes), and keeping
// the two in sync
public class Config {
  private readonly IConfiguration _configuration;
  private readonly GodotEnvConfig _godotEnvConfig;
  public IReadOnlyGodotEnvConfig GodotEnvConfig => _godotEnvConfig;

  public Config() : this(new GodotEnvConfig()) {
  }

  public Config(IConfiguration configuration) {
    _configuration = configuration;
    _godotEnvConfig = new();
    _configuration.Bind(GodotEnvConfig);
  }

  public Config(GodotEnvConfig godotEnvConfig) {
    _godotEnvConfig = godotEnvConfig;
    var json = JsonSerializer.Serialize(GodotEnvConfig);
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
    _configuration.GetValue<string>(key)
      ?? throw new ArgumentException($"Key \"{key}\" not valid");

  public void Set(string key, string value) {
    var oldValue = _configuration.GetValue<string>(key);
    if (string.IsNullOrEmpty(oldValue)) {
      throw new ArgumentException($"Key \"{key}\" not valid");
    }
    _configuration[key] = value;
    try {
      _configuration.Bind(GodotEnvConfig);
    }
    catch (Exception) {
      _configuration[key] = oldValue;
      _configuration.Bind(GodotEnvConfig);
      throw;
    }
  }

  public IEnumerable<KeyValuePair<string, string?>> AsEnumerable() =>
    _configuration.AsEnumerable();

  // We need the deprecated properties to update the values of the new properties
  /// <summary>
  /// Upgrades the configuration structure to the latest specification, removing
  /// deprecated properties/keys and transferring their values to newer ones
  /// if necessary.
  /// </summary>
#pragma warning disable CS0618
  public void Upgrade() {
    if (GodotEnvConfig.GodotInstallationsPath is not null) {
      if (
        GodotEnvConfig.GodotInstallationsPath != Defaults.CONFIG_GODOT_INSTALLATIONS_PATH
        && GodotEnvConfig.GodotInstallationsPath != GodotEnvConfig.Godot.InstallationsPath
      ) {
        _godotEnvConfig.Godot.InstallationsPath = GodotEnvConfig.GodotInstallationsPath;
        _configuration["Godot.InstallationsPath"] = GodotEnvConfig.GodotInstallationsPath;
      }
      _godotEnvConfig.GodotInstallationsPath = null;
      _configuration["Godot.InstallationsPath"] = null;
    }
  }
#pragma warning restore CS0618
}

public interface IReadOnlyGodotEnvConfig {
  public string? GodotInstallationsPath { get; }
  public IReadOnlyGodotConfig Godot { get; }
  public IReadOnlyTerminalConfig Terminal { get; }
}

// required to be able to write out new values, since Extensions.Configuration
// doesn't directly support that
/// <summary>
/// Configuration values as strongly-typed, named C# properties. Access from
/// <see cref="Config"/>.
/// </summary>
public class GodotEnvConfig : IReadOnlyGodotEnvConfig {
  /// <summary>
  /// The old location of Godot.InstallationsPath, kept for backwards
  /// compatibility with old installations.
  /// </summary>
  [
    Obsolete(
      "GodotInstallationsPath is deprecated. Please use Godot.InstallationsPath"
    ),
    JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull),
    JsonPropertyName("godotInstallationsPath"),
  ]
  public string? GodotInstallationsPath { get; set; }

  public GodotConfig Godot { get; set; } = new();

  public TerminalConfig Terminal { get; set; } = new();

  IReadOnlyGodotConfig IReadOnlyGodotEnvConfig.Godot => Godot;

  IReadOnlyTerminalConfig IReadOnlyGodotEnvConfig.Terminal => Terminal;
}

public interface IReadOnlyGodotConfig {
  public string InstallationsPath { get; }
}

public class GodotConfig : IReadOnlyGodotConfig {
  public string InstallationsPath { get; set; }
    = Defaults.CONFIG_GODOT_INSTALLATIONS_PATH;
}

public interface IReadOnlyTerminalConfig {
  public bool UseEmoji { get; }
}

public class TerminalConfig : IReadOnlyTerminalConfig {
  public bool UseEmoji { get; set; }
    = Defaults.CONFIG_TERMINAL_USE_EMOJI;
}
