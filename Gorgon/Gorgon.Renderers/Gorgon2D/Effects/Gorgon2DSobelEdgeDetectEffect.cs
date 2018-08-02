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
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Renderers.Properties;
using DX = SharpDX;

namespace Gorgon.Renderers
{
	/// <summary>
	/// An effect that renders the edges of an image with Sobel edge detection.
	/// </summary>
	public class Gorgon2DSobelEdgeDetectEffect
		: Gorgon2DEffect
	{
		#region Value Types.
		/// <summary>
		/// Settings for the effect shader.
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 32)]
		private struct Settings
		{
		    // Texel size and threshold.
			private readonly DX.Vector4 _texelThreshold;			

			/// <summary>
			/// Line color.
			/// </summary>
			public readonly GorgonColor LineColor;

			/// <summary>
			/// Property to return the size of a texel.
			/// </summary>
			public DX.Vector2 TexelSize => (DX.Vector2)_texelThreshold;

			/// <summary>
			/// Property to return the threshold for the effect.
			/// </summary>
			public float Threshold => _texelThreshold.Z;

			/// <summary>
			/// Initializes a new instance of the <see cref="Settings"/> struct.
			/// </summary>
			/// <param name="linecolor">The linecolor.</param>
			/// <param name="texelSize">Size of the texel.</param>
			/// <param name="threshold">The threshold.</param>
			public Settings(GorgonColor linecolor, DX.Vector2 texelSize, float threshold)
			{
				LineColor = linecolor;
				_texelThreshold = new DX.Vector4(texelSize, threshold, 0);
			}
		}
		#endregion

		#region Variables.
	    // Buffer for the sobel edge detection.
		private GorgonConstantBufferView _sobelBuffer;
        // The pixel shader for the effect.
	    private Gorgon2DShader<GorgonPixelShader> _shader;
        // The batch state used for the effect.
	    private Gorgon2DBatchState _batchState;
	    // Settings for the effect.
		private Settings _settings;								
	    // Flag to indicate that the parameters have been updated.
		private bool _isUpdated = true;
        // The thickness of the lines.
	    private float _lineThickness = 1.0f;
		#endregion

		#region Properties.
        /// <summary>
        /// Property to set or return the relative thickness of the line.
        /// </summary>
	    public float LineThickness
	    {
	        get => _lineThickness;
	        set
	        {
	            if (_lineThickness == value)
	            {
	                return;
	            }

	            _settings = new Settings(_settings.LineColor,
	                                     new DX.Vector2(_settings.TexelSize.X * value, _settings.TexelSize.Y * value),
	                                     _settings.Threshold);
	            _lineThickness = value;
	            _isUpdated = true;
	        }
	    }

		/// <summary>
		/// Property to set or return the threshold value for the edges.
		/// </summary>
		public float EdgeThreshold
		{
			get => _settings.Threshold;
		    set
			{
				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (_settings.Threshold == value)
				{
					return;
				}

				if (value < 0)
				{
					value = 0.0f;
				}
				if (value > 1.0f)
				{
					value = 1.0f;
				}

				_settings = new Settings(_settings.LineColor, _settings.TexelSize, value);
				_isUpdated = true;
			}
		}

		/// <summary>
		/// Property to set or return the color for the edges.
		/// </summary>
		public GorgonColor LineColor
		{
			get => _settings.LineColor;
		    set
			{
				if (_settings.LineColor.Equals(value))
				{
					return;
				}

				_settings = new Settings(value, _settings.TexelSize, _settings.Threshold);
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
	        _sobelBuffer = GorgonConstantBufferView.CreateConstantBuffer(Graphics, ref _settings, "Gogron 2D Sobel Edge Detect Filter Effect Constant Buffer");

	        _shader = PixelShaderBuilder
	                  .ConstantBuffer(_sobelBuffer, 1)
	                  .Shader(CompileShader<GorgonPixelShader>(Resources.BasicSprite, "GorgonPixelShaderSobelEdge"))
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
	    /// Function called prior to rendering.
	    /// </summary>
	    /// <param name="output">The final render target that will receive the rendering from the effect.</param>
	    /// <param name="sizeChanged"><b>true</b> if the output size changed since the last render, or <b>false</b> if it's the same.</param>
	    /// <remarks>
	    /// <para>
	    /// Applications can use this to set up common states and other configuration settings prior to executing the render passes. This is an ideal method to initialize and resize your internal render
	    /// targets (if applicable).
	    /// </para>
	    /// </remarks>
	    protected override void OnBeforeRender(GorgonRenderTargetView output, bool sizeChanged)
	    {

		    if (Graphics.RenderTargets[0] != output)
		    {
		        Graphics.SetRenderTarget(output, Graphics.DepthStencilView);
		        _isUpdated = true;
		    }

            if (sizeChanged)
            {
		        _settings = new Settings(_settings.LineColor, new DX.Vector2((1.0f / output.Width) * LineThickness, (1.0f / output.Height) * LineThickness), _settings.Threshold);
		        _isUpdated = true;
		    }

		    if (!_isUpdated)
		    {
		        return;
		    }
            
		    _sobelBuffer.Buffer.SetData(ref _settings);
			_isUpdated = false;
		}

	    /// <summary>
	    /// Function called to render a single effect pass.
	    /// </summary>
	    /// <param name="passIndex">The index of the pass being rendered.</param>
	    /// <param name="renderMethod">The method used to render a scene for the effect.</param>
	    /// <param name="output">The render target that will receive the final render data.</param>
	    /// <remarks>
	    /// <para>
	    /// Applications must implement this in order to see any results from the effect.
	    /// </para>
	    /// </remarks>
	    protected override void OnRenderPass(int passIndex, Action<int, int, DX.Size2> renderMethod, GorgonRenderTargetView output)
	    {
	        renderMethod(passIndex, PassCount, new DX.Size2(output.Width, output.Height));
	    }

	    /// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
	    {
	        GorgonConstantBufferView buffer = Interlocked.Exchange(ref _sobelBuffer, null);
	        Gorgon2DShader<GorgonPixelShader> shader = Interlocked.Exchange(ref _shader, null);

            buffer?.Dispose();
            shader?.Dispose();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Gorgon2DSobelEdgeDetectEffect"/> class.
		/// </summary>
		/// <param name="renderer">The renderer used to render this effect.</param>
		public Gorgon2DSobelEdgeDetectEffect(Gorgon2D renderer)
			: base(renderer, Resources.GOR2D_EFFECT_SHARPEMBOSS, Resources.GOR2D_EFFECT_SHARPEMBOSS_DESC, 1)
		{
		    _settings = new Settings(Color.Black, DX.Vector2.Zero, 0.75f);
            Macros.Add(new GorgonShaderMacro("SOBEL_EDGE_EFFECT"));
		}
		#endregion
	}
}
