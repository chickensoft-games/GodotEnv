// Figured out how to test CliFx console from:
// https://github.com/Tyrrrz/CliFx/blob/master/CliFx.Tests/ConsoleSpecs.cs#L192

namespace Chickensoft.GodotEnv.Tests;

using System;
using Chickensoft.GodotEnv.Common.Utilities;
using CliFx.Infrastructure;
using Moq;
using Shouldly;
using Xunit;

public class LogTest {
  [Fact]
  public void Prints() {
    FakeInMemoryConsole console = new();
    Log log = new(console);

    log.Print("Hello, world!");

    console.ReadOutputString().ShouldBe("Hello, world!\n");
  }

  [Fact]
  public void PrintsInfo() {
    FakeInMemoryConsole console = new();
    Log log = new(console);

    log.Info("Hello, world!");

    console.ReadOutputString().ShouldBe("Hello, world!\n");
  }

  [Fact]
  public void PrintsWarning() {
    FakeInMemoryConsole console = new();
    Log log = new(console);

    log.Warn("Hello, world!");

    console.ReadOutputString().ShouldBe("Hello, world!\n");
  }

  [Fact]
  public void PrintsErr() {
    FakeInMemoryConsole console = new();
    Log log = new(console);

    log.Err("Hello, world!");

    console.ReadOutputString().ShouldBe("Hello, world!\n");
  }

  [Fact]
  public void PrintsSuccess() {
    FakeInMemoryConsole console = new();
    Log log = new(console);

    log.Success("Hello, world!");

    console.ReadOutputString().ShouldBe("Hello, world!\n\n");
  }

  [Fact]
  public void GetsColorNames() {
    Log.GetColorName((int)ConsoleColor.Red).ShouldBe("red");
    Log.GetColorName(-1).ShouldBe("default");
  }

  [Fact]
  public void OutputsCorrectStyleChanges() {
    FakeInMemoryConsole console = new();
    Log log = new(console);

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

    console.ReadOutputString().ShouldBe("A\n\nB\n\nC\nD\n\nE\nF\n\n");

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
        """.ReplaceLineEndings()
    );
  }

  [Fact]
  public void OutputsNull() {
    FakeInMemoryConsole console = new();
    Log log = new(console);

    log.Print(null);
    log.Info(null);
    log.Warn(null);
    log.Err(null);
    log.Success(null);

    console.ReadOutputString().Trim().ShouldBe("");
    log.ToString().Trim().ShouldBe("");
  }

  [Fact]
  public void OutputsObject() {
    FakeInMemoryConsole console = new();
    Log log = new(console);

    log.Print(new { Hello = "world" });

    console.ReadOutputString().ShouldBe("{ Hello = world }\n");
  }

  [Fact]
  public void ReportableEventInvokesCallback() {
    var log = new Mock<ILog>();
    var called = false;
    var @event = new ReportableEvent((log) => called = true);

    @event.Report(log.Object);

    called.ShouldBeTrue();
  }
}
