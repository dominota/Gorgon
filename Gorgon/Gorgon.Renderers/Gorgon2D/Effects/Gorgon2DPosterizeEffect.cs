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
// Created: Thursday, April 05, 2012 8:23:51 AM
// 
#endregion

using System;
using System.Runtime.InteropServices;
using System.Threading;
using Gorgon.Graphics.Core;
using Gorgon.Renderers.Properties;
using SharpDX;

namespace Gorgon.Renderers
{
	/// <summary>
	/// An effect that renders a posterized image.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This will perform a posterize operation, which will reduce the number of colors in the image.
	/// </para>
	/// </remarks>
	public class Gorgon2DPosterizedEffect
		: Gorgon2DEffect, IGorgon2DTextureDrawEffect
	{
		#region Value Types.
		/// <summary>
		/// Settings for the effect shader.
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		private struct Settings
		{
			private readonly int _posterizeAlpha;								// Flag to posterize the alpha channel.

			/// <summary>
			/// Exponent power for the posterization.
			/// </summary>
			public readonly float PosterizeExponent;
			/// <summary>
			/// Number of bits to reduce down to.
			/// </summary>
			public readonly int PosterizeBits;
			
			/// <summary>
			/// Property to return whether to posterize the alpha channel.
			/// </summary>
			public bool PosterizeAlpha => _posterizeAlpha != 0;

			/// <summary>
			/// Initializes a new instance of the <see cref="Settings"/> struct.
			/// </summary>
			/// <param name="useAlpha">if set to <b>true</b> [use alpha].</param>
			/// <param name="power">The power.</param>
			/// <param name="bits">The bits.</param>
			public Settings(bool useAlpha, float power, int bits)
			{
				_posterizeAlpha = Convert.ToInt32(useAlpha);
				PosterizeExponent = power;
				PosterizeBits = bits;
			}
		}
		#endregion

		#region Variables.
	    // Buffer for the posterize effect.
		private GorgonConstantBufferView _posterizeBuffer;
        // The shader used to render the effect.
	    private Gorgon2DShader<GorgonPixelShader> _shader;
        // The renderer batch state.
	    private Gorgon2DBatchState _batchState;
	    // Settings for the effect shader.
		private Settings _settings;									
	    // Flag to indicate that the parameters have been updated.
		private bool _isUpdated = true;								
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return whether to posterize the alpha channel.
		/// </summary>
		public bool UseAlpha
		{
			get => _settings.PosterizeAlpha;
		    set
			{
				if (_settings.PosterizeAlpha == value)
				{
					return;
				}

				_settings = new Settings(value, _settings.PosterizeExponent, _settings.PosterizeBits);
				_isUpdated = true;
			}
		}

	    /// <summary>
	    /// Property to set or return the exponent power for the effect.
	    /// </summary>
	    public float Power
	    {
	        get => _settings.PosterizeExponent;
	        set
	        {
	            // ReSharper disable once CompareOfFloatsByEqualityOperator
	            if (_settings.PosterizeExponent == value)
	            {
	                return;
	            }

	            if (value < 1e-6f)
	            {
	                value = 1e-6f;
	            }

	            _settings = new Settings(_settings.PosterizeAlpha, value, _settings.PosterizeBits);
	            _isUpdated = true;
	        }
	    }

		/// <summary>
		/// Property to set or return the number of bits to reduce down to for the effect.
		/// </summary>
		public int Bits
		{
			get => _settings.PosterizeBits;
		    set
			{
				if (_settings.PosterizeBits == value)
				{
					return;
				}

				if (value < 1)
				{
					value = 1;
				}

				_settings = new Settings(_settings.PosterizeAlpha, _settings.PosterizeExponent, value);
				_isUpdated = true;
			}
		}
		#endregion

		#region Methods.
        /// <summary>
        /// Function called when the effect is being initialized.
        /// </summary>
        /// <remarks>
        /// Use this method to set up the effect upon its creation.  For example, this method could be used to create the required shaders for the effect.
        /// <para>When creating a custom effect, use this method to initialize the effect.  Do not put initialization code in the effect constructor.</para>
        /// </remarks>
	    protected override void OnInitialize()
	    {
	        _posterizeBuffer = GorgonConstantBufferView.CreateConstantBuffer(Graphics, ref _settings, "Gorgon 2D Posterize Effect Constant Buffer");

	        _shader = PixelShaderBuilder
	                  .ConstantBuffer(_posterizeBuffer, 1)
	                  .Shader(CompileShader<GorgonPixelShader>(Resources.BasicSprite, "GorgonPixelShaderPosterize"))
	                  .Build();

	        _batchState = BatchStateBuilder
	                      .PixelShader(_shader)
	                      .Build();
	    }

	    /// <summary>
	    /// Function called to build a new (or return an existing) 2D batch state.
	    /// </summary>
	    /// <param name="passIndex">The index of the current rendering pass.</param>
	    /// <param name="statesChanged"><b>true</b> if the blend, raster, or depth/stencil state was changed. <b>false</b> if not.</param>
	    /// <returns>The 2D batch state.</returns>
	    protected override Gorgon2DBatchState OnGetBatchState(int passIndex, bool statesChanged)
	    {
	        if (statesChanged)
	        {
	            _batchState = BatchStateBuilder.Build();
	        }

	        return _batchState;
	    }

	    /// <summary>
		/// Function called before rendering begins.
		/// </summary>
		/// <returns>
		/// <b>true</b> to continue rendering, <b>false</b> to exit.
		/// </returns>
		protected override bool OnBeforeRender()
		{
			if (!_isUpdated)
			{
			    return true;
			}

		    _posterizeBuffer.Buffer.SetData(ref _settings);
		    _isUpdated = false;

		    return true;
		}

	    /// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
	    {
	        GorgonConstantBufferView buffer = Interlocked.Exchange(ref _posterizeBuffer, null);
	        Gorgon2DShader<GorgonPixelShader> shader = Interlocked.Exchange(ref _shader, null);

            buffer?.Dispose();
	        shader?.Dispose();
	    }

	    /// <summary>
	    /// Function to render the effect.
	    /// </summary>
	    /// <param name="texture">The texture containing the image to burn or dodge.</param>
	    /// <param name="region">[Optional] The region to draw the texture info.</param>
	    /// <param name="textureCoordinates">[Optional] The texture coordinates, in texels, to use when drawing the texture.</param>
	    /// <param name="samplerStateOverride">[Optional] An override for the current texture sampler.</param>
	    /// <param name="blendStateOverride">[Optional] The blend state to use when rendering.</param>
	    /// <param name="camera">[Optional] The camera used to render the image.</param>
	    /// <remarks>
	    /// <para>
	    /// Renders the specified <paramref name="texture"/> using 1 bit color.
	    /// </para>
	    /// <para>
	    /// If the <paramref name="region"/> parameter is omitted, then the texture will be rendered to the full size of the current render target.  If it is provided, then texture will be rendered to the
	    /// location specified, and with the width and height specified.
	    /// </para>
	    /// <para>
	    /// If the <paramref name="textureCoordinates"/> parameter is omitted, then the full size of the texture is rendered.
	    /// </para>
	    /// <para>
	    /// If the <paramref name="samplerStateOverride"/> parameter is omitted, then the <see cref="GorgonSamplerState.Default"/> is used.  When provided, this will alter how the pixel shader samples our
	    /// texture in slot 0.
	    /// </para>
	    /// <para>
	    /// If the <paramref name="blendStateOverride"/>, parameter is omitted, then the <see cref="GorgonBlendState.Default"/> is used. 
	    /// </para>
	    /// <para>
	    /// The <paramref name="camera"/> parameter is used to render the texture using a different view, and optionally, a different coordinate set.  
	    /// </para>
	    /// <para>
	    /// <note type="important">
	    /// <para>
	    /// For performance reasons, any exceptions thrown by this method will only be thrown when Gorgon is compiled as DEBUG.
	    /// </para>
	    /// </note>
	    /// </para>
	    /// </remarks>
	    public void RenderEffect(GorgonTexture2DView texture,
	                             RectangleF? region = null,
	                             RectangleF? textureCoordinates = null,
	                             GorgonSamplerState samplerStateOverride = null,
	                             GorgonBlendState blendStateOverride = null,
	                             Gorgon2DCamera camera = null)
	    {
	        RenderTexture(texture, region, textureCoordinates, samplerStateOverride, blendStateOverride, camera: camera);
	    }
	    #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="Gorgon2DPosterizedEffect" /> class.
        /// </summary>
        /// <param name="renderer">The renderer used to render this effect.</param>
        public Gorgon2DPosterizedEffect(Gorgon2D renderer)
	        : base(renderer, Resources.GOR2D_EFFECT_POSTERIZE, Resources.GOR2D_EFFECT_POSTERIZE_DESC, 1)
	    {
            _settings = new Settings(false, 1.0f, 8);
            Macros.Add(new GorgonShaderMacro("POSTERIZE_EFFECT"));
	    }
	    #endregion
	}
}