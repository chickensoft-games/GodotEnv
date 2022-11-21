namespace Chickensoft.Chicken.Tests;

using CliFx.Exceptions;
using Shouldly;
using Xunit;

public class AdditionalArgParserTest {
  private readonly string[] _args
    = new string[] {
      "--a", "1", "--b", "--c", "value_c", "--d", "true", "--e", "false"
    };

  [Fact]
  public void InitializesPeeksAndAdvances() {
    var parser = new AdditionalArgParser(new string[] { "--a", "--b" });
    parser.Ok.ShouldBeTrue();
    parser.Peek.ShouldBe("--a");
    parser.Advance().ShouldBe("--a");
    parser.Peek.ShouldBe("--b");
    parser.Advance().ShouldBe("--b");
    parser.Peek.ShouldBeNull();
  }

  [Fact]
  public void ParsesArgs() {
    var parser = new AdditionalArgParser(_args);
    var result = parser.Parse();
    ((double?)result["a"])!.ShouldBe(1);
    ((bool?)result["b"])!.ShouldBe(true);
    ((string?)result["c"])!.ShouldBe("value_c");
    ((bool?)result["d"])!.ShouldBe(true);
    ((bool?)result["e"])!.ShouldBe(false);
  }

  [Fact]
  public void ThrowsOnValueWithNoNameAtStart() {
    var parser = new AdditionalArgParser(new string[] { "value" });
    Should.Throw<CommandException>(() => parser.Parse());
  }

  [Fact]
  public void ThrowsOnValueWithNoName() {
    var parser = new AdditionalArgParser(new string[] { "--a", "a", "value" });
    Should.Throw<CommandException>(() => parser.Parse());
  }

  [Fact]
  public void ReadsBooleanAtEnd() {
    var parser = new AdditionalArgParser(new string[] { "--a", "a", "--b" });
    var result = parser.Parse();
    ((bool?)result["b"])!.ShouldBe(true);
  }

  [Fact]
  public void IsFlagDeterminesFlags() {
    AdditionalArgParser.IsFlag("--a").ShouldBeTrue();
    AdditionalArgParser.IsFlag("-a").ShouldBeFalse();
    AdditionalArgParser.IsFlag("/a").ShouldBeFalse();
    AdditionalArgParser.IsFlag("a").ShouldBeFalse();
  }
}
