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
// Created: Friday, August 9, 2013 3:16:00 PM
// 
#endregion

using System;
using System.Runtime.InteropServices;
using Gorgon.Core;

namespace Gorgon.Renderers
{
    /// <summary>
    /// An immutable value for alpha testing.
    /// </summary>
    /// <remarks>This will define the range of alpha values to clip.  An alpha value that falls between the lower and upper range will not be rendered.</remarks>
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 16)]
    internal readonly struct AlphaTestData
        : IEquatable<AlphaTestData>
    {
        #region Variables.
        /// <summary>
        /// 4 byte compatiable flag for constant buffer.
        /// </summary>
        public readonly int IsEnabled;

        /// <summary>
        /// Lower alpha value.
        /// </summary>
        /// <remarks>If the alpha is higher than this value, it will be clipped.</remarks>
        public readonly float LowerAlpha;

        /// <summary>
        /// Upper alpha value.
        /// </summary>
        /// <remarks>If the alpha is lower than this value, it will be clipped.</remarks>
        public readonly float UpperAlpha;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to determine equality between two instances.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool Equals(in AlphaTestData left, in AlphaTestData right) =>
            // ReSharper disable CompareOfFloatsByEqualityOperator
            ((left.IsEnabled == right.IsEnabled)
                    && (left.UpperAlpha == right.UpperAlpha)
                    && (left.LowerAlpha == right.LowerAlpha));// ReSharper restore CompareOfFloatsByEqualityOperator

        /// <summary>
        /// Determines whether the specified <see cref="object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns>
        ///   <b>true</b> if the specified <see cref="object" /> is equal to this instance; otherwise, <b>false</b>.
        /// </returns>
        public override bool Equals(object obj) => obj is AlphaTestData data ? data.Equals(this) : base.Equals(obj);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => 281.GenerateHash(IsEnabled).GenerateHash(LowerAlpha).GenerateHash(UpperAlpha);
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="AlphaTestData"/> struct.
        /// </summary>
        /// <param name="isEnabled"><b>true</b> to enable alpha testing, <b>false</b> to disable.</param>
        /// <param name="alphaRange">The alpha range to clip.</param>
        public AlphaTestData(bool isEnabled, GorgonRangeF alphaRange)
        {
            IsEnabled = isEnabled ? 1 : 0;
            LowerAlpha = alphaRange.Minimum;
            UpperAlpha = alphaRange.Maximum;
        }
        #endregion

        #region IEquatable<Gorgon2DAlphaTest> Members
        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool Equals(AlphaTestData other) => Equals(in this, in other);
        #endregion
    }
}
