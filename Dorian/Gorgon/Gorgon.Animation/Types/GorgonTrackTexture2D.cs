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
// Created: Wednesday, October 3, 2012 9:14:34 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Drawing;
using SlimMath;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Animation
{
	/// <summary>
	/// A track that will animate properties with a 2D texture data type.
	/// </summary>
	class GorgonTrackTexture2D
		: GorgonAnimationTrack
	{
		#region Variables.
		private Func<Object, GorgonTexture2D> _getTextureProperty = null;			// Get property method.
		private Action<Object, GorgonTexture2D> _setTextureProperty = null;			// Set property method.
		private Func<Object, RectangleF> _getTextureRegionProperty = null;			// Get property method.
		private Action<Object, RectangleF> _setTextureRegionProperty = null;			// Set property method.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the supported interpolation modes for this track.
		/// </summary>
		public override TrackInterpolationMode SupportedInterpolation
		{
			get
			{
				return TrackInterpolationMode.Linear | TrackInterpolationMode.None;
			}
		}		
		#endregion

		#region Methods.
		/// <summary>
		/// Function to interpolate a new key frame from the nearest previous and next key frames.
		/// </summary>
		/// <param name="keyValues">Nearest previous and next key frames.</param>
		/// <param name="keyTime">The time to assign to the key.</param>
		/// <param name="unitTime">The time, expressed in unit time.</param>
		/// <returns>
		/// The interpolated key frame containing the interpolated values.
		/// </returns>
		protected override IKeyFrame GetTweenKey(ref GorgonAnimationTrack.NearestKeys keyValues, float keyTime, float unitTime)
		{
			GorgonKeyTexture2D prev = (GorgonKeyTexture2D)keyValues.PreviousKey;

			if (InterpolationMode == TrackInterpolationMode.Linear)
			{
				GorgonKeyTexture2D next = (GorgonKeyTexture2D)keyValues.NextKey;
				Vector4 regionStart = new Vector4(prev.TextureRegion.X, prev.TextureRegion.Y, prev.TextureRegion.Width, prev.TextureRegion.Height);
				Vector4 regionEnd = new Vector4(next.TextureRegion.X, next.TextureRegion.Y, next.TextureRegion.Width, next.TextureRegion.Height);
				Vector4 result = Vector4.Zero;

				Vector4.Lerp(ref regionStart, ref regionEnd, unitTime, out result);
				return new GorgonKeyTexture2D(keyTime, prev.Value, new RectangleF(result.X, result.Y, result.Z, result.W));
			}

			return prev;
		}

		/// <summary>
		/// Function to apply the key value to the object properties.
		/// </summary>
		/// <param name="key">Key to apply to the properties.</param>
		protected internal override void ApplyKey(ref IKeyFrame key)
		{
			GorgonKeyTexture2D value = (GorgonKeyTexture2D)key;
			GorgonTexture2D currentTexture = _getTextureProperty(Animation.AnimationController.AnimatedObject);

			if (currentTexture != value.Value)
				_setTextureProperty(Animation.AnimationController.AnimatedObject, value.Value);
			_setTextureRegionProperty(Animation.AnimationController.AnimatedObject, value.TextureRegion);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTrackTexture2D" /> class.
		/// </summary>
		/// <param name="textureProperty">Property to alter the texture.</param>
		/// <param name="regionProperty">Property to alter the region.</param>
		internal GorgonTrackTexture2D(GorgonAnimatedProperty textureProperty, GorgonAnimatedProperty regionProperty)
			: base(textureProperty)
		{
			_getTextureProperty = BuildGetAccessor<GorgonTexture2D>();
			_setTextureProperty = BuildSetAccessor<GorgonTexture2D>();
			_getTextureRegionProperty = BuildGetAccessor<RectangleF>(regionProperty.Property.GetGetMethod());
			_setTextureRegionProperty = BuildSetAccessor<RectangleF>(regionProperty.Property.GetSetMethod());

			InterpolationMode = TrackInterpolationMode.None;
		}
		#endregion
	}
}