namespace Chickensoft.GodotEnv.Common.Utilities;

using System;
using System.Diagnostics.CodeAnalysis;

public sealed class Result<T> {
  [MemberNotNullWhen(true, nameof(Value))]
  public bool IsSuccess { get; }
  public T? Value { get; }
  public string Error { get; }

  internal Result(bool isSuccess, T? value, string error) {
    if (isSuccess && value is null) {
      throw new ArgumentException("Value must not be null if result is successful");
    }
    IsSuccess = isSuccess;
    Value = value;
    Error = error;
  }
}

// Avoid CA1000 by placing factory methods in a sibling class
public static class Result {
  public static Result<T> Success<T>(T value) =>
    new(true, value, string.Empty);

  public static Result<T> Failure<T>(T? value, string error) =>
    new(false, value, error);
}
