﻿#region MIT
// 
// Gorgon.
// Copyright (C) 2016 Michael Winsor
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
// Created: June 20, 2016 11:49:00 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gorgon.Core;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Graphics.Imaging.Properties;
using Gorgon.IO;
using Gorgon.Math;
using DX = SharpDX;
using WIC = SharpDX.WIC;
using DXGI = SharpDX.DXGI;

namespace Gorgon.Graphics.Imaging
{
	/// <summary>
	/// Utilities that use WIC (Windows Imaging Component) to perform image manipulation operations.
	/// </summary> 
	class WicUtilities
		: IDisposable
	{
		#region Value Types.
		/// <summary>
		/// A value to hold a WIC to Gorgon buffer format value.
		/// </summary>
		private struct WICGorgonFormat
		{
			/// <summary>
			/// WIC GUID to convert from/to.
			/// </summary>
			public readonly Guid WICGuid;
			/// <summary>
			/// Gorgon buffer format to convert from/to.
			/// </summary>
			public readonly DXGI.Format Format;

			/// <summary>
			/// Initializes a new instance of the <see cref="WICGorgonFormat" /> struct.
			/// </summary>
			/// <param name="guid">The GUID.</param>
			/// <param name="format">The format.</param>
			public WICGorgonFormat(Guid guid, DXGI.Format format)
			{
				WICGuid = guid;
				Format = format;
			}
		}

		/// <summary>
		/// A value to hold a nearest supported format conversion.
		/// </summary>
		private struct WICNearest
		{
			/// <summary>
			/// Source format to convert from.
			/// </summary>
			public readonly Guid Source;
			/// <summary>
			/// Destination format to convert to.
			/// </summary>
			public readonly Guid Destination;

			/// <summary>
			/// Initializes a new instance of the <see cref="WICNearest" /> struct.
			/// </summary>
			/// <param name="source">The source.</param>
			/// <param name="dest">The destination.</param>
			public WICNearest(Guid source, Guid dest)
			{
				Source = source;
				Destination = dest;
			}
		}
		#endregion

		#region Conversion Tables.
		// Formats for conversion between Gorgon and WIC.
		private readonly WICGorgonFormat[] _wicDXGIFormats =
		{
			new WICGorgonFormat(WIC.PixelFormat.Format128bppRGBAFloat, DXGI.Format.R32G32B32A32_Float),
			new WICGorgonFormat(WIC.PixelFormat.Format64bppRGBAHalf, DXGI.Format.R16G16B16A16_Float),
			new WICGorgonFormat(WIC.PixelFormat.Format64bppRGBA, DXGI.Format.R16G16B16A16_UNorm),
			new WICGorgonFormat(WIC.PixelFormat.Format32bppRGBA, DXGI.Format.R8G8B8A8_UNorm),
			new WICGorgonFormat(WIC.PixelFormat.Format32bppBGRA, DXGI.Format.B8G8R8A8_UNorm),
			new WICGorgonFormat(WIC.PixelFormat.Format32bppBGR, DXGI.Format.B8G8R8X8_UNorm),
			new WICGorgonFormat(WIC.PixelFormat.Format32bppRGBA1010102XR,DXGI.Format.R10G10B10_Xr_Bias_A2_UNorm),
			new WICGorgonFormat(WIC.PixelFormat.Format32bppRGBA1010102, DXGI.Format.R10G10B10A2_UNorm),
			new WICGorgonFormat(WIC.PixelFormat.Format32bppRGBE, DXGI.Format.R9G9B9E5_Sharedexp),
			new WICGorgonFormat(WIC.PixelFormat.Format16bppBGR565, DXGI.Format.B5G6R5_UNorm),
			new WICGorgonFormat(WIC.PixelFormat.Format16bppBGRA5551, DXGI.Format.B5G5R5A1_UNorm),
			new WICGorgonFormat(WIC.PixelFormat.Format32bppRGBE, DXGI.Format.R9G9B9E5_Sharedexp),
			new WICGorgonFormat(WIC.PixelFormat.Format32bppGrayFloat, DXGI.Format.R32_Float),
			new WICGorgonFormat(WIC.PixelFormat.Format16bppGrayHalf, DXGI.Format.R16_Float),
			new WICGorgonFormat(WIC.PixelFormat.Format16bppGray, DXGI.Format.R16_UNorm),
			new WICGorgonFormat(WIC.PixelFormat.Format8bppGray, DXGI.Format.R8_UNorm),
			new WICGorgonFormat(WIC.PixelFormat.Format8bppAlpha, DXGI.Format.A8_UNorm)
		};

		// Best fit for supported format conversions.
		private readonly WICNearest[] _wicBestFitFormat =
		{
			new WICNearest(WIC.PixelFormat.Format1bppIndexed, WIC.PixelFormat.Format32bppRGBA),
			new WICNearest(WIC.PixelFormat.Format2bppIndexed, WIC.PixelFormat.Format32bppRGBA),
			new WICNearest(WIC.PixelFormat.Format4bppIndexed, WIC.PixelFormat.Format32bppRGBA),
			new WICNearest(WIC.PixelFormat.Format8bppIndexed, WIC.PixelFormat.Format32bppRGBA),
			new WICNearest(WIC.PixelFormat.Format2bppGray, WIC.PixelFormat.Format8bppGray),
			new WICNearest(WIC.PixelFormat.Format4bppGray, WIC.PixelFormat.Format8bppGray),
			new WICNearest(WIC.PixelFormat.Format16bppGrayFixedPoint, WIC.PixelFormat.Format16bppGrayHalf),
			new WICNearest(WIC.PixelFormat.Format32bppGrayFixedPoint, WIC.PixelFormat.Format32bppGrayFloat),
			new WICNearest(WIC.PixelFormat.Format16bppBGR555, WIC.PixelFormat.Format16bppBGRA5551),
			new WICNearest(WIC.PixelFormat.Format32bppBGR101010, WIC.PixelFormat.Format32bppRGBA1010102),
			new WICNearest(WIC.PixelFormat.Format24bppBGR, WIC.PixelFormat.Format32bppRGBA),
			new WICNearest(WIC.PixelFormat.Format24bppRGB, WIC.PixelFormat.Format32bppRGBA),
			new WICNearest(WIC.PixelFormat.Format32bppPBGRA, WIC.PixelFormat.Format32bppRGBA),
			new WICNearest(WIC.PixelFormat.Format32bppPRGBA, WIC.PixelFormat.Format32bppRGBA),
			new WICNearest(WIC.PixelFormat.Format48bppRGB, WIC.PixelFormat.Format64bppRGBA),
			new WICNearest(WIC.PixelFormat.Format48bppBGR, WIC.PixelFormat.Format64bppRGBA),
			new WICNearest(WIC.PixelFormat.Format64bppBGRA, WIC.PixelFormat.Format64bppRGBA),
			new WICNearest(WIC.PixelFormat.Format64bppPRGBA, WIC.PixelFormat.Format64bppRGBA),
			new WICNearest(WIC.PixelFormat.Format64bppPBGRA, WIC.PixelFormat.Format64bppRGBA),
			new WICNearest(WIC.PixelFormat.Format48bppRGBFixedPoint, WIC.PixelFormat.Format64bppRGBAHalf),
			new WICNearest(WIC.PixelFormat.Format48bppBGRFixedPoint, WIC.PixelFormat.Format64bppRGBAHalf),
			new WICNearest(WIC.PixelFormat.Format64bppRGBAFixedPoint, WIC.PixelFormat.Format64bppRGBAHalf),
			new WICNearest(WIC.PixelFormat.Format64bppBGRAFixedPoint, WIC.PixelFormat.Format64bppRGBAHalf),
			new WICNearest(WIC.PixelFormat.Format64bppRGBFixedPoint, WIC.PixelFormat.Format64bppRGBAHalf),
			new WICNearest(WIC.PixelFormat.Format64bppRGBHalf, WIC.PixelFormat.Format64bppRGBAHalf),
			new WICNearest(WIC.PixelFormat.Format48bppRGBHalf, WIC.PixelFormat.Format64bppRGBAHalf),
			new WICNearest(WIC.PixelFormat.Format128bppPRGBAFloat, WIC.PixelFormat.Format128bppRGBAFloat),
			new WICNearest(WIC.PixelFormat.Format128bppRGBFloat, WIC.PixelFormat.Format128bppRGBAFloat),
			new WICNearest(WIC.PixelFormat.Format128bppRGBAFixedPoint, WIC.PixelFormat.Format128bppRGBAFloat),
			new WICNearest(WIC.PixelFormat.Format128bppRGBFixedPoint, WIC.PixelFormat.Format128bppRGBAFloat),
			new WICNearest(WIC.PixelFormat.Format32bppCMYK, WIC.PixelFormat.Format32bppRGBA),
			new WICNearest(WIC.PixelFormat.Format64bppCMYK, WIC.PixelFormat.Format64bppRGBA),
			new WICNearest(WIC.PixelFormat.Format40bppCMYKAlpha, WIC.PixelFormat.Format64bppRGBA),
			new WICNearest(WIC.PixelFormat.Format80bppCMYKAlpha, WIC.PixelFormat.Format64bppRGBA),
			new WICNearest(WIC.PixelFormat.Format96bppRGBFixedPoint, WIC.PixelFormat.Format128bppRGBAFloat)
		};
		#endregion

		#region Variables.
		// The WIC factory.
		private readonly WIC.ImagingFactory _factory;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to find the best buffer format for a given pixel format.
		/// </summary>
		/// <param name="sourcePixelFormat">Pixel format to translate.</param>
		/// <param name="flags">Flags to apply to the pixel format conversion.</param>
		/// <param name="updatedPixelFormat">The updated pixel format after flags are applied.</param>
		/// <returns>The buffer format, or Unknown if the format couldn't be converted.</returns>
		private DXGI.Format FindBestFormat(Guid sourcePixelFormat, WICFlags flags, out Guid updatedPixelFormat)
		{
			DXGI.Format result = _wicDXGIFormats.FirstOrDefault(item => item.WICGuid == sourcePixelFormat).Format;
			updatedPixelFormat = Guid.Empty;

			if (result == DXGI.Format.Unknown)
			{
				if (sourcePixelFormat == WIC.PixelFormat.Format96bppRGBFixedPoint)
				{
					updatedPixelFormat = WIC.PixelFormat.Format128bppRGBAFloat;
					result = DXGI.Format.R32G32B32A32_Float;
				}
				else
				{
					// Find the best fit format if we couldn't find an exact match.
					for (int i = 0; i < _wicBestFitFormat.Length; i++)
					{
						if (_wicBestFitFormat[i].Source != sourcePixelFormat)
						{
							continue;
						}

						Guid bestFormat = _wicBestFitFormat[i].Destination;
						result = _wicDXGIFormats.FirstOrDefault(item => item.WICGuid == bestFormat).Format;
						
						// We couldn't find the format, bail out.
						if (result == DXGI.Format.Unknown)
						{
							return result;
						}

						updatedPixelFormat = bestFormat;
						break;
					}
				}
			}

			switch (result)
			{
				case DXGI.Format.B8G8R8A8_UNorm:
				case DXGI.Format.B8G8R8X8_UNorm:
					if ((flags & WICFlags.ForceRGB) == WICFlags.ForceRGB)
					{
						result = DXGI.Format.R8G8B8A8_UNorm;
						updatedPixelFormat = WIC.PixelFormat.Format32bppRGBA;
					}
					break;
				case DXGI.Format.R10G10B10_Xr_Bias_A2_UNorm:
					if ((flags & WICFlags.NoX2Bias) == WICFlags.NoX2Bias)
					{
						result = DXGI.Format.R10G10B10A2_UNorm;
						updatedPixelFormat = WIC.PixelFormat.Format32bppRGBA1010102;
					}
					break;
				case DXGI.Format.B5G5R5A1_UNorm:
				case DXGI.Format.B5G6R5_UNorm:
					if ((flags & WICFlags.No16BPP) == WICFlags.No16BPP)
					{
						result = DXGI.Format.R8G8B8A8_UNorm;
						updatedPixelFormat = WIC.PixelFormat.Format32bppRGBA;
					}
					break;
				case DXGI.Format.R1_UNorm:
					if ((flags & WICFlags.AllowMono) != WICFlags.AllowMono)
					{
						result = DXGI.Format.R1_UNorm;
						updatedPixelFormat = WIC.PixelFormat.Format8bppGray;
					}
					break;
			}

			return result;
		}

		/// <summary>
		/// Function to retrieve a WIC equivalent format GUID based on the Gorgon buffer format.
		/// </summary>
		/// <param name="format">Format to look up.</param>
		/// <returns>The GUID for the format, or NULL (<i>Nothing</i> in VB.Net) if not found.</returns>
		private Guid GetGUID(DXGI.Format format)
		{
			for (int i = 0; i < _wicDXGIFormats.Length; i++)
			{
				if (_wicDXGIFormats[i].Format == format)
				{
					return _wicDXGIFormats[i].WICGuid;
				}
			}

			switch (format)
			{
				case DXGI.Format.R8G8B8A8_UNorm_SRgb:
					return WIC.PixelFormat.Format32bppRGBA;
				case DXGI.Format.D32_Float:
					return WIC.PixelFormat.Format32bppGrayFloat;
				case DXGI.Format.D16_UNorm:
					return WIC.PixelFormat.Format16bppGray;
				case DXGI.Format.B8G8R8A8_UNorm_SRgb:
					return WIC.PixelFormat.Format32bppBGRA;
				case DXGI.Format.B8G8R8X8_UNorm_SRgb:
					return WIC.PixelFormat.Format32bppBGR;
			}

			return Guid.Empty;
		}

		/// <summary>
		/// Function to convert a <see cref="IGorgonImageBuffer"/> into a WIC bitmap.
		/// </summary>
		/// <param name="imageData">The image data buffer to convert.</param>
		/// <param name="pixelFormat">The WIC pixel format of the data in the buffer.</param>
		/// <returns>The WIC bitmap pointing to the data stored in <paramref name="imageData"/>.</returns>
		private WIC.Bitmap GetBitmap(IGorgonImageBuffer imageData, Guid pixelFormat)
		{
			var dataRect = new DX.DataRectangle(new IntPtr(imageData.Data.Address), imageData.PitchInformation.RowPitch);
			return new WIC.Bitmap(_factory, imageData.Width, imageData.Height, pixelFormat, dataRect);
		}

		/// <summary>
		/// Function to retrieve a WIC format converter.
		/// </summary>
		/// <param name="bitmap">The WIC bitmap source to use for the converter.</param>
		/// <param name="targetFormat">The WIC format to convert to.</param>
		/// <param name="dither">The type of dithering to apply to the image data during conversion (if down sampling).</param>
		/// <returns>The WIC converter.</returns>
		private WIC.FormatConverter GetFormatConverter(WIC.BitmapSource bitmap, Guid targetFormat, ImageDithering dither)
		{
			var result = new WIC.FormatConverter(_factory);

			if (!result.CanConvert(bitmap.PixelFormat, targetFormat))
			{
				throw new GorgonException(GorgonResult.FormatNotSupported, string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, "WICGuid{" + targetFormat + "}"));
			}

			// TODO: Get 8 bit palette information.
			// TODO: Pass palette info to this method, and to frame.Palette.
			result.Initialize(bitmap, targetFormat, (WIC.BitmapDitherType)dither, null, 0, WIC.BitmapPaletteType.Custom);

			return result;
		}

