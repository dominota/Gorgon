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
// Created: September 4, 2018 9:45:38 AM
// 
#endregion

namespace Gorgon.Editor
{
    /// <summary>
    /// Contains commonly used constants for the editor.
    /// </summary>
    public static class CommonEditorConstants
    {
        /// <summary>
        /// The attribute name for the content type attribute.
        /// </summary>
        public const string ContentTypeAttr = "Type";

        /// <summary>
        /// The current version for an editor project.
        /// </summary>
        public const string EditorCurrentProjectVersion = "GOREDIT30";

        /// <summary>
        /// The name of the file that holds the metadata for the project.
        /// </summary>
        public const string EditorMetadataFileName = "_$GEMD$_.db";

        /// <summary>
        /// The name of the attribute for thumbnails in content items.
        /// </summary>
        public const string ThumbnailAttr = "Thumb";

        /// <summary>
        /// The name of the attribute for the is new flag.
        /// </summary>
        public const string IsNewAttr = "IsNew";
    }
}
