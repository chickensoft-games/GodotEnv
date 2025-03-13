namespace Chickensoft.GodotEnv.Features.Godot.Models;

using System;

/// <summary>
/// Represents a canonical Godot version number, independent of stringification
/// schemes or .NET inclusion
/// </summary>
public record GodotVersionNumber {
  /// <summary>
  /// The major version number.
  /// </summary>
  public int Major { get; }
  /// <summary>
  /// The minor version number.
  /// </summary>
  public int Minor { get; }
  /// <summary>
  /// The patch version number (0 when the Godot release number has no patch).
  /// </summary>
  public int Patch { get; }
  /// <summary>
  /// The label string (e.g., "beta", "rc", "stable")
  /// </summary>
  public string Label { get; }
  /// <summary>
  /// The label number, if any (e.g., 1 if the Godot release ends with "-rc1").
  /// When the label is "stable", this value is -1.
  /// </summary>
  public int LabelNumber { get; }

  public GodotVersionNumber(int major, int minor, int patch, string label, int labelNumber) {
    if (major < 0) {
      throw new ArgumentException($"Major version {major} is invalid");
    }
    if (minor < 0) {
      throw new ArgumentException($"Minor version {minor} is invalid");
    }
    if (patch < 0) {
      throw new ArgumentException($"Patch version {patch} is invalid");
    }
    if (label.Length == 0) {
      throw new ArgumentException("Version must have a label");
    }
    if (char.IsDigit(label[^1])) {
      throw new ArgumentException("Label \"{label}\" ambiguously ends with number");
    }
    if (label != "stable" && labelNumber <= 0) {
      throw new ArgumentException($"Version label \"{label}\" with numeric identifier {labelNumber} is invalid");
    }
    if (label == "stable" && labelNumber >= 0) {
      throw new ArgumentException("Stable versions should not have a numeric label identifier.");
    }
    Major = major;
    Minor = minor;
    Patch = patch;
    Label = label;
    LabelNumber = labelNumber;
  }
}

public abstract record GodotVersion {
  /// <summary>
  /// The version number.
  /// </summary>
  public GodotVersionNumber Number { get; }

  public GodotVersion(GodotVersionNumber number) {
    Number = number;
  }

  public GodotVersion(
    int major, int minor, int patch, string label, int labelNumber
  ) {
    Number =
      new GodotVersionNumber(major, minor, patch, label, labelNumber);
  }
}

/// <summary>
/// Represents a version of Godot that may be .NET enabled or not.
/// </summary>
public sealed record DotnetAgnosticGodotVersion : GodotVersion {
  public DotnetAgnosticGodotVersion(GodotVersionNumber number)
    : base(number) { }

  public DotnetAgnosticGodotVersion(
    int major, int minor, int patch, string label, int labelNumber
  )
    : base(major, minor, patch, label, labelNumber) { }
}

/// <summary>
/// Represents a version of Godot that either is, or is not, .NET-enabled.
/// </summary>
public sealed record DotnetSpecificGodotVersion : GodotVersion {
  /// <summary>
  /// True if referencing the .NET version of Godot.
  /// </summary>
  public bool IsDotnet { get; }

  public DotnetSpecificGodotVersion(
    GodotVersionNumber number, bool isDotnet
  )
    : base(number) {
    IsDotnet = isDotnet;
  }

  public DotnetSpecificGodotVersion(
    int major,
    int minor,
    int patch,
    string label,
    int labelNumber,
    bool isDotnet
  )
    : base(major, minor, patch, label, labelNumber) {
    IsDotnet = isDotnet;
  }
}
