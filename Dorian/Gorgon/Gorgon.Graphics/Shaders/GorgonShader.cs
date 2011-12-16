﻿#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Thursday, December 15, 2011 9:29:56 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using D3D = SharpDX.Direct3D11;
using Shaders = SharpDX.D3DCompiler;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Version for the shaders.
	/// </summary>
	public enum ShaderVersion
	{
		/// <summary>
		/// Shader model 5.
		/// </summary>
		Version5 = 0,
		/// <summary>
		/// Shader model 4.
		/// </summary>
		Version4 = 1,
		/// <summary>
		/// Shader model 4, profile 1.
		/// </summary>
		Version4_1 = 2,
		/// <summary>
		/// Shader model 2, vertex shader profile a, pixel shader profile b.
		/// </summary>
		Version2a_b = 3
	}

	/// <summary>
	/// Shader types.
	/// </summary>
	public enum ShaderType
	{
		/// <summary>
		/// Vertex shader.
		/// </summary>
		Vertex = 0,
		/// <summary>
		/// Pixel shader.
		/// </summary>
		Pixel = 1,
		/// <summary>
		/// Geometry shader.
		/// </summary>
		Geometry = 2,
		/// <summary>
		/// Compute shader.
		/// </summary>
		Compute = 3,
		/// <summary>
		/// Domain shader.
		/// </summary>
		Domain = 4,
		/// <summary>
		/// Hull shader.
		/// </summary>
		Hull = 5
	}

	/// <summary>
	/// The base shader object.
	/// </summary>
	public abstract class GorgonShader
		: GorgonNamedObject, INotifier, IDisposable
	{
		#region Classes.
		/// <summary>
		/// A list of constant buffers.
		/// </summary>
		public class ShaderConstantBuffers
			: IEnumerable<GorgonConstantBuffer>
		{
			#region Variables.
			private GorgonConstantBuffer[] _buffers = null;
			private GorgonShader _shader = null;
			#endregion

			#region Properties.
			/// <summary>
			/// Property to return the number of buffers.
			/// </summary>
			public int Count
			{
				get
				{
					return _buffers.Length;
				}
			}

			/// <summary>
			/// Property to set or return a constant buffer at the specified index.
			/// </summary>
			public GorgonConstantBuffer this[int index]
			{
				get
				{
					return _buffers[index];
				}
				set
				{
					_buffers[index] = value;
					_shader.ApplyConstantBuffer(index, _buffers[index]);	
				}
			}
			#endregion

			#region Methods.
			/// <summary>
			/// Function to set a range of constant buffers at once.
			/// </summary>
			/// <param name="slot">Starting slot for the buffer.</param>
			/// <param name="buffers">Buffers to set.</param>
			/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="slot"/> is less than 0, or greater than the available number of constant buffer slots.
			/// <para>-or-</para>
			/// <para>Thrown when the <paramref name="buffers"/> count + the slot is greater than or equal to the number of available constant buffer slots.</para>
			/// </exception>
			public void SetRange(int slot, IEnumerable<GorgonConstantBuffer> buffers)
			{
				GorgonDebug.AssertNull<IEnumerable<GorgonConstantBuffer>>(buffers, "buffers");
#if DEBUG
				if ((slot < 0) || (slot >= _buffers.Length) || ((slot + buffers.Count()) >= _buffers.Length))
					throw new ArgumentOutOfRangeException("Cannot have more than " + _buffers.Length.ToString() + " slots occupied.");
#endif

				for (int i = 0; i < buffers.Count(); i++)
					_buffers[i + slot] = buffers.ElementAt(i);
				
				_shader.ApplyConstantBuffers(slot, buffers);
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="ShaderConstantBuffers"/> class.
			/// </summary>
			/// <param name="shader">Shader that owns these buffers.</param>
			internal ShaderConstantBuffers(GorgonShader shader)
			{
				_buffers = new GorgonConstantBuffer[14];
				_shader = shader;
			}
			#endregion

			#region IEnumerable<GorgonConstantBuffer> Members
			/// <summary>
			/// Returns an enumerator that iterates through a collection.
			/// </summary>
			/// <returns>
			/// An object that can be used to iterate through the collection.
			/// </returns>
			public IEnumerator<GorgonConstantBuffer> GetEnumerator()
			{
				foreach (var item in _buffers)
					yield return item;
			}
			#endregion

			#region IEnumerable Members
			/// <summary>
			/// Returns an enumerator that iterates through a collection.
			/// </summary>
			/// <returns>
			/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
			/// </returns>
			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
			#endregion
		}
		#endregion

		#region Variables.
		private bool _disposed = false;								// Flag to indicate that the object was disposed.
		private string _source = null;								// Shader source code.
		private ShaderVersion _version = ShaderVersion.Version2a_b;	// Shader model version.
		private bool _debug = false;								// Flag to indicate that the shader has debug information.		
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the shader byte code.
		/// </summary>
		internal Shaders.ShaderBytecode D3DByteCode
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return whether to include debug information in the shader or not.
		/// </summary>
		public bool IsDebug
		{
			get
			{
				return _debug;
			}
			set
			{
				if (_debug != value)
				{
					_debug = value;
					HasChanged = true;
				}
			}
		}

		/// <summary>
		/// Property to return the list of constant buffers for this shader.
		/// </summary>
		public ShaderConstantBuffers ConstantBuffers
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the type of shader.
		/// </summary>
		public ShaderType ShaderType
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the entry point method.
		/// </summary>
		public string EntryPoint
		{
			get;
			private set;
		}
		
		/// <summary>
		/// Property to return the graphics interface that created this shader.
		/// </summary>
		public GorgonGraphics Graphics
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the shader model version number for this shader.
		/// </summary>
		/// <remarks>It is not recommended to set this value manually.  Gorgon will attempt to find the best version for the supported feature level.</remarks>
		public ShaderVersion Version
		{
			get
			{
				return _version;
			}
			set
			{
				if (_version != value)
				{
					_version = value;
					HasChanged = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the source code for the shader.
		/// </summary>
		/// <remarks>This value will be NULL (Nothing in VB.Net) if the shader has no source code (i.e. it's loaded from a binary shader).</remarks>
		public string SourceCode
		{
			get
			{
				return _source;
			}
			set
			{
				if (_source != value)
				{
					_source = value;
					HasChanged = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the errors generated by the shader.
		/// </summary>
		public string Errors
		{
			get;
			protected set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the Direct3D shader version.
		/// </summary>
		/// <returns>The Direct3D shader version.</returns>
		private string GetD3DVersion()
		{			
			string prefix = string.Empty;
			string version = string.Empty;

			if (((ShaderType == GorgonLibrary.Graphics.ShaderType.Compute) || (ShaderType == GorgonLibrary.Graphics.ShaderType.Domain) || (ShaderType == GorgonLibrary.Graphics.ShaderType.Hull)) &&
				((Graphics.VideoDevice.SupportedFeatureLevels & DeviceFeatureLevel.SM5) != DeviceFeatureLevel.SM5))
				throw new NotSupportedException("Compute, domain and hull shaders are only supported by Shader Model 5 hardware.");

			if ((ShaderType == GorgonLibrary.Graphics.ShaderType.Geometry) && 
				(((Graphics.VideoDevice.SupportedFeatureLevels & DeviceFeatureLevel.SM4) != DeviceFeatureLevel.SM4) 
				|| ((Graphics.VideoDevice.SupportedFeatureLevels & DeviceFeatureLevel.SM4_1) != DeviceFeatureLevel.SM4_1)))
				throw new NotSupportedException("Geometry shaders are only supported by Shader Model 4 hardware.");
			
			switch (ShaderType)
			{
				case GorgonLibrary.Graphics.ShaderType.Pixel:
					prefix = "ps";
					break;
				case GorgonLibrary.Graphics.ShaderType.Compute:
					if ((Graphics.VideoDevice.SupportedFeatureLevels & DeviceFeatureLevel.SM5) != DeviceFeatureLevel.SM5)
						throw new GorgonException(GorgonResult.CannotCreate, "Cannot create the compute shader, the video device does not support compute shaders.");
					prefix = "cs";
					break;
				case GorgonLibrary.Graphics.ShaderType.Geometry:
					prefix = "gs";
					break;
				case GorgonLibrary.Graphics.ShaderType.Domain:
					prefix = "ds";
					break;
				default:
					prefix = "vs";
					break;
			}

			switch (_version)
			{
				case ShaderVersion.Version5:
					version = "5_0";
					break;
				case ShaderVersion.Version4_1:
					version = "4_1";
					break;
				case ShaderVersion.Version4:
					version = "4_0";
					break;
				default:
					version = "4_0_level_9_3";
					break;
			}

			return prefix + "_" + version;
		}

		/// <summary>
		/// Function to compile the shader.
		/// </summary>
		/// <param name="byteCode">Byte code for the shader.</param>
		protected abstract void CompileImpl(Shaders.ShaderBytecode byteCode);

		/// <summary>
		/// Function to assign this shader and its states to the device.
		/// </summary>
		protected abstract void AssignImpl();

		/// <summary>
		/// Function to apply a single constant buffer.
		/// </summary>
		/// <param name="slot">Slot to index.</param>
		/// <param name="buffer">Buffer to apply.</param>
		protected abstract void ApplyConstantBuffer(int slot, GorgonConstantBuffer buffer);

		/// <summary>
		/// Function to apply multiple constant buffers.
		/// </summary>
		/// <param name="slot">Slot to index.</param>
		/// <param name="buffers">Buffers to apply.</param>
		protected abstract void ApplyConstantBuffers(int slot, IEnumerable<GorgonConstantBuffer> buffers);

		/// <summary>
		/// Function to assign this shader and its states to the device.
		/// </summary>
		internal void Assign()
		{
			AssignImpl();
		}

		/// <summary>
		/// Function to compile the shader.
		/// </summary>
		/// <exception cref="System.NotSupportedException">Thrown when the shader is not supported by the current supported feature level for the video hardware.</exception>
		public void Compile()
		{
			Shaders.ShaderFlags flags = Shaders.ShaderFlags.OptimizationLevel3;			

			if ((!HasChanged) || (string.IsNullOrEmpty(SourceCode)))
				return;

			try
			{				
				if (IsDebug)
					flags = Shaders.ShaderFlags.Debug;

				if ((Graphics.VideoDevice.SupportedFeatureLevels & DeviceFeatureLevel.SM5) != DeviceFeatureLevel.SM5)
					flags |= Shaders.ShaderFlags.EnableBackwardsCompatibility;

				D3DByteCode = Shaders.ShaderBytecode.Compile(SourceCode, EntryPoint, GetD3DVersion(), flags, Shaders.EffectFlags.None, null, null);

				CompileImpl(D3DByteCode);
				HasChanged = false;
			}
			catch (SharpDX.CompilationException cex)
			{				
				Errors = cex.Message;
				throw GorgonException.Catch(cex);
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonShader"/> class.
		/// </summary>
		/// <param name="graphics">Graphics interface that created this shader.</param>
		/// <param name="name">The name of the shader.</param>
		/// <param name="type">Type of the shader.</param>
		/// <param name="entryPoint">The entry point method for the shader.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		protected GorgonShader(GorgonGraphics graphics, string name, ShaderType type, string entryPoint)
			: base(name)
		{
			Graphics = graphics;

#if DEBUG
			IsDebug = true;
#else
			IsDebug = false;
#endif

			// Create the constant buffers for this shader.
			ConstantBuffers = new ShaderConstantBuffers(this);

			ShaderType = type;
			EntryPoint = entryPoint;

			// Determine the version by the supported feature level.
			if ((Graphics.VideoDevice.SupportedFeatureLevels & DeviceFeatureLevel.SM5) == DeviceFeatureLevel.SM5)
				Version = ShaderVersion.Version5;
			else if ((Graphics.VideoDevice.SupportedFeatureLevels & DeviceFeatureLevel.SM4_1) == DeviceFeatureLevel.SM4_1)
				Version = ShaderVersion.Version4_1;
			else if ((Graphics.VideoDevice.SupportedFeatureLevels & DeviceFeatureLevel.SM4) == DeviceFeatureLevel.SM4)
				Version = ShaderVersion.Version4;
			else if ((Graphics.VideoDevice.SupportedFeatureLevels & DeviceFeatureLevel.SM2_a_b) == DeviceFeatureLevel.SM2_a_b)
				Version = ShaderVersion.Version2a_b;			
		}
		#endregion

		#region INotifier Members
		/// <summary>
		/// Property to set or return whether an object has been updated.
		/// </summary>
		public bool HasChanged
		{
			get;
			set;
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (D3DByteCode != null)
						D3DByteCode.Dispose();

					Graphics.RemoveTrackedObject(this);
				}

				D3DByteCode = null;
				_disposed = true;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
