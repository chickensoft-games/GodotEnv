namespace Chickensoft.Chicken;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;

[Command(
  "egg crack",
  Description = "Create a new project based on an existing egg (template).")
]
public class CreateCommand : ICommand {
  [CommandOption(
    "egg",
    'e',
    Description =
    "Template to use. Can be a git url, a local path, or a template name."
  )]
  public string? Egg { get; init; }

  [CommandOption(
    "checkout",
    'c',
    Description =
    "The branch to checkout. Use `tags/{name}` for specific tags/versions."
  )]
  public string? Checkout { get; init; }

  [CommandParameter(
    0, Description = "Name of the output folder for the generated project."
  )]
  public string? Name { get; init; }

  public readonly IApp AppContext;
  public readonly IFileSystem Fs;
  public readonly IFileCopier Copier;
  public readonly IAdditionalArgParser ArgParser;

  public CreateCommand() {
    AppContext = new App();
    Fs = new FileSystem();
    Copier = new FileCopier();
    ArgParser = new AdditionalArgParser(Chickensoft.Chicken.App.CommandArgs);
  }

  public CreateCommand(
    IApp app,
    IFileSystem fs,
    IFileCopier copier,
    IAdditionalArgParser argParser
  ) {
    AppContext = app;
    Fs = fs;
    Copier = copier;
    ArgParser = argParser;
  }

  public async ValueTask ExecuteAsync(IConsole console) {
    var template = Egg;
    var name = Name;
    var checkout = Checkout;

    if (template == null) {
      throw new CommandException("Egg (-e) is required.");
    }

    if (name == null) {
      throw new CommandException("Output name is required.");
    }

    var output = console.Output;
    var startDir = AppContext.WorkingDir;
    ISourceRepository repo
      = new SourceRepository(url: template, checkout: Checkout);
    var projectPath = Path.Combine(startDir, name);
    var log = AppContext.CreateLog(console);

    if (Fs.Directory.Exists(projectPath)) {
      throw new CommandException(
        $"Directory {projectPath} already exists. " +
        "Please choose a different project name."
      );
    }

    var isLocal = repo.IsLocal;
    var sourceUrl = repo.SourcePath(AppContext);

    // Determine if the source (remote or local) is a git repository.
    var isGitRepo = !isLocal;
    if (isLocal) {
      if (!Fs.Directory.Exists(sourceUrl)) {
        throw new CommandException(
          $"Cannot find template `{template}` at `{sourceUrl}`."
        );
      }
      isGitRepo = Fs.Directory.Exists(Path.Combine(sourceUrl, ".git"));
    }

    // Copy files from the source to the destination.
    if (isGitRepo) {
      var shell = AppContext.CreateShell(startDir);
      // use git to clone local or remote repository
      await shell.Run(
        "git", "clone", sourceUrl, "--recurse-submodules", projectPath
      );
      // checkout the specified branch/tag reference in the cloned repo
      if (checkout != null) {
        var projectShell = AppContext.CreateShell(projectPath);
        await projectShell.Run("git", "checkout", checkout);
      }
      // Remove any .git directories from the cloned repo
      // TODO: Reinstate this when tested IRL
      Copier.RemoveDirectories(Fs, projectPath, ".git");
      log.Info($"Cloned {Name} from {sourceUrl}.");
    }
    else {
      // copy files from local directory, excluding .git directories
      var filesCopied =
        Copier.CopyDotNet(
          fs: Fs,
          source: sourceUrl,
          destination: projectPath,
          exclusions: App.DEFAULT_EXCLUSIONS
        );
      foreach (var path in filesCopied) {
        log.Info($"Copied: {path}");
      }
      log.Print("");
    }

    // Perform edit actions on copied files.
    var inputs = ArgParser.Parse();
    var loader = AppContext.CreateEditActionsLoader(Fs);
    var editActions = loader.Load(projectPath);
    var editActionsRepo = AppContext.CreateEditActionsRepo(
      fs: Fs,
      repoPath: projectPath,
      editActions: editActions,
      inputs: inputs
    );

    var generator = AppContext.CreateTemplateGenerator(
      projectName: name,
      projectPath: projectPath,
      templateDescription: template,
      editActionsRepo: editActionsRepo,
      editActions: editActions,
      log: log
    );

    generator.Generate();
  }
}
