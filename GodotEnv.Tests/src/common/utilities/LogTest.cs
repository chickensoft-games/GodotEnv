// Figured out how to test CliFx console from:
// https://github.com/Tyrrrz/CliFx/blob/master/CliFx.Tests/ConsoleSpecs.cs#L192

namespace Chickensoft.GodotEnv.Tests;

using System;
using Common.Utilities;
using Moq;
using Shouldly;
using Xunit;

public sealed class LogTest : IDisposable {

  private readonly OutputTestFakeInMemoryConsole _console = new();

  public void Dispose() => _console.Dispose();

  [Fact]
  public void Prints() {
    Log log = new(_console) { TestEnvironment = true };

    log.Print("Hello, world!");

    _console.ReadOutputString().ShouldBe("Hello, world!\n");
  }

  [Fact]
  public void PrintsInfo() {
    Log log = new(_console) { TestEnvironment = true };

    log.Info("Hello, world!");

    _console.ReadOutputString().ShouldBe("Hello, world!\n");
  }

  [Fact]
  public void PrintsWarning() {
    Log log = new(_console) { TestEnvironment = true };

    log.Warn("Hello, world!");

    _console.ReadOutputString().ShouldBe("Hello, world!\n");
  }

  [Fact]
  public void PrintsErr() {
    Log log = new(_console) { TestEnvironment = true };

    log.Err("Hello, world!");

    _console.ReadOutputString().ShouldBe("Hello, world!\n");
  }

  [Fact]
  public void PrintsSuccess() {
    Log log = new(_console) { TestEnvironment = true };

    log.Success("Hello, world!");

    _console.ReadOutputString().ShouldBe("Hello, world!\n\n");
  }

  [Fact]
  public void GetsColorNames() {
    Log.GetColorName((int)ConsoleColor.Red).ShouldBe("red");
    Log.GetColorName(-1).ShouldBe("default");
  }

  [Fact]
  public void OutputsCorrectStyleChanges() {
    Log log = new(_console) { TestEnvironment = true };

    log.Print("A");
    log.Print("");
    log.Info("B");
    log.Print("");
    log.Warn("C");
    log.Output(
      "D", (console) => {
        console.ResetColor();
        console.BackgroundColor = ConsoleColor.Green;
      }
    );
    log.Err("E");
    log.Success("F");

    _console.ReadOutputString().ShouldBe("A\n\nB\n\nC\nD\n\nE\nF\n\n");

    var debugLog = log.ToString();

    debugLog.ShouldBe(
      """
        A

        [style fg="darkblue"]B

        [/style][style fg="yellow"]C
        [/style][style bg="green"]D

        [/style][style fg="red"]E
        [/style][style fg="green"]F

        [/style]
        """, StringCompareShould.IgnoreLineEndings);
  }

  [Fact]
  public void OutputsNull() {
    Log log = new(_console) { TestEnvironment = true };

    log.Print(null);
    log.Info(null);
    log.Warn(null);
    log.Err(null);
    log.Success(null);

    _console.ReadOutputString().Trim().ShouldBe("");
    log.ToString().Trim().ShouldBe("");
  }

  [Fact]
  public void OutputsObject() {
    Log log = new(_console) { TestEnvironment = true };

    log.Print(new { Hello = "world" });

    _console.ReadOutputString().ShouldBe("{ Hello = world }\n");
  }

  [Fact]
  public void ReportableEventInvokesCallback() {
    var log = new Mock<ILog>();
    var called = false;
    var @event = new ReportableEvent((_) => called = true);

    @event.Report(log.Object);

    called.ShouldBeTrue();
  }
}
