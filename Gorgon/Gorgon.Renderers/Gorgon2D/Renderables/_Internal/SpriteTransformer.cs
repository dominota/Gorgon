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
// Created: June 9, 2018 10:57:03 AM
// 
#endregion

using Gorgon.Graphics;
using DX = SharpDX;

namespace Gorgon.Renderers
{
    /// <summary>
    /// Provides functionality for transforming renderable vertices.
    /// </summary>
    internal class SpriteTransformer
    {
        /// <summary>
        /// Function to build up the renderable vertices.
        /// </summary>
        /// <param name="bounds">The bounds of the renderable.</param>
        /// <param name="anchor">The anchor point for the renderable.</param>
        /// <param name="corners">The corners of the renderable.</param>
        private static void BuildRenderable(ref DX.RectangleF bounds, ref DX.Vector2 anchor, out DX.Vector4 corners)
        {
            var vectorSize = new DX.Vector2(bounds.Size.Width, bounds.Size.Height);
            DX.Vector2 axisOffset = default;

            if (!anchor.IsZero)
            {
                DX.Vector2.Multiply(ref anchor, ref vectorSize, out axisOffset);
            }

            corners = new DX.Vector4(-axisOffset.X, -axisOffset.Y, vectorSize.X - axisOffset.X, vectorSize.Y - axisOffset.Y);
        }

        /// <summary>
        /// Function to update the colors for each corner of the renderable.
        /// </summary>
        /// <param name="vertices">The vertices for the renderable.</param>
        /// <param name="upperLeft">The color of the upper left corner.</param>
        /// <param name="upperRight">The color of the upper right corner.</param>
        /// <param name="lowerLeft">The color of the lower left corner.</param>
        /// <param name="lowerRight">The color of the lower right corner.</param>
        private static void UpdateVertexColors(Gorgon2DVertex[] vertices, in GorgonColor upperLeft, in GorgonColor upperRight, in GorgonColor lowerLeft, in GorgonColor lowerRight)
        {
            vertices[0].Color = upperLeft;
            vertices[1].Color = upperRight;
            vertices[2].Color = lowerLeft;
            vertices[3].Color = lowerRight;
        }

        /// <summary>
        /// Function to update the texture coordinates for the renderable.
        /// </summary>
        /// <param name="vertices">The vertices for the renderable.</param>
        /// <param name="textureRegion">The texture coordinates.</param>
        /// <param name="textureArrayIndex">The index into a texture array.</param>
        /// <param name="horizontalFlip"><b>true</b> if the texture is flipped horizontally.</param>
        /// <param name="verticalFlip"><b>true</b> if the texture is flipped vertically.</param>
        private static void UpdateTextureCoordinates(Gorgon2DVertex[] vertices,
                                                     ref DX.RectangleF textureRegion,
                                                     int textureArrayIndex,
                                                     bool horizontalFlip,
                                                     bool verticalFlip)
        {
            var rightBottom = new DX.Vector3(textureRegion.BottomRight, textureArrayIndex);
            var leftTop = new DX.Vector3(textureRegion.TopLeft, textureArrayIndex);

            if (horizontalFlip)
            {
                leftTop.X = textureRegion.Right;
                rightBottom.X = textureRegion.Left;
            }

            if (verticalFlip)
            {
                leftTop.Y = textureRegion.Bottom;
                rightBottom.Y = textureRegion.Top;
            }

            vertices[0].UV = leftTop;
            vertices[1].UV = new DX.Vector3(rightBottom.X, leftTop.Y, textureArrayIndex);
            vertices[2].UV = new DX.Vector3(leftTop.X, rightBottom.Y, textureArrayIndex);
            vertices[3].UV = rightBottom;
        }

