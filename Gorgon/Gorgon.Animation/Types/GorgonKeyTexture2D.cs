﻿#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Wednesday, October 3, 2012 9:14:18 PM
// 
#endregion

using System;
using DX = SharpDX;
using Gorgon.Graphics.Core;

namespace Gorgon.Animation
{
    /// <summary>
    /// An animation key frame for a <see cref="GorgonTexture2DView"/>, texture coordinates and a texture array index.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A key frame represents a value for an object property at a given time. 
    /// </para>
    /// <para>
    /// The track that the key frame is on is used to interpolate the value between key frames. This method makes it so that only a few keyframes are required for an animation rather then setting a value
    /// for every time index.
    /// </para>
    /// <para>
    /// The track for this key frame does not use interpolation. This means that there is no smooth transition between values and each value is "snapped" to when animating.
    /// </para>
    /// </remarks>
    /// <seealso cref="IGorgonTrack{T}"/>
	public class GorgonKeyTexture2D
		: IGorgonKeyFrame
	{
		#region Variables.
        // The texture coordinates.
	    private DX.RectangleF _textureCoordinates;
        // The texture array index.
	    private int _textureArrayIndex;
        #endregion

        #region Properties.
	    /// <summary>
	    /// Property to set or return the texture view to use.
	    /// </summary>
	    public GorgonTexture2DView Value
	    {
	        get;
	        set;
	    }

        /// <summary>
        /// Property to return the texture array index to use on a texture array.
        /// </summary>
	    public ref int TextureArrayIndex => ref _textureArrayIndex;

	    /// <summary>
	    /// Property to return the texture coordinates.
	    /// </summary>
	    public ref DX.RectangleF TextureCoordinates => ref _textureCoordinates;

	    /// <summary>
	    /// Property to return the time at which the key frame is stored.
	    /// </summary>
	    public float Time
	    {
            get;
	    }

	    /// <summary>
	    /// Property to return the type of data for this key frame.
	    /// </summary>
	    public Type DataType
	    {
	        get;
	    } = typeof(GorgonTexture2DView);
		#endregion

        #region Methods.
	    /// <summary>
	    /// Function to clone the key.
	    /// </summary>
	    /// <returns>The cloned key.</returns>
	    public IGorgonKeyFrame Clone()
	    {
	        return new GorgonKeyTexture2D(Time, Value, _textureCoordinates, _textureArrayIndex);
	    }
        #endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonKeyTexture2D" /> struct.
		/// </summary>
		/// <param name="time">The time for the key frame.</param>
		/// <param name="value">The value to apply to the key frame.</param>
		/// <param name="textureCoordinates">Region on the texture to update.</param>
		/// <param name="textureArrayIndex">The texture array index to use with a texture array.</param>
		public GorgonKeyTexture2D(float time, GorgonTexture2DView value, DX.RectangleF textureCoordinates, int textureArrayIndex)
		{
			Time = time;
			Value = value;
			_textureCoordinates = textureCoordinates;
		    _textureArrayIndex = textureArrayIndex;
		}
		#endregion
	}
}
