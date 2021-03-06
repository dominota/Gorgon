﻿#region MIT
// 
// Gorgon.
// Copyright (C) 2019 Michael Winsor
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
// Created: March 15, 2019 12:38:55 PM
// 
#endregion

using Gorgon.Math;
using DX = SharpDX;

namespace Gorgon.Graphics
{
    /// <summary>
    /// Extension methods for the SharpDX rectangle and rectanglef types.
    /// </summary>
    public static class SharpDXRectExtensions
    {
        /// <summary>
        /// Function to convert an integer rectangle to a floating point rectangle/
        /// </summary>
        /// <param name="rect">The rectangle to convert.</param>
        /// <returns>The converted rectangle.</returns>
        public static DX.RectangleF ToRectangleF(this DX.Rectangle rect) => new DX.RectangleF(rect.X, rect.Y, rect.Width, rect.Height);

        /// <summary>
        /// Function to truncate the rectangle coordinates to the whole number portion of their values.
        /// </summary>
        /// <param name="rect">The rectangle to truncate.</param>
        /// <returns>The truncated rectangle.</returns>
        /// <remarks>
        /// <para>
        /// This method converts the coordinates to integer values without applying rounding.
        /// </para>
        /// </remarks>
        public static DX.RectangleF Truncate(this DX.RectangleF rect) => new DX.RectangleF((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);

        /// <summary>
        /// Function to set the rectangle coordinates to the nearest integer values that are lower than or equal to the original values.
        /// </summary>
        /// <param name="rect">The rectangle to floor.</param>
        /// <returns>The truncated rectangle.</returns>
        public static DX.RectangleF Floor(this DX.RectangleF rect) => new DX.RectangleF(rect.X.FastFloor(), rect.Y.FastFloor(), rect.Width.FastFloor(), rect.Height.FastFloor());

        /// <summary>
        /// Function to set the rectangle coordinates to the nearest integer values that are higher than or equal to the original values.
        /// </summary>
        /// <param name="rect">The rectangle to floor.</param>
        /// <returns>The truncated rectangle.</returns>
        public static DX.RectangleF Ceiling(this DX.RectangleF rect) => new DX.RectangleF(rect.X.FastCeiling(), rect.Y.FastCeiling(), rect.Width.FastCeiling(), rect.Height.FastCeiling());

        /// <summary>
        /// Function to convert a floating point rectangle to an integer rectangle/
        /// </summary>
        /// <param name="rect">The rectangle to convert.</param>
        /// <returns>The converted rectangle.</returns>
        /// <remarks>
        /// <para>
        /// This method converts the coordinates to integer values without applying rounding.
        /// </para>
        /// </remarks>
        public static DX.Rectangle ToRectangle(this DX.RectangleF rect) => new DX.Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
    }
}
