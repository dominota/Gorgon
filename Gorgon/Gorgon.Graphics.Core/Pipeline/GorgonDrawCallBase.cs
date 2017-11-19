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
// Created: December 19, 2016 11:26:44 AM
// 
#endregion

using System;
using D3D = SharpDX.Direct3D;

namespace Gorgon.Graphics.Core
{
    #region Enums
    /// <summary>
    /// Specifies the type of primitive geometry to render from vertex data bound to the pipeline.
    /// </summary>
    public enum PrimitiveType
    {
        /// <summary>
        /// <para>
        /// The IA stage has not been initialized with a primitive topology. The IA stage will not function properly unless a primitive topology is defined.
        /// </para>
        /// </summary>
        None = D3D.PrimitiveTopology.Undefined,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a list of points.
        /// </para>
        /// </summary>
        PointList = D3D.PrimitiveTopology.PointList,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a list of lines.
        /// </para>
        /// </summary>
        LineList = D3D.PrimitiveTopology.LineList,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a line strip.
        /// </para>
        /// </summary>
        LineStrip = D3D.PrimitiveTopology.LineStrip,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a list of triangles.
        /// </para>
        /// </summary>
        TriangleList = D3D.PrimitiveTopology.TriangleList,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a triangle strip.
        /// </para>
        /// </summary>
        TriangleStrip = D3D.PrimitiveTopology.TriangleStrip,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as list of lines with adjacency data.
        /// </para>
        /// </summary>
        LineListWithAdjacency = D3D.PrimitiveTopology.LineListWithAdjacency,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as line strip with adjacency data.
        /// </para>
        /// </summary>
        LineStripWithAdjacency = D3D.PrimitiveTopology.LineStripWithAdjacency,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as list of triangles with adjacency data.
        /// </para>
        /// </summary>
        TriangleListWithAdjacency = D3D.PrimitiveTopology.TriangleListWithAdjacency,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as triangle strip with adjacency data.
        /// </para>
        /// </summary>
        TriangleStripWithAdjacency = D3D.PrimitiveTopology.TriangleStripWithAdjacency,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith1ControlPoints = D3D.PrimitiveTopology.PatchListWith1ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith2ControlPoints = D3D.PrimitiveTopology.PatchListWith2ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith3ControlPoints = D3D.PrimitiveTopology.PatchListWith3ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith4ControlPoints = D3D.PrimitiveTopology.PatchListWith4ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith5ControlPoints = D3D.PrimitiveTopology.PatchListWith5ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith6ControlPoints = D3D.PrimitiveTopology.PatchListWith6ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith7ControlPoints = D3D.PrimitiveTopology.PatchListWith7ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith8ControlPoints = D3D.PrimitiveTopology.PatchListWith8ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith9ControlPoints = D3D.PrimitiveTopology.PatchListWith9ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith10ControlPoints = D3D.PrimitiveTopology.PatchListWith10ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith11ControlPoints = D3D.PrimitiveTopology.PatchListWith11ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith12ControlPoints = D3D.PrimitiveTopology.PatchListWith12ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith13ControlPoints = D3D.PrimitiveTopology.PatchListWith13ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith14ControlPoints = D3D.PrimitiveTopology.PatchListWith14ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith15ControlPoints = D3D.PrimitiveTopology.PatchListWith15ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith16ControlPoints = D3D.PrimitiveTopology.PatchListWith16ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith17ControlPoints = D3D.PrimitiveTopology.PatchListWith17ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith18ControlPoints = D3D.PrimitiveTopology.PatchListWith18ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith19ControlPoints = D3D.PrimitiveTopology.PatchListWith19ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith20ControlPoints = D3D.PrimitiveTopology.PatchListWith20ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith21ControlPoints = D3D.PrimitiveTopology.PatchListWith21ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith22ControlPoints = D3D.PrimitiveTopology.PatchListWith22ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith23ControlPoints = D3D.PrimitiveTopology.PatchListWith23ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith24ControlPoints = D3D.PrimitiveTopology.PatchListWith24ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith25ControlPoints = D3D.PrimitiveTopology.PatchListWith25ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith26ControlPoints = D3D.PrimitiveTopology.PatchListWith26ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith27ControlPoints = D3D.PrimitiveTopology.PatchListWith27ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith28ControlPoints = D3D.PrimitiveTopology.PatchListWith28ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith29ControlPoints = D3D.PrimitiveTopology.PatchListWith29ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith30ControlPoints = D3D.PrimitiveTopology.PatchListWith30ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith31ControlPoints = D3D.PrimitiveTopology.PatchListWith31ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith32ControlPoints = D3D.PrimitiveTopology.PatchListWith32ControlPoints
    }

