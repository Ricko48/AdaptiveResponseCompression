// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO.Compression;
using AdaptiveResponseCompression.Server.CompressionProviders.Interfaces;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;

namespace AdaptiveResponseCompression.Server.CompressionProviders
{
    /// <summary>
    /// This is a placeholder for the CompressionProviderCollection that allows creating the given type via
    /// an <see cref="IServiceProvider" />.
    /// </summary>
    internal class AdaptiveCompressionProviderFactory : IAdaptiveCompressionProvider
    {
        public AdaptiveCompressionProviderFactory(Type providerType)
        {
            ProviderType = providerType;
        }

        private Type ProviderType { get; }

        public IAdaptiveCompressionProvider CreateInstance(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            return (IAdaptiveCompressionProvider)ActivatorUtilities.CreateInstance(serviceProvider, ProviderType, Type.EmptyTypes);
        }

        string ICompressionProvider.EncodingName
        {
            get { throw new NotSupportedException(); }
        }

        bool ICompressionProvider.SupportsFlush
        {
            get { throw new NotSupportedException(); }
        }

        Stream ICompressionProvider.CreateStream(Stream outputStream)
        {
            throw new NotSupportedException();
        }

        Stream IAdaptiveCompressionProvider.CreateStream(Stream outputStream, CompressionLevel level)
        {
            throw new NotSupportedException();
        }
    }
}