		/// <summary>
		/// Function to build the WIC color contexts used to convert to and/or from sRgb pixel formats.
		/// </summary>
		/// <param name="source">The WIC bitmap source to use for sRgb conversion.</param>
		/// <param name="pixelFormat">The pixel format of the image data.</param>
		/// <param name="srcIsSRgb"><b>true</b> if the source data is already in sRgb; otherwise <b>false</b>.</param>
		/// <param name="destIsSRgb"><b>true</b> if the destination data should be converted to sRgb; otherwise <b>false</b>.</param>
		/// <param name="srcContext">The WIC color context for the source image.</param>
		/// <param name="destContext">The WIC color context for the destination image.</param>
		/// <returns>A WIC color transformation object use to convert to/from sRgb.</returns>
		private WIC.BitmapSource GetSRgbTransform(WIC.BitmapSource source, Guid pixelFormat, bool srcIsSRgb, bool destIsSRgb, out WIC.ColorContext srcContext, out WIC.ColorContext destContext)
		{
			srcContext = new WIC.ColorContext(_factory);
			destContext = new WIC.ColorContext(_factory);

			srcContext.InitializeFromExifColorSpace(srcIsSRgb ? 1 : 2);
			destContext.InitializeFromExifColorSpace(destIsSRgb ? 1 : 2);

			var result = new WIC.ColorTransform(_factory);
			result.Initialize(source, srcContext, destContext, pixelFormat);

			return result;
		}

