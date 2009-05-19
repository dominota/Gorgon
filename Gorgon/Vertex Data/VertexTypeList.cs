#region MIT.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
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
// Created: Sunday, July 23, 2006 1:35:48 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Drawing = System.Drawing;

namespace GorgonLibrary.Internal
{
	/// <summary>
	/// Object representing a list of vertex types.
	/// </summary>
	public class VertexTypeList
		: BaseCollection<VertexType>, IDisposable
	{
		#region Value Types.
		/// <summary>
		/// Value type describing a sprite vertex.
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct PositionDiffuse2DTexture1
		{
			#region Variables.
			/// <summary>
			/// Position of the vertex.
			/// </summary>
			public Vector3D Position;

			/// <summary>
			/// Color of the vertex.
			/// </summary>
			public int Color;

			/// <summary>
			/// Texture coordinates.
			/// </summary>
			public Vector2D TextureCoordinates;
			#endregion

			#region Constructor.
			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="position">Position of the vertex.</param>
			/// <param name="color">Color of the vertex.</param>
			/// <param name="textureCoordinates">Texture coordinates.</param>
			public PositionDiffuse2DTexture1(Vector3D position, Drawing.Color color, Vector2D textureCoordinates)
			{
				// Copy data.
				Position = position;
				Color = color.ToArgb();
				TextureCoordinates = textureCoordinates;
			}
			#endregion
		}
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return a vertex type by index.
		/// </summary>
		public VertexType this[int index]
		{
			get
			{
				return GetItem(index);
			}
		}

		/// <summary>
		/// Property to return a vertex type by its key name.
		/// </summary>
		public VertexType this[string key]
		{
			get
			{
				return GetItem(key);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to clear the list.
		/// </summary>
		protected void Clear()
		{
			// Destroy all the vertex types.
			foreach (VertexType vertexType in this)
				vertexType.Dispose();

			base.ClearItems();
		}

		/// <summary>
		/// Function to create the vertex types.
		/// </summary>
		protected void CreateVertexTypes()
		{
			VertexType newType;		// Vertex type.

			// Position, Diffuse, Normal, 1 2D Texture Coord.
			newType = new VertexType();
			newType.CreateField(0, 0, VertexFieldContext.Position, VertexFieldType.Float3);
			newType.CreateField(0, 12, VertexFieldContext.Diffuse, VertexFieldType.Color);
			newType.CreateField(0, 16, VertexFieldContext.TexCoords, VertexFieldType.Float2);

			AddItem("PositionDiffuse2DTexture1", newType);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		internal VertexTypeList()
			: base(16, true)
		{
			CreateVertexTypes();
		}
		#endregion

		#region IDisposable Members

		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		/// <param name="disposing">TRUE to release all resources, FALSE to only release unmanaged.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
				Clear();
		}

		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
