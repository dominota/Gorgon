﻿#region MIT
// 
// Gorgon.
// Copyright (C) 2019 Michael Winsor
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
// Created: January 4, 2019 9:42:51 PM
// 
#endregion

using System;
using System.IO;
using System.Linq;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Editor.Content;
using Gorgon.Editor.ImageEditor.Properties;
using Gorgon.Editor.Services;
using Gorgon.Graphics;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// Provides I/O functionality for reading/writing image data.
    /// </summary>
    internal class ImageIOService : IImageIOService
    {
        #region Variables.
        // The block compressor.
        private readonly TexConvCompressor _compressor;
        // The export image dialog service.
        private readonly IExportImageDialogService _exportDialog;
        // The export image dialog service.
        private readonly IImportImageDialogService _importDialog;
        // The logging interface to use.
        private readonly IGorgonLog _log;
        // The busy state service.
        private readonly IBusyStateService _busyState;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the default plugin codec.
        /// </summary>
        public IGorgonImageCodec DefaultCodec
        {
            get;
        }

        /// <summary>
        /// Property to return the list of installed image codecs.
        /// </summary>
        public ICodecRegistry InstalledCodecs
        {
            get;
        }

        /// <summary>
        /// Property to return the file system writer used to write to the temporary area.
        /// </summary>
        public IGorgonFileSystemWriter<Stream> ScratchArea
        {
            get;
        }

        /// <summary>
        /// Property to return whether or not block compression is supported.
        /// </summary>
        public bool CanHandleBlockCompression => _compressor != null;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to import an image file from the physical file system into the current image.
        /// </summary>
        /// <param name="codec">The codec used to open the file.</param>
        /// <param name="filePath">The path to the file to import.</param>
        /// <returns>The source file information, image data, the virtual file entry for the working file and the original pixel format of the file.</returns>
        public (FileInfo file, IGorgonImage image, IGorgonVirtualFile workingFile, BufferFormat originalFormat) ImportImage(IGorgonImageCodec codec, string filePath)
        {
            var file = new FileInfo(filePath);
            IGorgonImageCodec importCodec = codec;
            IGorgonImageInfo metaData = null;
            IGorgonVirtualFile workFile = null;
            IGorgonImage importImage = null;
            string workFilePath = $"{Path.GetFileNameWithoutExtension(filePath)}_import_{Guid.NewGuid().ToString("N")}";

            // Try to determine if we can actually read the file using an installed codec, if we can't, then try to find a suitable codec.
            using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                if ((importCodec == null) || (!importCodec.IsReadable(stream)))
                {
                    importCodec = null;

                    foreach (IGorgonImageCodec newCodec in InstalledCodecs.Codecs.Where(item => (item.CodecCommonExtensions.Count > 0) && (item.CanDecode)))
                    {
                        if (newCodec.IsReadable(stream))
                        {
                            importCodec = newCodec;
                            break;
                        }
                    }
                }

                if (importCodec == null)
                {
                    throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORIMG_ERR_NO_CODEC, filePath));
                }

                metaData = importCodec.GetMetaData(stream);


                // We absolutely need to have an extension, or else the texconv tool will not work.
                var codecExtension = new GorgonFileExtension(importCodec.CodecCommonExtensions[0]);
                _log.Print($"Adding {codecExtension.Extension} extension to working file or else external tools may not be able to read it.", LoggingLevel.Verbose);
                workFilePath = $"{workFilePath}.{codecExtension.Extension}";

                using (Stream outStream = ScratchArea.OpenStream(workFilePath, FileMode.Create))
                {
                    stream.CopyTo(outStream);
                }
            }

            workFile = ScratchArea.FileSystem.GetFile(workFilePath);
            var formatInfo = new GorgonFormatInfo(metaData.Format);

            // This is always in DDS format.
            if (formatInfo.IsCompressed)
            {
                _log.Print($"Image is compressed using [{formatInfo.Format}] as its pixel format.", LoggingLevel.Intermediate);

                if (_compressor == null)
                {
                    throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORIMG_ERR_COMPRESSED_FILE, formatInfo.Format));
                }

                _log.Print($"Loading image '{workFile.FullPath}'...", LoggingLevel.Simple);
                importImage = _compressor.Decompress(ref workFile, metaData);

                if (importImage == null)
                {
                    throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORIMG_ERR_COMPRESSED_FILE, formatInfo.Format));
                }

                _log.Print($"Loaded compressed ([{formatInfo.Format}]) image data as [{importImage.Format}]", LoggingLevel.Intermediate);
            }
            else
            {
                using (Stream workStream = workFile.OpenStream())
                {
                    importImage = importCodec.LoadFromStream(workStream);
                }
            }

            return (file, importImage, workFile, metaData.Format);
        }

        /// <summary>Function to perform an import of an image into the current image mip level and array index/depth slice.</summary>
        /// <returns>The source file information, image data, the virtual file entry for the working file and the original pixel format of the file.</returns>
        public (FileInfo file, IGorgonImage image, IGorgonVirtualFile workingFile, BufferFormat originalFormat) ImportImage()
        {
            string importPath = _importDialog.GetFilename();

            if (string.IsNullOrWhiteSpace(importPath))
            {
                return (null, null, null, BufferFormat.Unknown);
            }

            try
            {
                _busyState.SetBusy();
                return ImportImage(_importDialog.SelectedCodec, importPath);
            }
            finally
            {
                _busyState.SetIdle();
            }
        }

        /// <summary>
        /// Function to perform an export of an image.
        /// </summary>
        /// <param name="file">The file to export.</param>
        /// <param name="image">The image data to export.</param>
        /// <param name="codec">The codec to use when encoding the image.</param>
        /// <returns>The path to the exported file.</returns>
        public FileInfo ExportImage(IContentFile file, IGorgonImage image, IGorgonImageCodec codec)
        {
            _exportDialog.ContentFile = file;
            _exportDialog.SelectedCodec = codec;
            string exportFilePath = _exportDialog.GetFilename();

            if (string.IsNullOrWhiteSpace(exportFilePath))
            {
                return null;
            }

            _busyState.SetBusy();

            var result = new FileInfo(exportFilePath);

            _log.Print($"Exporting '{file.Name}' to '{exportFilePath}' as {codec.CodecDescription}", LoggingLevel.Verbose);

            codec.SaveToFile(image, result.FullName);

            _busyState.SetIdle();

            return result;
        }

        /// <summary>
        /// Function to save the specified image file.
        /// </summary>
        /// <param name="name">The name of the file to write into.</param>
        /// <param name="image">The image to save.</param>
        /// <param name="pixelFormat">The pixel format for the image.</param>
        /// <param name="codec">[Optional] The codec to use when saving the image.</param>
        /// <returns>The updated working file.</returns>
        public IGorgonVirtualFile SaveImageFile(string name, IGorgonImage image, BufferFormat pixelFormat, IGorgonImageCodec codec = null)
        {
            if (codec == null)
            {
                codec = DefaultCodec;
            }

            // We absolutely need to have an extension, or else the texconv tool will not work.
            if ((codec.CodecCommonExtensions.Count > 0)
                && (!string.Equals(Path.GetExtension(name), codec.CodecCommonExtensions[0], StringComparison.OrdinalIgnoreCase)))
            {
                _log.Print("Adding extension to working file or else external tools may not be able to read it.", LoggingLevel.Verbose);
                name = Path.ChangeExtension(name, codec.CodecCommonExtensions[0]);
            }

            IGorgonVirtualFile workFile = ScratchArea.FileSystem.GetFile(name);
            var formatInfo = new GorgonFormatInfo(pixelFormat);

            // The file doesn't exist, so we need to create a dummy file.
            if (workFile == null)
            {
                using (Stream tempStream = ScratchArea.OpenStream(name, FileMode.Create))
                {
                    tempStream.WriteString("TEMP_WORKING_FILE");
                }

                workFile = ScratchArea.FileSystem.GetFile(name);
            }

            _log.Print($"Working image file: '{workFile.FullPath}'.", LoggingLevel.Verbose);


            IGorgonVirtualFile result;
            // For compressed images, we need to rely on an external tool to do the job.             
            if (formatInfo.IsCompressed)
            {
                _log.Print($"Pixel format [{pixelFormat}] is a block compression format, compressing using external tool...", LoggingLevel.Intermediate);
                if ((_compressor == null) || (!codec.SupportsBlockCompression))
                {
                    throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORIMG_ERR_COMPRESSED_FILE, formatInfo.Format));
                }

                // Send the image data as uncompressed to our working file, so we can have something to compress.
                using (Stream outStream = ScratchArea.OpenStream(workFile.FullPath, FileMode.Create))
                {
                    codec.SaveToStream(image, outStream);
                }

                _log.Print($"Saving to working file '{workFile.FullPath}'...", LoggingLevel.Simple);
                result = _compressor.Compress(workFile, pixelFormat, image.MipCount);

                // Convert to an uncompressed format if we aren't already in that format.
                switch (pixelFormat)
                {
                    case BufferFormat.BC5_SNorm when image.Format != BufferFormat.R8G8_SNorm:
                        image.ConvertToFormat(BufferFormat.R8G8_SNorm);
                        break;
                    case BufferFormat.BC5_Typeless when image.Format != BufferFormat.R8G8_UNorm:
                    case BufferFormat.BC5_UNorm when image.Format != BufferFormat.R8G8_UNorm:
                        image.ConvertToFormat(BufferFormat.R8G8_UNorm);
                        break;
                    case BufferFormat.BC6H_Sf16 when image.Format != BufferFormat.R16G16B16A16_Float:
                    case BufferFormat.BC6H_Typeless when image.Format != BufferFormat.R16G16B16A16_Float:
                    case BufferFormat.BC6H_Uf16 when image.Format != BufferFormat.R16G16B16A16_Float:
                        image.ConvertToFormat(BufferFormat.R16G16B16A16_Float);
                        break;
                    case BufferFormat.BC4_SNorm when image.Format != BufferFormat.R8G8_SNorm:
                        image.ConvertToFormat(BufferFormat.R8_SNorm);
                        break;
                    case BufferFormat.BC4_Typeless when image.Format != BufferFormat.R8G8_UNorm:
                    case BufferFormat.BC4_UNorm when image.Format != BufferFormat.R8G8_UNorm:
                        image.ConvertToFormat(BufferFormat.R8_UNorm);
                        break;
                    case BufferFormat.BC1_Typeless when image.Format != BufferFormat.R8G8B8A8_UNorm:
                    case BufferFormat.BC1_UNorm when image.Format != BufferFormat.R8G8B8A8_UNorm:
                    case BufferFormat.BC2_Typeless when image.Format != BufferFormat.R8G8B8A8_UNorm:
                    case BufferFormat.BC2_UNorm when image.Format != BufferFormat.R8G8B8A8_UNorm:
                    case BufferFormat.BC3_Typeless when image.Format != BufferFormat.R8G8B8A8_UNorm:
                    case BufferFormat.BC3_UNorm when image.Format != BufferFormat.R8G8B8A8_UNorm:
                    case BufferFormat.BC7_Typeless when image.Format != BufferFormat.R8G8B8A8_UNorm:
                    case BufferFormat.BC7_UNorm when image.Format != BufferFormat.R8G8B8A8_UNorm:
                        image.ConvertToFormat(BufferFormat.R8G8B8A8_UNorm);
                        break;
                    case BufferFormat.BC1_UNorm_SRgb when image.Format != BufferFormat.R8G8B8A8_UNorm_SRgb:
                    case BufferFormat.BC2_UNorm_SRgb when image.Format != BufferFormat.R8G8B8A8_UNorm_SRgb:
                    case BufferFormat.BC3_UNorm_SRgb when image.Format != BufferFormat.R8G8B8A8_UNorm_SRgb:
                    case BufferFormat.BC7_UNorm_SRgb when image.Format != BufferFormat.R8G8B8A8_UNorm_SRgb:
                        image.ConvertToFormat(BufferFormat.R8G8B8A8_UNorm_SRgb);
                        break;
                }

                if (result == null)
                {
                    throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORIMG_ERR_COMPRESSED_FILE, formatInfo.Format));
                }
            }
            else
            {
                // We've changed the pixel format, so convert prior to saving.
                if (pixelFormat != image.Format)
                {
                    _log.Print($"Image pixel format [{image.Format}] is different than requested format of [{pixelFormat}], converting...", LoggingLevel.Intermediate);
                    image.ConvertToFormat(pixelFormat);
                    _log.Print($"Converted image '{workFile.Name}' to pixel format: [{pixelFormat}].", LoggingLevel.Simple);
                }

                _log.Print($"Saving to working file '{workFile.FullPath}'...", LoggingLevel.Simple);
                using (Stream outStream = ScratchArea.OpenStream(workFile.FullPath, FileMode.Create))
                {
                    codec.SaveToStream(image, outStream);
                }

                ScratchArea.FileSystem.Refresh();
                result = ScratchArea.FileSystem.GetFile(workFile.FullPath);
            }

            return result;
        }


        /// <summary>Function to load the image file into memory.</summary>
        /// <param name="file">The stream for the file to load.</param>
        /// <param name="name">The name of the file.</param>
        /// <returns>The image data, the virtual file entry for the working file and the original pixel format of the file.</returns>
        public (IGorgonImage image, IGorgonVirtualFile workingFile, BufferFormat originalFormat) LoadImageFile(Stream file, string name)
        {
            IGorgonImage result = null;
            IGorgonVirtualFile workFile;
            BufferFormat originalFormat;

            IGorgonImageInfo imageInfo = DefaultCodec.GetMetaData(file);
            originalFormat = imageInfo.Format;
            var formatInfo = new GorgonFormatInfo(imageInfo.Format);

            // We absolutely need to have an extension, or else the texconv tool will not work.
            if ((DefaultCodec.CodecCommonExtensions.Count > 0)
                && (!string.Equals(Path.GetExtension(name), DefaultCodec.CodecCommonExtensions[0], System.StringComparison.OrdinalIgnoreCase)))
            {
                _log.Print("Adding DDS extension to working file or else external tools may not be able to read it.", LoggingLevel.Verbose);
                name = Path.ChangeExtension(name, DefaultCodec.CodecCommonExtensions[0]);
            }

            _log.Print($"Copying content file {name} to {ScratchArea.FileSystem.MountPoints.First().PhysicalPath} as working file...", LoggingLevel.Intermediate);

            // Copy to a working file.
            using (Stream outStream = ScratchArea.OpenStream(name, FileMode.Create))
            {
                file.CopyTo(outStream);
                workFile = ScratchArea.FileSystem.GetFile(name);
            }

            _log.Print($"{workFile.FullPath} is now the working file for the image editor.", LoggingLevel.Intermediate);

            if (formatInfo.IsCompressed)
            {
                _log.Print($"Image is compressed using [{formatInfo.Format}] as its pixel format.", LoggingLevel.Intermediate);

                if (_compressor == null)
                {
                    throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORIMG_ERR_COMPRESSED_FILE, formatInfo.Format));
                }

                _log.Print($"Loading image '{workFile.FullPath}'...", LoggingLevel.Simple);
                result = _compressor.Decompress(ref workFile, imageInfo);

                if (result == null)
                {
                    throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORIMG_ERR_COMPRESSED_FILE, formatInfo.Format));
                }

                _log.Print($"Loaded compressed ([{formatInfo.Format}]) image data as [{result.Format}]", LoggingLevel.Intermediate);
            }
            else
            {
                _log.Print($"Loading image '{workFile.FullPath}'...", LoggingLevel.Simple);
                using (Stream workingFileStream = workFile.OpenStream())
                {
                    result = DefaultCodec.LoadFromStream(workingFileStream);
                }
            }

            return (result, workFile, originalFormat);
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.ImageEditor.ImageIO"/> class.</summary>
        /// <param name="defaultCodec">The default codec used by the plug in.</param>
        /// <param name="installedCodecs">The list of installed codecs.</param>
        /// <param name="importDialog">The dialog service used to export an image.</param>
        /// <param name="busyService">The busy state service.</param>
        /// <param name="scratchArea">The file system writer used to write to the temporary area.</param>
        /// <param name="bcCompressor">The block compressor used to block (de)compress image data.</param>
        /// <param name="log">The logging interface to use.</param>
        public ImageIOService(IGorgonImageCodec defaultCodec,
            ICodecRegistry installedCodecs,
            IExportImageDialogService exportDialog,
            IImportImageDialogService importDialog,
            IBusyStateService busyService,
            IGorgonFileSystemWriter<Stream> scratchArea,
            TexConvCompressor bcCompressor,
            IGorgonLog log)
        {
            _exportDialog = exportDialog;
            _importDialog = importDialog;
            DefaultCodec = defaultCodec;
            InstalledCodecs = installedCodecs;
            ScratchArea = scratchArea;
            _compressor = bcCompressor;
            _busyState = busyService;
            _log = log ?? GorgonLog.NullLog;
        }
        #endregion
    }
}