    /// <summary>
    /// Flags to indicate which part of the pipeline resources need updating.
    /// </summary>
    [Flags]
	internal enum PipelineResourceChange
	{
		/// <summary>
		/// Nothing has changed, just draw the item.
		/// </summary>
		None = 0,
		/// <summary>
		/// The primitive topology has changed.
		/// </summary>
		PrimitiveTopology = 0x1,
		/// <summary>
		/// Vertex buffers have changed.
		/// </summary>
		VertexBuffers = 0x8,
		/// <summary>
		/// Index buffer has changed.
		/// </summary>
		IndexBuffer = 0x10,
		/// <summary>
		/// Input layout has changed.
		/// </summary>
		InputLayout = 0x20,
		/// <summary>
		/// Vertex shader constant buffers have changed.
		/// </summary>
		VertexShaderConstantBuffers = 0x80,
		/// <summary>
		/// Pixel shader constant buffers have changed.
		/// </summary>
		PixelShaderConstantBuffers = 0x100,
		/// <summary>
		/// Geometry shader constant buffers have changed.
		/// </summary>
		GeometryShaderConstantBuffers = 0x200,
		/// <summary>
		/// Hull shader constant buffers have changed.
		/// </summary>
		HullShaderConstantBuffers = 0x400,
		/// <summary>
		/// Domain shader constant buffers have changed.
		/// </summary>
		DomainShaderConstantBuffers = 0x800,
		/// <summary>
		/// Compute shader constant buffers have changed.
		/// </summary>
		ComputeShaderConstantBuffers = 0x1_000,
		/// <summary>
		/// Vertex shader resources have changed.
		/// </summary>
		VertexShaderResources = 0x2_000,
		/// <summary>
		/// Pixel shader resources have changed.
		/// </summary>
		PixelShaderResources = 0x4_000,
		/// <summary>
		/// Geometry shader resources have changed.
		/// </summary>
		GeometryShaderResources = 0x8_000,
		/// <summary>
		/// Hull shader resources have changed.
		/// </summary>
		HullShaderResources = 0x10_000,
		/// <summary>
		/// Domain shader resources have changed.
		/// </summary>
		DomainShaderResources = 0x20_000,
		/// <summary>
		/// Compute shader resources have changed.
		/// </summary>
		ComputeShaderResources = 0x40_000,
		/// <summary>
		/// Vertex shader samplers have changed.
		/// </summary>
		VertexShaderSamplers = 0x80_000,
		/// <summary>
		/// Pixel shader samplers have changed.
		/// </summary>
		PixelShaderSamplers = 0x100_000,
		/// <summary>
		/// Geometry shader samplers have changed.
		/// </summary>
		GeometryShaderSamplers = 0x200_000,
		/// <summary>
		/// Hull shader samplers have changed.
		/// </summary>
		HullShaderSamplers = 0x400_000,
		/// <summary>
		/// Domain shader samplers have changed.
		/// </summary>
		DomainShaderSamplers = 0x800_000,
		/// <summary>
		/// Compute shader samplers have changed.
		/// </summary>
		ComputeShaderSamplers = 0x1_000_000,
		/// <summary>
		/// The blending factor has been updated.
		/// </summary>
		BlendFactor = 0x2_000_000,
		/// <summary>
		/// The blending sample mask has been updated.
		/// </summary>
		BlendSampleMask = 0x4_000_000,
		/// <summary>
		/// The depth/stencil reference value has been updated.
		/// </summary>
		DepthStencilReference = 0x8_000_000,
        /// <summary>
        /// The unordered access views have been updated.
        /// </summary>
        Uavs = 0x10_000_000,
        /// <summary>
        /// The stream out bindings have changed.
        /// </summary>
        StreamOut = 0x20_000_000,
		/// <summary>
		/// All states changed.
		/// </summary>
		All = BlendFactor
			| BlendSampleMask
			| ComputeShaderConstantBuffers
			| ComputeShaderResources
			| ComputeShaderSamplers
			| DepthStencilReference
			| DomainShaderResources
			| DomainShaderConstantBuffers
			| DomainShaderResources
			| GeometryShaderConstantBuffers
			| GeometryShaderResources
			| GeometryShaderSamplers
			| HullShaderResources
			| HullShaderConstantBuffers
			| HullShaderSamplers
			| PixelShaderConstantBuffers
			| PixelShaderResources
			| PixelShaderSamplers
            | Uavs
			| VertexShaderConstantBuffers
			| VertexShaderResources
			| VertexShaderSamplers
			| IndexBuffer
			| InputLayout
			| VertexBuffers
			| PrimitiveTopology
            | StreamOut
	}
    #endregion

