// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using AdaptiveResponseCompression.Server.CompressionProviders.Interfaces;
using AdaptiveResponseCompression.Server.Models;
using AdaptiveResponseCompression.Server.Options;
using AdaptiveResponseCompression.Server.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System.Diagnostics;
using System.IO.Compression;
using AdaptiveResponseCompression.Server.Dependencies;

namespace AdaptiveResponseCompression.Server.CompressionProviders;

internal class AdaptiveResponseCompressionProvider : IAdaptiveResponseCompressionProvider
{
    private readonly ICompressionLevelComputer _levelComputer;
    private readonly IAdaptiveCompressionProvider[] _providers;
    private readonly HashSet<string> _mimeTypes;
    private readonly HashSet<string> _excludedMimeTypes;
    private readonly bool _enableForHttps;
    private readonly ILogger _logger;
    private readonly AdaptiveResponseCompressionOptions _options;

    public AdaptiveResponseCompressionProvider(
        ICompressionLevelComputer levelComputer,
        IServiceProvider services,
        IOptions<AdaptiveResponseCompressionOptions> options)
    {
        if (levelComputer == null)
        {
            throw new ArgumentNullException(nameof(levelComputer));
        }

        _levelComputer = levelComputer;

        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        _options = options.Value;

        _providers = _options.Providers.ToArray();
        if (_providers.Length == 0)
        {
            _providers =
            [
#if NETCOREAPP2_1 || NETCOREAPP2_2 || NETCOREAPP3_0 ||  NETCOREAPP3_1 || NET5_0_OR_GREATER
                    new AdaptiveCompressionProviderFactory(typeof(AdaptiveBrotliCompressionProvider)),
#elif NET461 || NETSTANDARD2_0
                    // Brotli is only supported in .NET Core 2.1+
#else
#error Target frameworks need to be updated.
#endif
                    new AdaptiveCompressionProviderFactory(typeof(AdaptiveGzipCompressionProvider)),
                    new AdaptiveCompressionProviderFactory(typeof(AdaptiveDeflateCompressionProvider)),
            ];
        }
        for (var i = 0; i < _providers.Length; i++)
        {
            var factory = _providers[i] as AdaptiveCompressionProviderFactory;
            if (factory != null)
            {
                _providers[i] = factory.CreateInstance(services);
            }
        }

        var mimeTypes = _options.MimeTypes;
        if (mimeTypes == null || !mimeTypes.Any())
        {
            mimeTypes = ResponseCompressionDefaults.MimeTypes;
        }

        _mimeTypes = new HashSet<string>(mimeTypes, StringComparer.OrdinalIgnoreCase);

        _excludedMimeTypes = new HashSet<string>(
            _options.ExcludedMimeTypes ?? Enumerable.Empty<string>(),
            StringComparer.OrdinalIgnoreCase
        );

        _enableForHttps = _options.EnableForHttps;

        _logger = services.GetRequiredService<ILogger<AdaptiveResponseCompressionProvider>>();
    }

    public IAdaptiveCompressionProvider? GetProvider(string encoding)
    {
        return _providers.FirstOrDefault(x => x.EncodingName.Equals(encoding, StringComparison.OrdinalIgnoreCase));
    }

    public IList<string> GetCompatibleEncodings(HttpContext context)
    {
        var accept = context.Request.Headers[HeaderNames.AcceptEncoding];

        if (!StringWithQualityHeaderValue.TryParseList(accept, out var encodings) || encodings == null || !encodings.Any())
        {
            _logger.NoAcceptEncoding();
            return Array.Empty<string>();
        }

        return encodings
            .Where(x => x.Value.Value != null && _providers.Any(p => p.EncodingName.Equals(x.Value.Value, StringComparison.OrdinalIgnoreCase)))
            .Select(x => x.Value.Value)
            .ToList()!;

    }

