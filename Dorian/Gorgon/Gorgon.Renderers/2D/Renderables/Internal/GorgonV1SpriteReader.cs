﻿#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Sunday, January 20, 2013 10:26:02 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimMath;
using GorgonLibrary.Graphics;
using GorgonLibrary.IO;

namespace GorgonLibrary.Renderers
{
	/// <summary>
	/// A Gorgon v1.x compatiable sprite reader.
	/// </summary>
	static class GorgonV1SpriteReader
	{
		#region Methods.
		/// <summary>
		/// Function to convert Gorgon 1.x stencil operations to 2.x stencil operations.
		/// </summary>
		/// <param name="stencilOp">Stencil operation to convert.</param>
		/// <returns>Stencil operation.</returns>
		private static StencilOperations ConvertStencilOp(int stencilOp)
		{
			switch (stencilOp)
			{
				case 0:
					return StencilOperations.Zero;
				case 1:
					return StencilOperations.Decrement;
				case 2:
					return StencilOperations.Increment;
				case 3:
					return StencilOperations.Invert;
				case 4:
					return StencilOperations.DecrementClamp;
				case 5:
					return StencilOperations.IncrementClamp;
				case 7:
					return StencilOperations.Replace;
				default:
					return StencilOperations.Keep;
			}
		}

		/// <summary>
		/// Function to convert Gorgon 1.x compare functions to 2.x comparison operators.
		/// </summary>
		/// <param name="compareFunc">Comparison function to convert.</param>
		/// <returns>The comparsion operator.</returns>
		private static ComparisonOperators ConvertCompare(int compareFunc)
		{
			switch (compareFunc)
			{
				case 0:
					return ComparisonOperators.Less;
				case 1:
					return ComparisonOperators.Greater;
				case 2:
					return ComparisonOperators.Equal;
				case 3:
					return ComparisonOperators.Always;
				case 4:
					return ComparisonOperators.Never;
				case 6:
					return ComparisonOperators.GreaterEqual;
				case 7:
					return ComparisonOperators.NotEqual;
				default:
					return ComparisonOperators.LessEqual;
			}
		}

		/// <summary>
		/// Function to convert Gorgon 1.x image smoothing to 2.x texture filtering values.
		/// </summary>
		/// <param name="smoothing">Smoothing value to convert.</param>
		/// <returns>Texture filtering value.</returns>
		private static TextureFilter ConvertSmoothingToFilter(int smoothing)
		{
			switch (smoothing)
			{
				case 0:
					return TextureFilter.Point;
				case 2:
					return TextureFilter.MagLinear;
				case 3:
					return TextureFilter.MinLinear;
				default:
					return TextureFilter.Linear;
			}
		}

		/// <summary>
		/// Function to convert Gorgon 1.x image addressing values to 2.x texture addressing values.
		/// </summary>
		/// <param name="imageAddress">Image addressing values.</param>
		/// <returns>Texture addressing values.</returns>
		private static TextureAddressing ConvertImageAddressToTextureAddress(int imageAddress)
		{
			switch (imageAddress)
			{
				case 0:
					return TextureAddressing.Wrap;
				case 1:
					return TextureAddressing.Mirror;
				case 2:
					return TextureAddressing.MirrorOnce;
				case 4:
					return TextureAddressing.Border;
				default:
					return TextureAddressing.Clamp;
			}
		}

		/// <summary>
		/// Function to convert a D3D 9 blending operation to a Direct 3D 11 blending type.
		/// </summary>
		/// <param name="blendOp">Blending operation.</param>
		/// <returns>The blending type.</returns>
		private static BlendType ConvertBlendOpToBlendType(int blendOp)
		{
			switch (blendOp)
			{
				case 0:
					return BlendType.Zero;
				case 2:
					return BlendType.SourceColor;
				case 3:
					return BlendType.SourceAlpha;
				case 4:
					return BlendType.InverseSourceColor;
				case 5:
					return BlendType.InverseSourceAlpha;
				case 6:
					return BlendType.DestinationColor;
				case 7:
					return BlendType.DestinationAlpha;
				case 8:
					return BlendType.InverseDestinationColor;
				case 9:
					return BlendType.InverseDestinationAlpha;
				case 10:
					return BlendType.SourceAlphaSaturate;
				case 11:
					return BlendType.BlendFactor;
				case 12:
					return BlendType.InverseBlendFactor;
				default:
					return BlendType.One;
			}
		}

