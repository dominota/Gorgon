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
// Created: Saturday, February 23, 2013 4:33:38 PM
// 
#endregion

using System;
using System.Threading;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Properties;
using DXGI = SharpDX.DXGI;
using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
	/// <summary>
	/// A wrapper for a Direct 3D 11 device object and adapter.
	/// </summary>
	class VideoDevice
		: IGorgonVideoDevice
	{
		#region Variables.
		// The Direct 3D 11 device object.
		private D3D11.Device1 _device;
		// The DXGI adapter.
		private DXGI.Adapter2 _adapter;
		// The logging interface for debug logging.
		private readonly IGorgonLog _log;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the Direct 3D 11.1 device context.
		/// </summary>
		public D3D11.DeviceContext1 D3DDeviceContext =>  _device?.ImmediateContext1;

		/// <summary>
		/// Property to return the Direct 3D 11.1 device object
		/// </summary>
		public D3D11.Device1 D3DDevice => _device;

		/// <summary>
		/// Property to return the adapter for the video device.
		/// </summary>
		public DXGI.Adapter2 Adapter => _adapter;

		/// <summary>
		/// Property to return the maximum number of render target view slots available.
		/// </summary>
		public int MaxRenderTargetViewSlots => 8;

		/// <summary>
		/// Property to return the maximum size, in bytes, for a constant buffer.
		/// </summary>
		/// <remarks>
		/// On devices with a a <see cref="FeatureLevelSupport"/> of <see cref="FeatureLevelSupport.Level_11_1"/>, this value will return <see cref="int.MaxValue"/>, indicating that there is no limit to the size of a 
		/// constant buffer. On devices with a lower feature level this value is limited to 65536 (4096 * 16) bytes.
		/// </remarks>
		public int MaxConstantBufferSize => RequestedFeatureLevel < FeatureLevelSupport.Level_11_1 ? int.MaxValue : 65536;

		/// <summary>
		/// Property to return the <see cref="VideoDeviceInfo"/> used to create this device.
		/// </summary>
		public IGorgonVideoDeviceInfo Info
		{
			get;
		}

		/// <summary>
		/// Property to return the actual supported <see cref="FeatureLevelSupport"/> from the device.
		/// </summary>
		/// <remarks>
		/// <para>
		/// A user may request a lower <see cref="FeatureLevelSupport"/> than what is supported by the device to allow the application to run on older video devices that lack support for newer functionality. 
		/// This requested feature level will be returned by this property if supported by the device. 
		/// </para>
		/// <para>
		/// If the user does not request a feature level, or has specified one higher than what the video device supports, then the highest feature level supported by the video device 
		/// (indicated by the <see cref="VideoDeviceInfo.SupportedFeatureLevel"/> property in the <see cref="IGorgonVideoDevice.Info"/> property) will be returned.
		/// </para>
		/// </remarks>
		/// <seealso cref="FeatureLevelSupport"/>
		public FeatureLevelSupport RequestedFeatureLevel
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to convert a Gorgon feature level into a D3D feature level.
		/// </summary>
		/// <param name="requestedFeatureLevel">The requested feature level.</param>
		/// <returns>The D3D feature levels to use.</returns>
		private static D3D.FeatureLevel[] GetFeatureLevel(D3D.FeatureLevel requestedFeatureLevel)
		{
			switch (requestedFeatureLevel)
			{
				case D3D.FeatureLevel.Level_11_1:
					return new[]
					       {
						       D3D.FeatureLevel.Level_11_1,
						       D3D.FeatureLevel.Level_11_0,
						       D3D.FeatureLevel.Level_10_1,
						       D3D.FeatureLevel.Level_10_0
					       };
				case D3D.FeatureLevel.Level_11_0:
					return new[] {
							D3D.FeatureLevel.Level_11_0,
							D3D.FeatureLevel.Level_10_1,
							D3D.FeatureLevel.Level_10_0
					};
				case D3D.FeatureLevel.Level_10_1:
					return new[] {
							D3D.FeatureLevel.Level_10_1,
							D3D.FeatureLevel.Level_10_0
					};
				case D3D.FeatureLevel.Level_10_0:
					return new[] {
							D3D.FeatureLevel.Level_10_0
					};
				default:
					throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_FEATURE_LEVEL_INVALID, requestedFeatureLevel));
			}
		}

		/// <summary>
		/// Function to create the Direct 3D device and Adapter for use with Gorgon.
		/// </summary>
		/// <param name="requestedFeatureLevel">The requested feature level for the device.</param>
		private void CreateDevice(D3D.FeatureLevel requestedFeatureLevel)
		{
			DXGI.Factory2 factory2 = null;
			D3D11.DeviceCreationFlags flags = GorgonGraphics.IsDebugEnabled ? D3D11.DeviceCreationFlags.Debug : D3D11.DeviceCreationFlags.None;

			try
			{
				using (var factory1 = new DXGI.Factory1())
				{
					factory2 = factory1.QueryInterface<DXGI.Factory2>();
				}

				switch (Info.VideoDeviceType)
				{
					case VideoDeviceType.ReferenceRasterizer:
#if DEBUG
						using (var device = new D3D11.Device(D3D.DriverType.Reference, D3D11.DeviceCreationFlags.Debug, GetFeatureLevel(requestedFeatureLevel))
						                    {
							                    DebugName = $"{Info.Name} D3D11.1 Reference Device"
						                    })
						{
							_device = device.QueryInterface<D3D11.Device1>();
						}

						using (var giDevice = _device.QueryInterface<DXGI.Device2>())
						{
							_adapter = giDevice.GetParent<DXGI.Adapter2>();
						}
						break;
#endif
					case VideoDeviceType.Software:
						using (var device = new D3D11.Device(D3D.DriverType.Warp, flags, GetFeatureLevel(requestedFeatureLevel))
						                    {
							                    DebugName = $"{Info.Name} D3D11.1 Software Device"
						                    })
						{
							_device = device.QueryInterface<D3D11.Device1>();
						}

						using (var giDevice = _device.QueryInterface<DXGI.Device2>())
						{
							_adapter = giDevice.GetParent<DXGI.Adapter2>();
						}
						break;
					default:
						using (DXGI.Adapter1 adapter1 = factory2.GetAdapter1(Info.Index))
						{
							_adapter = adapter1.QueryInterface<DXGI.Adapter2>();
						}

						using (var device = new D3D11.Device(_adapter, flags, GetFeatureLevel(requestedFeatureLevel))
						                    {
							                    DebugName = $"{Info.Name} D3D 11.1 Device"
						                    })
						{
							_device = device.QueryInterface<D3D11.Device1>();
						}
						break;
				}

				// Get the maximum supported feature level for this device.
				RequestedFeatureLevel = (FeatureLevelSupport)_device.FeatureLevel;

				_log.Print($"Direct 3D 11.1 device created for video device '{Info.Name}' at feature level [{RequestedFeatureLevel}]", LoggingLevel.Simple);
			}
			finally
			{
				factory2?.Dispose();
			}
		}

		/// <summary>
		/// Returns a <see cref="string"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="string"/> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return string.Format(Resources.GORGFX_TOSTR_DEVICE, Info.Name);
		}

		/// <summary>
		/// Function to retrieve the supported functionality for a given format.
		/// </summary>
		/// <param name="format">The format to evaluate.</param>
		/// <returns>A <c>FormatSupport</c> containing OR'd values representing the functionality supported by the format.</returns>
		/// <remarks>
		/// <para>
		/// Use this method to determine if a format can be used with a specific resource type (e.g. a 2D texture, vertex buffer, etc...). The value returned will be of the <c>FormatSupport</c> 
		/// enumeration and will contain the supported functionality represented as OR'd values.
		/// </para>
		/// </remarks>
		public D3D11.FormatSupport GetBufferFormatSupport(DXGI.Format format)
		{
			return _device.CheckFormatSupport(format);
		}

		/// <summary>
		/// Function to retrieve the supported unordered access compute resource functionality for a given format.
		/// </summary>
		/// <param name="format">The format to evaluate.</param>
		/// <returns>A <c>ComputeShaderFormatSupport</c> containing OR'd values representing the functionality supported by the format.</returns>
		/// <remarks>
		/// <para>
		/// Use this method to determine if a format can be used with specific unordered access view operations in a compute shader. The value returned will be of the <c>ComputeShaderFormatSupport</c> 
		/// enumeration type and will contain the supported functionality represented as OR'd values.
		/// </para>
		/// <para>
		/// Regardless of whether limited compute shader support is available on some Direct3D 10 class devices, this will always return <c>ComputeShaderFormatSupport.None</c> on devices with lower than 
		/// Level_11_0 feature level support.
		/// </para>
		/// </remarks>
		public D3D11.ComputeShaderFormatSupport GetBufferFormatComputeSupport(DXGI.Format format)
		{
			return RequestedFeatureLevel < FeatureLevelSupport.Level_11_0 ? D3D11.ComputeShaderFormatSupport.None : _device.CheckComputeShaderFormatSupport(format);
		}

		/// <summary>
		/// Function to return the maximum number of quality levels supported by the device for multi sampling.
		/// </summary>
		/// <param name="format">A <c>Format</c> to evaluate.</param>
		/// <param name="count">Number of multi samples.</param>
		/// <returns>A <see cref="GorgonMultiSampleInfo"/> containing the quality count and sample count for multi-sampling.</returns>
		/// <remarks>
		/// <para>
		/// Use this to return the quality count for a given multi-sample sample count. This method will return a <see cref="GorgonMultiSampleInfo"/> value type that contains both the sample count passed 
		/// to this method, and the quality count for that sample count. If the <see cref="GorgonMultiSampleInfo.Quality"/> is less than 1, then the sample count is not supported by this video device.
		/// </para>
		/// </remarks>
		public GorgonMultiSampleInfo GetMultiSampleQuality(DXGI.Format format, int count)
		{
			if (format == DXGI.Format.Unknown)
			{
				return GorgonMultiSampleInfo.NoMultiSampling;
			}

			if (count < 1)
			{
				count = 1;
			}

			return new GorgonMultiSampleInfo(count, _device.CheckMultisampleQualityLevels(format, count));
		}

		/// <summary>
		/// Function to find a display mode supported by the Gorgon.
		/// </summary>
		/// <param name="output">The output to use when looking for a video mode.</param>
		/// <param name="videoMode">The <c>ModeDescription1</c> used to find the closest match.</param>
		/// <returns>A <c>ModeDescription1</c> that is the nearest match for the provided video mode.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="output"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <remarks>
		/// <para>
		/// Users may leave the <c>ModeDescription1</c> values at unspecified (either 0, or default enumeration values) to indicate that these values should not be used in the search.
		/// </para>
		/// <para>
		/// The following members in <c>ModeDescription1</c> may be skipped (if not listed, then this member must be specified):
		/// <list type="bullet">
		///		<item>
		///			<description><c>ModeDescription1.Width</c> and <c>ModeDescription1.Height</c>.  Both values must be set to 0 if not filtering by width or height.</description>
		///		</item>
		///		<item>
		///			<description><c>ModeDescription1.RefreshRate</c> should be set to empty in order to skip filtering by refresh rate.</description>
		///		</item>
		///		<item>
		///			<description><c>ModeDescription1.Scaling</c> should be set to <c>DisplayModeScaling.Unspecified</c> in order to skip filtering by the scaling mode.</description>
		///		</item>
		///		<item>
		///			<description><c>ModeDescription1.ScanlineOrdering</c> should be set to <c>ScanlineOrder.Unspecified</c> in order to skip filtering by the scanline order.</description>
		///		</item>
		/// </list>
		/// </para>
		/// <para>
		/// <note type="important">
		/// <para>
		/// The <c>ModeDescription1.Format</c> member must be one of the UNorm format types and cannot be set to <c>Format.Unknown</c>.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public DXGI.ModeDescription1 FindNearestVideoMode(IGorgonVideoOutputInfo output, ref DXGI.ModeDescription1 videoMode)
		{
			using (DXGI.Output giOutput = _adapter.GetOutput(output.Index))
			{
				using (DXGI.Output1 giOutput1 = giOutput.QueryInterface<DXGI.Output1>())
				{
					DXGI.ModeDescription mode;
					DXGI.ModeDescription matchMode = videoMode.ToModeDesc();
					
					giOutput1.GetClosestMatchingMode(_device, matchMode, out mode);

					return new DXGI.ModeDescription1
					       {
						       Format = mode.Format,
							   Width = mode.Width,
							   Height = mode.Height,
							   RefreshRate = mode.RefreshRate,
							   Scaling = mode.Scaling,
							   ScanlineOrdering = mode.ScanlineOrdering
					       };
				}
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			D3D11.Device device = Interlocked.Exchange(ref _device, null);
			DXGI.Adapter1 adapter = Interlocked.Exchange(ref _adapter, null);

			device?.Dispose();
			adapter?.Dispose();
			_device = null;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="VideoDevice"/> class.
		/// </summary>
		/// <param name="deviceInfo">A <see cref="VideoDeviceInfo"/> containing information about which video device to use.</param>
		/// <param name="requestedFeatureLevel">The desired feature level for the device.</param>
		/// <param name="log">A <see cref="IGorgonLog"/> used for logging debug output.</param>
		public VideoDevice(IGorgonVideoDeviceInfo deviceInfo, FeatureLevelSupport requestedFeatureLevel, IGorgonLog log)
		{
			_log = log ?? GorgonLogDummy.DefaultInstance;
			Info = deviceInfo;
			CreateDevice((D3D.FeatureLevel)requestedFeatureLevel);
			RequestedFeatureLevel = FeatureLevelSupport.Level_10_0;
		}
		#endregion
	}
}