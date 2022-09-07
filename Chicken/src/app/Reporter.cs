namespace Chickensoft.Chicken {
  using System;
  using CliFx.Infrastructure;

  public interface IReporter {
    void Handle(IReportableEvent reportEvent);
  }

  public interface IReportableEvent {
    ConsoleColor Color { get; }
  }

  public class Reporter : IReporter {
    public IConsole Console { get; init; }
    private ConsoleWriter _output => Console.Output;

    public Reporter(IConsole console) => Console = console;

    public void Handle(IReportableEvent reportEvent)
      => Print(reportEvent.ToString(), reportEvent.Color);

    private void Print(string? message, ConsoleColor color) {
      Console.ForegroundColor = color;
      _output.WriteLine(message);
      _output.WriteLine("");
      Console.ResetColor();
    }
  }
}
