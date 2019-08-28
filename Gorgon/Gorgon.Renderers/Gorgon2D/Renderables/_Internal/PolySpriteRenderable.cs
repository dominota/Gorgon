﻿#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: August 9, 2018 11:37:42 AM
// 
#endregion

using Gorgon.Graphics.Core;
using DX = SharpDX;

namespace Gorgon.Renderers
{
    /// <summary>
    /// A renderable used by a polygon sprite.
    /// </summary>
    class PolySpriteRenderable
        : BatchRenderable
    {
        /// <summary>
        /// The vertex buffer for the polygon renderable.
        /// </summary>
        public GorgonVertexBufferBinding VertexBuffer;

        /// <summary>
        /// The index buffer for the polygon renderable.
        /// </summary>
        public GorgonIndexBuffer IndexBuffer;

        /// <summary>
        /// The world matrix for the polygon renderable.
        /// </summary>
        public DX.Matrix WorldMatrix;

        /// <summary>
        /// The transformation data for textures.
        /// </summary>
        public DX.Vector4 TextureTransform;
    }
}
