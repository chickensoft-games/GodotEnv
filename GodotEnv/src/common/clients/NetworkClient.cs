namespace Chickensoft.GodotEnv.Common.Clients;

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CliFx.Exceptions;
using Downloader;
using Humanizer;

public interface INetworkClient {
  IDownloadService DownloadService { get; }
  DownloadConfiguration DownloadConfiguration { get; }

  Task DownloadFileAsync(
    string url,
    string destinationDirectory,
    string filename,
    IProgress<DownloadProgress> progress,
    CancellationToken token,
    string? proxyUrl = null
  );

  Task DownloadFileAsync(
    string url,
    string destinationDirectory,
    string filename,
    CancellationToken token,
    string? proxyUrl = null
  );

  public Task<HttpResponseMessage> WebRequestGetAsync(string url, bool requestAgent = false, string? proxyUrl = null);
}

/// <summary>Download progress.</summary>
/// <param name="Percent">Amount completed as a percent between 0 and
/// 100.</param>
/// <param name="Speed">Humanized bytes per second speed.</param>
public readonly record struct DownloadProgress(int Percent, string Speed);

public class NetworkClient : INetworkClient {
  public IDownloadService DownloadService { get; }
  public DownloadConfiguration DownloadConfiguration { get; }

  private static HttpClient? _client;

  public NetworkClient(
    IDownloadService downloadService,
    DownloadConfiguration downloadConfiguration
  ) {
    DownloadService = downloadService;
    DownloadConfiguration = downloadConfiguration;
  }

  public async Task<HttpResponseMessage> WebRequestGetAsync(string url, bool requestAgent = false, string? proxyUrl = null) {
    if (_client == null || !string.IsNullOrEmpty(proxyUrl)) {
      var handler = string.IsNullOrEmpty(proxyUrl)
        ? new HttpClientHandler()
        : new HttpClientHandler {
          Proxy = new WebProxy(proxyUrl),
          UseProxy = true
        };

      _client = new HttpClient(handler);
    }

    if (requestAgent) {
      _client.DefaultRequestHeaders.UserAgent.TryParseAdd("request");
    }
    return await _client.GetAsync(url);
  }

  public async Task DownloadFileAsync(
    string url,
    string destinationDirectory,
    string filename,
    IProgress<DownloadProgress> progress,
    CancellationToken token,
    string? proxyUrl = null
  ) {
    // if proxyUrl is provided, set the proxy, otherwise clear the proxy
    if (!string.IsNullOrEmpty(proxyUrl)) {
      DownloadConfiguration.RequestConfiguration.Proxy = new WebProxy {
        Address = new Uri(proxyUrl),
        UseDefaultCredentials = false
      };
    }
    else {
      DownloadConfiguration.RequestConfiguration.Proxy = null;
    }

    var download = DownloadBuilder
      .New()
      .WithUrl(url)
      .WithDirectory(destinationDirectory)
      .WithFileName(filename)
      .WithConfiguration(DownloadConfiguration)
      .Build();

    var lastPercent = 0d;
    var threshold = 1d;

    token.Register(
      () => {
        if (download.Status == DownloadStatus.Running) {
          download.Stop();
        }
      }
    );

    void internalProgress(
      object? sender, Downloader.DownloadProgressChangedEventArgs args
    ) {
      var speed = args.BytesPerSecondSpeed;
      var humanizedSpeed = speed.Bytes().Per(1.Seconds()).Humanize("#.##");
      var percent = args.ProgressPercentage;
      var p = (int)Math.Round(percent);

      if (p - lastPercent >= threshold) {
        lastPercent = p;
        progress.Report(new(p, humanizedSpeed));
      }
    }

    void done(
      object? sender, System.ComponentModel.AsyncCompletedEventArgs args
    ) {
      if (args.Cancelled) {
        throw new CommandException(
          "🚨 Download cancelled!"
        );
      }
      if (args.Error != null) {
        throw new CommandException(
          $"Download failed. {args.Error.Message}"
        );
      }
    }

    download.DownloadProgressChanged += internalProgress;
    download.DownloadFileCompleted += done;

    await download.StartAsync(token);

    download.DownloadProgressChanged -= internalProgress;
    download.DownloadFileCompleted -= done;
  }

  public async Task DownloadFileAsync(
    string url,
    string destinationDirectory,
    string filename,
    CancellationToken token,
    string? proxyUrl = null
  ) {
    // if proxyUrl is provided, set the proxy, otherwise clear the proxy
    if (!string.IsNullOrEmpty(proxyUrl)) {
      DownloadConfiguration.RequestConfiguration.Proxy = new WebProxy {
        Address = new Uri(proxyUrl),
        UseDefaultCredentials = false
      };
    }
    else {
      DownloadConfiguration.RequestConfiguration.Proxy = null;
    }

    var download = DownloadBuilder
      .New()
      .WithUrl(url)
      .WithDirectory(destinationDirectory)
      .WithFileName(filename)
      .WithConfiguration(DownloadConfiguration)
      .Build();

    token.Register(
      () => {
        if (download.Status == DownloadStatus.Running) {
          download.Stop();
        }
      }
    );

    await download.StartAsync(token);
  }
}
