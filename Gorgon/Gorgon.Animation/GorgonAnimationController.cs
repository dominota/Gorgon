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
// Created: Monday, September 3, 2012 7:46:31 PM
// 
#endregion

using System;
using System.Collections.Generic;
using DX = SharpDX;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Timing;

namespace Gorgon.Animation
{
    /// <summary>
	/// Base class for applying animations to an object.
	/// </summary>
	/// <typeparam name="T">The type of object that this controller will use.  The type passed in must be a reference type (i.e. a class).</typeparam>
	/// <remarks>
	/// <para>
	/// A controller will update the object properties over a certain time frame (or continuously if looped) using a <see cref="IGorgonAnimation"/>.
	/// </para>
	/// <para>
	/// This controller will advance the time for an animation, and coordinate the changes from interpolation (if supported) between <see cref="IGorgonKeyFrame"/> items on a <see cref="IGorgonTrack{T}"/>.
	/// The values from the animation will then by applied to the object properties.
	/// </para>
	/// <para>
	/// Applications can force the playing animation to jump to a specific <see cref="Time"/>, or increment the time step smoothly using the <see cref="Update"/> method.
	/// </para>
	/// <para>
	/// <note type="important">
	/// Please note that this is an abstract class. Applications will provide specific controllers for specific types.
	/// </note>
	/// </para>
    /// <para>
    /// <note type="information">
    /// Because this is a base class, not all controllers will support all track types, or even components of a track key frame.
    /// </note>
    /// </para>
	/// </remarks>
	/// <seealso cref="IGorgonAnimation"/>
	public abstract class GorgonAnimationController<T>
        where T : class
	{
		#region Constants.
		// Animation data chunk.
		internal const string AnimationChunk = "ANIMDATA";

		/// <summary>
		/// Version header for the animation.
		/// </summary>
		public const string AnimationVersion = "GORANM10";
		#endregion

        #region Variables.
        // The time index.
	    private float _time;
        // The loop count for the current animation.
        private int _loopCount;
	    // The object that is to be animated.
	    private T _animatedObject;
        #endregion

		#region Properties.
		/// <summary>
		/// Property to return the currently playing animation.
		/// </summary>
		public IGorgonAnimation CurrentAnimation
		{
			get;
			private set;
		}

        /// <summary>
        /// Property to set or return the current time index.
        /// </summary>
	    public float Time
        {
            get => _time;
            set
            {
                if (_time.EqualsEpsilon(value))
                {
                    return;
                }

                _time = value.Max(0);

                if (CurrentAnimation == null)
                {
                    return;
                }

                if (((CurrentAnimation.IsLooped) && (_time > CurrentAnimation.Length)) || (value < 0))
                {
                    // Loop the animation.
                    if ((CurrentAnimation.LoopCount != 0) && (_loopCount == CurrentAnimation.LoopCount))
                    {
                        return;
                    }

                    _loopCount++;
                    _time = _time % CurrentAnimation.Length;

                    if (CurrentAnimation.Speed < 0)
                    {
                        _time += CurrentAnimation.Length;
                    }

                    NotifyAnimation();
                    return;
                }

                if (_time < 0)
                {
                    _time = 0;
                }

                if (_time > CurrentAnimation.Length)
                {
                    _time = CurrentAnimation.Length;
                }

                NotifyAnimation();
            }
	    }
		#endregion

		#region Methods.
        /// <summary>
        /// Function to update the animation.
        /// </summary>
	    private void NotifyAnimation()
	    {
            // Update each track.
	        if (TrackKeyProcessor.TryUpdateVector3(CurrentAnimation, CurrentAnimation.PositionTrack, _time, out DX.Vector3 posValue))
	        {
                OnPositionUpdate(_animatedObject, posValue);
	        }

	        if (TrackKeyProcessor.TryUpdateVector3(CurrentAnimation, CurrentAnimation.ScaleTrack, _time, out DX.Vector3 scaleValue))
	        {
                OnScaleUpdate(_animatedObject, scaleValue);
	        }

	        if (TrackKeyProcessor.TryUpdateVector3(CurrentAnimation, CurrentAnimation.RotationTrack, _time, out DX.Vector3 rotValue))
	        {
	            OnRotationUpdate(_animatedObject, rotValue);
	        }

	        if (TrackKeyProcessor.TryUpdateColor(CurrentAnimation, CurrentAnimation.ColorTrack, _time, out GorgonColor colorValue))
	        {
                OnColorUpdate(_animatedObject, colorValue);
	        }

	        if (TrackKeyProcessor.TryUpdateRectBounds(CurrentAnimation, CurrentAnimation.RectBoundsTrack, _time, out DX.RectangleF rectValue))
	        {
	            OnRectBoundsUpdate(_animatedObject, rectValue);
	        }

	        if (TrackKeyProcessor.TryUpdateTexture2D(CurrentAnimation,
	                                                 _time,
                                                     CurrentAnimation.Texture2DTrack,
	                                                 out GorgonTexture2DView texture,
	                                                 out DX.RectangleF textureCoordinates,
	                                                 out int textureArrayIndex))
	        {
                OnTexture2DUpdate(_animatedObject, texture, textureCoordinates, textureArrayIndex);
	        }
	    }

	    /// <summary>
	    /// Function called when a rectangle boundary needs to be updated on the object.
	    /// </summary>
	    /// <param name="animObject">The object being animated.</param>
	    /// <param name="bounds">The new bounds.</param>
	    protected abstract void OnRectBoundsUpdate(T animObject, DX.RectangleF bounds);

	    /// <summary>
	    /// Function called when a texture needs to be updated on the object.
	    /// </summary>
	    /// <param name="animObject">The object being animated.</param>
	    /// <param name="texture">The texture to switch to.</param>
	    /// <param name="textureCoordinates">The new texture coordinates to apply.</param>
	    /// <param name="textureArrayIndex">The texture array index.</param>
	    protected abstract void OnTexture2DUpdate(T animObject, GorgonTexture2DView texture, DX.RectangleF textureCoordinates, int textureArrayIndex);

        /// <summary>
        /// Function called when a position needs to be updated on the object.
        /// </summary>
        /// <param name="animObject">The object being animated.</param>
        /// <param name="position">The new position.</param>
	    protected abstract void OnPositionUpdate(T animObject, DX.Vector3 position);

	    /// <summary>
	    /// Function called when a scale needs to be updated on the object.
	    /// </summary>
	    /// <param name="animObject">The object being animated.</param>
	    /// <param name="scale">The new scale.</param>
	    protected abstract void OnScaleUpdate(T animObject, DX.Vector3 scale);

	    /// <summary>
	    /// Function called when the angle of rotation needs to be updated on the object.
	    /// </summary>
	    /// <param name="animObject">The object being animated.</param>
	    /// <param name="rotation">The new angle of rotation, in degrees, on the x, y and z axes.</param>
	    protected abstract void OnRotationUpdate(T animObject, DX.Vector3 rotation);

        /// <summary>
        /// Function called when the color needs to be updated on the object.
        /// </summary>
        /// <param name="animObject">The object being animated.</param>
        /// <param name="color">The new color.</param>
	    protected abstract void OnColorUpdate(T animObject, GorgonColor color);

		/// <summary>
		/// Function to update the currently playing animation time and bound properties.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This will update the animation time using the <see cref="GorgonTiming.Delta">Delta</see> time.  Note that the animation time is not affected by
		/// <see cref="GorgonTiming.ScaledDelta">ScaledDelta</see>.
		/// </para>
		/// <para>
		/// Users should call this method once per frame in order to update the current state of the playing (by calling <see cref="Play(T,IGorgonAnimation)"/> animation.  If no animation is playing,
		/// then this method will do nothing.
		/// </para>
		/// </remarks>
		/// <seealso cref="GorgonTiming"/>
		public void Update()
		{
		    if (CurrentAnimation == null)
		    {
		        return;
		    }

		    float lastTime = Time;
			float increment = (CurrentAnimation.Speed * GorgonTiming.Delta);

			// Push the animation time forward (or backward, depending on the Speed modifier).
		    Time += increment;
		    lastTime += increment;

		    // If we're not looping, put the animation into a stopped state.
			if ((CurrentAnimation.IsLooped) || ((lastTime < CurrentAnimation.Length) && (lastTime > 0)))
			{
				return;
			}

			Stop();
		}
		
		/// <summary>
		/// Function to set an animation playing on an object.
		/// </summary>
		/// <param name="animatedObject">The object to apply the animation onto.</param>
		/// <param name="animation">The <see cref="IGorgonAnimation"/> to play.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="animation"/> or <paramref name="animatedObject"/> parameters are <b>null</b>.</exception>
		/// <exception cref="KeyNotFoundException">Thrown when the animation could not be found in the collection.</exception>
		/// <remarks>
		/// <para>
		/// Applications should call this method to start an animation for an object. Otherwise, no animation will play when <see cref="Update"/> is called.
		/// </para>
		/// </remarks>
		public void Play(T animatedObject, IGorgonAnimation animation)
		{
			if (animation == null)
			{
				throw new ArgumentNullException(nameof(animation));
			}

			// This animation is already playing.
		    if (animation == CurrentAnimation)
		    {
		        return;
		    }

		    // Stop the current animation.
		    if (CurrentAnimation != null)
		    {
		        Stop();
		    }

		    if (Time > animation.Length)
		    {
		        Time = animation.Length;
		    }

		    if (animation.Speed < 0)
		    {
		        Time = animation.Length - Time;
		    }
            
		    _animatedObject = animatedObject ?? throw new ArgumentNullException(nameof(animatedObject));
			CurrentAnimation = animation;
		    _loopCount = 0;

			// Update to the first frame.
		    NotifyAnimation();
		}

		/// <summary>
		/// Function to stop the currently playing animation.
		/// </summary>
		public void Stop()
		{
			if (CurrentAnimation == null)
			{
				return;
			}

		    _loopCount = 0;
			_animatedObject = null;
			CurrentAnimation = null;
		}
		#endregion
	}
}
