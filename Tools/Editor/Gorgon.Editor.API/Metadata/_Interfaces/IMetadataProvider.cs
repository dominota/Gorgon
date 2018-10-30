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
// Created: September 5, 2018 12:57:32 PM
// 
#endregion

using System.Collections.Generic;
using System.IO;
using Gorgon.Editor.ProjectData;

namespace Gorgon.Editor.Metadata
{
    /// <summary>
    /// The metadata access interface.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Plug in writers can use this to create, remove and retrieve metadata items for their plug ins, plus handle standard meta data items.
    /// </para>
    /// </remarks>
    public interface IMetadataProvider
    {
        /// <summary>
        /// Property to return the file pointing to the metadata database.
        /// </summary>
        FileInfo MetadataFile
        {
            get;
        }

        /// <summary>
        /// Function to update project data and metadata.
        /// </summary>
        /// <param name="project">The project data to send.</param>
        /// <param name="title">The title of the project.</param>
        /// <param name="writerType">The type of writer used to write the project data.</param>
        void UpdateProjectData(string title, string writerType, IProject project);

        /// <summary>
        /// Function to retrieve the list of files included in this project.
        /// </summary>
        /// <returns>A list of included paths.</returns>
        IList<IncludedFileSystemPathMetadata> GetIncludedPaths();
    }
}