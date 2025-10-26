namespace Chickensoft.GodotEnv.Common.Utilities;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Chickensoft.GodotEnv.Common.Models;
using CliFx.Infrastructure;

/// <summary>CLI log interface.</summary>
public interface ILog
{
  ISystemInfo SystemInfo { get; }
  /// <summary>Application configuration.</summary>
  IConfig Config { get; }
  /// <summary>CLI command console.</summary>
  IConsole Console { get; }
  /// <summary>Print an error message to the console.</summary>
  /// <param name="message">Error message.</param>
  void Err(object? message);
  /// <summary>Print an error message to the console in place.</summary>
  /// <param name="message">Error message.</param>
  void ErrInPlace(object? message);
  /// <summary>Print a message to the console.</summary>
  /// <param name="message">Message.</param>
  void Print(object? message);
  /// <summary>Print a message to the console in place.</summary>
  /// <param name="message">Message.</param>
  void PrintInPlace(object? message);
  /// <summary>Print an information message to the console.</summary>
  /// <param name="message">Informational message.</param>
  void Info(object? message);
  /// <summary>Print an information message to the console in place.</summary>
  /// <param name="message">Informational message.</param>
  void InfoInPlace(object? message);
  /// <summary>Print a success message to the console.</summary>
  /// <param name="message">Success message.</param>
  void Success(object? message);
  /// <summary>Print a success message to the console in place.</summary>
  /// <param name="message">Success message.</param>
  void SuccessInPlace(object? message);
  /// <summary>Print a warning message to the console.</summary>
  /// <param name="message">Warning message.</param>
  void Warn(object? message);
  /// <summary>Print a warning message to the console in place.</summary>
  /// <param name="message">Warning message.</param>
  void WarnInPlace(object? message);
  /// <summary>
  /// Clears the last written line of the console.
  /// </summary>
  void ClearCurrentLine();
}

/// <summary>
/// Interface of event objects which can output themselves to a log.
/// </summary>
public interface IReportableEvent
{
  /// <summary>Report the event to a log.</summary>
  /// <param name="log">Log to output to.</param>
  void Report(ILog log);
}

/// <summary>Custom reportable event.</summary>
public record ReportableEvent : IReportableEvent
{
  /// <summary>Callback that receives a log for outputting messages.</summary>
  public Action<ILog> Action { get; }

  public ReportableEvent(Action<ILog> action)
  {
    Action = action;
  }

  public void Report(ILog log) => Action(log);
}

public partial class Log : ILog
{
  private record Style(
    ConsoleColor Foreground, ConsoleColor Background
  );

  public ISystemInfo SystemInfo { get; }

  public IConfig Config { get; }

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

  public Log(ISystemInfo systemInfo, IConfig config, IConsole console)
  {
    SystemInfo = systemInfo;
    Config = config;
    Console = console;

    if (!IsInRedirectedEnv)
    {
      console.ResetColor();
    }

    _defaultFgColor = console.ForegroundColor;
    _defaultBgColor = console.BackgroundColor;
    _defaultStyle = new Style(_defaultFgColor, _defaultBgColor);
    _styles.Push(_defaultStyle);
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
    message, Styles.Success, false, false
  );
  public void SuccessInPlace(object? message) => Output(
    message, Styles.Success, true, false
  );

  public void ClearCurrentLine()
  {
    if (IsInRedirectedEnv)
    { return; }
    lock (Console)
    {
      Console.CursorLeft = 0;
      var top = Console.CursorTop;
      OutputConsole.Write(new string(' ', Console.WindowWidth - 1));
      Console.CursorLeft = 0;
      Console.CursorTop = top;
    }
  }

  public void Output(
    object? message, Action<IConsole> consoleStyle, bool inPlace = false, bool addExtraLine = true
  )
  {
    if (inPlace && IsInRedirectedEnv)
    {
      // Don't print in-place messages in a redirected environment, like
      // GitHub actions.
      return;
    }
    lock (Console)
    {
      // Set the new foreground and background colors.
      consoleStyle(Console);

      if (message is string str && str != "")
      {
        if (SystemInfo.OS == OSType.Windows
          || !Config.ConfigValues.Terminal.DisplayEmoji
        )
        {
          // Remove emoji from message.
          str = RemoveNonANSICharacters(str);
          message = str;
        }
        UpdateStyle();
      }
      else if (message is not string and not null)
      {
        UpdateStyle();
      }

      var left = 0;
      var top = 0;

      if (!IsInRedirectedEnv)
      {
        left = Console.CursorLeft;
        top = Console.CursorTop;
      }

      if (inPlace)
      {
        OutputConsole.Write(message);
      }
      else
      {
        OutputConsole.WriteLine(message);
      }
      _sb.AppendLine(message?.ToString());

      if (addExtraLine)
      {
        OutputConsole.WriteLine();
        _sb.AppendLine();
      }

      Styles.Normal(Console);

      if (inPlace && !IsInRedirectedEnv)
      {
        Console.CursorLeft = left;
        Console.CursorTop = top;
      }
      OutputConsole.Flush();
    }
  }

  /// <summary>
  /// Removes non-ASCII chars from string. If matches, tries to remove 1 whitespace at the end.
  /// </summary>
  /// <remarks>
  /// About Windows cmd encoding see: https://stackoverflow.com/a/75788701/8903027
  /// </remarks>
  /// <param name="str"></param>
  /// <returns>Processed string.</returns>
  private static string RemoveNonANSICharacters(string str) => ANSIOnlyRegex().Replace(str, "");

  [GeneratedRegex(@"[^\x00-\x7F][ ]?")]
  private static partial Regex ANSIOnlyRegex();

  public void UpdateStyle()
  {
    var style = new Style(Console.ForegroundColor, Console.BackgroundColor);
    var currentStyle = _styles.Peek();
    if (style != currentStyle)
    {
      while (_styles.Count > 1)
      {
        PopStyle();
      }
      PushStyle(style);
    }
  }

  private void PushStyle(Style consoleStyle)
  {
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

  private void PopStyle()
  {
    if (_styles.Count > 1)
    {
      _sb.Append("[/style]");
      _styles.Pop();
    }
  }

  public static class Styles
  {
    internal static Action<IConsole> Normal
      => static (console) => console.ResetColor();
    internal static Action<IConsole> Info
      => static (console) =>
      {
        console.ResetColor();
        console.ForegroundColor = ConsoleColor.Cyan;
      };
    internal static Action<IConsole> Error
      => static (console) =>
      {
        console.ResetColor();
        console.ForegroundColor = ConsoleColor.Red;
      };
    internal static Action<IConsole> Warning
      => static (console) =>
      {
        console.ResetColor();
        console.ForegroundColor = ConsoleColor.Yellow;
      };
    internal static Action<IConsole> Success
      => static (console) =>
      {
        console.ResetColor();
        console.ForegroundColor = ConsoleColor.Green;
      };
  }

  public override string ToString()
  {
    while (_styles.Count > 1)
    {
      PopStyle();
    }
    return _sb.ToString();
  }

  public static string GetColorName(int color) => color == -1
    ? "default"
    : ((ConsoleColor)color)
    .ToString().ToLower(CultureInfo.CurrentCulture);
}
