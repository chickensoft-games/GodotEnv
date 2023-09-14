namespace Chickensoft.GodotEnv.Common.Utilities;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using CliFx.Infrastructure;

/// <summary>CLI log interface.</summary>
public interface ILog {
  /// <summary>CLI command console.</summary>
  IConsole Console { get; }
  /// <summary>Print an error message to the console.</summary>
  /// <param name="message">Error message.</param>
  /// <param name="inPlace">True to print without moving the cursor position.
  /// </param>
  void Err(object? message);
  /// <summary>Print an error message to the console in place.</summary>
  /// <param name="message">Error message.</param>
  /// <param name="inPlace">True to print without moving the cursor position.
  /// </param>
  void ErrInPlace(object? message);
  /// <summary>Print a message to the console.</summary>
  /// <param name="message">Message.</param>
  /// <param name="inPlace">True to print without moving the cursor position.
  /// </param>
  void Print(object? message);
  /// <summary>Print a message to the console in place.</summary>
  /// <param name="message">Message.</param>
  /// <param name="inPlace">True to print without moving the cursor position.
  /// </param>
  void PrintInPlace(object? message);
  /// <summary>Print an information message to the console.</summary>
  /// <param name="message">Informational message.</param>
  /// <param name="inPlace">True to print without moving the cursor position.
  /// </param>
  void Info(object? message);
  /// <summary>Print an information message to the console in place.</summary>
  /// <param name="message">Informational message.</param>
  /// <param name="inPlace">True to print without moving the cursor position.
  /// </param>
  void InfoInPlace(object? message);
  /// <summary>Print a success message to the console.</summary>
  /// <param name="message">Success message.</param>
  /// <param name="inPlace">True to print without moving the cursor position.
  /// </param>
  void Success(object? message);
  /// <summary>Print a success message to the console in place.</summary>
  /// <param name="message">Success message.</param>
  /// <param name="inPlace">True to print without moving the cursor position.
  /// </param>
  void SuccessInPlace(object? message);
  /// <summary>Print a warning message to the console.</summary>
  /// <param name="message">Warning message.</param>
  /// <param name="inPlace">True to print without moving the cursor position.
  /// </param>
  void Warn(object? message);
  /// <summary>Print a warning message to the console in place.</summary>
  /// <param name="message">Warning message.</param>
  /// <param name="inPlace">True to print without moving the cursor position.
  /// </param>
  void WarnInPlace(object? message);
  /// <summary>
  /// Clears the last written line of the console.
  /// </summary>
  void ClearLastLine();
}

/// <summary>
/// Interface of event objects which can output themselves to a log.
/// </summary>
public interface IReportableEvent {
  /// <summary>Report the event to a log.</summary>
  /// <param name="log">Log to output to.</param>
  void Report(ILog log);
}

/// <summary>Custom reportable event.</summary>
public record ReportableEvent : IReportableEvent {
  /// <summary>Callback that receives a log for outputting messages.</summary>
  public Action<ILog> Action { get; }

  public ReportableEvent(Action<ILog> action) {
    Action = action;
  }

  public void Report(ILog log) => Action(log);
}

public class Log : ILog {
  private record Style(
    ConsoleColor Foreground, ConsoleColor Background
  );

  public IConsole Console { get; }

  private ConsoleWriter OutputConsole => Console.Output;
  private readonly StringBuilder _sb = new();
  private readonly ConsoleColor _defaultFgColor;
  private readonly ConsoleColor _defaultBgColor;
  private readonly Style _defaultStyle;
  private readonly Stack<Style> _styles = new();

  // True if either stdout or stderr is redirected, which likely means we are
  // running an environment without an actual console. Redirected environments
  // cause errors when manipulating the cursor on Windows.
  public bool IsInRedirectedEnv =>
    Console.IsOutputRedirected || Console.IsErrorRedirected;

  public Log(IConsole console) {
    console.ResetColor();
    _defaultFgColor = console.ForegroundColor;
    _defaultBgColor = console.BackgroundColor;
    _defaultStyle = new Style(_defaultFgColor, _defaultBgColor);
    _styles.Push(_defaultStyle);
    Console = console;
  }