    /// <summary>
    /// A common class for draw calls.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A draw call is used to submit vertex (and potentially index and instance) data to the GPU pipeline or an output buffer. This type will contain all the necessary information used to set the state of the 
    /// pipeline prior to rendering any data.
    /// </para>
    /// </remarks>
    public class GorgonDrawCallBase 
        : IGorgonShaderStates
	{
		#region Properties.
		/// <summary>
		/// Property to set or return the vertex buffers to bind to the pipeline.
		/// </summary>
		public GorgonVertexBufferBindings VertexBuffers
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the index buffer to bind to the pipeline.
		/// </summary>
		public GorgonIndexBuffer IndexBuffer
		{
			get;
			set;
		}

        /// <summary>
		/// Property to return the constant buffers for the vertex shader.
		/// </summary>
		public GorgonConstantBuffers VertexShaderConstantBuffers
		{
			get;
		}

		/// <summary>
		/// Property to return the constant buffers for the pixel shader.
		/// </summary>
		public GorgonConstantBuffers PixelShaderConstantBuffers
		{
			get;
		}

		/// <summary>
		/// Property to return the vertex shader resources to bind to the pipeline.
		/// </summary>
		public GorgonShaderResourceViews VertexShaderResourceViews
		{
			get;
		}

		/// <summary>
		/// Property to set or return the list of pixel shader resource views.
		/// </summary>
		public GorgonShaderResourceViews PixelShaderResourceViews
		{
			get;
		}

		/// <summary>
		/// Property to return the pixel shader samplers to bind to the pipeline.
		/// </summary>
		public GorgonSamplerStates PixelShaderSamplers
		{
			get;
		}

        /// <summary>
        /// Property to return the unordered access views to bind to the pipeline.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This will make <see cref="GorgonUnorderedAccessView"/> objects visible to a <see cref="GorgonPixelShader"/>.  
        /// </para>
        /// <para>
        /// An unordered access view uses the same slots as <see cref="GorgonRenderTargetView"/> objects. Thus, if there is a render target already occupying the same slot, it will be unbound and replaced 
        /// by the <see cref="GorgonUnorderedAccessView"/> at that slot. Conversely, if there is an unordered access view bound to a specific slot, and a render target is assigned to that slot, then the 
        /// unordered access view will be unbound.
        /// </para>
        /// <para>
        /// <note type="information">
        /// <para>
        /// These views require a feature set of <c>Level_11_0</c> or better. For devices that support a feature set of <c>Level_11_1</c>, these views can be accessed from any shader stage. However, on 
        /// devices with only <c>Level_11_0</c> support, only the pixel and compute shaders can access unordered access views.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
	    public GorgonUavBindings UnorderedAccessViews
	    {
	        get;
	    }

		/// <summary>
		/// Property to return the vertex shader samplers to bind to the pipeline.
		/// </summary>
		/// <remarks>
		/// <para>
		/// <note type="important">
		/// <para>
		/// This only applies to an <see cref="IGorgonVideoAdapter"/> that has a <see cref="IGorgonVideoAdapter.RequestedFeatureLevel"/> of <c>Level_11_0</c> or better.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public GorgonSamplerStates VertexShaderSamplers
		{
			get;
		}

        /// <summary>
        /// Property to return the geometry shader resources to bind to the pipeline.
        /// </summary>
	    public GorgonShaderResourceViews GeometryShaderResourceViews
	    {
	        get;
	    }

	    /// <summary>
	    /// Property to return the geometry shader samplers to bind to the pipeline.
	    /// </summary>
	    public GorgonSamplerStates GeometryShaderSamplers
	    {
	        get;
	    }

        /// <summary>
        /// Property to return the geometry shader constant buffers to bind to the pipeline.
        /// </summary>
	    public GorgonConstantBuffers GeometryShaderConstantBuffers
	    {
	        get;
	    }

	    /// <summary>
	    /// Property to return the hull shader resources to bind to the pipeline.
	    /// </summary>
	    public GorgonShaderResourceViews HullShaderResourceViews
	    {
	        get;
	    }

	    /// <summary>
	    /// Property to return the hull shader samplers to bind to the pipeline.
	    /// </summary>
	    public GorgonSamplerStates HullShaderSamplers
	    {
	        get;
	    }

	    /// <summary>
	    /// Property to return the hull shader constant buffers to bind to the pipeline.
	    /// </summary>
	    public GorgonConstantBuffers HullShaderConstantBuffers
	    {
	        get;
	    }

	    /// <summary>
	    /// Property to return the domain shader resources to bind to the pipeline.
	    /// </summary>
	    public GorgonShaderResourceViews DomainShaderResourceViews
	    {
	        get;
	    }

	    /// <summary>
	    /// Property to return the domain shader samplers to bind to the pipeline.
	    /// </summary>
	    public GorgonSamplerStates DomainShaderSamplers
	    {
	        get;
	    }

	    /// <summary>
	    /// Property to return the domain shader constant buffers to bind to the pipeline.
	    /// </summary>
	    public GorgonConstantBuffers DomainShaderConstantBuffers
	    {
	        get;
	    }

        /// <summary>
        /// Property to return the type of primitives to draw.
        /// </summary>
        public PrimitiveType PrimitiveType
		{
			get;
			set;
		} = PrimitiveType.TriangleList;

		/// <summary>
		/// Property to set or return the current pipeline state.
		/// </summary>
		/// <remarks>
		/// If this value is <b>null</b>, then the previous state will remain set.
		/// </remarks>
		public GorgonPipelineState PipelineState
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the factor used to modulate the pixel shader, render target or both.
		/// </summary>
		/// <remarks>
		/// To use this value, ensure that the blend state was creating using <c>Factor</c> operation.
		/// </remarks>
		public GorgonColor BlendFactor
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the mask used to define which samples get updated in the active render targets.
		/// </summary>
		public int BlendSampleMask
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the depth/stencil reference value used when performing a depth/stencil test.
		/// </summary>
		public int DepthStencilReference
		{
			get;
			set;
		}

        /// <summary>
        /// Property to set or return the bindings used for stream output.
        /// </summary>
	    public GorgonStreamOutBindings StreamOutBuffers
	    {
	        get;
	        set;
	    }
		#endregion

		#region Methods.
		/// <summary>
		/// Function to reset the states on this draw call back to an initialized state.
		/// </summary>
		public void Reset()
		{
			PrimitiveType = PrimitiveType.TriangleList;
			VertexBuffers = null;
			IndexBuffer = null;
			PipelineState = null;
		    StreamOutBuffers = null;

			unchecked
			{
				BlendSampleMask = (int)(0xffffffff);
			}

			BlendFactor = GorgonColor.White;
			DepthStencilReference = 0;
			
			VertexShaderResourceViews.Clear();
			VertexShaderConstantBuffers.Clear();
			VertexShaderSamplers.Clear();

			PixelShaderResourceViews.Clear();
			PixelShaderConstantBuffers.Clear();
			PixelShaderSamplers.Clear();
            UnorderedAccessViews.Clear();

            GeometryShaderResourceViews.Clear();
            GeometryShaderSamplers.Clear();
            GeometryShaderConstantBuffers.Clear();
		    HullShaderResourceViews.Clear();
		    HullShaderSamplers.Clear();
		    HullShaderConstantBuffers.Clear();
		    DomainShaderResourceViews.Clear();
		    DomainShaderSamplers.Clear();
		    DomainShaderConstantBuffers.Clear();
		}
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonDrawCallBase"/> class.
        /// </summary>
        protected internal GorgonDrawCallBase()
		{
			unchecked
			{
				BlendSampleMask = (int)(0xffffffff);
				BlendFactor = GorgonColor.White;
				DepthStencilReference = 0;
			}

			VertexShaderConstantBuffers = new GorgonConstantBuffers();
			PixelShaderConstantBuffers = new GorgonConstantBuffers();
			PixelShaderResourceViews = new GorgonShaderResourceViews();
			VertexShaderResourceViews = new GorgonShaderResourceViews();
			PixelShaderSamplers = new GorgonSamplerStates();
			VertexShaderSamplers = new GorgonSamplerStates();
            UnorderedAccessViews = new GorgonUavBindings();

            GeometryShaderResourceViews = new GorgonShaderResourceViews();
            GeometryShaderSamplers = new GorgonSamplerStates();
            GeometryShaderConstantBuffers = new GorgonConstantBuffers();
		    HullShaderResourceViews = new GorgonShaderResourceViews();
		    HullShaderSamplers = new GorgonSamplerStates();
		    HullShaderConstantBuffers = new GorgonConstantBuffers();
		    DomainShaderResourceViews = new GorgonShaderResourceViews();
		    DomainShaderSamplers = new GorgonSamplerStates();
		    DomainShaderConstantBuffers = new GorgonConstantBuffers();
		}
        #endregion
    }
}
