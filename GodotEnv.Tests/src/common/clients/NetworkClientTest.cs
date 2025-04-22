namespace Chickensoft.GodotEnv.Tests;

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Chickensoft.GodotEnv.Common.Clients;
using Chickensoft.GodotEnv.Common.Models;
using CliFx.Exceptions;
using Downloader;
using Moq;
using Shouldly;
using Xunit;

public class NetworkClientTest {

  [Fact]
  public void WebRequestGetAsyncWithInvalidProxyThrowsCommandException() {
    var testUrl = "https://example.com/get";
    var mockDownloadService = new Mock<IDownloadService>();
    var downloadConfig = Defaults.DownloadConfiguration;
    var invalidProxyUrl = "invalid-proxy-url";

    var networkClient = new NetworkClient(
      mockDownloadService.Object,
      downloadConfig
    );

    Should.Throw<CommandException>(async () => await networkClient.WebRequestGetAsync(testUrl, false, invalidProxyUrl)).Message.ShouldContain("Invalid proxy URL");
  }

  [Fact]
  public void CreateHttpClientHandlerWithValidProxy() {
    var proxyUrl = "http://proxy.example.com:8080";

    var mockDownloadService = new Mock<IDownloadService>();
    var downloadConfig = Defaults.DownloadConfiguration;

    var networkClient = new TestableNetworkClient(mockDownloadService.Object, downloadConfig);

    var handler = networkClient.PublicCreateHttpClientHandler(proxyUrl);

    handler.UseProxy.ShouldBeTrue();
    handler.Proxy.ShouldNotBeNull();
    var webProxy = handler.Proxy as WebProxy;
    webProxy.ShouldNotBeNull();
    webProxy.Address.ShouldNotBeNull();

    webProxy.Address.Host.ShouldBe("proxy.example.com");
    webProxy.Address.Port.ShouldBe(8080);
    webProxy.Address.Scheme.ShouldBe("http");
  }

  [Fact]
  public void CreateDownloadWithValidProxy() {
    var url = "https://example.com/file.zip";
    var destinationDirectory = "/tmp/downloads";
    var filename = "test-file.zip";
    var proxyUrl = "http://proxy.example.com:8080";

    var mockDownloadService = new Mock<IDownloadService>();
    var downloadConfig = Defaults.DownloadConfiguration;

    var networkClient = new TestableNetworkClient(mockDownloadService.Object, downloadConfig);

    var download = networkClient.PublicCreateDownloadWithProxy(url, destinationDirectory, filename, proxyUrl);

    var proxy = downloadConfig.RequestConfiguration.Proxy;
    proxy.ShouldNotBeNull();
    var webProxy = proxy as WebProxy;
    webProxy.ShouldNotBeNull();
    webProxy.Address.ShouldNotBeNull();

    webProxy.Address.Host.ShouldBe("proxy.example.com");
    webProxy.Address.Port.ShouldBe(8080);
    webProxy.Address.Scheme.ShouldBe("http");
    webProxy.UseDefaultCredentials.ShouldBeFalse();

    download.ShouldNotBeNull();
    download.Url.ShouldBe(url);
    download.Folder.ShouldBe(destinationDirectory);
    download.Filename.ShouldBe(filename);
  }

  [Fact]
  public void CreateDownloadWithInvalidProxyThrowsCommandException() {
    var invalidProxyUrl = "invalid-proxy-url";
    var url = "https://example.com/file.zip";
    var destinationDirectory = "/tmp/downloads";
    var filename = "test-file.zip";

    var mockDownloadService = new Mock<IDownloadService>();
    var downloadConfig = Defaults.DownloadConfiguration;

    var networkClient = new TestableNetworkClient(mockDownloadService.Object, downloadConfig);

    Should.Throw<CommandException>(() => networkClient.PublicCreateDownloadWithProxy(url, destinationDirectory, filename, invalidProxyUrl)).Message.ShouldContain("Invalid proxy URL");
  }

  [Fact]
  public async Task DownloadFileAsyncWithInvalidUrlWithProgressThrowsCommandException() {
    var invalidUrl = "https://invalid-domain-that-doesnt-exist-12345.com/file.zip";
    var tempDir = Path.GetTempPath();
    var fileName = $"test_download_{Guid.NewGuid()}.bin";
    var progress = new Mock<IProgress<DownloadProgress>>().Object;
    var token = new CancellationToken();

    var downloadConfig = new DownloadConfiguration();
    var downloadService = new Mock<IDownloadService>().Object;
    var networkClient = new NetworkClient(downloadService, downloadConfig);

    var exception = await Should.ThrowAsync<CommandException>(async () =>
      await networkClient.DownloadFileAsync(
        invalidUrl,
        tempDir,
        fileName,
        progress,
        token
      )
    );
    exception.Message.ShouldContain("Download failed");
  }

  [Fact]
  public async Task DownloadFileAsyncWithInvalidUrlWithNoProgressThrowsCommandException() {
    var invalidUrl = "https://invalid-domain-that-doesnt-exist-12345.com/file.zip";
    var tempDir = Path.GetTempPath();
    var fileName = $"test_download_{Guid.NewGuid()}.bin";
    var token = new CancellationToken();

    var downloadConfig = new DownloadConfiguration();
    var downloadService = new Mock<IDownloadService>().Object;
    var networkClient = new NetworkClient(downloadService, downloadConfig);

    var exception = await Should.ThrowAsync<CommandException>(async () =>
      await networkClient.DownloadFileAsync(
        invalidUrl,
        tempDir,
        fileName,
        token
      )
    );
    exception.Message.ShouldContain("Download failed");
  }

  // a testable network client that exposes protected methods
  private sealed class TestableNetworkClient : NetworkClient {

    public TestableNetworkClient(IDownloadService downloadService, DownloadConfiguration downloadConfiguration)
      : base(downloadService, downloadConfiguration) {
    }

    // public for testing
    public HttpClientHandler PublicCreateHttpClientHandler(string? proxyUrl) => CreateHttpClientHandler(proxyUrl);

    // public for testing
    public IDownload PublicCreateDownloadWithProxy(string url, string destinationDirectory, string filename, string? proxyUrl = null) => CreateDownloadWithProxy(url, destinationDirectory, filename, proxyUrl);
  }
}
