namespace Chickensoft.Chicken.Tests;

using Moq;
using Shouldly;
using Xunit;

public class SourceRepositoryTest {
  [Fact]
  public void InitializesWithRemote() {
    var repo = new SourceRepository(
      url: "git@github.com:chickensoft-games/Chicken.git",
      checkout: "main"
    );

    repo.Source.ShouldBe(RepositorySource.Remote);
  }

  [Fact]
  public void InitializesWithLocal() {
    var repo = new SourceRepository(
      url: "/home/user/Chicken",
      checkout: "main"
    );

    repo.Source.ShouldBe(RepositorySource.Local);
  }

  [Fact]
  public void InitializesWithDefaultCheckout() {
    var repo = new SourceRepository("/home/user/Chicken", null);
    repo.Checkout.ShouldBe(App.DEFAULT_CHECKOUT);
  }

  [Fact]
  public void SourcePathReturnsRootedPathForLocalUrl() {
    var repo = new SourceRepository(url: "user/Chicken", checkout: "main");

    var app = new Mock<IApp>();
    app.Setup(app => app.WorkingDir).Returns("/");
    app.Setup(app => app.GetRootedPath("user/Chicken", "/"))
      .Returns("/user/Chicken");

    (repo as ISourceRepository)
      .SourcePath(app.Object).ShouldBe("/user/Chicken");
  }

  [Fact]
  public void SourcePathReturnsUrlForRemoteUrl() {
    var url = "git@github.com:chickensoft-games/Chicken.git";
    var repo = new SourceRepository(url: url, checkout: "main");
    var app = new Mock<IApp>();

    (repo as ISourceRepository)
      .SourcePath(app.Object).ShouldBe(url);
  }
}
