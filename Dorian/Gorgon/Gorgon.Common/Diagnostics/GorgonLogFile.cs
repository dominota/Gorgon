﻿#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Tuesday, June 14, 2011 8:50:49 PM
// 
#endregion

using System;
using System.IO;
using System.Text;

namespace GorgonLibrary.Diagnostics
{
	/// <summary>
	/// Enumeration containing the logging levels.
	/// </summary>
	public enum LoggingLevel
	{
		/// <summary>This will disable the log file.</summary>
		NoLogging = 0,
		/// <summary>This will only pass messages marked as simple.</summary>
		Simple = 1,
		/// <summary>This will only pass messages marked as intermediate.</summary>
		Intermediate = 2,
		/// <summary>This will only pass messages marked as verbose.</summary>
		Verbose = 3,
		/// <summary>This will print all messages regardless of level.</summary>
		All = 4
	}

	/// <summary>
	/// Sends logging information to a file.
	/// </summary>
	public class GorgonLogFile
		: IDisposable
	{
		#region Variables.
		private StreamWriter _stream = null;								// File stream object.
		private LoggingLevel _filterLevel = LoggingLevel.All;	// Logging filter.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the filtering level of this log.
		/// </summary>
		public LoggingLevel LogFilterLevel
		{
			get
			{
				return _filterLevel;
			}
			set
			{
				if ((_filterLevel != value) && (value != LoggingLevel.NoLogging))
				{
					Print(string.Empty, LoggingLevel.All);
					Print("**** Log Filter Level: {0}", LoggingLevel.All, value);
					Print(string.Empty, LoggingLevel.All);
				}

				_filterLevel = value;
			}
		}

		/// <summary>
		/// Property to return the name of the application that is being logged.
		/// </summary>
		public string LogApplication
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the path to the log.
		/// </summary>
		public string LogPath
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return whether or not the log object is in a closed state.
		/// </summary>
		public bool IsClosed
		{
			get;
			private set;
		}
		#endregion

		#region Functions.
		/// <summary>
		/// Print a line to the logfile.
		/// </summary>
		/// <param name="formatSpecifier">Format specifier for the line.</param>
		/// <param name="level">Level that this message falls under.</param>
		/// <param name="arguments">List of optional arguments.</param>
		public void Print(string formatSpecifier, LoggingLevel level, params object[] arguments)
		{
			if (((LogFilterLevel == LoggingLevel.NoLogging) && (level != LoggingLevel.All)) || (IsClosed))
				return;

			StringBuilder outputLine = new StringBuilder(512);			// Output string 
			string[] lines = null;										// List of lines.

			if ((level <= LogFilterLevel) || (level == LoggingLevel.All))
			{
				if (string.IsNullOrEmpty(formatSpecifier) || (formatSpecifier == "\n") || (formatSpecifier == "\r"))
				{
					outputLine.Append("[");
					outputLine.Append(System.DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt"));
					outputLine.Append("]\r\n");
				}
				else
				{
					lines = formatSpecifier.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

					for (int i = 0; i < lines.Length; i++)
					{
						outputLine.Append("[");
						outputLine.Append(System.DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt"));
						outputLine.Append("] ");

						outputLine.Append(string.Format(lines[i] + "\r\n", arguments));
					}
				}
		
				_stream.Write(outputLine.ToString());
				_stream.Flush();
			}
		}

		/// <summary>
		/// Function to close the log file.
		/// </summary>
		public void Close()
		{
			if (!IsClosed)
			{
				// Clean up.
				Print(string.Empty, LoggingLevel.All);
				Print("**** {0} (Version {1}) logging ends. ****", LoggingLevel.All, LogApplication, GetType().Assembly.GetName().Version.ToString());

				if (_stream != null)
				{
					_stream.Close();
					_stream = null;
				}

				IsClosed = true;
			}			

			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Function to open the log file.
		/// </summary>
		public void Open()
		{
			if (IsClosed)
			{
				// Create the directory if it doesn't exist.
				if (!Directory.Exists(Path.GetDirectoryName(LogPath)))
					Directory.CreateDirectory(Path.GetDirectoryName(LogPath));

				// Open the stream.
				_stream = new StreamWriter(File.Open(LogPath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read), Encoding.UTF8);
				_stream.Flush();

				IsClosed = false;
				Print("**** {0} (Version {1}) logging begins ****", LoggingLevel.All, LogApplication, GetType().Assembly.GetName().Version.ToString());
				if (LogFilterLevel != LoggingLevel.NoLogging)
					Print("**** Log Filter Level: {0}", LoggingLevel.All, LogFilterLevel);
				Print(string.Empty, LoggingLevel.All);
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonLogFile"/> class.
		/// </summary>
		/// <param name="appname">File name for the log file.</param>
		/// <param name="extraPath">Additional directories for the path.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="appname"/> parameter is NULL (or Nothing in VB.NET).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the appname parameter is empty.</exception>
		public GorgonLogFile(string appname, string extraPath)
		{	
			GorgonDebug.AssertParamString(appname, "appname");

			IsClosed = true;

			LogApplication = appname;

			LogPath = GorgonComputerInfo.FolderPath(Environment.SpecialFolder.ApplicationData);
			LogPath += Path.DirectorySeparatorChar.ToString();

			// Verify the extra path information.
			if (!string.IsNullOrEmpty(extraPath))
			{
				// Remove any text up to and after the volume separator character.
				if (extraPath.Contains(Path.VolumeSeparatorChar.ToString()))
				{
					if (extraPath.IndexOf(Path.VolumeSeparatorChar) < (extraPath.Length - 1))
						extraPath = extraPath.Substring(extraPath.IndexOf(Path.VolumeSeparatorChar) + 1);
					else
						extraPath = string.Empty;
				}

				if ((extraPath.StartsWith(Path.AltDirectorySeparatorChar.ToString())) || (extraPath.StartsWith(Path.DirectorySeparatorChar.ToString())))
					extraPath = extraPath.Substring(1);

				if (!extraPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
					extraPath += Path.DirectorySeparatorChar.ToString();

				if (!string.IsNullOrEmpty(extraPath))
				{
					LogPath += extraPath;					
				}
			}

			LogPath += appname;
			LogPath = LogPath.FormatDirectory(Path.DirectorySeparatorChar);
			LogPath += "ApplicationLogging.txt";

			if (string.IsNullOrEmpty(LogPath))
				throw new IOException("The assembly name is not valid for a file name.");
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Function to remove resources.
		/// </summary>
		/// <param name="disposing">TRUE if we're removing managed resources and unmanaged, FALSE if only unmanaged.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
				Close();
		}

		/// <summary>
		/// Function to clean up.
		/// </summary>
		void IDisposable.Dispose()
		{
			Dispose(true);			
		}
		#endregion
	}
}
