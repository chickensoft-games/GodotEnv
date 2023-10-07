namespace Chickensoft.GodotEnv.Tests.features.godot.commands;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Shouldly;
using CliFx.Infrastructure;

using Common.Models;
using Common.Utilities;
using Features.Godot.Domain;
using Features.Addons.Commands;

public sealed class GodotListCommandTest : IDisposable {

  private readonly Mock<IExecutionContext> _context;
  private readonly Mock<IGodotContext> _godotContext;
  private readonly Mock<IGodotRepository> _godotRepo;
  private readonly FakeInMemoryConsole _console;
  private readonly Log _log;

  public GodotListCommandTest() {
    _context = new Mock<IExecutionContext>();
    _godotContext = new Mock<IGodotContext>();
    _godotRepo = new Mock<IGodotRepository>();
    _console = new FakeInMemoryConsole();
    _log = new Log(_console);

    _godotContext.SetupGet(c => c.GodotRepo).Returns(_godotRepo.Object);
    _context.SetupGet(context => context.Godot).Returns(_godotContext.Object);
    _context.Setup(context => context.CreateLog(_console)).Returns(_log);
  }

  public void Dispose() => _console.Dispose();

  [Fact]
  public async Task Executes() {
    var testVersions = new List<string> { "3.2.3", "4.0.1" };
    _godotRepo.Setup(r => r.GetRemoteVersionsList()).ReturnsAsync(testVersions);

    var listCommand = new GodotListCommand(_context.Object) { ListRemoteAvailable = true };
    await listCommand.ExecuteAsync(_console);

    _godotRepo.Verify(r => r.GetRemoteVersionsList());
    _log.ToString().ShouldBe(
      """
        Retrieving available Godot versions...
        3.2.3
        4.0.1

        """);
  }

  [Fact]
  public async Task FailsGracefullyOnHttpException() {
    _godotRepo.Setup(r => r.GetRemoteVersionsList()).Throws<HttpRequestException>();

    var listCommand = new GodotListCommand(_context.Object) { ListRemoteAvailable = true };
    await listCommand.ExecuteAsync(_console);

    _godotRepo.Verify(r => r.GetRemoteVersionsList());
    _log.ToString().ShouldBe(
      """
        Retrieving available Godot versions...
        Unable to reach remote Godot versions list.

        """);
  }
}
