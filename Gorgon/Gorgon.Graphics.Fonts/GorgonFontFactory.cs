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
// Created: Sunday, May 20, 2012 10:06:05 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Gorgon.Core;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Fonts.Properties;
using Gorgon.Math;

namespace Gorgon.Graphics.Fonts
{
	/// <summary>
	/// A factory used to create, read, and cache font data.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This factory will create new bitmap fonts for applications to use when rendering text data. Fonts generated by the factory are cached for the lifetime of the factory and will be reused if font information 
	/// (e.g. name, information, etc...) is the same.
	/// </para>
	/// <para>
	/// Applications can also use this factory to load fonts from the file system, and have them cached as well.
	/// </para>
	/// <para>
	/// The factory will also built a <see cref="DefaultFont">default font</see> for quick use in applications. 
	/// </para>
	/// <para>
	/// Applications should only create a single instance of object for the lifetime of the application, any more than that may be wasteful.
	/// </para>
	/// <para>
	/// Because this object can contain a large amount of data, and implements <see cref="IDisposable"/>, it is required for applications to call the <see cref="IDisposable.Dispose"/> method when shutting down the 
	/// factory.
	/// </para>
	/// </remarks>
	public class GorgonFontFactory
		: IDisposable
	{
		#region Variables.
		// The default font.
		private GorgonFont _default;
		// Thread synchronization.
		private readonly object _syncLock = new object();
		// The cache used to hold previously created font data.
		private readonly Dictionary<string, GorgonFont> _fontCache = new Dictionary<string, GorgonFont>(StringComparer.OrdinalIgnoreCase);
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the graphics interface used to generate the fonts.
		/// </summary>
		public GorgonGraphics Graphics
		{
			get;
		}

		/// <summary>
		/// Property to return the default font.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This will return a default font used in applications for quick testing.
		/// </para>
		/// <para>
		/// The font will be based on Segoe UI, have a size of 10 pixels, and will be bolded and antialiased.
		/// </para>
		/// </remarks>
		public GorgonFont DefaultFont
		{
			get
			{
			    lock(_syncLock)
			    {
			        if (_default != null)
			        {
			            return _default;
			        }

			        // Create the default font.
				    _default = new GorgonFont("Gorgon.Font.Default.SegoeUI_14px",
				                              this,
				                              new GorgonFontInfo("Segoe UI", 14)
				                              {
					                              AntiAliasingMode = FontAntiAliasMode.AntiAlias,
					                              FontStyle = FontStyle.Bold,
					                              OutlineSize = 0
				                              });

			        _default.GenerateFont();
			    }

			    return _default;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to compare two <see cref="IGorgonFontInfo"/> types to determine equality.
		/// </summary>
		/// <param name="left">The left instance to compare.</param>
		/// <param name="right">The right instance to compare.</param>
		/// <returns><b>true</b> if the fonts are different, <b>false</b> if not.</returns>
		private static bool IsFontDifferent(IGorgonFontInfo left, IGorgonFontInfo right)
		{
			return ((left.UseKerningPairs != right.UseKerningPairs)
					|| (left.FontHeightMode != right.FontHeightMode)
					|| (left.TextureWidth != right.TextureWidth)
					|| (left.TextureHeight != right.TextureHeight)
					|| (left.AntiAliasingMode != right.AntiAliasingMode)
					|| (left.Brush != right.Brush)
					|| (left.DefaultCharacter != right.DefaultCharacter)
					|| (!string.Equals(left.FontFamilyName, right.FontFamilyName, StringComparison.CurrentCultureIgnoreCase))
					|| (left.FontStyle != right.FontStyle)
					|| (left.OutlineColor1 != right.OutlineColor1)
					|| (left.OutlineColor2 != right.OutlineColor2)
					|| (left.OutlineSize != right.OutlineSize)
					|| (left.PackingSpacing != right.PackingSpacing)
					|| (!left.Size.EqualsEpsilon(right.Size))
					|| (!left.Characters.SequenceEqual(right.Characters)));
		}
		
		/// <summary>
		/// Function to register a font within the font cache.
		/// </summary>
		/// <param name="font">The font to register.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="font"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Thrown if the <paramref name="font"/> is already registered in the factory cache.</exception>
		internal void RegisterFont(GorgonFont font)
		{
			if (font == null)
			{
				throw new ArgumentNullException(nameof(font));
			}

			if (font.Factory != this)
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ERR_FONT_ALREADY_REGISTERED, font.Name), nameof(font));
			}

			lock (_syncLock)
			{

				if (_fontCache.TryGetValue(font.Name, out GorgonFont existing))
				{
					// This is the exact same reference, so do nothing.
					if (existing == font)
					{
						return;
					}

					throw new ArgumentException(string.Format(Resources.GORGFX_ERR_FONT_ALREADY_REGISTERED, font.Name), nameof(font));
				}

				_fontCache[font.Name] = font;
			}
		}

