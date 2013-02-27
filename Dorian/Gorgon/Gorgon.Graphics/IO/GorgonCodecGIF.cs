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
// Created: Thursday, February 14, 2013 9:24:55 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DX = SharpDX;
using WIC = SharpDX.WIC;
using GorgonLibrary.Math;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.IO
{
	/// <summary>
	/// A codec to handle reading/writing GIF files.
	/// </summary>
	/// <remarks>A codec allows for reading and/or writing of data in an encoded format.  Users may inherit from this object to define their own 
	/// image formats, or use one of the predefined image codecs available in Gorgon.
	/// <para>The limitations of this codec are as follows:
	/// <list type="bullet">
	///		<item>
	///			<description>Only supports saving as 8 bit indexed only.  Fidelity loss may be dramatic.</description>
	///		</item>
	/// </list>
	/// </para>
	/// </remarks>
	public sealed unsafe class GorgonCodecGIF
		: GorgonCodecWIC
	{
		#region Variables.
		private double _alphaPercent = 0.0;			// Alpha threshold.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the delays between each frame in 1/10 of a second.
		/// </summary>
		/// <remarks>This property will store the delays between individual frames (image array indices) for animation.  If this value is left as NULL (Nothing in VB.Net), the no frame delays 
		/// will be put in to the GIF file.  If the array has less elements than the number of frames available, then a delay of 0 will be used for remaining delays and if the array has more 
		/// delays than frames, then any frame delays after the number of images will be discarded.
		/// <para>This property is only used on image data with multiple array indices.</para>
		/// <para>The property only applies to encoding of the image.</para>
		/// <para>The default value is NULL.</para>
		/// </remarks>
		public IList<ushort> FrameDelays
		{
			get;
			set;
		}

		// Looping won't work.  We need to write out NETSCAPE2.0 to the global encoder block and 
		// SharpDX's WIC implementation isn't encoding our byte array properly (or at least, I 
		// can't make it work).  Disable this for now.
		/*/// <summary>
		/// Property to set or return whether an encoded animation is looped or not.
		/// </summary>
		/// <remarks>This property will set the looping flag for a GIF animation.
		/// <para>This property is only used on image data with multiple array indices and the <see cref="P:GorgonLibrary.IO.GorgonCodecGIF.FrameDelays">FrameDelays</see> property must be non-NULL (Nothing in VB.Net).</para>
		/// <para>This property only applies to encoding of the image.</para>
		/// <para>The default value is FALSE.</para>
		/// </remarks>
		public bool LoopAnimation
		{
			get;
			set;
		}*/

		/// <summary>
		/// Property to return whether the codec supports decoding/encoding multiple frames or not.
		/// </summary>
		public override bool SupportsMultipleFrames
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Property to set or return whether all frames in a multi-frame image should be encoded/decoded or not.
		/// </summary>
		/// <remarks>This property will encode or decode multiple frames from or into an array.  Note that this is only supported on codecs that support multiple frames (e.g. animated Gif).  
		/// Images that do not support multiple frames will ignore this flag.
		/// <para>This property applies to both encoding and decoding of image data.</para>
		/// <para>The default value is FALSE.</para>
		/// </remarks>
		public bool UseAllFrames
		{
			get
			{
				return base.CodecUseAllFrames;
			}
			set
			{
				base.CodecUseAllFrames = value;
			}
		}

		/// <summary>
		/// Property to set or return the palette to assign to the 8 bit indexed data for this image.
		/// </summary>		
		/// <remarks>
		/// Use this to alter the color palette for the GIF as it's decoded or encoded.  This array will only support up to 256 indices.  More than 256 indices will 
		/// be ignored.  Set this to NULL (Nothing in VB.Net) to use the palette contained within in the image.
		/// <para>This value does not apply to GIF files with multiple frames.</para>
		/// <para>This property affects both encoding and decoding.</para>
		/// <para>The default value is NULL.</para>
		/// </remarks>
		public IList<GorgonColor> Palette
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the alpha threshold percentage for this codec.
		/// </summary>
		/// <remarks>Use this to determine what percentage of alpha values should be considered transparent for the GIF.  A value of 50 will mean that alpha with values 
		/// of 128 or less will be considered transparent.
		/// <para>This value does not apply to GIF files with multiple frames.</para>
		/// <para>This property affects both encoding and decoding.</para>
		/// <para>The default value is 0.</para>
		/// </remarks>
		public double AlphaThresholdPercent
		{
			get
			{
				return _alphaPercent;
			}
			set
			{
				if (value < 0)
				{
					value = 0;
				}
				if (value > 100)
				{
					value = 100;
				}

				_alphaPercent = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to add custom metadata to the frame.
		/// </summary>
		/// <param name="encoder">Encoder being used to encode the image.</param>
		/// <param name="frame">Frame to encode.</param>
		/// <param name="frameIndex">Index of the current frame.</param>
		/// <param name="settings">Image data settings.</param>
		/// <param name="paletteColors">Palette colors used to encode the images.</param>
		internal override void AddCustomMetaData(WIC.BitmapEncoder encoder, WIC.BitmapFrameEncode frame, int frameIndex, IImageSettings settings, SharpDX.Color[] paletteColors)
		{
			// Do nothing.
			if (FrameDelays == null)
			{
				return;
			}

			if (frame != null)
			{
				if ((settings.ArrayCount > 1) && (UseAllFrames))
				{
					using (var writer = frame.MetadataQueryWriter)
					{
						ushort delayValue = 0;

						if ((FrameDelays != null) && (frameIndex >= 0) && (frameIndex < FrameDelays.Count))
						{
							delayValue = FrameDelays[frameIndex];
						}

						bool hasTransparency = paletteColors.Any(item => item.A == 0);

						writer.SetMetadataByName("/grctlext/Delay", delayValue);
						writer.SetMetadataByName("/grctlext/Disposal", (byte)1);
						writer.SetMetadataByName("/grctlext/TransparencyFlag", hasTransparency);
						if (hasTransparency)
						{
							byte transparentIndex = (byte)Array.FindIndex<DX.Color>(paletteColors, item => item.A == 0);
							writer.SetMetadataByName("/grctlext/TransparentColorIndex", transparentIndex);
						}
					}
				}
			}
		}

		/// <summary>
		/// Function to retrieve the offset for the frame being decoded.
		/// </summary>
		/// <param name="frame">Frame to decode.</param>
		/// <returns>
		/// The position of the offset.
		/// </returns>
		internal override System.Drawing.Point GetFrameOffset(WIC.BitmapFrameDecode frame)
		{
			System.Drawing.Point offset = System.Drawing.Point.Empty;

			if (frame == null)
			{
				return offset;
			}

			// Get frame offsets.
			using (var reader = frame.MetadataQueryReader)
			{
				var offsetX = reader.GetMetadataByName("/imgdesc/Left");
				var offsetY = reader.GetMetadataByName("/imgdesc/Top");

				if (offsetX != null)
				{
					offset.X = (ushort)offsetX;
				}
				if (offsetY != null)
				{
					offset.Y = (ushort)offsetY;
				}
			}

			return offset;
		}

		/// <summary>
		/// Function to retrieve palette information for indexed images.
		/// </summary>
		/// <param name="wic">The WIC interface.</param>
		/// <param name="bitmap">The bitmap to derive the palette from (only used when encoding).</param>
		/// <returns>
		/// A tuple containing the palette data, alpha percentage and the type of palette.
		/// </returns>
		internal override Tuple<WIC.Palette, double, WIC.BitmapPaletteType> GetPaletteInfo(GorgonWICImage wic, WIC.Bitmap bitmap)
		{			
			WIC.Palette palette = null;

			if (Palette == null)
			{
				// If decoding, just return the default, otherwise we'll need to generate from the frame.
				if (bitmap == null)
				{
					return base.GetPaletteInfo(wic, bitmap);
				}
				else
				{
					palette = new WIC.Palette(wic.Factory);
					palette.Initialize(bitmap, 256, true);
					
					return new Tuple<WIC.Palette,double,WIC.BitmapPaletteType>(palette, AlphaThresholdPercent, WIC.BitmapPaletteType.Custom);
				}
			}

			// Generate from our custom palette.
			DX.Color4[] paletteColors = new DX.Color4[256];
			int size = paletteColors.Length.Min(Palette.Count);

			for (int i = 0; i < size; i++)
			{
				paletteColors[i] = Palette[i].SharpDXColor4;
			}

			palette = new WIC.Palette(wic.Factory);
			palette.Initialize(paletteColors);

			return new Tuple<WIC.Palette, double, WIC.BitmapPaletteType>(palette, AlphaThresholdPercent, WIC.BitmapPaletteType.Custom);
		}

        /// <summary>
        /// Function to retrieve a list of frame delays for each frame in an animated GIF.
        /// </summary>
        /// <param name="filePath">Path to the animated GIF file.</param>
        /// <returns>An array of frame delays (1/100th of a second), or an empty array if the image is not an animated GIF.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="filePath"/> parameter is NULL (Nothing in VB.Net).</exception>
        /// <exception cref="System.ArgumentException">Thown when the filePath parameter is empty.</exception>
        /// <exception cref="System.IO.IOException">Thrown when the stream parameter is write-only.
        /// <para>-or-</para>
        /// <para>The data in the stream could not be decoded as GIF file.</para>
        /// </exception>
        /// <exception cref="System.IO.EndOfStreamException">Thrown when an attempt to read beyond the end of the stream is made.</exception>
        public ushort[] GetFrameDelays(string filePath)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException("filePath");
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("The parameter must not be NULL or empty.", "filePath");
            }

            using (var fileStream = System.IO.File.Open(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
            {
                return GetFrameDelays(fileStream);
            }
        }

        /// <summary>
        /// Function to retrieve a list of frame delays for each frame in an animated GIF.
        /// </summary>
        /// <param name="stream">Stream containing the animated GIF.</param>
        /// <returns>
        /// An array of frame delays (1/100th of a second), or an empty array if the image is not an animated GIF.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is NULL (Nothing in VB.Net).</exception>
        /// <exception cref="System.IO.IOException">Thrown when the stream parameter is write-only.
        /// <para>-or-</para>
        /// <para>The data in the stream could not be decoded as GIF file.</para>
        /// <para>-or-</para>
        /// <para>The stream cannot perform seek operations.</para>
        /// </exception>
        /// <exception cref="System.IO.EndOfStreamException">Thrown when an attempt to read beyond the end of the stream is made.</exception>
        public ushort[] GetFrameDelays(System.IO.Stream stream)
        {
            IImageSettings settings = null;
            ushort[] result = new ushort[0];
            Guid bestFormat = Guid.Empty;
            long position = 0;

            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            if (!stream.CanRead)
            {
                throw new System.IO.IOException("Stream is write-only.");
            }

            if (!stream.CanSeek)
            {
                throw new System.IO.IOException("The stream cannot perform seek operations.");
            }

			if (!UseAllFrames)
			{
				return result;
			}

            position = stream.Position;
            
			try
			{
				using (var wrapperStream = new GorgonStreamWrapper(stream))
				{
					// Get our WIC interface.				
					using (var wic = new GorgonWICImage())
					{
						using (var decoder = new WIC.BitmapDecoder(wic.Factory, SupportedFormat))
						{
							using (WIC.WICStream wicStream = new WIC.WICStream(wic.Factory, wrapperStream))
							{
								try
								{
									decoder.Initialize(wicStream, WIC.DecodeOptions.CacheOnDemand);
								}
								catch (DX.SharpDXException sdex)
								{
									// Repackage the exception to keep in line with our API defintion.
									throw new System.IO.IOException("Cannot decode the " + Codec + " file. " + sdex.Descriptor.Description, sdex);
								}

								if (decoder.FrameCount < 2)
								{
									return result;
								}

								result = new ushort[decoder.FrameCount];

								for (int frame = 0; frame < result.Length; frame++)
								{
									using (var frameImage = decoder.GetFrame(frame))
									{
										// Check to see if we can actually read this thing.
										if (frame == 0)
										{
											Guid temp = Guid.Empty;
											settings = ReadMetaData(wic, decoder, frameImage, ref temp);

											if (settings.Format == BufferFormat.Unknown)
											{
												throw new System.IO.IOException("Cannot decode the GIF file.  The data could not be decoded as a GIF file.");
											}
										}

										using (var reader = frameImage.MetadataQueryReader)
										{
											var metaData = reader.GetMetadataByName("/grctlext/Delay");

											if (metaData != null)
											{
												result[frame] = (ushort)metaData;
											}
											else
											{
												result[frame] = 0;
											}
										}
									}
								}
							}
						}
					}
				}
			}
            finally
            {
                stream.Position = position;
            }

            return result;
        }
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonCodecWIC" /> class.
		/// </summary>
		public GorgonCodecGIF()
			: base("GIF", "Graphics Interchange Format", new string[] { "gif" }, WIC.ContainerFormatGuids.Gif)
		{
			FrameDelays = null;
		}
		#endregion
	}
}
