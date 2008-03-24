#region LGPL.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Saturday, April 07, 2007 2:12:28 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using SharpUtilities;
using GorgonLibrary.PlugIns;

namespace GorgonLibrary.FileSystems
{
	/// <summary>
	/// Object containing file system type information.
	/// </summary>
	public class FileSystemProvider
		: NamedObject, IDisposable
	{
		#region Variables.
		private FileSystemPlugIn _fileSystemPlugIn;			// File system plug-in.
		private FileSystemInfoAttribute _info;				// File system information.
		private Type _fileSystemType;						// File system type.
		private bool _isDisposed = false;					// Flag to indicate whether this object is disposed already or not.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether this plug-in is shared with other providers.
		/// </summary>
		private bool PlugInShared
		{
			get
			{
				if (_fileSystemPlugIn == null)
					return false;

				foreach (FileSystemProvider provider in FileSystemProviderCache.Providers)
				{
					if (provider.PlugIn == _fileSystemPlugIn)
						return true;
				}

				return false;
			}
		}

		/// <summary>
		/// Property to return the plug-in entry point, if one exists.
		/// </summary>
		internal FileSystemPlugIn PlugIn
		{
			get
			{				
				return _fileSystemPlugIn;
			}
		}

		/// <summary>
		/// Property to return the name of the provider plug-in.
		/// </summary>
		public string ProviderPlugInName
		{
			get
			{
				if (_fileSystemPlugIn != null)
					return _fileSystemPlugIn.Name;
				else
					return string.Empty;
			}
		}

		/// <summary>
		/// Property to return the path for the plug-in that this provider uses.
		/// </summary>
		/// <remarks>This will return an empty string if the provider is instanced locally (i.e. not loaded from a plug-in).</remarks>
		public string PlugInPath
		{
			get
			{
				if (_fileSystemPlugIn != null)
					return _fileSystemPlugIn.PlugInPath;

				return string.Empty;
			}
		}

		/// <summary>
		/// Property to return whether we're using a plug-in or not.
		/// </summary>
		public bool IsPlugIn
		{
			get
			{
				return _fileSystemPlugIn != null;
			}
		}

		/// <summary>
		/// Property to return the file system type.
		/// </summary>
		public Type Type
		{
			get
			{
				return _fileSystemType;
			}
		}

        /// <summary>
        /// Property to return the description of the file system.
        /// </summary>
        public string Description
        {
            get
            {
                return _info.Description;
            }
        }

        /// <summary>
		/// Property to return whether this provider uses a packed file or loose files.
		/// </summary>
		public bool IsPackedFile
		{
			get
			{
				return _info.IsPackFile;
			}
		}

		/// <summary>
		/// Property to return whether this provider uses compression.
		/// </summary>
		public bool IsCompressed
		{
			get
			{
				return _info.IsCompressed;
			}
		}

		/// <summary>
		/// Property to return the ID of the file system.
		/// </summary>
		public string ID
		{
			get
			{
				return _info.ID;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to return a list of file systems that use a specific provider.
		/// </summary>
		/// <returns>The list of providers</returns>
		private List<string> GetFileSystemsByProvider()
		{
			List<string> fileSystems = new List<string>();		// List of file systems.

			// Add each file system that belongs to the list.
			foreach (FileSystem system in FileSystemCache.FileSystems)
			{
				if (system.Provider == this)
					fileSystems.Add(system.Name);
			}

			return fileSystems;
		}
		
		/// <summary>
		/// Function to create a file system.
		/// </summary>
		/// <param name="name">Name of the file system.</param>
		/// <returns>A new file system object.</returns>
		internal FileSystem CreateFileSystemInstance(string name)
		{
			if (IsPlugIn)
				return _fileSystemPlugIn.Create(name, this);
			else
				return _fileSystemType.Assembly.CreateInstance(_fileSystemType.FullName, false, BindingFlags.CreateInstance, null, new object[] { name, this }, null, null) as FileSystem;
		}

		/// <summary>
		/// Function to load a file system provider plug-in.
		/// </summary>
		/// <param name="providerPlugInPath">Path to the provider plug-in.</param>
		/// <param name="plugInName">Name of the file system plug-in.</param>
		/// <returns>The filesystem plug-in provider.</returns>
		/// <remarks>This will only load the file system provider if it hasn't already been loaded.<para>The filesystem plugInName is the name of the plug-in within the DLL, not the name of the provider.</para></remarks>
		public static FileSystemProvider Load(string providerPlugInPath, string plugInName)
		{
			FileSystemPlugIn plugIn = null;					// File system plug-in.

			if (string.IsNullOrEmpty(providerPlugInPath))
				throw new ArgumentNullException("providerPlugInPath");

			try
			{
				// Load the plug-in.
				plugIn = PlugInFactory.Load(providerPlugInPath, plugInName) as FileSystemPlugIn;

				// Add each plug-in (if it doesn't already exist).
				if ((plugIn == null) || (plugIn.PlugInType != PlugInType.FileSystem))
					throw new ApplicationException("The plug-in is not a file system plug-in.\nThe type returned was: " + plugIn.PlugInType.ToString());

				if (!FileSystemProviderCache.Providers.Contains(plugInName))
					FileSystemProviderCache.Providers.Add(plugIn);

				return FileSystemProviderCache.Providers[plugInName];
			}
			catch (Exception ex)
			{
				throw new FileSystemPlugInLoadException(providerPlugInPath, ex);
			}
		}

		/// <summary>
		/// Function to load all file system providers from a plug-in.
		/// </summary>
		/// <param name="providerPlugInPath">Path to the provider plug-in.</param>
		/// <remarks>This will load all file system providers contained within the DLL.</remarks>
		public static void Load(string providerPlugInPath)
		{
			FileSystemPlugIn plugIn = null;					// File system plug-in.

			if (string.IsNullOrEmpty(providerPlugInPath))
				throw new ArgumentNullException("providerPlugInPath");

			try
			{
				// Load the plug-in.
				PlugInFactory.Load(providerPlugInPath);

				// Add each plug-in (if it doesn't already exist).
				foreach (PlugInEntryPoint plugInItem in PlugInFactory.PlugIns)
				{
					plugIn = plugInItem as FileSystemPlugIn;
					if (plugIn != null)
					{
						if (plugIn.PlugInType != PlugInType.FileSystem)
							throw new ApplicationException("The plug-in is not a file system plug-in.\nThe type returned was: " + plugIn.PlugInType.ToString());

						if (!FileSystemProviderCache.Providers.Contains(plugIn.Name))
							FileSystemProviderCache.Providers.Add(plugIn);
					}
				}
			}
			catch (Exception ex)
			{
				throw new FileSystemPlugInLoadException(providerPlugInPath, ex);
			}
		}

		/// <summary>
		/// Function to create a file system provider type.
		/// </summary>
		/// <param name="providerType">Type of file system provider.</param>
		/// <returns>The file system provider based on the type passed.</returns>
		/// <remarks>This will only load the file system provider if it hasn't already been loaded.</remarks>
		public static FileSystemProvider Create(Type providerType)
		{
			if (providerType == null)
				throw new ArgumentNullException("providerType");

			try
			{
				if (!providerType.IsSubclassOf(typeof(FileSystem)))
					throw new TypeLoadException("The provider type is not a file system.");

				// Add the type.
				if (!FileSystemProviderCache.Providers.Contains(providerType))
					FileSystemProviderCache.Providers.Add(providerType);

				return FileSystemProviderCache.Providers[providerType];
			}
			catch (Exception ex)
			{
				throw new FileSystemPlugInLoadException(providerType.FullName, ex);
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="plugin">Plug-in to use for the file system.</param>
		internal FileSystemProvider(FileSystemPlugIn plugin)
            : base(plugin.Name)
		{
			_info = plugin.FileSystemInfo;
			_fileSystemPlugIn = plugin;
			_fileSystemType = null;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="fileSystemType">File system type.</param>
		internal FileSystemProvider(Type fileSystemType)
            : base(fileSystemType.Name)
		{
			object[] attributes = null;		// File system type attributes.

			_fileSystemPlugIn = null;
			_fileSystemType = fileSystemType;

			// Get the attributes.
			attributes = fileSystemType.GetCustomAttributes(typeof(FileSystemInfoAttribute), true);

			if ((attributes == null) || (attributes.Length == 0))
				throw new FileSystemAttributeMissingException(typeof(FileSystemInfoAttribute));

			// Get the attribute.
			_info = (FileSystemInfoAttribute)attributes[0];
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		/// <param name="disposing">TRUE to release all resources, FALSE to only release unmanaged.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (!_isDisposed)
				{
					// Remove any file systems that depend on this.
					foreach (string fileSystem in GetFileSystemsByProvider())
						FileSystemCache.FileSystems[fileSystem].Dispose();

					// If we exist in the factory collection, then remove us.
					if (FileSystemProviderCache.Providers.Contains(Name))
						FileSystemProviderCache.Providers.Remove(Name);

					// Unload the plug-in.
					if (_fileSystemPlugIn != null)
					{
						// If we're the last provider to use the file system, then unload the plug-in.
						if (!PlugInShared)
							PlugInFactory.Unload(_fileSystemPlugIn.Name);
					}
				}

				_isDisposed = true;
			}		
		}

		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
