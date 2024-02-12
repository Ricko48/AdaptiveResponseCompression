// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using AdaptiveResponseCompression.Server.CompressionProviders.Interfaces;

namespace AdaptiveResponseCompression.Server.CompressionProviders
{
    /// <summary>
    /// A Collection of IAdaptiveCompressionProvider's that also allows them to be instantiated from an <see cref="IServiceProvider" />.
    /// </summary>
    public class AdaptiveCompressionProviderCollection : Collection<IAdaptiveCompressionProvider>
    {
        /// <summary>
        /// Adds a type representing an <see cref="IAdaptiveCompressionProvider"/>.
        /// </summary>
        /// <remarks>
        /// Provider instances will be created using an <see cref="IServiceProvider" />.
        /// </remarks>
        public void Add<TCompressionProvider>() where TCompressionProvider : IAdaptiveCompressionProvider
        {
            Add(typeof(TCompressionProvider));
        }

        /// <summary>
        /// Adds a type representing an <see cref="IAdaptiveCompressionProvider"/>.
        /// </summary>
        /// <param name="providerType">Type representing an <see cref="IAdaptiveCompressionProvider"/>.</param>
        /// <remarks>
        /// Provider instances will be created using an <see cref="IServiceProvider" />.
        /// </remarks>
        public void Add(Type providerType)
        {
            if (providerType == null)
            {
                throw new ArgumentNullException(nameof(providerType));
            }

            if (!typeof(IAdaptiveCompressionProvider).IsAssignableFrom(providerType))
            {
                throw new ArgumentException($"The provider must implement {nameof(IAdaptiveCompressionProvider)}.", nameof(providerType));
            }

            var factory = new AdaptiveCompressionProviderFactory(providerType);
            Add(factory);
        }
    }
}