﻿#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Thursday, August 01, 2013 8:10:32 PM
// 
#endregion

using System;
using Gorgon.Graphics.Imaging;
using Gorgon.Native;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using DX = SharpDX;

namespace Gorgon.Graphics
{
#warning This object needs a serious rewrite.
	/// <summary>
	/// This object contains data about a locked sub resource in a texture.
	/// </summary>
	/// <remarks>This object is returned from the Lock method of the texture.  Disposing of this object will unlock the texture sub resource.
	/// <para>The lock will allow the data in the sub resource to be accessible to the CPU and will keep the GPU from accessing the texture.  Because of this, it is important to only 
	/// keep the lock open for as short a duration as possible.</para>
	/// <para>If a texture is disposed while a lock is open, that lock will be unlocked and become invalid.</para>
	/// <seealso cref="Gorgon.Graphics.GorgonTexture1D.Lock"/>
	/// <seealso cref="Gorgon.Graphics.GorgonTexture2D.Lock"/>
	/// <seealso cref="Gorgon.Graphics.GorgonTexture3D.Lock"/>
	/// </remarks>
	public class GorgonTextureLockData
        : IGorgonImageBuffer, IDisposable 
    {
		#region Variables.
		// Flag to indicate that the object was disposed.
		private bool _disposed;                             
		// Cache that contains this lock.
		private readonly GorgonTextureLockCache _cache;     
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the graphics context that owns this texture lock.
        /// </summary>
        public GorgonGraphics Graphics
        {
            get;
        }

        /// <summary>
        /// Property to return the texture is locked.
        /// </summary>
        public GorgonTexture_OLDEN Texture
        {
            get;
        }

		public Format Format
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public int Width
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public int Height
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public int Depth
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public int MipLevel
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public int ArrayIndex
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public int DepthSliceIndex
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public IGorgonPointer Data
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public GorgonPitchLayout PitchInformation
		{
			get
			{
				throw new NotImplementedException();
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTextureLockData"/> class.
		/// </summary>
		/// <param name="graphics">The graphics context that owns this lock.</param>
		/// <param name="texture">The texture that owns this lock.</param>
		/// <param name="cache">Lock cache that will contain this lock.</param>
		/// <param name="data">The data returned from the lock.</param>
		/// <param name="mipLevel">The mip level of the sub resource.</param>
		/// <param name="arrayIndex">Array index of the sub resource.</param>
		internal GorgonTextureLockData(GorgonGraphics graphics, GorgonTexture_OLDEN texture, GorgonTextureLockCache cache, DX.DataBox data, int mipLevel, int arrayIndex)
			: base()
        {
/*            Graphics = graphics;
            Texture = texture;
            _cache = cache;

            Width = Texture.Settings.Width;
            Height = Texture.Settings.Height;
            Depth = Texture.Settings.Depth;

            // Calculate the current size at the given mip level.
            for (int mip = 0; mip < mipLevel; ++mip)
            {
                if (Width > 1)
                {
                    Width >>= 1;
                }
                if (Height > 1)
                {
                    Height >>= 1;
                }
                if (Depth > 1)
                {
                    Depth >>= 1;
                }
            }

			PitchInformation = new GorgonPitchLayout(data.RowPitch, data.SlicePitch);
            Data = new GorgonPointerAlias(data.DataPointer, data.SlicePitch);*/
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if ((disposing) && (Graphics != null) && (Texture != null))
            {
                _cache?.Unlock(this);
            }

            _disposed = true;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

		public void CopyTo(IGorgonImageBuffer buffer, RawRectangle? sourceRegion = default(RawRectangle?), int destX = 0, int destY = 0)
		{
			throw new NotImplementedException();
		}
		#endregion
	}
}