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
        : Gorgon2DEffect
    {
        #region Value Types.
        /// <summary>
        /// Settings for the effect shader.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct Settings
        {
            private readonly int _posterizeAlpha;                               // Flag to posterize the alpha channel.

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
        private GorgonPixelShader _posterizeShader;
        private Gorgon2DShaderState<GorgonPixelShader> _posterizeState;
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

            _posterizeShader = CompileShader<GorgonPixelShader>(Resources.BasicSprite, "GorgonPixelShaderPosterize");
            _posterizeState = PixelShaderBuilder
                      .ConstantBuffer(_posterizeBuffer, 1)
                      .Shader(_posterizeShader)
                      .Build();

            _batchState = BatchStateBuilder
                          .PixelShaderState(_posterizeState)
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
        /// <param name="camera">The currently active camera.</param>
        /// <param name="sizeChanged"><b>true</b> if the output size changed since the last render, or <b>false</b> if it's the same.</param>
        /// <remarks>
        /// <para>
        /// Applications can use this to set up common states and other configuration settings prior to executing the render passes. This is an ideal method to initialize and resize your internal render
        /// targets (if applicable).
        /// </para>
        /// </remarks>
        protected override void OnBeforeRender(GorgonRenderTargetView output, IGorgon2DCamera camera, bool sizeChanged)
        {
            if (Graphics.RenderTargets[0] != output)
            {
                Graphics.SetRenderTarget(output, Graphics.DepthStencilView);
            }

            if (!_isUpdated)
            {
                return;
            }

            _posterizeBuffer.Buffer.SetData(ref _settings);
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
        protected override void OnRenderPass(int passIndex, Action<int, int, Size2> renderMethod, GorgonRenderTargetView output) => renderMethod(passIndex, PassCount, new Size2(output.Width, output.Height));

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            GorgonConstantBufferView buffer = Interlocked.Exchange(ref _posterizeBuffer, null);
            GorgonPixelShader shader = Interlocked.Exchange(ref _posterizeShader, null);

            buffer?.Dispose();
            shader?.Dispose();
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
