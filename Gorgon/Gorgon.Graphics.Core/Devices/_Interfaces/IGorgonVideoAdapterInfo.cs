#region MIT
// 
// Gorgon.
// Copyright (C) 2016 Michael Winsor
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
// Created: June 13, 2016 12:20:16 AM
// 
#endregion

using D3D = SharpDX.Direct3D;
using Gorgon.Collections;
using Gorgon.Core;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// Defines the level of support for functionality for a <see cref="IGorgonVideoAdapter"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A feature set is used to describe what functionality a video adapter can support when using Direct 3D. For example, shader model 5 shaders are only supported by devices that support Direct 3D 12.0
	/// or greater, and this will be reflected by a value of <see cref="Level_12_0"/>, or <see cref="Level_12_1"/>.
	/// </para>
	/// <para>
	/// Feature levels do not necessarily mean the hardware is limited, it may be that a device does not support a feature because the current driver does not expose that functionality. 
	/// </para>
	/// <para>
	/// Applications can use this to define a minimum supported video adapter, or take an alternate code (potentially slower) path to achieve the same result.
	/// </para>
	/// </remarks>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
	public enum FeatureSet
	{
	    /// <summary>
	    /// Device supports the equivalent of Direct 3D 12.0 functionality.
	    /// </summary>
	    Level_12_0 = D3D.FeatureLevel.Level_12_0,
	    /// <summary>
	    /// Device supports the equivalent of Direct 3D 12.1 functionality.
	    /// </summary>
	    Level_12_1 = D3D.FeatureLevel.Level_12_1,
    }

    /// <summary>
    /// Defines the type of video adapter.
    /// </summary>
    public enum VideoDeviceType
	{
		/// <summary>
		/// Hardware video adapter.
		/// </summary>
		Hardware = 0,
		/// <summary>
		/// Software video adapter (WARP).
		/// </summary>
		Software = 1,
		/// <summary>
		/// Reference rasterizer video adapter.
		/// </summary>
		ReferenceRasterizer = 2
	}

	/// <summary>
	/// Provides information about a video adapter in the system.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This information may be for a physical hardware adapter, or a software rasterizer.  To determine which type this device falls under, se the <see cref="VideoDeviceType"/> property to determine the 
	/// type of device.
	/// </para>
	/// </remarks>
	public interface IGorgonVideoAdapterInfo 
		: IGorgonNamedObject
	{
		/// <summary>
		/// Property to return the index of the video adapter within a list returned by <see cref="GorgonGraphics.EnumerateAdapters"/>.
		/// </summary>
		int Index
		{
			get;
		}

		/// <summary>
		/// Property to return the type of video adapter.
		/// </summary>
		VideoDeviceType VideoDeviceType
		{
			get;
		}

		/// <summary>
		/// Property to return the highest feature set that the hardware can support.
		/// </summary>
		FeatureSet SupportedFeatureLevel
		{
			get;
		}

		/// <summary>
		/// Property to return the device ID.
		/// </summary>
		int DeviceID
		{
			get;
		}

		/// <summary>
		/// Property to return the unique identifier for the device.
		/// </summary>
		long Luid
		{
			get;
		}

		/// <summary>
		/// Property to return the revision for the device.
		/// </summary>
		int Revision
		{
			get;
		}

		/// <summary>
		/// Property to return the sub system ID for the device.
		/// </summary>
		int SubSystemID
		{
			get;
		}

		/// <summary>
		/// Property to return the vendor ID for the device.
		/// </summary>
		int VendorID
		{
			get;
		}

		/// <summary>
		/// Property to return the amount of dedicated system memory for the device, in bytes.
		/// </summary>
		/// <remarks>
		/// If the application is running as an x86 application, this value may report an incorrect value. This is a known issue with SharpDX.
		/// </remarks>
		long DedicatedSystemMemory
		{
			get;
		}

		/// <summary>
		/// Property to return the amount of dedicated video memory for the device, in bytes.
		/// </summary>
		/// <remarks>
		/// If the application is running as an x86 application, this value may report an incorrect value. is a known issue with SharpDX.
		/// </remarks>
		long DedicatedVideoMemory
		{
			get;
		}

		/// <summary>
		/// Property to return the amount of shared system memory for the device.
		/// </summary>
		/// <remarks>
		/// If the application is running as an x86 application, this value may report an incorrect value.
		/// </remarks>
		long SharedSystemMemory
		{
			get;
		}

		/// <summary>
		/// Property to return the outputs on this device.
		/// </summary>
		/// <remarks>The outputs are typically monitors attached to the device.</remarks>
		IGorgonNamedObjectReadOnlyList<IGorgonVideoOutputInfo> Outputs
		{
			get;
		}
	}
}