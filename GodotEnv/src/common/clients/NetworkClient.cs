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
      if (!string.IsNullOrEmpty(proxyUrl)) {
        var handler = new HttpClientHandler {
          Proxy = new WebProxy(proxyUrl),
          UseProxy = true
        };
        _client = new HttpClient(handler);
      }
      else {
        _client = new HttpClient();
      }
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
    if (!string.IsNullOrEmpty(proxyUrl)) {
      // 如果有代理，使用WebClient代替
      using var webClient = new WebClient();
      webClient.Proxy = new WebProxy(proxyUrl);

      // 创建临时进度报告
      var lastPercent = 0;
      webClient.DownloadProgressChanged += (sender, e) => {
        var percent = e.ProgressPercentage;
        if (percent > lastPercent) {
          lastPercent = percent;
          progress.Report(new(percent, $"{e.BytesReceived / 1024d / Math.Max(1, e.ProgressPercentage):F2} KB/s"));
        }
      };

      // 设置取消
      token.Register(() => webClient.CancelAsync());

      // 创建任务
      var tcs = new TaskCompletionSource<bool>();
      webClient.DownloadFileCompleted += (sender, e) => {
        if (e.Cancelled) {
          tcs.SetException(new CommandException("Download cancelled!"));
        }
        else if (e.Error != null) {
          tcs.SetException(new CommandException($"Download failed. {e.Error.Message}"));
        }
        else {
          tcs.SetResult(true);
        }
      };

      // 开始下载
      var filePath = System.IO.Path.Combine(destinationDirectory, filename);
      webClient.DownloadFileAsync(new Uri(url), filePath);
      await tcs.Task;
    }
    else {
      // 使用原来的下载方式
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
  }

  public async Task DownloadFileAsync(
    string url,
    string destinationDirectory,
    string filename,
    CancellationToken token,
    string? proxyUrl = null
  ) {
    if (!string.IsNullOrEmpty(proxyUrl)) {
      // 使用带进度的方法
      await DownloadFileAsync(url, destinationDirectory, filename,
        new Progress<DownloadProgress>(), token, proxyUrl);
    }
    else {
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
}
