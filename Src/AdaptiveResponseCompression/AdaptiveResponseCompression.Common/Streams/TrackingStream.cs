namespace AdaptiveResponseCompression.Common.Streams;

/// <summary>
/// Serves for tracking the length of the inner stream.
/// </summary>
public class TrackingStream : Stream
{
    private readonly Stream _stream;

    /// <summary>
    /// Total number of bytes written into the stream.
    /// </summary>
    public long NumberOfWrittenBytes { get; private set; }

    /// <summary>
    /// Creates a new instance of the <see cref="TrackingStream"/>.
    /// </summary>
    /// <param name="stream">Stream to be tracked.</param>
    public TrackingStream(Stream stream)
    {
        _stream = stream;
    }

    [Obsolete("Obsolete")]
    public override object InitializeLifetimeService()
    {
        return _stream.InitializeLifetimeService();
    }

    public override int GetHashCode()
    {
        return _stream.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return _stream.Equals(obj);
    }

    public override string? ToString()
    {
        return _stream.ToString();
    }

    public override int WriteTimeout
    {
        get => _stream.WriteTimeout;

        set => _stream.WriteTimeout = value;
    }

    public override int ReadTimeout
    {
        get => _stream.ReadTimeout;

        set => _stream.ReadTimeout = value;
    }

    public override int ReadByte()
    {
        return _stream.ReadByte();
    }

    public override int Read(Span<byte> buffer)
    {
        return _stream.Read(buffer);
    }

    public override async Task FlushAsync(CancellationToken cancellationToken)
    {
        await _stream.FlushAsync(cancellationToken);
    }

    public override int EndRead(IAsyncResult asyncResult)
    {
        return _stream.EndRead(asyncResult);
    }

    public override ValueTask DisposeAsync()
    {
        return _stream.DisposeAsync();
    }

    public override void CopyTo(Stream destination, int bufferSize)
    {
        _stream.CopyTo(destination, bufferSize);
    }

    public override void Close()
    {
        _stream.Close();
    }

    public override bool CanTimeout => _stream.CanTimeout;

    public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
    {
        return _stream.BeginRead(buffer, offset, count, callback, state);
    }

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
    {
        return _stream.ReadAsync(buffer, cancellationToken);
    }

    public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
    {
        await _stream.CopyToAsync(destination, bufferSize, cancellationToken);
    }

    public override void EndWrite(IAsyncResult asyncResult)
    {
        _stream.EndWrite(asyncResult);
    }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        return await _stream.ReadAsync(buffer, offset, count, cancellationToken);
    }

    public override void Flush()
    {
        _stream.Flush();
    }

    public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback,
        object? state)
    {
        NumberOfWrittenBytes += count;
        return _stream.BeginWrite(buffer, offset, count, callback, state);
    }

    public override void WriteByte(byte value)
    {
        ++NumberOfWrittenBytes;
        _stream.WriteByte(value);
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        NumberOfWrittenBytes += buffer.Length;
        _stream.Write(buffer);
    }

    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        NumberOfWrittenBytes += count;
        await _stream.WriteAsync(buffer, offset, count, cancellationToken);
    }

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        NumberOfWrittenBytes += buffer.Length;
        await _stream.WriteAsync(buffer, cancellationToken);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return _stream.Read(buffer, offset, count);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return _stream.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        _stream.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        NumberOfWrittenBytes += count;
        _stream.Write(buffer, offset, count);
    }

    public override bool CanRead => _stream.CanRead;

    public override bool CanSeek => _stream.CanSeek;

    public override bool CanWrite => _stream.CanWrite;

    public override long Length => _stream.Length;

    public override long Position
    {
        get => _stream.Position;
        set => _stream.Position = value;
    }
}
