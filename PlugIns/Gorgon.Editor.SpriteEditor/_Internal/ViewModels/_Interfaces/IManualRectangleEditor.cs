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
// Created: March 23, 2019 4:30:59 PM
// 
#endregion

using Gorgon.Editor.UI;
using DX = SharpDX;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// The view model for the manual rectangle editor for sprite clipping.
    /// </summary>
    internal interface IManualRectangleEditor
        : IManualInputViewModel
    {
        /// <summary>
        /// Property to set or return the rectangle dimensions.
        /// </summary>
        DX.RectangleF Rectangle
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return whether to use a fixed size for the rectangle, or free form.
        /// </summary>
        bool IsFixedSize
        {
            get;
        }

        /// <summary>
        /// Property to return the width and height to use when 
        /// </summary>
        DX.Size2F FixedSize
        {
            get;
        }

        /// <summary>
        /// Property to set or return the current texture array index.
        /// </summary>
        int TextureArrayIndex
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the padding, in pixels, applied to the selection rectangle.
        /// </summary>
        int Padding
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the command used to toggle the fixed size rectangle.
        /// </summary>
        IEditorCommand<DX.Size2F> ToggleFixedSizeCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command to execute when assigning the fixed width/height.
        /// </summary>
        IEditorCommand<DX.Size2F> SetFixedWidthHeightCommand
        {
            get;
        }

        /// <summary>
        /// Property to set or return the command to set the region to the full size of the texture.
        /// </summary>
        IEditorCommand<object> SetFullSizeCommand
        {
            get;
            set;
        }
    }
}