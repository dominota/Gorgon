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
// Created: Tuesday, February 19, 2013 9:13:28 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WIC = SharpDX.WIC;

namespace GorgonLibrary.IO
{
    /// <summary>
    /// Filter to apply for compression optimization.
    /// </summary>
    public enum PNGFilter
    {
        /// <summary>
        /// The system will chose the best filter based on the image data.
        /// </summary>
        DontCare = WIC.PngFilterOption.Unspecified,
        /// <summary>
        /// No filtering.
        /// </summary>
        None = WIC.PngFilterOption.None,
        /// <summary>
        /// Sub filtering.
        /// </summary>
        Sub = WIC.PngFilterOption.Sub,
        /// <summary>
        /// Up filtering.
        /// </summary>
        Up = WIC.PngFilterOption.Up,
        /// <summary>
        /// Average filtering.
        /// </summary>
        Average = WIC.PngFilterOption.Average,
        /// <summary>
        /// Paeth filtering.
        /// </summary>
        Paeth = WIC.PngFilterOption.Paeth,
        /// <summary>
        /// Adaptive filtering.  The system will choose the best filter based on a per-scanline basis.
        /// </summary>
        Adaptive = WIC.PngFilterOption.Adaptive
    }

    /// <summary>
    /// A codec to handle read/writing of PNG files.
    /// </summary>
    /// <remarks>A codec allows for reading and/or writing of data in an encoded format.  Users may inherit from this object to define their own 
    /// image formats, or use one of the predefined image codecs available in Gorgon.
    /// <para>This format requires that the Windows Imaging Components are installed on the system.</para>
    /// </remarks>
    public sealed class GorgonCodecPNG
        : GorgonCodecWIC
    {
        #region Properties.
        /// <summary>
        /// Property to set or return the filter to apply.
        /// </summary>
        /// <remarks>This property will control the filtering applied to the image for compression optimization.
        /// <para>This property only applies when encoding the image.</para>
        /// <para>The default value is None.</para>
        /// </remarks>
        public PNGFilter CompressionFilter
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return whether to use interlacing on the image data.
        /// </summary>
        /// <remarks>
        /// Use this property to control the output of vertical rows in the image.  
        /// <para>This property only applies when encoding the image.</para>
        /// <para>The default value is FALSE.</para>
        /// </remarks>
        public bool UseInterlacing
        {
            get;
            set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to set custom encoding options.
        /// </summary>
        /// <param name="frame">Frame encoder to use.</param>
        internal override void SetFrameOptions(WIC.BitmapFrameEncode frame)
        {
            frame.Options.InterlaceOption = UseInterlacing;
            frame.Options.FilterOption = (WIC.PngFilterOption)CompressionFilter;
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonCodecPNG"/> class.
        /// </summary>
        public GorgonCodecPNG()
            : base("PNG", "Portable Network Graphics", new string[] { "png" }, WIC.ContainerFormatGuids.Png)
        {
            UseInterlacing = false;
            CompressionFilter = PNGFilter.None;
        }
        #endregion
    }
}