		/// <summary>
		/// Function to assign frame encoding options to the frame based on the codec.
		/// </summary>
		/// <param name="frame">The frame that holds the options to set.</param>
		/// <param name="options">The list of options to apply.</param>
		private static void SetFrameOptions(WIC.BitmapFrameEncode frame, IGorgonCodecWicEncodingOptions options)
		{
			// Options that do not exist will have their default value for that type applied.
			frame.Options.InterlaceOption = options.Options.GetOption<bool>("Interlacing");
			frame.Options.FilterOption = (WIC.PngFilterOption)options.Options.GetOption<PNGFilter>("Filter");
		}

		/// <summary>
		/// Function to encode image data into a single frame for a WIC bitmap image.
		/// </summary>
		/// <param name="encoder">The WIC encoder to use.</param>
		/// <param name="imageData">The image data to encode.</param>
		/// <param name="pixelFormat">The pixel format for the image data.</param>
		/// <param name="options">Optional encoding options (depends on codec).</param>
		/// <param name="frameIndex">The current frame index.</param>
		private void EncodeFrame(WIC.BitmapEncoder encoder, IGorgonImage imageData, Guid pixelFormat, IGorgonCodecWicEncodingOptions options, int frameIndex)
		{
			Guid requestedFormat = pixelFormat;

			using (var frame = new WIC.BitmapFrameEncode(encoder))
			{
				IGorgonImageBuffer buffer = imageData.Buffers[frameIndex];

				frame.Initialize();
				frame.SetSize(buffer.Width, buffer.Height);
				frame.SetResolution(options?.DpiX ?? 72, options?.DpiY ?? 72);
				frame.SetPixelFormat(ref pixelFormat);

				// We expect these values to be set to their defaults.  If they are not, then we will have an error.
				// These are for PNG only.
				if (options != null)
				{
					SetFrameOptions(frame, options);
				}

				// If there's a disparity between what we asked for, and what we actually support, then convert to the correct format.
				if (requestedFormat != pixelFormat)
				{
					using (WIC.Bitmap bitmap = GetBitmap(buffer, requestedFormat))
					{
						using (WIC.BitmapSource converter = GetFormatConverter(bitmap, pixelFormat, options?.Dithering ?? ImageDithering.None))
						{
							frame.WriteSource(converter);
						}
					}
				}
				else
				{
					frame.WritePixels(buffer.Height, new IntPtr(buffer.Data.Address), buffer.PitchInformation.RowPitch, buffer.PitchInformation.SlicePitch);
				}

				frame.Commit();
			}
		}

