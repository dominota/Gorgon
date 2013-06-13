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
// Created: Friday, May 31, 2013 9:34:22 AM
// 
#endregion

using System;
using GorgonLibrary.Diagnostics;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
    /// <summary>
    /// A render target bound to a 1D texture.
    /// </summary>
    public class GorgonRenderTarget1D
        : GorgonTexture2D
	{
		#region Variables.
        private GorgonRenderTargetView _defaultRenderTargetView;    // The default render target view for this render target.
	    #endregion

		#region Properties.
	    /// <summary>
	    /// Property to return the settings for this render target.
	    /// </summary>
	    public new GorgonRenderTarget1DSettings Settings
	    {
		    get;
		    private set;
	    }

	    /// <summary>
		/// Property to return the default viewport for this render target.
		/// </summary>
		public GorgonViewport Viewport
		{
			get;
			private set;
		}

        /// <summary>
        /// Property to return the default depth/stencil buffer attached to this render target.
        /// </summary>
        public GorgonDepthStencil1D DepthStencilBuffer
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to create the depth/stencil buffer for the target.
        /// </summary>
        private void CreateDepthStencilBuffer()
        {
			// Create the internal depth/stencil.
			if (Settings.DepthStencilFormat == BufferFormat.Unknown)
			{
				return;
			}

			Gorgon.Log.Print("GorgonRenderTarget '{0}': Creating internal depth/stencil...", LoggingLevel.Verbose, Name);

			if (DepthStencilBuffer == null)
			{
				DepthStencilBuffer = new GorgonDepthStencil1D(Graphics,
															Name + "_Internal_DepthStencil_" + Guid.NewGuid(),
															new GorgonDepthStencil1DSettings
															{
																Format = Settings.DepthStencilFormat,
																Width = Settings.Width,
																ArrayCount = Settings.ArrayCount,
																MipCount = Settings.MipCount
															});
			}
			else
			{
				DepthStencilBuffer.Settings.Format = Settings.DepthStencilFormat;
				DepthStencilBuffer.Settings.Width = Settings.Width;
			}

#if DEBUG
			Graphics.Output.ValidateDepthStencilSettings(DepthStencilBuffer.Settings);
#endif

			DepthStencilBuffer.Initialize(null);
			DepthStencilBuffer.RenderTarget = this;
		}

        /// <summary>
        /// Function to clean up any internal resources.
        /// </summary>
        protected override void CleanUpResource()
        {
            Gorgon.Log.Print("GorgonRenderTarget '{0}': Releasing D3D11 render target view...", LoggingLevel.Intermediate, Name);
            GorgonRenderStatistics.RenderTargetCount--;
            GorgonRenderStatistics.RenderTargetSize -= SizeInBytes;

            if (DepthStencilBuffer != null)
            {
                Gorgon.Log.Print("GorgonRenderTarget '{0}': Releasing internal depth stencil...",
                                    LoggingLevel.Verbose,
                                    Name);
                DepthStencilBuffer.RenderTarget = null;
                DepthStencilBuffer.Dispose();
                DepthStencilBuffer = null;
            }

            base.CleanUpResource();
        }

        /// <summary>
        /// Function to initialize the texture with optional initial data.
        /// </summary>
        /// <param name="initialData">Data used to populate the image.</param>
        protected override void OnInitialize(GorgonImageData initialData)
        {
            if ((Settings.Format != BufferFormat.Unknown) && (Settings.TextureFormat == BufferFormat.Unknown))
            {
                Settings.TextureFormat = Settings.Format;
            }

            var desc = new D3D.Texture1DDescription
            {
                ArraySize = Settings.ArrayCount,
                Format = (SharpDX.DXGI.Format)Settings.TextureFormat,
                Width = Settings.Width,
                MipLevels = Settings.MipCount,
                BindFlags = GetBindFlags(false, true),
                Usage = D3D.ResourceUsage.Default,
                CpuAccessFlags = D3D.CpuAccessFlags.None,
                OptionFlags = D3D.ResourceOptionFlags.None
            };

            Gorgon.Log.Print("{0} {1}: Creating 1D render target texture...", LoggingLevel.Verbose, GetType().Name, Name);

            // Create the texture.
            D3DResource = initialData != null
                              ? new D3D.Texture1D(Graphics.D3DDevice, desc, initialData.GetDataBoxes())
                              : new D3D.Texture1D(Graphics.D3DDevice, desc);

            // Create the default render target view.
            _defaultRenderTargetView = CreateRenderTargetView(Settings.Format, 0, 0, 1);

            GorgonRenderStatistics.RenderTargetCount++;
            GorgonRenderStatistics.RenderTargetSize += SizeInBytes;

            CreateDepthStencilBuffer();

            // Set default viewport.
            Viewport = new GorgonViewport(0, 0, Settings.Width, 1.0f, 0.0f, 1.0f);
        }

        /// <summary>
        /// Function to create a new render target view.
        /// </summary>
        /// <param name="format">Format of the new render target view.</param>
        /// <param name="mipSlice">Mip level index to use in the view.</param>
        /// <param name="arrayIndex">Array index to use in the view.</param>
        /// <param name="arrayCount">Number of array indices to use.</param>
        /// <returns>A render target view.</returns>
        /// <remarks>Use this to create a render target view that can bind a portion of the target to the pipeline as a render target.
        /// <para>The <paramref name="format"/> for the render target view does not have to be the same as the render target backing texture, and if the format is set to Unknown, then it will 
        /// use the format from the texture.</para>
        /// </remarks>
        /// <exception cref="GorgonLibrary.GorgonException">Thrown when the render target view could not be created.</exception>
        public GorgonRenderTargetTextureView CreateRenderTargetView(BufferFormat format, int mipSlice, int arrayIndex,
                                                               int arrayCount)
        {
            return OnCreateRenderTargetView(format, mipSlice, arrayIndex, arrayCount);
        }

        /// <summary>
        /// Function to clear the render target.
        /// </summary>
        /// <param name="color">Color used to clear the render target.</param>
        /// <remarks>This will only clear the render target.  Only the default view will be cleared, any extra views will not be cleared. Any attached depth/stencil buffer will remain untouched.</remarks>
        public void Clear(GorgonColor color)
        {
            _defaultRenderTargetView.Clear(color);
        }

        /// <summary>
        /// Function to clear the render target and an attached depth buffer.
        /// </summary>
        /// <param name="color">Color used to clear the render target.</param>
        /// <param name="depthValue">Value used to clear the depth buffer.</param>
        /// <remarks>This will only clear the render target.  Only the default view will be cleared, any extra views will not be cleared. Any stencil buffer will remain untouched.</remarks>
        public void Clear(GorgonColor color, float depthValue)
        {
            Clear(color);

            if ((DepthStencilBuffer != null)
                && (DepthStencilBuffer.FormatInformation.HasDepth))
            {
                DepthStencilBuffer.ClearDepth(depthValue);
            }
        }

        /// <summary>
        /// Function to clear the render target and an attached depth/stencil buffer.
        /// </summary>
        /// <param name="color">Color used to clear the render target.</param>
        /// <param name="depthValue">Value used to clear the depth buffer.</param>
        /// <param name="stencilValue">Value used to clear the stencil buffer.</param>
        /// <remarks>This will only clear the render target.  Only the default view will be cleared, any extra views will not be cleared.</remarks>
        public void Clear(GorgonColor color, float depthValue, int stencilValue)
        {
            if ((DepthStencilBuffer != null) && (DepthStencilBuffer.FormatInformation.HasDepth) && (!DepthStencilBuffer.FormatInformation.HasStencil))
            {
                Clear(color, depthValue);
                return;
            }

            Clear(color);

	        if ((DepthStencilBuffer == null) || (!DepthStencilBuffer.FormatInformation.HasDepth) ||
	            (!DepthStencilBuffer.FormatInformation.HasStencil))
	        {
		        return;
	        }

	        DepthStencilBuffer.ClearDepth(depthValue);
	        DepthStencilBuffer.ClearStencil(stencilValue);
        }

        /// <summary>
        /// Function to retrieve the render target view for a render target.
        /// </summary>
        /// <param name="target">Render target to evaluate.</param>
        /// <returns>The render target view for the swap chain.</returns>
        public static GorgonRenderTargetView ToRenderTargetView(GorgonRenderTarget1D target)
        {
            return target == null ? null : target._defaultRenderTargetView;
        }

        /// <summary>
        /// Implicit operator to return the render target view for a render target
        /// </summary>
        /// <param name="target">Render target to evaluate.</param>
        /// <returns>The render target view for the swap chain.</returns>
        public static implicit operator GorgonRenderTargetView(GorgonRenderTarget1D target)
        {
            return target == null ? null : target._defaultRenderTargetView;
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonRenderTarget1D"/> class.
        /// </summary>
        /// <param name="graphics">The graphics interface that created this object.</param>
        /// <param name="name">The name of the render target.</param>
        /// <param name="settings">Settings to apply to the render target.</param>
        internal GorgonRenderTarget1D(GorgonGraphics graphics, string name, GorgonRenderTarget1DSettings settings)
            : base(graphics, name, settings)
        {
	        Settings = settings;
        }
        #endregion
    }
}