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
// Created: September 17, 2018 8:44:42 AM
// 
#endregion

using Gorgon.Editor.Services;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// Parameters for the <see cref="IStageNewVm"/> view model.
    /// </summary>
    internal class StageNewVmParameters
        : ViewModelCommonParameters
    {
        /// <summary>
        /// Property to return the editor settings.
        /// </summary>
        public EditorSettings EditorSettings
        {
            get;
        }

        /// <summary>
        /// Property to return the directory locator service.
        /// </summary>
        public IDirectoryLocateService DirectoryLocator => ViewModelFactory.DirectoryLocator;

        /// <summary>
        /// Initializes a new instance of the <see cref="StageNewVmParameters"/> class.
        /// </summary>
        /// <param name="projectManager">The project manager for the application.</param>
        /// <param name="viewModelFactory">The view model factory for creating view models.</param>
        /// <param name="settings">The settings for the editor.</param>
        /// <param name="messageDisplay">The message display service to use.</param>
        /// <param name="busyService">The busy state service to use.</param>
        public StageNewVmParameters(ViewModelFactory viewModelFactory)
            : base(viewModelFactory)
        {
        }
    }
}