		/// <summary>
		/// Function to determine if a format can be converted to any of the requested formats.
		/// </summary>
		/// <param name="sourceFormat">The source format to convert.</param>
		/// <param name="destFormats">The destination formats to convert to.</param>
		/// <returns>A list of formats that the <paramref name="sourceFormat"/> can convert into.</returns>
		public IReadOnlyList<DXGI.Format> CanConvertFormats(DXGI.Format sourceFormat, IEnumerable<DXGI.Format> destFormats)
		{
			Guid sourceGuid = GetGUID(sourceFormat);

			if (sourceGuid == Guid.Empty)
			{
				return new DXGI.Format[0];
			}

			var result = new List<DXGI.Format>();

			using (var converter = new WIC.FormatConverter(_factory))
			{
				foreach (DXGI.Format destFormat in destFormats)
				{
					Guid destGuid = GetGUID(destFormat);

					if (destGuid == Guid.Empty)
					{
						continue;
					}

					if (destGuid == sourceGuid)
					{
						result.Add(destFormat);
						continue;
					}

					if (converter.CanConvert(sourceGuid, destGuid))
					{
						result.Add(destFormat);
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Function to read metadata about an image file stored within a stream.
		/// </summary>
		/// <param name="stream">The stream containing the image file data.</param>
		/// <param name="fileFormat">The file format of the image data.</param>
		/// <param name="options">Options used for decoding the image data.</param>
		/// <param name="frame">The WIC frame decoder used to read the image data.</param>
		/// <param name="decoder">The WIC decoder used to read the file data.</param>
		/// <param name="wicStream">The WIC stream containing the file data.</param>
		/// <param name="actualPixelFormat">The actual pixel format of the image data, used when conversion is necessary.</param>
		/// <returns>A <see cref="GorgonImageInfo"/> containing information about the image data.</returns>
		private GorgonImageInfo GetImageMetaData(Stream stream, Guid fileFormat, IGorgonCodecWicDecodingOptions options, out WIC.BitmapFrameDecode frame, out WIC.BitmapDecoder decoder, out WIC.WICStream wicStream, out Guid actualPixelFormat)
		{
			wicStream = new WIC.WICStream(_factory, stream);

			decoder = new WIC.BitmapDecoder(_factory, fileFormat);
			decoder.Initialize(wicStream, WIC.DecodeOptions.CacheOnDemand);

			if (decoder.ContainerFormat != fileFormat)
			{
				actualPixelFormat = Guid.Empty;
				frame = null;
				return null;
			}

			frame = decoder.GetFrame(0);
			DXGI.Format format = FindBestFormat(frame.PixelFormat, options?.Flags ?? WICFlags.None, out actualPixelFormat);

			int arrayCount = 1;

			if ((options != null) && (decoder.DecoderInfo.IsMultiframeSupported))
			{
				arrayCount = options.ReadAllFrames ? decoder.FrameCount : 1;
			}

			return new GorgonImageInfo(ImageType.Image2D, format)
			{
				Width = frame.Size.Width,
				Height = frame.Size.Height,
				ArrayCount = arrayCount,
				Depth = 1,
				MipCount = 1
			};
		}

		/// <summary>
		/// Function to encode a <see cref="IGorgonImage"/> into a known WIC file format.
		/// </summary>
		/// <param name="imageData">The image to encode.</param>
		/// <param name="imageStream">The stream that will contain the encoded data.</param>
		/// <param name="imageFileFormat">The file format to use for encoding.</param>
		/// <param name="options">The encoding options for the codec performing the encode operation.</param>
		public void EncodeImageData(IGorgonImage imageData, Stream imageStream, Guid imageFileFormat, IGorgonCodecWicEncodingOptions options)
		{
			WIC.BitmapEncoder encoder = null;
			WIC.BitmapEncoderInfo encoderInfo = null;
			int frameCount = 1;

			try
			{
				Guid pixelFormat = GetGUID(imageData.Info.Format);

				if (pixelFormat == Guid.Empty)
				{
					throw new GorgonException(GorgonResult.FormatNotSupported, string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, imageData.Info.Format));
				}

				encoder = new WIC.BitmapEncoder(_factory, imageFileFormat);
				encoder.Initialize(imageStream);

				encoderInfo = encoder.EncoderInfo;

				if ((options != null) 
					&& (options.SaveAllFrames) 
					&& (imageData.Info.ArrayCount > 1) 
					&& (encoderInfo.IsMultiframeSupported))
				{
					frameCount = imageData.Info.ArrayCount;
				}

				for (int i = 0; i < frameCount; ++i)
				{
					EncodeFrame(encoder, imageData, pixelFormat, options, i);
				}

				encoder.Commit();
			}
			finally
			{
				encoderInfo?.Dispose();
				encoder?.Dispose();
			}
		}

		/// <summary>
		/// Function to read the data from a frame.
		/// </summary>
		/// <param name="data">Image data to populate.</param>
		/// <param name="srcFormat">Source image format.</param>
		/// <param name="convertFormat">Conversion format.</param>
		/// <param name="frame">Frame containing the image data.</param>
		/// <param name="dither">The type of dithering to use when converting the pixel format bit depth.</param>
		private void ReadFrame(IGorgonImage data, Guid srcFormat, Guid convertFormat, WIC.BitmapFrameDecode frame, ImageDithering dither)
		{
			var buffer = data.Buffers[0];

			// We don't need to convert, so just leave.
			if ((convertFormat == Guid.Empty) || (srcFormat == convertFormat))
			{
				frame.CopyPixels(buffer.PitchInformation.RowPitch, new IntPtr(buffer.Data.Address), buffer.PitchInformation.SlicePitch);
				return;
			}

			WIC.BitmapSource formatConverter = null;
			WIC.BitmapSource sourceBitmap = frame;
			Tuple<WIC.Palette, double, WIC.BitmapPaletteType> paletteInfo = null;

			try
			{
				// Perform conversion.
				bool isIndexed = ((frame.PixelFormat == WIC.PixelFormat.Format8bppIndexed)
				                  || (frame.PixelFormat == WIC.PixelFormat.Format4bppIndexed)
				                  || (frame.PixelFormat == WIC.PixelFormat.Format2bppIndexed)
				                  || (frame.PixelFormat == WIC.PixelFormat.Format1bppIndexed));
				

				if (isIndexed)
				{
					//paletteInfo = GetPaletteInfo(wic, null);

					// Create a temporary bitmap to convert our indexed image.
					sourceBitmap = new WIC.Bitmap(_factory, frame, WIC.BitmapCreateCacheOption.NoCache);
				}

				formatConverter = GetFormatConverter(sourceBitmap, convertFormat, dither);
				formatConverter.CopyPixels(buffer.PitchInformation.RowPitch, new IntPtr(buffer.Data.Address), buffer.PitchInformation.SlicePitch);
			}
			finally
			{
				paletteInfo?.Item1?.Dispose();
				formatConverter?.Dispose();
				sourceBitmap?.Dispose();
			}
		}

		/// <summary>
		/// Function to scale the WIC bitmap data to a new size and place it into the buffer provided.
		/// </summary>
		/// <param name="bitmap">The WIC bitmap to scale.</param>
		/// <param name="buffer">The buffer that will receive the data.</param>
		/// <param name="width">The new width of the image data.</param>
		/// <param name="height">The new height of the image data.</param>
		/// <param name="filter">The filter to apply when smoothing the image during scaling.</param>
		private void ScaleBitmapData(WIC.BitmapSource bitmap, IGorgonImageBuffer buffer, int width, int height, ImageFilter filter)
		{
			using (WIC.BitmapScaler scaler = new WIC.BitmapScaler(_factory))
			{
				scaler.Initialize(bitmap, width, height, (WIC.BitmapInterpolationMode)filter);

				if (bitmap.PixelFormat == scaler.PixelFormat)
				{
					scaler.CopyPixels(buffer.PitchInformation.RowPitch, new IntPtr(buffer.Data.Address), buffer.PitchInformation.SlicePitch);
					return;
				}

				// There's a chance that, due the filter applied, that the format is now different. 
				// So we'll need to convert.
				using (WIC.FormatConverter converter = GetFormatConverter(scaler, bitmap.PixelFormat, ImageDithering.None))
				{
					converter.CopyPixels(buffer.PitchInformation.RowPitch, new IntPtr(buffer.Data.Address), buffer.PitchInformation.SlicePitch);
				}
			}
		}

		/// <summary>
		/// Function to scale the WIC bitmap data to a new size and place it into the buffer provided.
		/// </summary>
		/// <param name="bitmap">The WIC bitmap to scale.</param>
		/// <param name="buffer">The buffer that will receive the data.</param>
		/// <param name="offsetX">The horizontal offset to start cropping at.</param>
		/// <param name="offsetY">The vertical offset to start cropping at.</param>
		/// <param name="width">The new width of the image data.</param>
		/// <param name="height">The new height of the image data.</param>
		private void CropBitmapData(WIC.BitmapSource bitmap, IGorgonImageBuffer buffer, int offsetX, int offsetY, int width, int height)
		{
			using (WIC.BitmapClipper clipper = new WIC.BitmapClipper(_factory))
			{
				DX.Rectangle rect = DX.Rectangle.Intersect(new DX.Rectangle(0, 0, bitmap.Size.Width, bitmap.Size.Height),
				                                           new DX.Rectangle(offsetX, offsetY, width, height));

				if (rect.IsEmpty)
				{
					return;
				}

				// Intersect our clipping rectangle with the buffer size.
				clipper.Initialize(bitmap, rect);
				clipper.CopyPixels(buffer.PitchInformation.RowPitch, new IntPtr(buffer.Data.Address), buffer.PitchInformation.SlicePitch);
			}
		}

		/// <summary>
		/// Function to retrieve the metadata for an image from a stream.
		/// </summary>
		/// <param name="stream">The stream containing the image data.</param>
		/// <param name="fileFormat">The file format of the data in the stream.</param>
		/// <param name="options">Options for image decoding.</param>
		/// <returns>The image metadata from the stream and the file format for the file in the stream.</returns>
		public GorgonImageInfo GetImageMetaDataFromStream(Stream stream, Guid fileFormat, IGorgonImageCodecDecodingOptions options)
		{
			long oldPosition = stream.Position;
			var wrapper = new GorgonStreamWrapper(stream, stream.Position);
			WIC.BitmapDecoder decoder = null;
			WIC.WICStream wicStream = null;
			WIC.BitmapFrameDecode frame = null;
			
			try
			{
				// We don't be needing this.
				Guid dummy;

				return GetImageMetaData(wrapper,
				                        fileFormat,
										options as IGorgonCodecWicDecodingOptions, 
				                        out frame,
				                        out decoder,
				                        out wicStream,
				                        out dummy);
			}
			finally
			{
				stream.Position = oldPosition;

				wicStream?.Dispose();
				decoder?.Dispose();
				frame?.Dispose();
			}
		}
		
		/// <summary>
		/// Function to decode image data from a file within a stream.
		/// </summary>
		/// <param name="stream">The stream containing the image data to decode.</param>
		/// <param name="length">The size of the image data to read, in bytes.</param>
		/// <param name="imageFileFormat">The file format for the image data in the stream.</param>
		/// <param name="decodingOptions">Options used for decoding the image data.</param>
		/// <returns>A <see cref="IGorgonImage"/> containing the decoded image file data.</returns>
		public IGorgonImage DecodeImageData(Stream stream, long length, Guid imageFileFormat, IGorgonCodecWicDecodingOptions decodingOptions)
		{
			WIC.BitmapDecoder decoder = null;
			WIC.BitmapFrameDecode frame = null;
			WIC.WICStream decoderStream = null;
			IGorgonImage result = null;

			try
			{
				var wrapper = new GorgonStreamWrapper(stream, stream.Position, length);

				Guid pixelFormat;
				GorgonImageInfo info = GetImageMetaData(stream, imageFileFormat, decodingOptions, out frame, out decoder, out decoderStream, out pixelFormat);

				if (info == null)
				{
					return null;
				}

				if (info.Format == DXGI.Format.Unknown)
				{
					throw new GorgonException(GorgonResult.FormatNotSupported, string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, info.Format));
				}

				// Build the image.
				result = new GorgonImage(info);

				// TODO: Read multiple frames if necessary.
				// Read a single frame of data.
				ReadFrame(result, frame.PixelFormat, pixelFormat, frame, decodingOptions?.Dithering ?? ImageDithering.None);

				// If we've not read the full length of the data (WIC seems to ignore the CRC on the IEND chunk for PNG files for example),
				// then we need to move the pointer up by however many bytes we've missed.
				if (wrapper.Position < length)
				{
					wrapper.Position = length;
				}

				return result;
			}
			catch
			{
				result?.Dispose();
				throw;
			}
			finally
			{
				frame?.Dispose();
				decoderStream?.Dispose();
				decoder?.Dispose();
			}
		}

		/// <summary>
		/// Function to generate mip map images.
		/// </summary>
		/// <param name="destImageData">The image that will receive the mip levels.</param>
		/// <param name="filter">The filter to apply when resizing the buffers.</param>
		public void GenerateMipImages(IGorgonImage destImageData, ImageFilter filter)
		{
			Guid pixelFormat = GetGUID(destImageData.Info.Format);

			if (pixelFormat == Guid.Empty)
			{
				throw new GorgonException(GorgonResult.FormatNotSupported, string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, destImageData.Info.Format));
			}

			WIC.Bitmap bitmap = null;

			try
			{
				// Begin scaling.
				for (int array = 0; array < destImageData.Info.ArrayCount; ++array)
				{
					int mipDepth = destImageData.Info.Depth;

					// Start at 1 because we're copying from the first mip level..
					for (int mipLevel = 1; mipLevel < destImageData.Info.MipCount; ++mipLevel)
					{
						for (int depth = 0; depth < mipDepth; ++depth)
						{
							var sourceBuffer = destImageData.Buffers[0, destImageData.Info.ImageType == ImageType.Image3D ? (destImageData.Info.Depth / mipDepth) * depth : array];
							var destBuffer = destImageData.Buffers[mipLevel, destImageData.Info.ImageType == ImageType.Image3D ? depth : array];

							bitmap = GetBitmap(sourceBuffer, pixelFormat);

							ScaleBitmapData(bitmap, destBuffer, destBuffer.Width, destBuffer.Height, filter);

							bitmap.Dispose();
						}

						// Scale the depth.
						if (mipDepth > 1)
						{
							mipDepth >>= 1;
						}
					}
				}
			}
			finally
			{
				bitmap?.Dispose();
			}
		}

		/// <summary>
		/// Function to resize an image to the specified dimensions.
		/// </summary>
		/// <param name="imageData">The image data to resize.</param>
		/// <param name="offsetX">The horizontal offset to start at for cropping (ignored when crop = false).</param>
		/// <param name="offsetY">The vertical offset to start at for cropping (ignored when crop = false).</param>
		/// <param name="newWidth">The new width for the image.</param>
		/// <param name="newHeight">The new height for the image.</param>
		/// <param name="newDepth">The new depth for the image.</param>
		/// <param name="calculatedMipLevels">The number of mip levels to support.</param>
		/// <param name="scaleFilter">The filter to apply when smoothing the image during scaling.</param>
		/// <param name="crop"><b>true</b> to crop the image, <b>false</b> to scale it.</param>
		/// <returns>A new <see cref="IGorgonImage"/> containing the resized data.</returns>
		public IGorgonImage Resize(IGorgonImage imageData, int offsetX, int offsetY, int newWidth, int newHeight, int newDepth, int calculatedMipLevels, ImageFilter scaleFilter, bool crop)
		{
			Guid pixelFormat = GetGUID(imageData.Info.Format);

			if (pixelFormat == Guid.Empty)
			{
				throw new GorgonException(GorgonResult.FormatNotSupported, string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, imageData.Info.Format));
			}

			var imageInfo = new GorgonImageInfo(imageData.Info.ImageType, imageData.Info.Format)
			                {
				                Width = newWidth,
				                Height = newHeight,
				                Depth = newDepth.Max(1),
				                ArrayCount = imageData.Info.ArrayCount,
				                MipCount = calculatedMipLevels
			                };

			var result = new GorgonImage(imageInfo);
			WIC.Bitmap bitmap = null;

			try
			{
				for (int array = 0; array < imageInfo.ArrayCount; ++array)
				{
					for (int mip = 0; mip < calculatedMipLevels.Min(imageData.Info.MipCount); ++mip)
					{
						int mipDepth = result.GetDepthCount(mip).Min(imageData.Info.Depth);

						for (int depth = 0; depth < mipDepth; ++depth)
						{
							IGorgonImageBuffer destBuffer = result.Buffers[mip, imageData.Info.ImageType == ImageType.Image3D ? depth : array];
							IGorgonImageBuffer srcBuffer = imageData.Buffers[mip, imageData.Info.ImageType == ImageType.Image3D ? depth : array];
							
							bitmap = GetBitmap(srcBuffer, pixelFormat);

							if (!crop)
							{
								ScaleBitmapData(bitmap, destBuffer, newWidth, newHeight, scaleFilter);
							}
							else
							{
								CropBitmapData(bitmap, destBuffer, offsetX, offsetY, newWidth, newHeight);
							}

							bitmap.Dispose();
							bitmap = null;
						}
					}
				}

				return result;
			}
			catch
			{
				result.Dispose();
				throw;
			}
			finally
			{
				bitmap?.Dispose();
			}
		}