        /// <summary>
        /// Function to transform each vertex of the renderable to change its location, size and rotation.
        /// </summary>
        /// <param name="vertices">The vertices for the renderable.</param>
        /// <param name="corners">The corners of the renderable.</param>
        /// <param name="bounds">The boundaries for the renderable.</param>
        /// <param name="scale">The scale of the renderable.</param>
        /// <param name="angleRads">The cached angle, in radians.</param>
        /// <param name="angleSin">The cached sine of the angle.</param>
        /// <param name="angleCos">The cached cosine of the angle.</param>
        /// <param name="depth">The depth value for the renderable.</param>
        /// <param name="cornerUpperLeft">The upper left corner offset.</param>
        /// <param name="cornerUpperRight">The upper right corner offset.</param>
        /// <param name="cornerLowerLeft">The lower left corner offset.</param>
        /// <param name="cornerLowerRight">The lower right corner offset.</param>
        private static void TransformVertices(Gorgon2DVertex[] vertices,
                                              ref DX.Vector4 corners,
                                              ref DX.RectangleF bounds,
                                              ref DX.Vector2 scale,
                                              float angleRads,
                                              float angleSin,
                                              float angleCos,
                                              float depth,
                                              ref DX.Vector3 cornerUpperLeft,
                                              ref DX.Vector3 cornerUpperRight,
                                              ref DX.Vector3 cornerLowerLeft,
                                              ref DX.Vector3 cornerLowerRight)
        {
            var upperLeft = new DX.Vector2(corners.X + cornerUpperLeft.X, corners.Y + cornerUpperLeft.Y);
            var upperRight = new DX.Vector2(corners.Z + cornerUpperRight.X, corners.Y + cornerUpperRight.Y);
            var lowerRight = new DX.Vector2(corners.Z + cornerLowerRight.X, corners.W + cornerLowerRight.Y);
            var lowerLeft = new DX.Vector2(corners.X + cornerLowerLeft.X, corners.W + cornerLowerLeft.Y);

            if ((scale.X != 1.0f) || (scale.Y != 1.0f))
            {
                DX.Vector2.Multiply(ref upperLeft, ref scale, out upperLeft);
                DX.Vector2.Multiply(ref upperRight, ref scale, out upperRight);
                DX.Vector2.Multiply(ref lowerRight, ref scale, out lowerRight);
                DX.Vector2.Multiply(ref lowerLeft, ref scale, out lowerLeft);
            }

            ref Gorgon2DVertex v1 = ref vertices[0];
            ref Gorgon2DVertex v2 = ref vertices[1];
            ref Gorgon2DVertex v3 = ref vertices[2];
            ref Gorgon2DVertex v4 = ref vertices[3];

            if (angleRads != 0.0f)
            {
                v1.Position.X = ((upperLeft.X * angleCos) - (upperLeft.Y * angleSin)) + bounds.X;
                v1.Position.Y = ((upperLeft.X * angleSin) + (upperLeft.Y * angleCos)) + bounds.Y;
                v1.Position.Z = depth + cornerUpperLeft.Z;
                v1.Angle = new DX.Vector2(angleCos, angleSin);

                v2.Position.X = ((upperRight.X * angleCos) - (upperRight.Y * angleSin)) + bounds.X;
                v2.Position.Y = ((upperRight.X * angleSin) + (upperRight.Y * angleCos)) + bounds.Y;
                v2.Position.Z = depth + cornerUpperRight.Z;
                v2.Angle = new DX.Vector2(angleCos, angleSin);

                v3.Position.X = ((lowerLeft.X * angleCos) - (lowerLeft.Y * angleSin)) + bounds.X;
                v3.Position.Y = ((lowerLeft.X * angleSin) + (lowerLeft.Y * angleCos)) + bounds.Y;
                v3.Position.Z = depth + cornerLowerLeft.Z;
                v3.Angle = new DX.Vector2(angleCos, angleSin);

                v4.Position.X = ((lowerRight.X * angleCos) - (lowerRight.Y * angleSin)) + bounds.X;
                v4.Position.Y = ((lowerRight.X * angleSin) + (lowerRight.Y * angleCos)) + bounds.Y;
                v4.Position.Z = depth + cornerLowerRight.Z;
                v4.Angle = new DX.Vector2(angleCos, angleSin);
            }
            else
            {
                v1.Position.X = upperLeft.X + bounds.X;
                v1.Position.Y = upperLeft.Y + bounds.Y;
                v1.Position.Z = depth + cornerUpperLeft.Z;
                v1.Angle = DX.Vector2.UnitX;
                v2.Position.X = upperRight.X + bounds.X;
                v2.Position.Y = upperRight.Y + bounds.Y;
                v2.Position.Z = depth + cornerUpperRight.Z;
                v2.Angle = DX.Vector2.UnitX;
                v3.Position.X = lowerLeft.X + bounds.X;
                v3.Position.Y = lowerLeft.Y + bounds.Y;
                v3.Position.Z = depth + cornerLowerLeft.Z;
                v3.Angle = DX.Vector2.UnitX;
                v4.Position.X = lowerRight.X + bounds.X;
                v4.Position.Y = lowerRight.Y + bounds.Y;
                v4.Position.Z = depth + cornerLowerRight.Z;
                v4.Angle = DX.Vector2.UnitX;
            }
        }

        /// <summary>
        /// Function to transform the vertices for a renderable.
        /// </summary>
        /// <param name="renderable">The renderable to transform.</param>
        public void Transform(BatchRenderable renderable)
        {
            Gorgon2DVertex[] vertices = renderable.Vertices;

            if (renderable.HasVertexChanges)
            {
                BuildRenderable(ref renderable.Bounds, ref renderable.Anchor, out renderable.Corners);

                // If we've updated the physical dimensions for the renderable, then we need to update the transform as well.
                renderable.HasTransformChanges = true;
                renderable.HasVertexChanges = false;
            }

            if (renderable.HasTransformChanges)
            {
                TransformVertices(vertices,
                                  ref renderable.Corners,
                                  ref renderable.Bounds,
                                  ref renderable.Scale,
                                  renderable.AngleRads,
                                  renderable.AngleSin,
                                  renderable.AngleCos,
                                  renderable.Depth,
                                  ref renderable.UpperLeftOffset,
                                  ref renderable.UpperRightOffset,
                                  ref renderable.LowerLeftOffset,
                                  ref renderable.LowerRightOffset);

                renderable.HasTransformChanges = false;
            }

            if (renderable.HasVertexColorChanges)
            {
                UpdateVertexColors(vertices, in renderable.UpperLeftColor, in renderable.UpperRightColor, in renderable.LowerLeftColor, in renderable.LowerRightColor);
                renderable.HasVertexColorChanges = false;
            }

            if (!renderable.HasTextureChanges)
            {
                return;
            }

            UpdateTextureCoordinates(vertices, ref renderable.TextureRegion, renderable.TextureArrayIndex, renderable.HorizontalFlip, renderable.VerticalFlip);
            renderable.HasTextureChanges = false;
        }
    }
}
