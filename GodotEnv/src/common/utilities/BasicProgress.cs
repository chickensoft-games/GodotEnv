namespace Chickensoft.GodotEnv.Common.Utilities;

using System;

/// <summary>
/// Basic progress implementation.
/// Credit: https://stackoverflow.com/a/42436311
/// </summary>
/// <typeparam name="T">Type of parameter received by progress callback.
/// </typeparam>
public class BasicProgress<T> : IProgress<T> {
  private readonly Action<T> _handler;

  public BasicProgress(Action<T> handler) {
    _handler = handler;
  }

  void IProgress<T>.Report(T value) => _handler(value);
}
