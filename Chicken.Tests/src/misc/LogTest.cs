// Figured out how to test CliFx console from:
// https://github.com/Tyrrrz/CliFx/blob/master/CliFx.Tests/ConsoleSpecs.cs#L192

namespace Chickensoft.Chicken.Tests;
using CliFx.Infrastructure;
using Shouldly;
using Xunit;


public class LogTest {
  [Fact]
  public void Prints() {
    var console = new FakeInMemoryConsole();
    var log = new Log(console);

    log.Print("Hello, world!");

    console.ReadOutputString().ShouldBe("Hello, world!\n");
  }

  [Fact]
  public void PrintsInfo() {
    var console = new FakeInMemoryConsole();
    var log = new Log(console);

    log.Info("Hello, world!");

    console.ReadOutputString().ShouldBe("Hello, world!\n");
  }

  [Fact]
  public void PrintsWarning() {
    var console = new FakeInMemoryConsole();
    var log = new Log(console);

    log.Warn("Hello, world!");

    console.ReadOutputString().ShouldBe("Hello, world!\n\n");
  }

  [Fact]
  public void PrintsErr() {
    var console = new FakeInMemoryConsole();
    var log = new Log(console);

    log.Err("Hello, world!");

    console.ReadOutputString().ShouldBe("Hello, world!\n\n");
  }

  [Fact]
  public void PrintsSuccess() {
    var console = new FakeInMemoryConsole();
    var log = new Log(console);

    log.Success("Hello, world!");

    console.ReadOutputString().ShouldBe("Hello, world!\n\n");
  }
}
