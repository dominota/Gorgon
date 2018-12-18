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
// Created: October 29, 2018 1:04:03 PM
// 
#endregion

using System.Collections.Generic;
using Gorgon.Editor.Plugins;
using Gorgon.Plugins;

namespace Gorgon.Editor.Services
{
    /// <summary>
    /// Provides access to the various content specific plugins in the application.
    /// </summary>
    public interface IContentPluginService        
        : IDisabledPluginService
    {
        #region Properties.
        /// <summary>
        /// Property to return the list of content plugins loaded in to the application.
        /// </summary>
        IReadOnlyDictionary<string, ContentPlugin> Plugins
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Funcion to read the settings for a content plug in from a JSON file.
        /// </summary>
        /// <typeparam name="T">The type of settings to read. Must be a reference type.</typeparam>
        /// <param name="plugin">The plug in that owns the settings being read.</param>
        /// <returns>The settings object for the plug in, or <b>null</b> if no settings file was found for the plug in.</returns>
        /// <remarks>
        /// <para>
        /// This will read in the settings for a content plug from the same location where the editor stores its application settings file.
        /// </para>
        /// </remarks>
        T ReadContentSettings<T>(ContentPlugin plugin) where T : class;

        /// <summary>
        /// Function to write out the settings for a content plug in as a JSON file.
        /// </summary>
        /// <typeparam name="T">The type of settings to write. Must be a reference type.</typeparam>
        /// <param name="plugin">The plug in that owns the settings being written.</param>
        /// <param name="contentSettings">The content settings to persist as JSON file.</param>
        /// <remarks>
        /// <para>
        /// This will write out the settings for a content plug in to the same location where the editor stores its application settings file.
        /// </para>
        /// </remarks>
        void WriteContentSettings<T>(ContentPlugin plugin, T contentSettings) where T : class;
        #endregion
    }
}