  public void Print(object? message) => Output(
    message, Styles.Normal, false, false
  );
  public void PrintInPlace(object? message) => Output(
    message, Styles.Normal, true, false
  );
  public void Info(object? message) => Output(
    message, Styles.Info, false, false
  );
  public void InfoInPlace(object? message) => Output(
    message, Styles.Info, true, false
  );
  public void Warn(object? message) => Output(
    message, Styles.Warning, false, false
  );
  public void WarnInPlace(object? message) => Output(
    message, Styles.Warning, true, false
  );
  public void Err(object? message) => Output(
    message, Styles.Error, false, false
  );
  public void ErrInPlace(object? message) => Output(
    message, Styles.Error, true, false
  );
  public void Success(object? message) => Output(
    message, Styles.Success, false
  );
  public void SuccessInPlace(object? message) => Output(
    message, Styles.Success, true
  );

  public void ClearLastLine() {
    if (IsInRedirectedEnv) { return; }
    lock (Console) {
      Console.CursorLeft = 0;
      var top = Console.CursorTop;
      OutputConsole.Write(new string(' ', Console.WindowWidth - 1));
      Console.CursorLeft = 0;
      Console.CursorTop = top;
    }
  }

  public void Output(
    object? message, Action<IConsole> consoleStyle, bool inPlace = false, bool addExtraLine = true
  ) {
    if (inPlace && IsInRedirectedEnv) {
      // Don't print in-place messages in a redirected environment, like
      // GitHub actions.
      return;
    }
    lock (Console) {
      // Set the new foreground and background colors.
      consoleStyle(Console);

      if (
        (message is string str && str != "") ||
        message is not string and not null
      ) {
        UpdateStyle();
      }

      var left = 0;
      var top = 0;

      if (!IsInRedirectedEnv) {
        left = Console.CursorLeft;
        top = Console.CursorTop;
      }

      if (inPlace) {
        OutputConsole.Write(message);
      }
      else {
        OutputConsole.WriteLine(message);
      }
      _sb.AppendLine(message?.ToString());

      if (addExtraLine) {
        OutputConsole.WriteLine();
        _sb.AppendLine();
      }

      if (inPlace && !IsInRedirectedEnv) {
        Console.CursorLeft = left;
        Console.CursorTop = top;
      }
    }
  }

  public void UpdateStyle() {
    var style = new Style(Console.ForegroundColor, Console.BackgroundColor);
    var currentStyle = _styles.Peek();
    if (style != currentStyle) {
      while (_styles.Count > 1) {
        PopStyle();
      }
      PushStyle(style);
    }
  }

  private void PushStyle(Style consoleStyle) {
    var currentStyle = _styles.Peek();
    var fg = Console.ForegroundColor;
    var bg = Console.BackgroundColor;
    _sb.Append(
      "[style").Append(
          fg != currentStyle.Foreground
            ? $" fg=\"{GetColorName((int)fg)}\""
            : ""
        ).Append(
          bg != currentStyle.Background
            ? $" bg=\"{GetColorName((int)bg)}\""
            : ""
      ).Append(']'
);
    _styles.Push(consoleStyle);
  }

  private void PopStyle() {
    if (_styles.Count > 1) {
      _sb.Append("[/style]");
      _styles.Pop();
    }
  }

  public static class Styles {
    internal static Action<IConsole> Normal
      => static (console) => console.ResetColor();
    internal static Action<IConsole> Info
      => static (console) => {
        console.ResetColor();
        console.ForegroundColor = ConsoleColor.DarkBlue;
      };
    internal static Action<IConsole> Error
      => static (console) => {
        console.ResetColor();
        console.ForegroundColor = ConsoleColor.Red;
      };
    internal static Action<IConsole> Warning
      => static (console) => {
        console.ResetColor();
        console.ForegroundColor = ConsoleColor.Yellow;
      };
    internal static Action<IConsole> Success
      => static (console) => {
        console.ResetColor();
        console.ForegroundColor = ConsoleColor.Green;
      };
  }

  public override string ToString() {
    while (_styles.Count > 1) {
      PopStyle();
    }
    return _sb.ToString();
  }

  public static string GetColorName(int color) {
    if (color == -1) { return "default"; }
    return ((ConsoleColor)color)
      .ToString().ToLower(CultureInfo.CurrentCulture);
  }
}
