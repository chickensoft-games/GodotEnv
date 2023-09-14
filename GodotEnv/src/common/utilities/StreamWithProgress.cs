namespace Chickensoft.GodotEnv.Common.Utilities;

using System;
using System.IO;

/// <summary>
/// Progress reporting stream wrapper.
/// Credit: https://stackoverflow.com/a/42436311
/// </summary>
public class ProgressStream : Stream {
  // NOTE: for illustration purposes. For production code, one would want to
  // override *all* of the virtual methods, delegating to the base _stream object,
  // to ensure performance optimizations in the base _stream object aren't
  // bypassed.

  private readonly Stream _stream;
  private readonly IProgress<int> _readProgress;
  private readonly IProgress<int> _writeProgress;

  public ProgressStream(Stream stream, IProgress<int> readProgress, IProgress<int> writeProgress) {
    _stream = stream;
    _readProgress = readProgress;
    _writeProgress = writeProgress;
  }

  public override bool CanRead => _stream.CanRead;
  public override bool CanSeek => _stream.CanSeek;
  public override bool CanWrite => _stream.CanWrite;
  public override long Length => _stream.Length;
  public override long Position {
    get => _stream.Position;
    set => _stream.Position = value;
  }

  public override void Flush() => _stream.Flush();
  public override long Seek(long offset, SeekOrigin origin) => _stream.Seek(offset, origin);
  public override void SetLength(long value) => _stream.SetLength(value);

  public override int Read(byte[] buffer, int offset, int count) {
    var bytesRead = _stream.Read(buffer, offset, count);

    _readProgress?.Report(bytesRead);
    return bytesRead;
  }

  public override void Write(byte[] buffer, int offset, int count) {
    _stream.Write(buffer, offset, count);
    _writeProgress?.Report(count);
  }
}