		/// <summary>
		/// Function to unregister a font from the factory cache.
		/// </summary>
		/// <param name="font">The font to unregister.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="font"/> parameter is <b>null</b>.</exception>
		/// <remarks>
		/// <para>
		/// This will remove the specified <paramref name="font"/> from the cache, but it will not destroy the font. If a font with the same name exists, but the font is not the same reference as the font passed in 
		/// then nothing is done and the function will exit, the same happens when the font does not exist in the cache.
		/// </para>
		/// </remarks>
		internal void UnregisterFont(GorgonFont font)
		{
			if (font == null)
			{
				throw new ArgumentException(nameof(font));
			}

			// Don't allow fonts not registered with this factory to be removed from the cache.
			if (font.Factory != this)
			{
				return;
			}

			lock (_syncLock)
			{

				if (!_fontCache.TryGetValue(font.Name, out GorgonFont existing))
				{
					return;
				}

				// If this font is not the same as the one we want, then do nothing.
				if (existing != font)
				{
					return;
				}

				_fontCache.Remove(font.Name);
			}
		}

		/// <summary>
		/// Function to invalidate the cached fonts for this factory.
		/// </summary>
		public void InvalidateCache()
		{
			lock (_syncLock)
			{
				foreach (KeyValuePair<string, GorgonFont> font in _fontCache)
				{
					font.Value.Dispose();
				}

				_fontCache.Clear();
			}
		}

