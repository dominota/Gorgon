﻿#region MIT
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
// Created: June 15, 2016 9:33:57 PM
// 
#endregion

using System;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Math;
using Gorgon.Native;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// A buffer for holding vertex data.
    /// </summary>
    /// <remarks>
    /// <para>
    /// To send vertices to the GPU using a vertex buffer, an application can upload vertices, represented as a value type, to the buffer using one of the
    /// <see cref="O:Gorgon.Graphics.Core.GorgonBufferCommon.SetData"/> overloads. For best performance, it is recommended to upload vertex data only once, or rarely. However, in some scenarios, and with the
    /// correct <see cref="GorgonGraphicsResource.Usage"/> flag, vertex animation is possible by uploading data to a <see cref="ResourceUsage.Dynamic"/> vertex buffer.
    /// </para>
    /// <para>
    /// To use a vertex buffer with the GPU pipeline, one must create a <see cref="GorgonVertexBufferBinding"/> to inform the GPU on how to use the vertex buffer.
    /// </para>
    /// <para> 
    /// </para>
    /// </remarks>
    /// <example language="csharp">
    /// For example, to send a list of vertices to a vertex buffer:
    /// <code language="csharp">
    /// <![CDATA[
    /// // Our vertex, with a position and color component.
    /// [StructLayout(LayoutKind = LayoutKind.Sequential)] 
    /// struct MyVertex
    /// {
    ///		public Vector4 Position;
    ///		public Vector4 Color;
    /// }
    /// 
    /// GorgonGraphics graphics;
    /// MyVertex[] _vertices = new MyVertex[100];
    /// GorgonVertexBuffer _vertexBuffer;
    /// 
    /// void InitializeVertexBuffer()
    /// {
    ///		_vertices = ... // Fill your vertex array here.
    /// 
    ///		// Create the vertex buffer large enough so that it'll hold 100 vertices.
    ///		_vertexBuffer = new GorgonVertexBuffer(graphics, GorgonVertexBufferInfo.CreateFromType<MyVertex>(_vertices.Length, Usage.Default));
    /// 
    ///		// Copy our data to the vertex buffer.
    ///     _vertexBuffer.SetData<MyVertex>(_vertices);
    /// 
    ///		// Copy our data to the vertex buffer, using the 5th index in the vertex array, and 25 vertices.
    ///     _vertexBuffer.SetData<MyVertex>(_vertices, 5, 25);
    ///
    ///		// Copy our data to the vertex buffer, using the 5th index in the vertex array, 25 vertices, and storing at index 2 in the vertex buffer.
    ///     _vertexBuffer.SetData<MyVertex>(_vertices, 5, 25, 2);
    ///
    ///     // Copy our data to the vertex buffer, using the 5th index in the native buffer, 25 vertices, and storing at index 2 in the vertex buffer, using a copy mode.
    ///     _vertexBuffer.SetData(vertices, 5, 25, 2, CopyMode.NoOverWrite);
    /// 
    ///     // Copy our data from a GorgonNativeBuffer.
    ///     using (GorgonNativeBuffer<MyVertex> vertices = new GorgonNativeBuffer<MyVertex>(100))
    ///     {
    ///        // Copy vertices into the native buffer here....
    ///
    ///        // Copy everything.
    ///        _vertexBuffer.SetData(vertices);
    /// 
    ///        // Copy our data to the vertex buffer, using the 5th index in the native buffer, 25 vertices, and storing at index 2 in the vertex buffer.
    ///        _vertexBuffer.SetData(vertices, 5, 25, 2);
    ///
    ///        // Copy our data to the vertex buffer, using the 5th index in the native buffer, 25 vertices, and storing at index 2 in the vertex buffer, using a copy mode.
    ///        _vertexBuffer.SetData(vertices, 5, 25, 2, CopyMode.NoOverWrite);
    ///
    ///        // Get the data back out from the buffer, using index 5 and up to 10 vertices, storing at index 2 of the native buffer.
    ///        _vertexBuffer.GetData<MyVertex>(vertices, 5, 10, 2); 
    ///     }
    ///
    ///     // Get the data back out from the buffer.
    ///     MyVertex[] readBack = _vertexBuffer.GetData<MyVertex>();
    ///
    ///     // Get the data back out from the buffer, starting at index 5 and a count of 10 vertices.
    ///     readBack = _vertexBuffer.GetData<MyVertex>(5, 10);
    ///
    ///     // Get the data back out from the buffer, using index 5 and up to 10 vertices, storing at index 2.
    ///     _vertexBuffer.GetData<MyVertex>(readBack, 5, 10, 2);
    /// }
    /// ]]>
    /// </code>
    /// </example>
    /// <seealso cref="GorgonVertexBufferBinding"/>
    public sealed class GorgonVertexBuffer
        : GorgonBufferCommon, IGorgonVertexBufferInfo
    {
        #region Constants.
        /// <summary>
        /// The prefix to assign to a default name.
        /// </summary>
	    internal const string NamePrefix = nameof(GorgonVertexBuffer);
        #endregion

        #region Variables.
        // The information used to create the buffer
        private readonly GorgonVertexBufferInfo _info;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the bind flags used for the D3D 11 resource.
        /// </summary>
        internal override D3D11.BindFlags BindFlags => Native?.Description.BindFlags ?? D3D11.BindFlags.None;

        /// <summary>
        /// Property to return the binding used to bind this buffer to the GPU.
        /// </summary>
        public VertexIndexBufferBinding Binding => _info.Binding;


        /// <summary>
        /// Property to return the usage for the resource.
        /// </summary>
        public override ResourceUsage Usage => _info.Usage;

        /// <summary>
        /// Property to return the size, in bytes, of the resource.
        /// </summary>
        public override int SizeInBytes => _info.SizeInBytes;

        /// <summary>
        /// Property to return whether or not the buffer is directly readable by the CPU via one of the <see cref="O:Gorgon.Graphics.Core.GorgonBufferCommon.GetData"/> methods.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Buffers must meet the following criteria in order to qualify for direct CPU read:
        /// <list type="bullet">
        ///     <item>Must have a <see cref="GorgonGraphicsResource.Usage"/> of <see cref="ResourceUsage.Default"/> (or <see cref="ResourceUsage.Staging"/>).</item>
        ///     <item>Must be bindable to a shader resource view (<see cref="ResourceUsage.Default"/> only).</item>
        /// </list>
        /// </para>
        /// <para>
        /// This will always return <b>false</b> for this buffer type, except when its <see cref="GorgonGraphicsResource.Usage"/> is <see cref="ResourceUsage.Staging"/>.
        /// </para>
        /// </remarks>
        /// <seealso cref="O:Gorgon.Graphics.Core.GorgonBufferCommon.GetData"/>
        public override bool IsCpuReadable => Usage == ResourceUsage.Staging;

        /// <summary>
        /// Property to return the name of this object.
        /// </summary>
        public override string Name => _info.Name;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to validate the bindings for a given buffer.
        /// </summary>
        /// <param name="usage">The usage flags for the buffer.</param>
        /// <param name="binding">The bindings to apply to the buffer, pass <b>null</b> to skip usage and binding check.</param>
        internal static void ValidateBufferBindings(ResourceUsage usage, VertexIndexBufferBinding binding)
        {
            switch (usage)
            {
                case ResourceUsage.Dynamic:
                case ResourceUsage.Immutable:
                    if ((binding & VertexIndexBufferBinding.StreamOut) == VertexIndexBufferBinding.StreamOut)
                    {
                        throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_BUFFER_NO_SO, usage));
                    }

                    if ((binding & VertexIndexBufferBinding.UnorderedAccess) == VertexIndexBufferBinding.UnorderedAccess)
                    {
                        throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_BUFFER_NO_UAV, usage));
                    }
                    break;
                case ResourceUsage.Staging:
                    if (binding != VertexIndexBufferBinding.None)
                    {
                        throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_BUFFER_STAGING_CANNOT_BE_BOUND_TO_GPU, binding));
                    }
                    break;
            }
        }

        /// <summary>
        /// Function to initialize the buffer data.
        /// </summary>
        /// <param name="initialData">The initial data used to populate the buffer.</param>
        private void Initialize(GorgonNativeBuffer<byte> initialData)
        {
            D3D11.CpuAccessFlags cpuFlags = GetCpuFlags(false, D3D11.BindFlags.VertexBuffer);

            Log.Print($"{Name} Vertex Buffer: Creating D3D11 buffer. Size: {SizeInBytes} bytes", LoggingLevel.Simple);

            ValidateBufferBindings(_info.Usage, 0);

            D3D11.BindFlags bindFlags = Usage == ResourceUsage.Staging ? D3D11.BindFlags.None : D3D11.BindFlags.VertexBuffer;

            if ((_info.Binding & VertexIndexBufferBinding.StreamOut) == VertexIndexBufferBinding.StreamOut)
            {
                bindFlags |= D3D11.BindFlags.StreamOutput;
            }

            if ((_info.Binding & VertexIndexBufferBinding.UnorderedAccess) == VertexIndexBufferBinding.UnorderedAccess)
            {
                bindFlags |= D3D11.BindFlags.UnorderedAccess;
            }

            var desc = new D3D11.BufferDescription
            {
                SizeInBytes = _info.SizeInBytes,
                Usage = (D3D11.ResourceUsage)_info.Usage,
                BindFlags = bindFlags,
                OptionFlags = D3D11.ResourceOptionFlags.None,
                CpuAccessFlags = cpuFlags,
                StructureByteStride = 0
            };

            if ((initialData != null) && (initialData.Length > 0))
            {
                unsafe
                {
                    D3DResource = Native = new D3D11.Buffer(Graphics.D3DDevice, new IntPtr((void*)initialData), desc)
                    {
                        DebugName = Name
                    };
                }
            }
            else
            {
                D3DResource = Native = new D3D11.Buffer(Graphics.D3DDevice, desc)
                {
                    DebugName = Name
                };
            }
        }

        /// <summary>
        /// Function to retrieve a copy of this buffer as a staging resource.
        /// </summary>
        /// <returns>The staging buffer to retrieve.</returns>
        protected override GorgonBufferCommon GetStagingInternal() => GetStaging();

        /// <summary>
        /// Function to retrieve a copy of this buffer as a staging resource.
        /// </summary>
        /// <returns>The staging buffer to retrieve.</returns>
        public GorgonVertexBuffer GetStaging()
        {
            var buffer = new GorgonVertexBuffer(Graphics, new GorgonVertexBufferInfo(_info, $"{Name}_Staging")
            {
                Binding = VertexIndexBufferBinding.None,
                Usage = ResourceUsage.Staging
            });

            CopyTo(buffer);

            return buffer;
        }

        /// <summary>
        /// Function to retrieve the total number of elements in a buffer.
        /// </summary>
        /// <param name="format">The desired format for the view.</param>
        /// <returns>The total number of elements.</returns>
        /// <remarks>
        /// <para>
        /// Use this to retrieve the number of elements based on the <paramref name="format"/> that will be passed to a shader resource view.
        /// </para>
        /// </remarks>
        public int GetTotalElementCount(BufferFormat format) => format == BufferFormat.Unknown ? 0 : GetTotalElementCount(new GorgonFormatInfo(format));

        /// <summary>
        /// Function to create a new <see cref="GorgonVertexBufferReadWriteView"/> for this buffer.
        /// </summary>
        /// <param name="format">The format for the view.</param>
        /// <param name="startElement">[Optional] The first element to start viewing from.</param>
        /// <param name="elementCount">[Optional] The number of elements to view.</param>
        /// <returns>A <see cref="GorgonVertexBufferReadWriteView"/> used to bind the buffer to a shader.</returns>
        /// <exception cref="GorgonException">Thrown when this buffer does not have a <see cref="Binding"/> of <see cref="VertexIndexBufferBinding.UnorderedAccess"/>.
        /// <para>-or-</para>
        /// <para>Thrown when this buffer has a usage of <see cref="ResourceUsage.Staging"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="format"/> is typeless or is not a supported format for unordered access views.</exception>
        /// <remarks>
        /// <para>
        /// This will create an unordered access view that makes a buffer accessible to shaders using unordered access to the data. This allows viewing of the buffer data in a 
        /// different format, or even a subsection of the buffer from within the shader.
        /// </para>
        /// <para>
        /// The <paramref name="format"/> parameter is used present the buffer data as another format type to the shader. 
        /// </para>
        /// <para>
        /// The <paramref name="startElement"/> parameter defines the starting data element to allow access to within the shader. If this value falls outside of the range of available elements, then it 
        /// will be clipped to the upper and lower bounds of the element range. If this value is left at 0, then first element is viewed.
        /// </para>
        /// <para>
        /// The <paramref name="elementCount"/> parameter defines how many elements to allow access to inside of the view. If this value falls outside of the range of available elements, then it will be 
        /// clipped to the upper or lower bounds of the element range. If this value is left at 0, then the entire buffer is viewed.
        /// </para>
        /// </remarks>
        public GorgonVertexBufferReadWriteView GetReadWriteView(BufferFormat format, int startElement = 0, int elementCount = 0)
        {
            if ((Usage == ResourceUsage.Staging)
                || ((Binding & VertexIndexBufferBinding.UnorderedAccess) != VertexIndexBufferBinding.UnorderedAccess))
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_UAV_RESOURCE_NOT_VALID, Name));
            }

            if (!Graphics.FormatSupport.TryGetValue(format, out IGorgonFormatSupportInfo support))
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_UAV_FORMAT_INVALID, format));
            }

            if ((support.FormatSupport & BufferFormatSupport.TypedUnorderedAccessView) != BufferFormatSupport.TypedUnorderedAccessView)
            {
                throw new ArgumentException(string.Format(Resources.GORGFX_ERR_UAV_FORMAT_INVALID, format), nameof(format));
            }

            // Ensure the size of the data type fits the requested format.
            var info = new GorgonFormatInfo(format);

            if (info.IsTypeless)
            {
                throw new ArgumentException(Resources.GORGFX_ERR_VIEW_NO_TYPELESS, nameof(format));
            }

            int totalElementCount = GetTotalElementCount(info);

            startElement = startElement.Min(totalElementCount - 1).Max(0);

            if (elementCount <= 0)
            {
                elementCount = totalElementCount - startElement;
            }

            elementCount = elementCount.Min(totalElementCount - startElement).Max(1);

            var key = new BufferShaderViewKey(startElement, elementCount, format);
            GorgonVertexBufferReadWriteView result = GetReadWriteView<GorgonVertexBufferReadWriteView>(key);

            if (result != null)
            {
                return result;
            }

            result = new GorgonVertexBufferReadWriteView(this, format, info, startElement, elementCount, totalElementCount);
            result.CreateNativeView();
            RegisterReadWriteView(key, result);

            return result;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonVertexBuffer" /> class.
        /// </summary>
        /// <param name="graphics">The <see cref="GorgonGraphics"/> object used to create and manipulate the buffer.</param>
        /// <param name="info">Information used to create the buffer.</param>
        /// <param name="initialData">[Optional] The initial data used to populate the buffer.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, or the <paramref name="info"/> parameters are <b>null</b>.</exception>
        public GorgonVertexBuffer(GorgonGraphics graphics, IGorgonVertexBufferInfo info, GorgonNativeBuffer<byte> initialData = null)
            : base(graphics)
        {
            _info = new GorgonVertexBufferInfo(info ?? throw new ArgumentNullException(nameof(info)));
            Initialize(initialData);
        }
        #endregion
    }
}