		/// <summary>
		/// Function to convert from one pixel format to another.
		/// </summary>
		/// <param name="imageData">The image data to convert.</param>
		/// <param name="format">The format to convert to.</param>
		/// <param name="dithering">The type of dithering to apply if the image bit depth has to be reduced.</param>
		/// <param name="isSrcSRgb"><b>true</b> if the image data uses sRgb; otherwise <b>false</b>.</param>
		/// <param name="isDestSRgb"><b>true</b> if the resulting image data should use sRgb; otherwise <b>false</b>.</param>
		/// <returns>A <see cref="IGorgonImage"/> containing the converted image data.</returns>
		public IGorgonImage ConvertToFormat(IGorgonImage imageData, DXGI.Format format, ImageDithering dithering, bool isSrcSRgb, bool isDestSRgb)
		{
			Guid sourceFormat = GetGUID(imageData.Info.Format);
			Guid destFormat = GetGUID(format);

			// Duplicate the settings, and update the format.
			var resultInfo = new GorgonImageInfo(imageData.Info.ImageType, format)
			                 {
				                 Width = imageData.Info.Width,
				                 Height = imageData.Info.Height,
				                 ArrayCount = imageData.Info.ArrayCount,
				                 Depth = imageData.Info.Depth,
				                 MipCount = imageData.Info.MipCount
			                 };

			var result = new GorgonImage(resultInfo);

			try
			{
				for (int array = 0; array < resultInfo.ArrayCount; array++)
				{
					for (int mip = 0; mip < resultInfo.MipCount; mip++)
					{
						int depthCount = result.GetDepthCount(mip);

						for (int depth = 0; depth < depthCount; depth++)
						{
							// Get the array/mip/depth buffer.
							IGorgonImageBuffer destBuffer = result.Buffers[mip, resultInfo.ImageType == ImageType.Image3D ? depth : array];
							IGorgonImageBuffer srcBuffer = imageData.Buffers[mip, resultInfo.ImageType == ImageType.Image3D ? depth : array];
							var rect = new DX.DataRectangle(new IntPtr(srcBuffer.Data.Address), srcBuffer.PitchInformation.RowPitch);

							WIC.Bitmap bitmap = null;
							WIC.BitmapSource formatConverter = null;
							WIC.BitmapSource sRgbConverter = null;
							WIC.ColorContext srcColorContext = null;
							WIC.ColorContext destColorContext = null;

							try
							{
								// Create a WIC bitmap so we have a source for conversion.
								bitmap = new WIC.Bitmap(_factory, srcBuffer.Width, srcBuffer.Height, sourceFormat, rect, srcBuffer.PitchInformation.SlicePitch);
								WIC.BitmapSource converterSource = formatConverter = GetFormatConverter(bitmap, destFormat, dithering);

								// If we have an sRgb conversion, then apply that after converting formats.
								if ((isSrcSRgb) || (isDestSRgb))
								{
									converterSource = sRgbConverter = GetSRgbTransform(formatConverter, destFormat, isSrcSRgb, isDestSRgb, out srcColorContext, out destColorContext);
								}

								converterSource.CopyPixels(destBuffer.PitchInformation.RowPitch, new IntPtr(destBuffer.Data.Address), destBuffer.PitchInformation.SlicePitch);
							}
							finally
							{
								srcColorContext?.Dispose();
								destColorContext?.Dispose();
								sRgbConverter?.Dispose();
								formatConverter?.Dispose();
								bitmap?.Dispose();
							}
						}
					}
				}

				return result;
			}
			catch
			{
				result.Dispose();
				throw;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			_factory?.Dispose();
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="WicUtilities"/> class.
		/// </summary>
		public WicUtilities()
		{
			_factory = new WIC.ImagingFactory();
		}
		#endregion
	}
}