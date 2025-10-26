namespace Chickensoft.GodotEnv.Tests;

using System;
using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Utilities;
using Shouldly;

public class ProcessRunnerTest
{
  [PlatformFact(TestPlatform.Windows)]
  public async Task RunsProcessOnWindows()
  {
    var runner = new ProcessRunner();
    var result = await runner.Run(
      Environment.CurrentDirectory, "cmd", ["/c echo \"hello\""]
    );
    result.ExitCode.ShouldBe(0);
    result.Succeeded.ShouldBe(true);
    result.StandardOutput.ShouldBe($"""\"hello\""{Environment.NewLine}""");
    result.StandardError.ShouldBe("");
  }

  [PlatformFact(TestPlatform.MacLinux)]
  public async Task RunsProcessOnMacLinux()
  {
    var runner = new ProcessRunner();
    var result = await runner.Run(
      Environment.CurrentDirectory, "echo", ["hello"]
    );
    result.ExitCode.ShouldBe(0);
    result.Succeeded.ShouldBe(true);
    result.StandardOutput.ShouldBe("hello\n");
    result.StandardError.ShouldBe("");
  }
}
