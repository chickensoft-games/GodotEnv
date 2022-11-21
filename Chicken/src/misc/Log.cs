namespace Chickensoft.Chicken;
using System;
using CliFx.Infrastructure;

public interface ILog {
  IConsole Console { get; init; }

  void Err(object? message);
  void Print(object? message);
  void Info(object? message);
  void Success(object? message);
  void Warn(object? message);
}

public interface IReportableEvent {
  void Log(ILog log);
}

public class Log : ILog {
  public static class Status {
    public static Action<IConsole> Normal
      => (console) => console.ResetColor();
    public static Action<IConsole> Info
      => (console) => console.ForegroundColor = ConsoleColor.DarkBlue;
    public static Action<IConsole> Error
      => (console) => console.ForegroundColor = ConsoleColor.Red;
    public static Action<IConsole> Warning
      => (console) => console.ForegroundColor = ConsoleColor.Yellow;
    public static Action<IConsole> Success
      => (console) => console.ForegroundColor = ConsoleColor.Green;
  }

  public IConsole Console { get; init; }
  private ConsoleWriter _output => Console.Output;

  public Log(IConsole console) => Console = console;

  public void Print(object? message) => Output(message, Status.Normal, false);
  public void Info(object? message) => Output(message, Status.Info, false);
  public void Warn(object? message) => Output(message, Status.Warning);
  public void Err(object? message) => Output(message, Status.Error);
  public void Success(object? message) => Output(message, Status.Success);

  protected void Output(
    object? message, Action<IConsole> color, bool spacing = true
  ) {
    color(Console);
    _output.WriteLine(message);
    if (spacing) { _output.WriteLine(""); }
    Console.ResetColor();
  }
}
