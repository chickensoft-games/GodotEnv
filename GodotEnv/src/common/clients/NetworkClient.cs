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

  /// <summary>
  /// Downloads a file from the specified URL to the specified destination directory.
  /// </summary>
  /// <param name="url">The URL of the file to download.</param>
  /// <param name="destinationDirectory">The directory to save the file to.</param>
  /// <param name="filename">The name of the file to download.</param>
  /// <param name="progress">The progress of the download.</param>
  /// <param name="token">The cancellation token.</param>
  /// <param name="proxyUrl">The proxy URL to use for the download.</param>
  /// <returns>The downloaded file.</returns>
  Task DownloadFileAsync(
    string url,
    string destinationDirectory,
    string filename,
    IProgress<DownloadProgress> progress,
    CancellationToken token,
    string? proxyUrl = null
  );

  /// <summary>
  /// Downloads a file from the specified URL to the specified destination directory.
  /// </summary>
  /// <param name="url">The URL of the file to download.</param>
  /// <param name="destinationDirectory">The directory to save the file to.</param>
  /// <param name="filename">The name of the file to download.</param>
  /// <param name="token">The cancellation token.</param>
  /// <param name="proxyUrl">The proxy URL to use for the download.</param>
  /// <returns>The downloaded file.</returns>
  Task DownloadFileAsync(
    string url,
    string destinationDirectory,
    string filename,
    CancellationToken token,
    string? proxyUrl = null
  );

  /// <summary>
  /// Sends a GET request to the specified URL.
  /// </summary>
  /// <param name="url">The URL to send the GET request to.</param>
  /// <param name="requestAgent">Whether to request an agent.</param>
  /// <param name="proxyUrl">The proxy URL to use for the request.</param>
  /// <returns>The response from the GET request.</returns>
  Task<HttpResponseMessage> WebRequestGetAsync(
    string url,
    bool requestAgent = false,
    string? proxyUrl = null
  );
}

/// <summary>Download progress.</summary>
/// <param name="Percent">Amount completed as a percent between 0 and
/// 100.</param>
/// <param name="Speed">Humanized bytes per second speed.</param>
public readonly record struct DownloadProgress(int Percent, string Speed);

public class NetworkClient : INetworkClient {
  public IDownloadService DownloadService { get; }
  public DownloadConfiguration DownloadConfiguration { get; }

  public NetworkClient(
    IDownloadService downloadService,
    DownloadConfiguration downloadConfiguration
  ) {
    DownloadService = downloadService;
    DownloadConfiguration = downloadConfiguration;
  }

  public async Task<HttpResponseMessage> WebRequestGetAsync(string url, bool requestAgent = false, string? proxyUrl = null) {
    var handler = CreateHttpClientHandler(proxyUrl);
    using var client = new HttpClient(handler);

    if (requestAgent) {
      client.DefaultRequestHeaders.UserAgent.TryParseAdd("request");
    }

    return await client.GetAsync(url);
  }

  protected virtual HttpClientHandler CreateHttpClientHandler(string? proxyUrl) {
    var handler = new HttpClientHandler();

    if (!string.IsNullOrEmpty(proxyUrl)) {
      if (!Uri.TryCreate(proxyUrl, UriKind.Absolute, out var proxyUri)) {
        throw new CommandException($"Invalid proxy URL: {proxyUrl}");
      }

      handler.Proxy = new WebProxy(proxyUri);
      handler.UseProxy = true;
    }

    return handler;
  }

  public async Task DownloadFileAsync(
    string url,
    string destinationDirectory,
    string filename,
    IProgress<DownloadProgress> progress,
    CancellationToken token,
    string? proxyUrl = null
  ) {
    var download = CreateDownloadWithProxy(url, destinationDirectory, filename, proxyUrl);

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
    var download = CreateDownloadWithProxy(url, destinationDirectory, filename, proxyUrl);

    token.Register(
      () => {
        if (download.Status == DownloadStatus.Running) {
          download.Stop();
        }
      }
    );

    void done(
    object? sender, System.ComponentModel.AsyncCompletedEventArgs args) {
      if (args.Error != null) {
        throw new CommandException(
          $"Download failed. {args.Error.Message}"
        );
      }
    }

    download.DownloadFileCompleted += done;

    await download.StartAsync(token);

    download.DownloadFileCompleted -= done;
  }

  protected virtual IDownload CreateDownloadWithProxy(string url, string destinationDirectory, string filename, string? proxyUrl = null) {
    if (!string.IsNullOrEmpty(proxyUrl)) {
      DownloadConfiguration.RequestConfiguration.Proxy = new WebProxy {
        Address = new Uri(proxyUrl),
        UseDefaultCredentials = false
      };
    }
    else {
      DownloadConfiguration.RequestConfiguration.Proxy = null;
    }

    return DownloadBuilder
      .New()
      .WithUrl(url)
      .WithDirectory(destinationDirectory)
      .WithFileName(filename)
      .WithConfiguration(DownloadConfiguration)
      .Build();
  }
}
