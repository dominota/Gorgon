﻿#region MIT
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Monday, December 14, 2015 8:41:48 PM
// 
#endregion

using Gorgon.Math;
using DXGI = SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// Extension methods for SharpDX object conversion.
	/// </summary>
	internal static class SharpDXExtensions
	{
		/// <summary>
		/// Function to convert a <see cref="GorgonBox"/> to a D3D11 resource region.
		/// </summary>
		/// <param name="box">The box value to convert.</param>
		/// <returns>The D3D11 resource region.</returns>
		public static D3D11.ResourceRegion ToResourceRegion(this GorgonBox box)
		{
			return new D3D11.ResourceRegion(box.Left, box.Top, box.Front, box.Right, box.Bottom, box.Back);
		}

		/// <summary>
		/// Function to retrieve the underlying Direct 3D device from a <see cref="IGorgonVideoAdapter"/> interface.
		/// </summary>
		/// <param name="videoAdapter">The video adapter to use.</param>
		/// <returns>The Direct 3D video adapter.</returns>
		public static D3D11.Device1 D3DDevice(this IGorgonVideoAdapter videoAdapter)
		{
			return ((VideoAdapter)videoAdapter).D3DDevice;
		}

		/// <summary>
		/// Function to retrieve the underlying DXGI adapter from a <see cref="IGorgonVideoAdapter"/> interface.
		/// </summary>
		/// <param name="videoAdapter">The video adapter to use.</param>
		/// <returns>The DXGI adapter.</returns>
		public static DXGI.Adapter2 DXGIAdapter(this IGorgonVideoAdapter videoAdapter)
		{
			return ((VideoAdapter)videoAdapter).Adapter;
		}

		/// <summary>
		/// Function to convert a DXGI rational number to a Gorgon rational number.
		/// </summary>
		/// <param name="rational">The rational number to convert.</param>
		/// <returns>A Gorgon rational number.</returns>
		public static GorgonRationalNumber ToGorgonRational(this DXGI.Rational rational)
		{
			return new GorgonRationalNumber(rational.Numerator, rational.Denominator);
		}

		/// <summary>
		/// Function to convert a Gorgon rational number to a DXGI rational number.
		/// </summary>
		/// <param name="rational">The rational number to convert.</param>
		/// <returns>The DXGI ration number.</returns>
		public static DXGI.Rational ToDxGiRational(this GorgonRationalNumber rational)
		{
			return new DXGI.Rational(rational.Numerator, rational.Denominator);
		}

	    /// <summary>
	    /// Function to convert a GorgonVideoMode to a ModeDescription.
	    /// </summary>
	    /// <param name="mode">ModeDescription1 to convert.</param>
	    /// <returns>The new mode description.</returns>
	    public static DXGI.ModeDescription ToModeDesc(this GorgonVideoMode mode)
	    {
	        return new DXGI.ModeDescription
	               {
	                   Format = (DXGI.Format)mode.Format,
	                   Height = mode.Height,
	                   Scaling = (DXGI.DisplayModeScaling)mode.Scaling,
	                   Width = mode.Width,
	                   ScanlineOrdering = (DXGI.DisplayModeScanlineOrder)mode.ScanlineOrder,
	                   RefreshRate = mode.RefreshRate.ToDxGiRational()
	               };
	    }

	    /// <summary>
	    /// Function to convert a GorgonVideoMode to a ModeDescription1.
	    /// </summary>
	    /// <param name="mode">ModeDescription to convert.</param>
	    /// <returns>The new mode description.</returns>
	    public static DXGI.ModeDescription1 ToModeDesc1(this GorgonVideoMode mode)
	    {
	        return new DXGI.ModeDescription1
	               {
	                   Format = (DXGI.Format)mode.Format,
	                   Height = mode.Height,
	                   Scaling = (DXGI.DisplayModeScaling)mode.Scaling,
	                   Width = mode.Width,
	                   ScanlineOrdering = (DXGI.DisplayModeScanlineOrder)mode.ScanlineOrder,
	                   RefreshRate = mode.RefreshRate.ToDxGiRational(),
                       Stereo = mode.SupportsStereo
	               };
	    }

	    /// <summary>
	    /// Function to convert a DXGI ModeDescription1 to a <see cref="GorgonVideoMode"/>.
	    /// </summary>
	    /// <param name="mode">ModeDescription1 to convert.</param>
	    /// <returns>The new mode description.</returns>
	    public static GorgonVideoMode ToGorgonVideoMode(this DXGI.ModeDescription1 mode)
	    {
	        return new GorgonVideoMode(mode);
	    }

        /// <summary>
        /// Function to convert a DXGI ModeDescription to a <see cref="GorgonVideoMode"/>.
        /// </summary>
        /// <param name="mode">ModeDescription to convert.</param>
        /// <returns>The new mode description.</returns>
        public static GorgonVideoMode ToGorgonVideoMode(this DXGI.ModeDescription mode)
	    {
	        return new GorgonVideoMode(mode.ToModeDesc1());
	    }

        /// <summary>
        /// Function to convert a ModeDescription1 to a ModeDescription.
        /// </summary>
        /// <param name="mode">ModeDescription1 to convert.</param>
        /// <returns>The new mode description.</returns>
        public static DXGI.ModeDescription ToModeDesc(this DXGI.ModeDescription1 mode)
		{
			return new DXGI.ModeDescription
			       {
				       Format = mode.Format,
				       Height = mode.Height,
				       Scaling = mode.Scaling,
				       Width = mode.Width,
				       ScanlineOrdering = mode.ScanlineOrdering,
				       RefreshRate = mode.RefreshRate
			       };
		}

		/// <summary>
		/// Function to convert a ModeDescription to a ModeDescription1.
		/// </summary>
		/// <param name="mode">ModeDescription to convert.</param>
		/// <returns>The new mode description.</returns>
		public static DXGI.ModeDescription1 ToModeDesc1(this DXGI.ModeDescription mode)
		{
			return new DXGI.ModeDescription1
			       {
				       Format = mode.Format,
				       Height = mode.Height,
				       Scaling = mode.Scaling,
				       Width = mode.Width,
				       ScanlineOrdering = mode.ScanlineOrdering,
				       RefreshRate = mode.RefreshRate,
				       Stereo = false
			       };
		}

		/// <summary>
		/// Function to convert a <see cref="GorgonMultisampleInfo"/> to a DXGI multi sample description.
		/// </summary>
		/// <param name="samplingInfo">The Gorgon multi sample info to convert.</param>
		/// <returns>The DXGI multi sample description.</returns>
		public static DXGI.SampleDescription ToSampleDesc(this GorgonMultisampleInfo samplingInfo)
		{
			return new DXGI.SampleDescription(samplingInfo.Count, samplingInfo.Quality);
		}

		/// <summary>
		/// Function to convert a <see cref="IGorgonSwapChainInfo"/> to a DXGI swap chain description value.
		/// </summary>
		/// <param name="swapChainInfo">The swap chain info to convert.</param>
		/// <returns>A DXGI swap chain description.</returns>
		public static DXGI.SwapChainDescription1 ToSwapChainDesc(this GorgonSwapChainInfo swapChainInfo)
		{
			return new DXGI.SwapChainDescription1
			       {
				       BufferCount = 2,
				       AlphaMode = DXGI.AlphaMode.Unspecified,
				       Flags = DXGI.SwapChainFlags.AllowModeSwitch,
				       Format = (DXGI.Format)swapChainInfo.Format,
				       Width = swapChainInfo.Width,
				       Height = swapChainInfo.Height,
				       Scaling = swapChainInfo.StretchBackBuffer ? DXGI.Scaling.Stretch : DXGI.Scaling.None,
					   SampleDescription = ToSampleDesc(GorgonMultisampleInfo.NoMultiSampling),
					   SwapEffect = swapChainInfo.UseFlipMode ? DXGI.SwapEffect.FlipSequential : DXGI.SwapEffect.Discard,
					   Usage = DXGI.Usage.RenderTargetOutput
			       };
		}

		/// <summary>
		/// Function to convert a gorgon vertex buffer binding to a D3D 11 vertex buffer binding.
		/// </summary>
		/// <param name="binding">The binding to convert.</param>
		/// <returns>A new D3D 11 vertex buffer binding.</returns>
		public static D3D11.VertexBufferBinding ToVertexBufferBinding(this GorgonVertexBufferBinding binding)
		{
			return new D3D11.VertexBufferBinding(binding.VertexBuffer?.NativeBuffer, binding.Stride, binding.Offset);
		}

		/// <summary>
		/// Function to convert a gorgon raster state info to a D3D raster state desc.
		/// </summary>
		/// <param name="state">The state to convert.</param>
		/// <returns>A new D3D 11 raster state desc.</returns>
		public static D3D11.RasterizerStateDescription1 ToRasterStateDesc1(this GorgonRasterState state)
		{
			return new D3D11.RasterizerStateDescription1
			       {
						CullMode = state.CullMode,
						DepthBias = state.DepthBias,
						IsFrontCounterClockwise = state.IsFrontCounterClockwise,
						FillMode = state.FillMode,
						DepthBiasClamp = state.DepthBiasClamp,
						SlopeScaledDepthBias = state.SlopeScaledDepthBias,
						ForcedSampleCount = state.ForcedUavSampleCount,
						IsAntialiasedLineEnabled = state.IsAntialiasedLineEnabled,
						IsDepthClipEnabled = state.IsDepthClippingEnabled,
						IsMultisampleEnabled = state.IsMultisamplingEnabled,
						IsScissorEnabled = state.IsScissorClippingEnabled
			       };
		}

		/// <summary>
		/// Function to convert a gorgon sampler state info to a D3D sampler state desc.
		/// </summary>
		/// <param name="state">The state to convert.</param>
		/// <returns>A new D3D 11 sampler state desc.</returns>
		public static D3D11.SamplerStateDescription ToSamplerStateDesc(this GorgonSamplerState state)
		{
			return new D3D11.SamplerStateDescription
			       {
				       Filter = state.Filter,
				       AddressU = (D3D11.TextureAddressMode)state.WrapU,
				       AddressV = (D3D11.TextureAddressMode)state.WrapV,
				       AddressW = (D3D11.TextureAddressMode)state.WrapW,
				       BorderColor = state.BorderColor.ToRawColor4(),
				       ComparisonFunction = state.ComparisonFunction,
				       MaximumAnisotropy = state.MaxAnisotropy,
				       MaximumLod = state.MaximumLevelOfDetail,
				       MinimumLod = state.MinimumLevelOfDetail,
				       MipLodBias = state.MipLevelOfDetailBias
			       };
		}

		/// <summary>
		/// FUnction to convert a Gorgon render target blend state info to a D3D render target blend state 1 desc.
		/// </summary>
		/// <param name="state">The state convert.</param>
		/// <returns>A new D3D blend state 1 description. </returns>
		public static D3D11.RenderTargetBlendDescription1 ToRenderTargetBlendStateDesc1(this GorgonBlendState state)
		{
			return new D3D11.RenderTargetBlendDescription1
			       {
				       LogicOperation = state.LogicOperation,
				       SourceAlphaBlend = state.SourceAlphaBlend,
				       AlphaBlendOperation = state.AlphaBlendOperation,
				       SourceBlend = state.SourceColorBlend,
				       DestinationAlphaBlend = state.DestinationAlphaBlend,
				       BlendOperation = state.ColorBlendOperation,
				       DestinationBlend = state.DestinationColorBlend,
				       IsBlendEnabled = state.IsBlendingEnabled,
				       IsLogicOperationEnabled = state.IsLogicalOperationEnabled,
				       RenderTargetWriteMask = state.WriteMask
			       };
		}

		/// <summary>
		/// Function to convert a gorgon depth/stencil state info to a D3D depth/stencil state desc.
		/// </summary>
		/// <param name="state">The state to convert.</param>
		/// <returns>A new D3D 11 depth/stencil state desc.</returns>
		public static D3D11.DepthStencilStateDescription ToDepthStencilStateDesc(this GorgonDepthStencilState state)
		{
			return new D3D11.DepthStencilStateDescription
			       {
				       StencilReadMask = state.StencilReadMask,
				       DepthWriteMask = state.IsDepthWriteEnabled ? D3D11.DepthWriteMask.All : D3D11.DepthWriteMask.Zero,
				       StencilWriteMask = state.StencilWriteMask,
				       DepthComparison = state.DepthComparison,
				       IsStencilEnabled = state.IsStencilEnabled,
				       IsDepthEnabled = state.IsDepthEnabled,
				       BackFace = new D3D11.DepthStencilOperationDescription
				                  {
					                  Comparison = state.BackFaceStencilOp.Comparison,
					                  FailOperation = state.BackFaceStencilOp.FailOperation,
					                  PassOperation = state.BackFaceStencilOp.PassOperation,
					                  DepthFailOperation = state.BackFaceStencilOp.DepthFailOperation
				                  },
				       FrontFace = new D3D11.DepthStencilOperationDescription
				                   {
					                   Comparison = state.FrontFaceStencilOp.Comparison,
					                   FailOperation = state.FrontFaceStencilOp.FailOperation,
					                   PassOperation = state.FrontFaceStencilOp.PassOperation,
					                   DepthFailOperation = state.FrontFaceStencilOp.DepthFailOperation
				                   }
			       };
		}
	}
}
