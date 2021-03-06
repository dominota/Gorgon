﻿#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: August 14, 2018 6:43:34 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Gorgon.Core;
using Gorgon.IO.Properties;
using Gorgon.PlugIns;

namespace Gorgon.IO
{
    /// <summary>
    /// A plug in for allowing users to supply their own 3rd party animation codecs.
    /// </summary>
    public abstract class GorgonAnimationCodecPlugIn
        : GorgonPlugIn
    {
        #region Properties.
        /// <summary>
        /// Property to return the names of the available codecs for this plug in.
        /// </summary>
        /// <remarks>
        /// This returns a <see cref="IReadOnlyDictionary{TKey,TValue}"/> containing the name of the plug in as its key, and an optional friendly description as its value.
        /// </remarks>
        public abstract IReadOnlyList<GorgonSpriteCodecDescription> Codecs
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to create a new <see cref="IGorgonAnimationCodec"/>.
        /// </summary>
        /// <param name="codec">The codec to retrieve from the plug in.</param>
        /// <returns>A new <see cref="IGorgonAnimationCodec"/> object.</returns>
        /// <remarks>
        /// <para>
        /// Implementors must implement this method to return the codec from the plug in assembly.
        /// </para>
        /// </remarks>
        protected abstract IGorgonAnimationCodec OnCreateCodec(string codec);

        /// <summary>
        /// Function to create a new image codec object.
        /// </summary>
        /// <param name="codec">The name of the codec to look up within the plug in.</param>
        /// <returns>A new instance of a <see cref="IGorgonAnimationCodec"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="codec"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="codec"/> parameter is empty.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the <paramref name="codec"/> was not found in this plug in.</exception>
        /// <remarks>
        /// <para>
        /// If the <paramref name="codec"/> is not found within the plug in, then an exception will be thrown. To determine whether the plug in has the desired <paramref name="codec"/>, check the 
        /// <see cref="Codecs"/> property on the plug in to locate the plug in name.
        /// </para>
        /// </remarks>
        public IGorgonAnimationCodec CreateCodec(string codec)
        {
            if (codec == null)
            {
                throw new ArgumentNullException(nameof(codec));
            }

            if (string.IsNullOrWhiteSpace(codec))
            {
                throw new ArgumentEmptyException(nameof(codec));
            }

            if (!Codecs.Any(item => string.Equals(codec, item.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new KeyNotFoundException(string.Format(Resources.GOR2DIO_ERR_CODEC_NOT_IN_PLUGIN, codec));
            }

            IGorgonAnimationCodec result = OnCreateCodec(codec);

            if (result == null)
            {
                throw new KeyNotFoundException(string.Format(Resources.GOR2DIO_ERR_CODEC_NOT_IN_PLUGIN, codec));
            }

            return result;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonAnimationCodecPlugIn"/> class.
        /// </summary>
        /// <param name="description">Optional description of the plugin.</param>
        /// <remarks>
        /// <para>
        /// Objects that implement this base class should pass in a hard coded description on the base constructor.
        /// </para>
        /// </remarks>
        protected GorgonAnimationCodecPlugIn(string description)
            : base(description)
        {

        }
        #endregion
    }
}
