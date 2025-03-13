namespace Chickensoft.GodotEnv.Features.Godot.Models;

using System;

public partial record GodotVersion {
  public int Major { get; }
  public int Minor { get; }
  public int Patch { get; }
  public string Label { get; }
  public int LabelNumber { get; }

  internal GodotVersion(int major, int minor, int patch, string label, int labelNumber, bool isCustomBuild = false) {
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
    if (label != "stable" && !isCustomBuild && labelNumber <= 0) {
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
