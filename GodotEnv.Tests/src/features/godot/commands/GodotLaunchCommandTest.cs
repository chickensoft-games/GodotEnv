namespace Chickensoft.GodotEnv.Tests.features.godot.commands;

using System;
using System.IO;
using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Features.Godot.Commands;
using Chickensoft.GodotEnv.Features.Godot.Domain;
using Chickensoft.GodotEnv.Features.Godot.Models;
using CliFx.Infrastructure;
using Common.Models;
using Common.Utilities;
using global::GodotEnv.Common.Utilities;
using Moq;
using Shouldly;
using Xunit;

public sealed class GodotLaunchCommandTest : IDisposable {
  private readonly MockSystemInfo _systemInfo;
  private readonly Mock<IExecutionContext> _context;
  private readonly Mock<IGodotContext> _godotContext;
  private readonly Mock<IGodotEnvironment> _environment;
  private readonly Mock<IGodotRepository> _godotRepo;
  private readonly Mock<IProcessRunner> _processRunner;
  private readonly FakeInMemoryConsole _console;
  private readonly Mock<IFileClient> _fileClient;
  private readonly Log _log;

  public GodotLaunchCommandTest() {
    _systemInfo = new MockSystemInfo(OSType.Linux, CPUArch.X64);
    _context = new Mock<IExecutionContext>();
    _godotContext = new Mock<IGodotContext>();
    _environment = new Mock<IGodotEnvironment>();
    _godotRepo = new Mock<IGodotRepository>();
    _processRunner = new Mock<IProcessRunner>();
    _console = new FakeInMemoryConsole();
    _fileClient = new Mock<IFileClient>();


    _environment.Setup(env => env.SystemInfo).Returns(_systemInfo);
    _godotContext.SetupGet(c => c.GodotRepo).Returns(_godotRepo.Object);
    _godotContext.Setup(c => c.Platform).Returns(_environment.Object);
    _context.SetupGet(context => context.Godot).Returns(_godotContext.Object);
    _log = new Log(_systemInfo, _console);
    _context.Setup(context => context.CreateLog(_console)).Returns(_log);

    _godotRepo.SetupGet(r => r.ProcessRunner).Returns(_processRunner.Object);
  }

  public void Dispose() {
    _console.Dispose();
    Environment.SetEnvironmentVariable("GODOT", null);
  }

  [Fact]
  public async Task Launches_Godot_When_Env_Variable_Is_Valid() {
    var godotPath = Path.Combine(Path.GetTempPath(), "godot-test-launcher");

    // Create a fake Godot binary
    File.WriteAllText(godotPath, string.Empty);
    Environment.SetEnvironmentVariable("GODOT", godotPath);

    var launchCommand = new GodotLaunchCommand(_context.Object);

    await launchCommand.ExecuteAsync(_console);

    _processRunner.Verify(p =>
      p.RunDetached(godotPath, Array.Empty<string>()), Times.Once
    );

    _log.ToString().ShouldContain($"Launching Godot from {godotPath}");

    // Clean up
    File.Delete(godotPath);
  }

  [Fact]
  public async Task Fails_When_GODOT_Variable_Is_Unset() {
    Environment.SetEnvironmentVariable("GODOT", null);

    var launchCommand = new GodotLaunchCommand(_context.Object);
    await launchCommand.ExecuteAsync(_console);

    _processRunner.Verify(p =>
      p.RunDetached(It.IsAny<string>(), It.IsAny<string[]>()), Times.Never
    );

    _log.ToString().ShouldContain("The GODOT environment variable is not set");
  }

  [Fact]
  public async Task Fails_When_GODOT_Target_Does_Not_Exist() {
    var godotPath = "/nonexistent/godot";
    Environment.SetEnvironmentVariable("GODOT", godotPath);

    var launchCommand = new GodotLaunchCommand(_context.Object);
    await launchCommand.ExecuteAsync(_console);

    _processRunner.Verify(p =>
      p.RunDetached(It.IsAny<string>(), It.IsAny<string[]>()), Times.Never
    );

    _log.ToString().ShouldContain($"The GODOT environment variable points to a missing file: {godotPath}");
  }
}

