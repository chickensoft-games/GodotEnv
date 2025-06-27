// Figured out how to test CliFx console from:
// https://github.com/Tyrrrz/CliFx/blob/master/CliFx.Tests/ConsoleSpecs.cs#L192

namespace Chickensoft.GodotEnv.Tests;

using System;
using Common.Models;
using Common.Utilities;
using Moq;
using Shouldly;
using Xunit;

public sealed class LogTest : IDisposable {

  private readonly OutputTestFakeInMemoryConsole _console = new();

  public void Dispose() => _console.Dispose();

  [Fact]
  public void Prints() {
    var systemInfo = new MockSystemInfo(OSType.Linux, CPUArch.X64);
    var config = MockConfig.Get();
    Log log = new(systemInfo, config.Object, _console);

    log.Print("Hello, world!");

    _console.ReadOutputString().ShouldBe("Hello, world!\n");
  }

  [Fact]
  public void PrintsInfo() {
    var systemInfo = new MockSystemInfo(OSType.Linux, CPUArch.X64);
    var config = MockConfig.Get();
    Log log = new(systemInfo, config.Object, _console);

    log.Info("Hello, world!");

    _console.ReadOutputString().ShouldBe("Hello, world!\n");
  }

  [Fact]
  public void PrintsWarning() {
    var systemInfo = new MockSystemInfo(OSType.Linux, CPUArch.X64);
    var config = MockConfig.Get();
    Log log = new(systemInfo, config.Object, _console);

    log.Warn("Hello, world!");

    _console.ReadOutputString().ShouldBe("Hello, world!\n");
  }

  [Fact]
  public void PrintsErr() {
    var systemInfo = new MockSystemInfo(OSType.Linux, CPUArch.X64);
    var config = MockConfig.Get();
    Log log = new(systemInfo, config.Object, _console);

    log.Err("Hello, world!");

    _console.ReadOutputString().ShouldBe("Hello, world!\n");
  }

  [Fact]
  public void PrintsSuccess() {
    var systemInfo = new MockSystemInfo(OSType.Linux, CPUArch.X64);
    var config = MockConfig.Get();
    Log log = new(systemInfo, config.Object, _console);

    log.Success("Hello, world!");

    _console.ReadOutputString().ShouldBe("Hello, world!\n");
  }

  [Fact]
  public void GetsColorNames() {
    Log.GetColorName((int)ConsoleColor.Red).ShouldBe("red");
    Log.GetColorName(-1).ShouldBe("default");
  }

  [Fact]
  public void OutputStringWithEmojisOnUnix() {
    var systemInfo = new MockSystemInfo(OSType.Linux, CPUArch.X64);
    var config = MockConfig.Get();
    Log log = new(systemInfo, config.Object, _console);

    log.Print("✅ Installed successfully.");

    var debugLog = log.ToString();

    debugLog.ShouldBe(
      """
      ✅ Installed successfully.

      """, StringCompareShould.IgnoreLineEndings);
  }

  [Fact]
  public void OutputStringWithEmojisOnWindows() {
    var systemInfo = new MockSystemInfo(OSType.Windows, CPUArch.X64);
    var config = MockConfig.Get();
    Log log = new(systemInfo, config.Object, _console);

    log.Print("✅ Installed successfully.");

    var debugLog = log.ToString();

    // NOTE: Should remove emoji from string.
    debugLog.ShouldBe(
      """
      Installed successfully.

      """, StringCompareShould.IgnoreLineEndings);
  }

  [Fact]
  public void OutputsCorrectStyleChanges() {
    var systemInfo = new MockSystemInfo(OSType.Linux, CPUArch.X64);
    var config = MockConfig.Get();
    Log log = new(systemInfo, config.Object, _console);

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

    _console.ReadOutputString().ShouldBe("A\n\nB\n\nC\nD\n\nE\nF\n");

    var debugLog = log.ToString();

    debugLog.ShouldBe(
      """
        A

        [style fg="cyan"]B

        [/style][style fg="yellow"]C
        [/style][style bg="green"]D

        [/style][style fg="red"]E
        [/style][style fg="green"]F
        [/style]
        """, StringCompareShould.IgnoreLineEndings);
  }

  [Fact]
  public void OutputsNull() {
    var systemInfo = new MockSystemInfo(OSType.Linux, CPUArch.X64);
    var config = MockConfig.Get();
    Log log = new(systemInfo, config.Object, _console);

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
    var systemInfo = new MockSystemInfo(OSType.Linux, CPUArch.X64);
    var config = MockConfig.Get();
    Log log = new(systemInfo, config.Object, _console);

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
