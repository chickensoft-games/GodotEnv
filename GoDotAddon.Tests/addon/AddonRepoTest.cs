namespace Chickensoft.GoDotAddon.Tests {
  using System;
  using System.Collections.Generic;
  using System.IO.Abstractions;
  using System.Threading.Tasks;
  using global::GoDotAddon;

  using Moq;

  using Xunit;

  public class AddonRepoTest {
    private const string CACHE_PATH = ".addons";
    private const string ADDONS_PATH = "addons";
    private readonly RequiredAddon _addon = new(
      Name: "GoDotAddon",
      Url: "git@github.com:chickensoft-games/GoDotAddon.git",
      Subfolder: "/",
      Checkout: "Main"
    );

    [Fact]
    public async void CacheAddonCachesAddon() {
      var cachedAddonDir = $"{CACHE_PATH}/{_addon.Name}";

      var app = new Mock<IApp>();
      var fs = new Mock<IFileSystem>();
      var directory = new Mock<IDirectory>();

      fs.Setup(fs => fs.Directory).Returns(directory.Object);
      directory.Setup(dir => dir.Exists(cachedAddonDir)).Returns(false);

      var cli = new ShellVerifier();

      var cachePathShell = cli.CreateShell(CACHE_PATH);
      var addonPathShell = cli.CreateShell(cachedAddonDir);

      app.Setup(app => app.FS).Returns(fs.Object);
      app.Setup(app => app.CreateShell(CACHE_PATH))
        .Returns(cachePathShell.Object);
      app.Setup(app => app.CreateShell(cachedAddonDir))
        .Returns(addonPathShell.Object);

      cli.Setup(
        CACHE_PATH,
        new ProcessResult(0),
        RunMode.Run,
        "git", "clone", "--recurse-submodules", _addon.Url, _addon.Name
      );

      cli.Setup(
        cachedAddonDir,
        new ProcessResult(0),
        RunMode.Run,
        "git", "checkout", _addon.Checkout
      );

      cli.Setup(
        cachedAddonDir,
        new ProcessResult(0),
        RunMode.RunUnchecked,
        "git", "pull"
      );

      cli.Setup(
        cachedAddonDir,
        new ProcessResult(0),
        RunMode.RunUnchecked,
        "git", "submodule", "update", "--init", "--recursive"
      );

      var addonRepo = new AddonRepo(app.Object);

      await addonRepo.CacheAddon(_addon, CACHE_PATH);

      directory.VerifyAll();
      cli.VerifyAll();
    }

    private enum RunMode {
      Run,
      RunUnchecked,
    }

    // Test utility class that makes it easier to verify the order of
    // shell commands that were executed.
    private class ShellVerifier {
      private readonly Dictionary<string, Mock<IShell>> _shells = new();
      private readonly MockSequence _sequence = new();

      public ShellVerifier() { }

      /// <summary>
      /// Creates a mock shell for the given working directory.
      /// </summary>
      /// <param name="workingDir">Directory in which the shell commands should
      /// be run from.</param>
      /// <returns>The mocked shell.</returns>
      public Mock<IShell> CreateShell(string workingDir) {
        var shell = new Mock<IShell>(MockBehavior.Strict);
        _shells.Add(workingDir, shell);
        return shell;
      }

      /// <summary>
      /// Adds a mock shell command to be verified later.
      /// </summary>
      /// <param name="workingDir">Directory in which the shell command should
      /// be run. Must have created a mock shell previously for this
      /// directory.</param>
      /// <param name="result">Execution result.</param>
      /// <param name="runMode">How the execution of the process runner is
      /// expected to be called from the system under test.</param>
      /// <param name="exe">Cli executable.</param>
      /// <param name="args">Executable args.</param>
      public void Setup(
        string workingDir,
        ProcessResult result,
        RunMode runMode,
        string exe,
        params string[] args
      ) {
        if (_shells.TryGetValue(workingDir, out var sh)) {
          MockSetup(sh, result, runMode, exe, args);
        }
        else {
          throw new InvalidOperationException("Shell not found");
        }
      }

      /// <summary>
      /// After creating mock shells and setting up verification calls, call
      /// this to verify that all of your mocked calls are actually run in the
      /// expected order by the system under test.
      /// </summary>
      public void VerifyAll() {
        foreach (var (_, value) in _shells) {
          value.VerifyAll();
        }
      }

      private void MockSetup(
        Mock<IShell> shell,
        ProcessResult result,
        RunMode runMode,
        string exe,
        string[] args
      ) {
        if (runMode == RunMode.Run) {
          shell.InSequence(_sequence).Setup(shell => shell.Run(exe, args))
            .Returns(Task.FromResult(result));
        }
        else {
          shell.InSequence(_sequence).Setup(
            shell => shell.RunUnchecked(exe, args)
          ).Returns(Task.FromResult(result));
        }
      }
    }
  }
}