		/// <summary>
		/// Function to determine if the font cache contains a font with the specified name, and the specified font information.
		/// </summary>
		/// <param name="name">The name of the font to find.</param>
		/// <param name="fontInfo">The information about the font to find.</param>
		/// <returns><b>true</b> if found, <b>false</b> if not.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/>, or the <paramref name="fontInfo"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
		/// <remarks>
		/// <para>
		/// The <paramref name="name"/> parameter is used in caching, and is user defined. It is not necessary to have it share the same name as the font family name in the <paramref name="fontInfo"/> parameter, 
		/// however it is best practice to indicate the font family name in the name for ease of use. 
		/// </para>
		/// <para>
		/// If a font with the same name was previously created by this factory, then this method will return <b>true</b> if the <paramref name="fontInfo"/> is the same as the cached version. If no font with the 
		/// same name or the <paramref name="fontInfo"/> is different, then this method will return <b>false</b>.
		/// </para>
		/// </remarks>
		public bool HasFont(string name, IGorgonFontInfo fontInfo)
		{
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}

			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException(Resources.GORGFX_ERR_PARAMETER_MUST_NOT_BE_EMPTY, nameof(name));
			}

			if (fontInfo == null)
			{
				throw new ArgumentNullException(nameof(fontInfo));
			}

			lock (_syncLock)
			{

				return ((_fontCache.TryGetValue(name, out GorgonFont result))
						&& (!IsFontDifferent(fontInfo, result.Info)));
			}
		}

		/// <summary>
		/// Function to return or create a new <see cref="GorgonFont"/>.
		/// </summary>
		/// <param name="name">The name of the font.</param>
		/// <param name="fontInfo">The information used to create the font.</param>
		/// <returns>A new or existing <see cref="GorgonFont"/>.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/>, or the <paramref name="fontInfo"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="IGorgonFontInfo.TextureWidth"/> or <see cref="IGorgonFontInfo.TextureHeight"/> parameters exceed the <see cref="IGorgonVideoDevice.MaxTextureWidth"/> or 
		/// <see cref="IGorgonVideoDevice.MaxTextureHeight"/> available for the current <see cref="FeatureLevelSupport"/>.</para>
		/// <para>-or-</para>
		/// <para>Thrown if the <see cref="IGorgonFontInfo.Characters"/> list does not contain the <see cref="IGorgonFontInfo.DefaultCharacter"/> character.</para>
		/// </exception>
		/// <remarks>
		/// <para>
		/// This method creates an object that contains a group of textures with font glyphs.  These textures can be used by another application to display text (or symbols) on the screen.  
		/// Kerning information (the proper spacing for a glyph), advances, etc... are all included for the glyphs with font.
		/// </para>
		/// <para>
		/// The <paramref name="name"/> parameter is used in caching, and is user defined. It is not necessary to have it share the same name as the font family name in the <paramref name="fontInfo"/> parameter, 
		/// however it is best practice to indicate the font family name in the name for ease of use. 
		/// </para>
		/// <para>
		/// If a font with the same name was previously created by this factory, then that font will be returned if the <paramref name="fontInfo"/> is the same as the cached version. If no font with the same 
		/// name or the <paramref name="fontInfo"/> is different, then a new font is generated. If the names are the same, then the new font will replace the old.
		/// </para>
		/// </remarks>
		public GorgonFont GetFont(string name, IGorgonFontInfo fontInfo)
		{
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}

			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException(Resources.GORGFX_ERR_PARAMETER_MUST_NOT_BE_EMPTY, nameof(name));
			}

			if (fontInfo == null)
			{
				throw new ArgumentNullException(nameof(fontInfo));
			}

			lock (_syncLock)
			{

				// Check the cache for a font with the same name.
				if ((_fontCache.TryGetValue(name, out GorgonFont result))
					&& (!IsFontDifferent(fontInfo, result.Info)))
				{
					return result;
				}

				if ((fontInfo.TextureWidth > Graphics.VideoDevice.MaxTextureWidth)
				    || (fontInfo.TextureHeight > Graphics.VideoDevice.MaxTextureHeight))
				{
					throw new ArgumentException(string.Format(Resources.GORGFX_ERR_FONT_TEXTURE_SIZE_TOO_LARGE,
					                                          fontInfo.TextureWidth,
					                                          fontInfo.TextureHeight),
					                            nameof(fontInfo));
				}

				if (!fontInfo.Characters.Contains(fontInfo.DefaultCharacter))
				{
					throw new ArgumentException(string.Format(Resources.GORGFX_ERR_FONT_DEFAULT_CHAR_DOES_NOT_EXIST, fontInfo.DefaultCharacter));
				}

				if (Convert.ToInt32(fontInfo.DefaultCharacter) < 32)
				{
					throw new ArgumentException(string.Format(Resources.GORGFX_ERR_FONT_DEFAULT_CHAR_NOT_VALID, Convert.ToInt32(fontInfo.DefaultCharacter)));
				}

				// Destroy the previous font if one exists with the same name.
				result?.Dispose();

				// If not found, then create a new font and cache it.
				_fontCache[name] = result = new GorgonFont(name, this, fontInfo);
				result.GenerateFont();

				return result;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			GorgonFont defaultFont = Interlocked.Exchange(ref _default, null);

			defaultFont?.Dispose();

			InvalidateCache();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonFontFactory"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface used to create the font data.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/> parameter is <b>null</b>.</exception>
		public GorgonFontFactory(GorgonGraphics graphics)
		{
			Graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
		}
		#endregion
	}
}