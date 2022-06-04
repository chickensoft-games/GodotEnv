namespace Chickensoft.GoDotAddon.Tests {
  using System.Collections.Generic;
  using System.IO.Abstractions;
  using System.Threading.Tasks;
  using Chickensoft.GoDotAddon;
  using Moq;
  using Shouldly;
  using Xunit;


  public class CacheRepoTest {

    [Fact]
    public async Task LoadsCacheCorrectlyWhenAddonIsCached() {
      var app = new Mock<IApp>(MockBehavior.Strict);
      var fs = new Mock<IFileSystem>(MockBehavior.Strict);
      app.Setup(app => app.FS).Returns(fs.Object);
      var cacheRepo = new CacheRepo(app.Object);
      var config = new Config(
        ProjectPath: "project/",
        CachePath: "project/.addons",
        AddonsPath: "project/addons"
      );

      var directory = new Mock<IDirectory>(MockBehavior.Strict);
      fs.Setup(fs => fs.Directory).Returns(directory.Object);
      directory.Setup(dir => dir.Exists("project/.addons")).Returns(true);
      directory.Setup(dir => dir.GetDirectories("project/.addons")).Returns(
        new string[] {
          "project/.addons/addon_1",
          "project/.addons/addon_2"
        }
      );
      var shell1 = new Mock<IShell>(MockBehavior.Strict);
      var shell2 = new Mock<IShell>(MockBehavior.Strict);
      app.Setup(app => app.CreateShell("project/.addons/addon_1"))
        .Returns(shell1.Object);
      app.Setup(app => app.CreateShell("project/.addons/addon_2"))
        .Returns(shell2.Object);

      var url1 = "git@github.com:chickensoft-games/addon_1.git";
      var url2 = "git@github.com:chickensoft-games/addon_2.git";

      var result1 = new Mock<IProcessResult>(MockBehavior.Strict);
      result1.Setup(r => r.StandardOutput).Returns(url1);

      var result2 = new Mock<IProcessResult>(MockBehavior.Strict);
      result2.Setup(r => r.StandardOutput).Returns(url2);

      shell1.Setup(sh => sh.Run("git", "remote", "get-url", "origin"))
        .Returns(Task.FromResult(result1.Object));

      shell2.Setup(sh => sh.Run("git", "remote", "get-url", "origin"))
        .Returns(Task.FromResult(result2.Object));

      var urls = await cacheRepo.LoadCache(config);
      urls.ShouldBe(new HashSet<string> { url1, url2 });
    }
  }
}
