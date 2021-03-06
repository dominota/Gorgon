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
// Created: December 17, 2018 10:57:45 PM
// 
#endregion

using System.IO;
using System.Threading;

namespace Gorgon.Editor.Services
{
    /// <summary>
    /// A custom importer for content data.
    /// </summary>
    public interface IEditorContentImporter
    {
        /// <summary>
        /// Property to return the file being imported.
        /// </summary>
        FileInfo SourceFile
        {
            get;
        }

        /// <summary>
        /// Property to return whether or not the imported file needs to be cleaned up after processing.
        /// </summary>
        bool NeedsCleanup
        {
            get;
        }

        /// <summary>
        /// Function to import content.
        /// </summary>
        /// <param name="temporaryDirectory">The temporary directory for writing any transitory data.</param>
        /// <param name="cancelToken">The token used to cancel the operation.</param>
        /// <returns>A new file information object pointing to the imported file data.</returns>
        /// <remarks>
        /// <para>
        /// The <paramref name="temporaryDirectory"/> should be used to write any working/temporary data used by the import.  Note that all data written into this directory will be deleted when the 
        /// project is unloaded from memory.
        /// </para>
        /// </remarks>
        FileInfo ImportData(DirectoryInfo temporaryDirectory, CancellationToken cancelToken);
    }
}