		/// <summary>
		/// Function to load a version 1.x Gorgon sprite.
		/// </summary>
		/// <param name="sprite">Sprite to fill in with data.</param>
		/// <param name="reader">Binary reader to use to read in the data.</param>
		public static void LoadSprite(GorgonSprite sprite, GorgonBinaryReader reader)
		{
			string headerVersion = string.Empty;
			string dummyString = string.Empty;
			Version version = null;

			reader.BaseStream.Position = 0;

			headerVersion = reader.ReadString();
			if ((!headerVersion.StartsWith("GORSPR", StringComparison.CurrentCultureIgnoreCase)) || (headerVersion.Length < 7))
			{
				throw new GorgonException(GorgonResult.CannotRead, "Cannot read this sprite.  It is not a Gorgon sprite or it is a newer version.");
			}

			// Get the version information.
			switch (headerVersion.ToLower())
			{
				case "gorspr1":
					version = new Version(1, 0);
					break;
				case "gorspr1.1":
					version = new Version(1, 1);
					break;
				case "gorspr1.2":
					version = new Version(1, 2);
					break;
				default:
					throw new GorgonException(GorgonResult.CannotRead, "Cannot read this sprite.  It is not a Gorgon sprite or it is a newer version.");
			}

			// We don't need the sprite name.
			dummyString = reader.ReadString();

			// Find out if we have an image.
			if (reader.ReadBoolean())
			{
				bool isRenderTarget = reader.ReadBoolean();
				string imageName = reader.ReadString();

				// We won't be supporting reading render targets from sprites in this version.
				if (isRenderTarget)
				{
					// Skip the target data.
					reader.ReadInt32();
					reader.ReadInt32();
					reader.ReadInt32();
					reader.ReadBoolean();
					reader.ReadBoolean();
				}
				else
				{
					// Bind the texture if it's already loaded, otherwise defer it.
					sprite.DeferredTextureName = imageName;
				}
			}

			// We don't use "inherited" values anymore.  But we need them because
			// the file doesn't include inherited data.
			bool InheritAlphaMaskFunction = reader.ReadBoolean();
			bool InheritAlphaMaskValue = reader.ReadBoolean();
			bool InheritBlending = reader.ReadBoolean();
			bool InheritHorizontalWrapping = reader.ReadBoolean();
			bool InheritSmoothing = reader.ReadBoolean();
			bool InheritStencilCompare = reader.ReadBoolean();
			bool InheritStencilEnabled = reader.ReadBoolean();
			bool InheritStencilFailOperation = reader.ReadBoolean();
			bool InheritStencilMask = reader.ReadBoolean();
			bool InheritStencilPassOperation = reader.ReadBoolean();
			bool InheritStencilReference = reader.ReadBoolean();
			bool InheritStencilZFailOperation = reader.ReadBoolean();
			bool InheritVerticalWrapping = reader.ReadBoolean();
			bool InheritDepthBias = true;
			bool InheritDepthTestFunction = true;
			bool InheritDepthWriteEnabled = true;

			// Get version 1.1 fields.
			if ((version.Major == 1) && (version.Minor >= 1))
			{
				InheritDepthBias = reader.ReadBoolean();
				InheritDepthTestFunction = reader.ReadBoolean();
				InheritDepthWriteEnabled = reader.ReadBoolean();
			}

			// Get the size of the sprite.
			sprite.Size = new Vector2(reader.ReadSingle(), reader.ReadSingle());

			// Since older versions of the sprite object used pixel space for their texture coordinates, we have to try
			// and transform these coordinates into texel space.  This is a problem if we have a deferred texture since
			// we have no basis to do the transformation.  So, if there's no texture bound at this time, then skip the 
			// transformation.
			// Also, older versions used the size the determine the area on the texture to cover.  So use the size to
			// get the texture bounds.
			Vector2 textureOffset = new Vector2(reader.ReadSingle(), reader.ReadSingle());
			Vector2 textureSize = Vector2.Zero;

			if (sprite.Texture != null)
			{
				sprite.TextureOffset = sprite.Texture.ToTexel(textureOffset);
				sprite.TextureSize = sprite.Texture.ToTexel(sprite.Size);
			}
			else
			{
				// Cover the entire texture.  It's better than showing nothing
				// at all.
				sprite.TextureOffset = Vector2.Zero;
				sprite.TextureSize = new Vector2(1);
			}

			// Read the anchor.
			sprite.Anchor = new Vector2(reader.ReadSingle(), reader.ReadSingle());

			// Get vertex offsets.
			sprite.SetCornerOffset(RectangleCorner.UpperLeft, new Vector2(reader.ReadSingle(), reader.ReadSingle()));
			sprite.SetCornerOffset(RectangleCorner.UpperRight, new Vector2(reader.ReadSingle(), reader.ReadSingle()));
			sprite.SetCornerOffset(RectangleCorner.LowerRight, new Vector2(reader.ReadSingle(), reader.ReadSingle()));
			sprite.SetCornerOffset(RectangleCorner.LowerLeft, new Vector2(reader.ReadSingle(), reader.ReadSingle()));

			// Get vertex colors.
			sprite.SetCornerColor(RectangleCorner.UpperLeft, new GorgonColor(reader.ReadInt32()));
			sprite.SetCornerColor(RectangleCorner.UpperRight, new GorgonColor(reader.ReadInt32()));
			sprite.SetCornerColor(RectangleCorner.LowerLeft, new GorgonColor(reader.ReadInt32()));
			sprite.SetCornerColor(RectangleCorner.LowerRight, new GorgonColor(reader.ReadInt32()));

			// Skip shader information.  Version 1.0 had shader information attached to the sprite.
			if ((version.Major == 1) && (version.Minor < 1))
			{
				if (reader.ReadBoolean())
				{
					reader.ReadString();
					reader.ReadBoolean();
					if (reader.ReadBoolean())
					{
						reader.ReadString();
					}
				}
			}

			// We no longer have an alpha mask function.
			if (!InheritAlphaMaskFunction)
			{
				reader.ReadInt32();
			}

			if (!InheritAlphaMaskValue)
			{
				// Direct 3D 9 used a value from 0..255 for alpha masking, we use
				// a scalar value so convert to a scalar.
				sprite.AlphaTestValues = new GorgonMinMaxF(0.0f, reader.ReadInt32() / 255.0f);
			}

			// Set the blending mode.
			if (!InheritBlending)
			{
				sprite.Blending.SourceBlend = ConvertBlendOpToBlendType(reader.ReadInt32());
				sprite.Blending.DestinationBlend = ConvertBlendOpToBlendType(reader.ReadInt32());
				// Skip the blending mode, this gets detected automatically now.
				reader.ReadInt32();
			}

			// Get alpha blending mode.
			if ((version.Major == 1) && (version.Minor >= 2))
			{
				sprite.Blending.DestinationAlphaBlend = ConvertBlendOpToBlendType(reader.ReadInt32());
				sprite.Blending.SourceAlphaBlend = ConvertBlendOpToBlendType(reader.ReadInt32());
			}

			// Get horizontal wrapping mode.
			if (!InheritHorizontalWrapping)
			{
				sprite.TextureSampler.HorizontalWrapping = ConvertImageAddressToTextureAddress(reader.ReadInt32());
			}

			// Get smoothing mode.
			if (!InheritSmoothing)
			{
				sprite.TextureSampler.TextureFilter = ConvertSmoothingToFilter(reader.ReadInt32());
			}

			// Get stencil stuff.
			if (!InheritStencilCompare)
			{
				sprite.DepthStencil.FrontFace.ComparisonOperator = ConvertCompare(reader.ReadInt32());
			}
			if (!InheritStencilEnabled)
			{
				// We don't enable stencil in the same way anymore, so skip this value.
				reader.ReadBoolean();
			}
			if (!InheritStencilFailOperation)
			{
				sprite.DepthStencil.FrontFace.FailOperation = ConvertStencilOp(reader.ReadInt32());
			}
			if (!InheritStencilMask)
			{
				sprite.DepthStencil.StencilReadMask = (byte)(reader.ReadInt32() & 0xFF);
			}
			if (!InheritStencilPassOperation)
			{
				sprite.DepthStencil.FrontFace.PassOperation = ConvertStencilOp(reader.ReadInt32());
			}
			if (!InheritStencilReference)
			{
				sprite.DepthStencil.DepthStencilReference = reader.ReadInt32();
			}
			if (!InheritStencilZFailOperation)
			{
				sprite.DepthStencil.FrontFace.DepthFailOperation = ConvertStencilOp(reader.ReadInt32());
			}

			// Get vertical wrapping mode.
			if (!InheritVerticalWrapping)
			{
				sprite.TextureSampler.VerticalWrapping = ConvertImageAddressToTextureAddress(reader.ReadInt32());
			}

			// Get depth info.
			if ((version.Major == 1) && (version.Minor >= 1))
			{
				if (!InheritDepthBias)
				{
					// Depth bias values are quite different on D3D9 than they are on D3D11, so skip this.
					reader.ReadSingle();
				}
				if (!InheritDepthTestFunction)
				{
					sprite.DepthStencil.DepthComparison = ConvertCompare(reader.ReadInt32());
				}
				if (!InheritDepthWriteEnabled)
				{
					sprite.DepthStencil.IsDepthWriteEnabled = reader.ReadBoolean();
				}

				sprite.TextureSampler.BorderColor = new GorgonColor(reader.ReadInt32());
			}

			// Get flipped flags.
			sprite.HorizontalFlip = reader.ReadBoolean();
			sprite.VerticalFlip = reader.ReadBoolean();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes the <see cref="GorgonV1SpriteReader" /> class.
		/// </summary>
		static GorgonV1SpriteReader()
		{
		}
		#endregion
	}
}
