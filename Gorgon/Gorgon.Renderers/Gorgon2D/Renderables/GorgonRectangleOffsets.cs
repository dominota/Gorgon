﻿#region MIT
// 
// Gorgon.
// Copyright (C) 2017 Michael Winsor
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
// Created: February 8, 2017 7:22:29 PM
// 
#endregion
using System;
using System.Collections;
using System.Collections.Generic;
using DX = SharpDX;

namespace Gorgon.Renderers
{
    /// <summary>
    /// Defines the offsets for each corner of a rectangle.
    /// </summary>
    public class GorgonRectangleOffsets
        : IReadOnlyList<DX.Vector3>
    {
        #region Variables.
        // The renderable object to update.
        private readonly BatchRenderable _renderable;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the corner offset value by index.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is not between 0 and 3.</exception>
        /// <remarks>
        /// The ordering of the indices is as follows: 0 - Upper left, 1 - Upper right, 2 - Lower right, 3 - Lower left.
        /// </remarks>
        public DX.Vector3 this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return _renderable.UpperLeftOffset;
                    case 1:
                        return _renderable.UpperRightOffset;
                    case 2:
                        return _renderable.LowerRightOffset;
                    case 3:
                        return _renderable.LowerLeftOffset;
                }

#pragma warning disable CA1065 // Do not raise exceptions in unexpected locations
                throw new ArgumentOutOfRangeException();
#pragma warning restore CA1065 // Do not raise exceptions in unexpected locations
            }
            set
            {
                switch (index)
                {
                    case 0:
                        UpperLeft = value;
                        return;
                    case 1:
                        UpperRight = value;
                        return;
                    case 2:
                        LowerRight = value;
                        return;
                    case 3:
                        LowerLeft = value;
                        return;
                }

                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Property to set or return the offset of the upper left corner.
        /// </summary>
        public DX.Vector3 UpperLeft
        {
            get => _renderable.UpperLeftOffset;
            set
            {
                if (_renderable.UpperLeftOffset.Equals(ref value))
                {
                    return;
                }

                _renderable.UpperLeftOffset = value;
                _renderable.HasTransformChanges = true;
            }
        }

        /// <summary>
        /// Property to set or return the offset of the upper right corner.
        /// </summary>
        public DX.Vector3 UpperRight
        {
            get => _renderable.UpperRightOffset;
            set
            {
                if (_renderable.UpperRightOffset.Equals(ref value))
                {
                    return;
                }

                _renderable.UpperRightOffset = value;
                _renderable.HasTransformChanges = true;
            }
        }

        /// <summary>
        /// Property to set or return the offset of the lower left corner.
        /// </summary>
        public DX.Vector3 LowerLeft
        {
            get => _renderable.LowerLeftOffset;
            set
            {
                if (_renderable.LowerLeftOffset.Equals(ref value))
                {
                    return;
                }

                _renderable.LowerLeftOffset = value;
                _renderable.HasTransformChanges = true;
            }
        }

        /// <summary>
        /// Property to set or return the offset of the lower right corner.
        /// </summary>
        public DX.Vector3 LowerRight
        {
            get => _renderable.LowerRightOffset;
            set
            {
                if (_renderable.LowerRightOffset.Equals(ref value))
                {
                    return;
                }

                _renderable.LowerRightOffset = value;
                _renderable.HasTransformChanges = true;
            }
        }

        /// <summary>Gets the number of elements in the collection.</summary>
        public int Count => 4;

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<DX.Vector3> GetEnumerator()
        {
            for (int i = 0; i < 4; ++i)
            {
                yield return this[i];
            }
        }

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = 0; i < 4; ++i)
            {
                yield return this[i];
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to assign a single offset to all corners.
        /// </summary>
        /// <param name="offset">The offset to assign.</param>
        public void SetAll(DX.Vector3 offset)
        {
            if ((offset.Equals(ref _renderable.LowerLeftOffset))
                && (offset.Equals(ref _renderable.LowerRightOffset))
                && (offset.Equals(ref _renderable.UpperRightOffset))
                && (offset.Equals(ref _renderable.UpperLeftOffset)))
            {
                return;
            }

            _renderable.LowerLeftOffset = _renderable.LowerRightOffset = _renderable.UpperRightOffset = _renderable.UpperLeftOffset;
        }

        /// <summary>
        /// Function to copy the offsets into the specified destination.
        /// </summary>
        /// <param name="destination">The destination that will receive the copy of the offsets.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="destination"/> parameter is <b>null</b>.</exception>
        public void CopyTo(GorgonRectangleOffsets destination)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            destination.LowerLeft = LowerLeft;
            destination.LowerRight = LowerRight;
            destination.UpperRight = UpperRight;
            destination.UpperLeft = UpperLeft;
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonRectangleOffsets"/> class.
        /// </summary>
        /// <param name="renderable">The renderable to update.</param>
        internal GorgonRectangleOffsets(BatchRenderable renderable) => _renderable = renderable;
        #endregion
    }
}
