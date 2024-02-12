using AdaptiveResponseCompression.Server.CompressionProviders.Interfaces;
using Microsoft.AspNetCore.Http;

namespace AdaptiveResponseCompression.Server.Streams;

/// <summary>
/// On first write, checks if the response should be compressed and switches to the appropriate stream.
/// </summary>
internal class AdaptiveCompressionBody : Stream
{
    private readonly Stream _originalBodyStream;
    private readonly IAdaptiveResponseCompressionProvider _provider;
    private readonly HttpContext _context;
    private bool _checkedShouldCompress = false;
    private Stream _currentBodyStream;

    public bool ShouldBeCompressed { get; private set; } = true;

    public AdaptiveCompressionBody(
        Stream originalBodyStream,
        Stream compressionStream,
        IAdaptiveResponseCompressionProvider provider,
        HttpContext context)
    {
        _currentBodyStream = compressionStream;
        _originalBodyStream = originalBodyStream;
        _provider = provider;
        _context = context;
    }

    [Obsolete("Obsolete")]
    public override object InitializeLifetimeService()
    {
        return _currentBodyStream.InitializeLifetimeService();
    }

    public override int GetHashCode()
    {
        return _currentBodyStream.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return _currentBodyStream.Equals(obj);
    }

    public override string? ToString()
    {
        return _currentBodyStream.ToString();
    }

    public override int WriteTimeout
    {
        get => _currentBodyStream.WriteTimeout;

        set => _currentBodyStream.WriteTimeout = value;
    }

    public override int ReadTimeout
    {
        get => _currentBodyStream.ReadTimeout;

        set => _currentBodyStream.ReadTimeout = value;
    }

    public override int ReadByte()
    {
        return _currentBodyStream.ReadByte();
    }

    public override int Read(Span<byte> buffer)
    {
        return _currentBodyStream.Read(buffer);
    }

    public override async Task FlushAsync(CancellationToken cancellationToken)
    {
        await _currentBodyStream.FlushAsync(cancellationToken);
    }

    public override int EndRead(IAsyncResult asyncResult)
    {
        return _currentBodyStream.EndRead(asyncResult);
    }

    public override ValueTask DisposeAsync()
    {
        return _currentBodyStream.DisposeAsync();
    }

    public override void CopyTo(Stream destination, int bufferSize)
    {
        _currentBodyStream.CopyTo(destination, bufferSize);
    }

    public override void Close()
    {
        _currentBodyStream.Close();
    }

    public override bool CanTimeout => _currentBodyStream.CanTimeout;

    public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
    {
        return _currentBodyStream.BeginRead(buffer, offset, count, callback, state);
    }

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
    {
        return _currentBodyStream.ReadAsync(buffer, cancellationToken);
    }

    public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
    {
        await _currentBodyStream.CopyToAsync(destination, bufferSize, cancellationToken);
    }

    public override void EndWrite(IAsyncResult asyncResult)
    {
        _currentBodyStream.EndWrite(asyncResult);
    }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        return await _currentBodyStream.ReadAsync(buffer, offset, count, cancellationToken);
    }

    public override void Flush()
    {
        _currentBodyStream.Flush();
    }

    public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback,
        object? state)
    {
        CheckShouldCompress();
        return _currentBodyStream.BeginWrite(buffer, offset, count, callback, state);
    }

    public override void WriteByte(byte value)
    {
        CheckShouldCompress();
        _currentBodyStream.WriteByte(value);
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        CheckShouldCompress();
        _currentBodyStream.Write(buffer);
    }

    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        CheckShouldCompress();
        await _currentBodyStream.WriteAsync(buffer, offset, count, cancellationToken);
    }

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        CheckShouldCompress();
        await _currentBodyStream.WriteAsync(buffer, cancellationToken);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return _currentBodyStream.Read(buffer, offset, count);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return _currentBodyStream.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        _currentBodyStream.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        CheckShouldCompress();
        _currentBodyStream.Write(buffer, offset, count);
    }

    public override bool CanRead => _currentBodyStream.CanRead;

    public override bool CanSeek => _currentBodyStream.CanSeek;

    public override bool CanWrite => _currentBodyStream.CanWrite;

    public override long Length => _currentBodyStream.Length;

    public override long Position
    {
        get => _currentBodyStream.Position;
        set => _currentBodyStream.Position = value;
    }

    private void CheckShouldCompress()
    {
        if (_checkedShouldCompress)
        {
            return;
        }

        if (!_provider.ShouldCompressResponse(_context))
        {
            _currentBodyStream = _originalBodyStream;
            ShouldBeCompressed = false;
        }

        _checkedShouldCompress = true;
    }
}