    public virtual CompressionMethodAnalysis GetCompressionAnalysis(double uploadBandwidth, string route, long dataSize, IList<string> acceptedEncodings)
    {
        try
        {
            return GetCompressionAnalysisInternal(uploadBandwidth, route, dataSize, acceptedEncodings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while computing the best compression method");
        }

        // in case of error, use default compression
        return GetDefaultCompressionMethodAnalysis(acceptedEncodings);
    }

    private CompressionMethodAnalysis GetCompressionAnalysisInternal(double uploadBandwidth, string route, long dataSize,
        IList<string> acceptedEncodings)
    {
        var analysis = new CompressionMethodAnalysis();

        var bestProcessingTime = double.MaxValue;

        foreach (var provider in _providers)
        {
            if (acceptedEncodings.All(x => !x.Equals(provider.EncodingName, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            var levelAnalysis =
                _levelComputer.GetCompressionLevelAnalysis(uploadBandwidth, provider.EncodingName, route, dataSize);

            if (bestProcessingTime > levelAnalysis.BestProcessingTime)
            {
                analysis.BestLevel = levelAnalysis.BestLevel;
                analysis.BestProvider = provider;
                bestProcessingTime = levelAnalysis.BestProcessingTime;
            }
        }

        // if no metrics were available, use default compression
        if (analysis.BestLevel == null)
        {
            return GetDefaultCompressionMethodAnalysis(acceptedEncodings);
        }

        var processingTimeWithoutCompression = dataSize / uploadBandwidth;

        // if processing time without compression is lower than with the best possible compression, no compression will be performed
        if (analysis.BestLevel != null && processingTimeWithoutCompression < bestProcessingTime)
        {
            analysis.BestLevel = null;
            analysis.BestProvider = null;
            return analysis;
        }

        return analysis;
    }

    private CompressionMethodAnalysis GetDefaultCompressionMethodAnalysis(IList<string> acceptedEncodings)
    {
        return new CompressionMethodAnalysis
        {
            BestProvider = _providers.First(x => acceptedEncodings.Any(e => e.Equals(x.EncodingName, StringComparison.OrdinalIgnoreCase))),
            BestLevel = CompressionLevel.Fastest,
        };
    }

    public virtual IAdaptiveCompressionProvider GetAdaptiveCompressionProvider(HttpContext context)
    {
        // e.g. Accept-Encoding: gzip, deflate, sdch
        var accept = context.Request.Headers[HeaderNames.AcceptEncoding];

        // Note this is already checked in CheckRequestAcceptsCompression which _should_ prevent any of these other methods from being called.
        if (StringValues.IsNullOrEmpty(accept))
        {
            Debug.Assert(false, "Duplicate check failed.");
            _logger.NoAcceptEncoding();
            return null;
        }

        if (!StringWithQualityHeaderValue.TryParseList(accept, out var encodings) || !encodings.Any())
        {
            _logger.NoAcceptEncoding();
            return null!;
        }

        var candidates = new HashSet<AdaptiveProviderCandidate>();

        foreach (var encoding in encodings)
        {
            var encodingName = encoding.Value;
            var quality = encoding.Quality.GetValueOrDefault(1);

            if (quality < double.Epsilon)
            {
                continue;
            }

            for (var i = 0; i < _providers.Length; i++)
            {
                var provider = _providers[i];

                if (StringSegment.Equals(provider.EncodingName, encodingName, StringComparison.OrdinalIgnoreCase))
                {
                    candidates.Add(new AdaptiveProviderCandidate(provider.EncodingName, quality, i, provider));
                }
            }

            // Uncommon but valid options
            if (StringSegment.Equals("*", encodingName, StringComparison.Ordinal))
            {
                for (var i = 0; i < _providers.Length; i++)
                {
                    var provider = _providers[i];

                    // Any provider is a candidate.
                    candidates.Add(new AdaptiveProviderCandidate(provider.EncodingName, quality, i, provider));
                }

                break;
            }

            if (StringSegment.Equals("identity", encodingName, StringComparison.OrdinalIgnoreCase))
            {
                // We add 'identity' to the list of "candidates" with a very low priority and no provider.
                // This will allow it to be ordered based on its quality (and priority) later in the method.
                candidates.Add(new AdaptiveProviderCandidate(encodingName.Value!, quality, priority: int.MaxValue, provider: null!));
            }
        }

        IAdaptiveCompressionProvider selectedProvider = null!;
        if (candidates.Count <= 1)
        {
            selectedProvider = candidates.FirstOrDefault().Provider;
        }
        else
        {
            selectedProvider = candidates
                .OrderByDescending(x => x.Quality)
                .ThenBy(x => x.Priority)
                .First().Provider;
        }

        if (selectedProvider == null)
        {
            // "identity" would match as a candidate but not have a provider implementation
            _logger.NoCompressionProvider();
            return null!;
        }

        _logger.CompressingWith(selectedProvider.EncodingName);
        return selectedProvider;
    }

    /// <inheritdoc />
    public virtual ICompressionProvider GetCompressionProvider(HttpContext context)
    {
        // e.g. Accept-Encoding: gzip, deflate, sdch
        var accept = context.Request.Headers[HeaderNames.AcceptEncoding];

        // Note this is already checked in CheckRequestAcceptsCompression which _should_ prevent any of these other methods from being called.
        if (StringValues.IsNullOrEmpty(accept))
        {
            Debug.Assert(false, "Duplicate check failed.");
            _logger.NoAcceptEncoding();
            return null;
        }

        if (!StringWithQualityHeaderValue.TryParseList(accept, out var encodings) || !encodings.Any())
        {
            _logger.NoAcceptEncoding();
            return null!;
        }

        var candidates = new HashSet<ProviderCandidate>();

        foreach (var encoding in encodings)
        {
            var encodingName = encoding.Value;
            var quality = encoding.Quality.GetValueOrDefault(1);

            if (quality < double.Epsilon)
            {
                continue;
            }

            for (int i = 0; i < _providers.Length; i++)
            {
                var provider = _providers[i];

                if (StringSegment.Equals(provider.EncodingName, encodingName, StringComparison.OrdinalIgnoreCase))
                {
                    candidates.Add(new ProviderCandidate(provider.EncodingName, quality, i, provider));
                }
            }

            // Uncommon but valid options
            if (StringSegment.Equals("*", encodingName, StringComparison.Ordinal))
            {
                for (int i = 0; i < _providers.Length; i++)
                {
                    var provider = _providers[i];

                    // Any provider is a candidate.
                    candidates.Add(new ProviderCandidate(provider.EncodingName, quality, i, provider));
                }

                break;
            }

            if (StringSegment.Equals("identity", encodingName, StringComparison.OrdinalIgnoreCase))
            {
                // We add 'identity' to the list of "candidates" with a very low priority and no provider.
                // This will allow it to be ordered based on its quality (and priority) later in the method.
                candidates.Add(new ProviderCandidate(encodingName.Value!, quality, priority: int.MaxValue, provider: null!));
            }
        }

        ICompressionProvider selectedProvider = null!;
        if (candidates.Count <= 1)
        {
            selectedProvider = candidates.FirstOrDefault().Provider;
        }
        else
        {
            selectedProvider = candidates
                .OrderByDescending(x => x.Quality)
                .ThenBy(x => x.Priority)
                .First().Provider;
        }

        if (selectedProvider == null!)
        {
            // "identity" would match as a candidate but not have a provider implementation
            _logger.NoCompressionProvider();
            return null!;
        }

        _logger.CompressingWith(selectedProvider.EncodingName);
        return selectedProvider;
    }

    /// <inheritdoc />
    public virtual bool ShouldCompressResponse(HttpContext context)
    {
        if (context.Response.Headers.ContainsKey(HeaderNames.ContentRange))
        {
            _logger.NoCompressionDueToHeader(HeaderNames.ContentRange);
            return false;
        }

        if (context.Response.Headers.ContainsKey(HeaderNames.ContentEncoding))
        {
            _logger.NoCompressionDueToHeader(HeaderNames.ContentEncoding);
            return false;
        }

        var mimeType = context.Response.ContentType;

        if (string.IsNullOrEmpty(mimeType))
        {
            _logger.NoCompressionForContentType(mimeType!);
            return false;
        }

        var separator = mimeType.IndexOf(';');
        if (separator >= 0)
        {
            // Remove the content-type optional parameters
            mimeType = mimeType.Substring(0, separator);
            mimeType = mimeType.Trim();
        }

        var shouldCompress = ShouldCompressExact(mimeType) //check exact match type/subtype
            ?? ShouldCompressPartial(mimeType) //check partial match type/*
            ?? _mimeTypes.Contains("*/*"); //check wildcard */*

        if (shouldCompress)
        {
            _logger.ShouldCompressResponse();  // Trace, there will be more logs
            return true;
        }

        _logger.NoCompressionForContentType(mimeType);
        return false;
    }

    /// <inheritdoc />
    public bool CheckRequestAcceptsCompression(HttpContext context)
    {
        if (context.Request.IsHttps && !_enableForHttps)
        {
            _logger.NoCompressionForHttps();
            return false;
        }

        if (string.IsNullOrEmpty(context.Request.Headers[HeaderNames.AcceptEncoding]))
        {
            _logger.NoAcceptEncoding();
            return false;
        }

        _logger.RequestAcceptsCompression(); // Trace, there will be more logs
        return true;
    }

    private bool? ShouldCompressExact(string mimeType)
    {
        //Check excluded MIME types first, then included
        if (_excludedMimeTypes.Contains(mimeType))
        {
            return false;
        }

        if (_mimeTypes.Contains(mimeType))
        {
            return true;
        }

        return null;
    }

    private bool? ShouldCompressPartial(string mimeType)
    {
        var slashPos = mimeType?.IndexOf('/');

        if (slashPos >= 0)
        {
            var partialMimeType = mimeType!.Substring(0, slashPos.Value) + "/*";
            return ShouldCompressExact(partialMimeType);
        }

        return null;
    }

    private readonly struct ProviderCandidate : IEquatable<ProviderCandidate>
    {
        public ProviderCandidate(string encodingName, double quality, int priority, ICompressionProvider provider)
        {
            EncodingName = encodingName;
            Quality = quality;
            Priority = priority;
            Provider = provider;
        }

        public string EncodingName { get; }

        public double Quality { get; }

        public int Priority { get; }

        public ICompressionProvider Provider { get; }

        public bool Equals(ProviderCandidate other)
        {
            return string.Equals(EncodingName, other.EncodingName, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj)
        {
            return obj is ProviderCandidate candidate && Equals(candidate);
        }

        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(EncodingName);
        }
    }

    private readonly struct AdaptiveProviderCandidate : IEquatable<AdaptiveProviderCandidate>
    {
        public AdaptiveProviderCandidate(string encodingName, double quality, int priority, IAdaptiveCompressionProvider provider)
        {
            EncodingName = encodingName;
            Quality = quality;
            Priority = priority;
            Provider = provider;
        }

        public string EncodingName { get; }

        public double Quality { get; }

        public int Priority { get; }

        public IAdaptiveCompressionProvider Provider { get; }

        public bool Equals(AdaptiveProviderCandidate other)
        {
            return string.Equals(EncodingName, other.EncodingName, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj)
        {
            return obj is AdaptiveProviderCandidate candidate && Equals(candidate);
        }

        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(EncodingName);
        }
    }
}
