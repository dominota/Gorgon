﻿#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Monday, April 30, 2012 6:28:28 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using GorgonLibrary;
using GorgonLibrary.UI;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.GorgonEditor
{
	/// <summary>
	/// Main application interface.
	/// </summary>
	static class Program
	{		
		#region Properties.
		/// <summary>
		/// Property to return the list of documents.
		/// </summary>
		internal static DocumentCollection Documents
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the settings for the application.
		/// </summary>
		public static GorgonEditorSettings Settings
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the list of cached fonts on the system.
		/// </summary>
		public static IDictionary<string, Font> CachedFonts
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the currently active document.
		/// </summary>
		public static Document CurrentDocument
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the graphics interface.
		/// </summary>
		public static GorgonGraphics Graphics
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to initialize the graphics interface.
		/// </summary>
		public static void InitializeGraphics()
		{
			if (Graphics != null)
			{
				Graphics.Dispose();
				Graphics = null;
			}
			Graphics = new GorgonGraphics();
			SharedResources.Initialize();
		}

		/// <summary>
		/// Funciton to update the font cache.
		/// </summary>
		public static void UpdateCachedFonts()
		{
			SortedDictionary<string, Font> fonts = null;

			// Clear the cached fonts.
			if (CachedFonts != null)
			{
				foreach (var font in CachedFonts)
					font.Value.Dispose();
			}

			fonts = new SortedDictionary<string,Font>();

			// Get font families.
			foreach (var family in FontFamily.Families)
			{
				Font newFont = null;

				if (!fonts.ContainsKey(family.Name))
				{
					if (family.IsStyleAvailable(FontStyle.Regular))
						newFont = new Font(family, 16.0f, FontStyle.Regular, GraphicsUnit.Pixel);
					else
					{
						if (family.IsStyleAvailable(FontStyle.Bold))
							newFont = new Font(family, 16.0f, FontStyle.Bold, GraphicsUnit.Pixel);
						else
						{
							if (family.IsStyleAvailable(FontStyle.Italic))
								newFont = new Font(family, 16.0f, FontStyle.Italic, GraphicsUnit.Pixel);
						}
					}

					// Only add if we could use the regular, bold or italic style.
					if (newFont != null)
						fonts.Add(family.Name, newFont);
				}
			}

			CachedFonts = fonts;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes the <see cref="Program"/> class.
		/// </summary>
		static Program()
		{
			Settings = new GorgonEditorSettings();
			Settings.Load();

			Documents = new DocumentCollection();
		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			try
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Gorgon.Run(new AppContext());
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(null, ex);
			}
			finally
			{
				SharedResources.Terminate();

				// Shut down the graphics interface.
				if (Graphics != null)
				{
					Graphics.Dispose();
					Graphics = null;
				}

				// Clear the cached fonts.
				if (CachedFonts != null)
				{
					foreach (var font in CachedFonts)
						font.Value.Dispose();
				}
			}
		}
	}
}
