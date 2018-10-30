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
// Created: August 29, 2018 8:01:46 PM
// 
#endregion

using System;
using System.Runtime.InteropServices;
// ReSharper disable All

namespace Gorgon.Native
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    [StructLayout(LayoutKind.Sequential)]
    internal struct IMAGEINFO
    {
        internal IntPtr hbmImage;
        internal IntPtr hbmMask;
        internal int Unused1;
        internal int Unused2;
        internal RECT rcImage;
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}