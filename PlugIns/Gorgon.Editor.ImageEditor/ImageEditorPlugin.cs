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
// Created: October 29, 2018 2:52:09 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gorgon.Diagnostics;
using Gorgon.Editor.Content;
using Gorgon.Editor.ImageEditor.Properties;
using Gorgon.Editor.ImageEditor.ViewModels;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;
using Gorgon.Math;
using DX = SharpDX;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// Gorgon image editor content plug in interface.
    /// </summary>
    internal class ImageEditorPlugIn
        : ContentPlugIn, IContentPlugInMetadata
    {
        #region Variables.
        // This is the only codec supported by the image plug in.  Images will be converted when imported.
        private readonly GorgonCodecDds _ddsCodec = new GorgonCodecDds();

        // The synchronization lock for threads.
        private readonly object _syncLock = new object();

        // The codec registry.
        private ICodecRegistry _codecs;

        // The plug in settings.
        private ISettings _settings;

        /// <summary>
        /// The name of the settings file.
        /// </summary>
        public static readonly string SettingsName = typeof(ImageEditorPlugIn).FullName;
        #endregion

        #region Properties.
        /// <summary>Property to return the name of the plug in.</summary>
        string IContentPlugInMetadata.PlugInName => Name;

        /// <summary>Property to return the description of the plugin.</summary>
        string IContentPlugInMetadata.Description => Description;

        /// <summary>Property to return whether or not the plugin is capable of creating content.</summary>
        public override bool CanCreateContent => false;

        /// <summary>Property to return the ID of the small icon for this plug in.</summary>
        public Guid SmallIconID
        {
            get;
        }
        /// <summary>Property to return the ID of the new icon for this plug in.</summary>
        public Guid NewIconID => Guid.Empty;

        /// <summary>Property to return the ID for the type of content produced by this plug in.</summary>
        public override string ContentTypeID => CommonEditorContentTypes.ImageType;

        /// <summary>Property to return the friendly (i.e shown on the UI) name for the type of content.</summary>
        public string ContentType => string.Empty;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to render the thumbnail into the image passed in.
        /// </summary>
        /// <param name="image">The image to render the thumbnail into.</param>
        /// <param name="scale">The scale of the image.</param>
        private void RenderThumbnail(ref IGorgonImage image, float scale)
        {
            lock (_syncLock)
            {
                using (GorgonTexture2D texture = image.ToTexture2D(GraphicsContext.Graphics, new GorgonTexture2DLoadOptions
                {
                    Usage = ResourceUsage.Immutable,
                    IsTextureCube = false
                }))
                using (var rtv = GorgonRenderTarget2DView.CreateRenderTarget(GraphicsContext.Graphics, new GorgonTexture2DInfo
                {
                    ArrayCount = 1,
                    Binding = TextureBinding.ShaderResource,
                    Format = BufferFormat.R8G8B8A8_UNorm,
                    MipLevels = 1,
                    Height = (int)(image.Height * scale),
                    Width = (int)(image.Width * scale),
                    Usage = ResourceUsage.Default
                }))
                {
                    GorgonTexture2DView view = texture.GetShaderResourceView(mipCount: 1, arrayCount: 1);
                    rtv.Clear(GorgonColor.BlackTransparent);
                    GraphicsContext.Graphics.SetRenderTarget(rtv);
                    GraphicsContext.Graphics.DrawTexture(view, new DX.Rectangle(0, 0, rtv.Width, rtv.Height), blendState: GorgonBlendState.Default, samplerState: GorgonSamplerState.Default);
                    GraphicsContext.Graphics.SetRenderTarget(null);

                    image?.Dispose();
                    image = rtv.Texture.ToImage();
                }
            }
        }

        /// <summary>
        /// Function to retrieve the path to the texture converted used to convert compressed images.
        /// </summary>
        /// <returns>The file info for the texture converter file.</returns>
        private FileInfo GetTexConvExe()
        {
            FileInfo result;

            // The availability of texconv.exe determines whether or not we can use block compressed formats or not.
            CommonServices.Log.Print("Checking for texconv.exe...", LoggingLevel.Simple);
            var pluginDir = new DirectoryInfo(Path.GetDirectoryName(GetType().Assembly.Location));
            result = new FileInfo(Path.Combine(pluginDir.FullName, "texconv.exe"));

            if (!result.Exists)
            {
                CommonServices.Log.Print($"WARNING: Texconv.exe was not found at {pluginDir.FullName}. Block compressed formats will be unavailable.", LoggingLevel.Simple);
            }
            else
            {
                CommonServices.Log.Print($"Found texconv.exe at '{result.FullName}'.", LoggingLevel.Simple);
            }

            return result;
        }

        /// <summary>
        /// Function to load the image to be used a thumbnail.
        /// </summary>
        /// <param name="thumbnailCodec">The codec for the thumbnail images.</param>
        /// <param name="thumbnailFile">The path to the thumbnail file.</param>
        /// <param name="content">The content being thumbnailed.</param>
        /// <param name="cancelToken">The token used to cancel the operation.</param>
        /// <returns>The thumbnail image, and a flag to indicate whether the thumbnail needs conversion.</returns>
        private (IGorgonImage thumbnailImage, bool needsConversion) LoadThumbNailImage(IGorgonImageCodec thumbnailCodec, FileInfo thumbnailFile, IContentFile content, CancellationToken cancelToken)
        {
            IGorgonImage result;
            Stream inStream = null;

            try
            {
                // If we've already got the file, then leave.
                if (thumbnailFile.Exists)
                {
                    inStream = thumbnailFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
                    result = thumbnailCodec.LoadFromStream(inStream);

                    return cancelToken.IsCancellationRequested ? (null, false) : (result, false);
                }

                inStream = content.OpenRead();
                result = _ddsCodec.LoadFromStream(inStream);

                return (result, true);
            }
            catch (Exception ex)
            {
                CommonServices.Log.Print($"[ERROR] Cannot create thumbnail for '{content.Path}'", LoggingLevel.Intermediate);
                CommonServices.Log.LogException(ex);
                return (null, false);
            }
            finally
            {
                inStream?.Dispose();
            }
        }

        /// <summary>
        /// Function to update the metadata for a file that is missing metadata attributes.
        /// </summary>
        /// <param name="attributes">The attributes to update.</param>
        /// <returns><b>true</b> if the metadata needs refreshing for the file, <b>false</b> if not.</returns>
        private bool UpdateFileMetadataAttributes(Dictionary<string, string> attributes)
        {
            bool needsRefresh = false;

            if ((attributes.TryGetValue(ImageContent.CodecAttr, out string currentCodecType))
                && (!string.IsNullOrWhiteSpace(currentCodecType)))
            {
                attributes.Remove(ImageContent.CodecAttr);
                needsRefresh = true;
            }

            if ((attributes.TryGetValue(CommonEditorConstants.ContentTypeAttr, out string currentContentType))
                && (string.Equals(currentContentType, CommonEditorContentTypes.ImageType, StringComparison.OrdinalIgnoreCase)))
            {
                attributes.Remove(CommonEditorConstants.ContentTypeAttr);
                needsRefresh = true;
            }

            string codecType = _ddsCodec.GetType().FullName;
            if ((!attributes.TryGetValue(ImageContent.CodecAttr, out currentCodecType))
                || (!string.Equals(currentCodecType, codecType, StringComparison.OrdinalIgnoreCase)))
            {
                attributes[ImageContent.CodecAttr] = codecType;
                needsRefresh = true;
            }

            if ((!attributes.TryGetValue(CommonEditorConstants.ContentTypeAttr, out currentContentType))
                || (!string.Equals(currentContentType, CommonEditorContentTypes.ImageType, StringComparison.OrdinalIgnoreCase)))
            {
                attributes[CommonEditorConstants.ContentTypeAttr] = CommonEditorContentTypes.ImageType;
                needsRefresh = true;
            }

            return needsRefresh;
        }

        /// <summary>Function to register plug in specific search keywords with the system search.</summary>
        /// <typeparam name="T">The type of object being searched, must implement <see cref="T:Gorgon.Core.IGorgonNamedObject"/>.</typeparam>
        /// <param name="searchService">The search service to use for registration.</param>
        protected override void OnRegisterSearchKeywords<T>(ISearchService<T> searchService) => searchService.MapKeywordToContentAttribute(Resources.GORIMG_SEARCH_KEYWORD_CODEC, ImageContent.CodecAttr);

        /// <summary>Function to open a content object from this plugin.</summary>
        /// <param name="file">The file that contains the content.</param>
        /// <param name = "fileManager" > The file manager used to access other content files.</param>
        /// <param name="injector">Parameters for injecting dependency objects.</param>
        /// <param name="scratchArea">The file system for the scratch area used to write transitory information.</param>
        /// <param name="undoService">The undo service for the plug in.</param>
        /// <returns>A new IEditorContent object.</returns>
        /// <remarks>
        /// The <paramref name="scratchArea" /> parameter is the file system where temporary files to store transitory information for the plug in is stored. This file system is destroyed when the
        /// application or plug in is shut down, and is not stored with the project.
        /// </remarks>
        protected async override Task<IEditorContent> OnOpenContentAsync(IContentFile file, IContentFileManager fileManager, IGorgonFileSystemWriter<Stream> scratchArea, IUndoService undoService)
        {
            FileInfo texConvExe = GetTexConvExe();
            TexConvCompressor compressor = null;

            if (texConvExe.Exists)
            {
                compressor = new TexConvCompressor(texConvExe, scratchArea, _ddsCodec);
            }

            var imageIO = new ImageIOService(_ddsCodec,
                _codecs,
                new ExportImageDialogService(_settings),
                new ImportImageDialogService(_settings, _codecs),
                CommonServices.BusyService,
                scratchArea,
                compressor,
                CommonServices.Log);

            (IGorgonImage image, IGorgonVirtualFile workingFile, BufferFormat originalFormat) imageData = await Task.Run(() =>
            {
                using (Stream inStream = file.OpenRead())
                {
                    return imageIO.LoadImageFile(inStream, file.Name);
                }
            });

            var services = new ImageEditorServices
            {
                CommonServices = CommonServices,
                ImageIO = imageIO,
                UndoService = undoService,
                ImageUpdater = new ImageUpdaterService(),
                ExternalEditorService = new ImageExternalEditService(CommonServices.Log)
            };

            var cropResizeSettings = new CropResizeSettings();
            var dimensionSettings = new DimensionSettings();
            var mipSettings = new MipMapSettings();
            var alphaSettings = new AlphaSettings
            {
                AlphaValue = _settings.LastAlphaValue,
                UpdateRange = _settings.LastAlphaRange
            };

            cropResizeSettings.Initialize(CommonServices);
            dimensionSettings.Initialize(new DimensionSettingsParameters(GraphicsContext.Graphics.VideoAdapter, CommonServices));
            mipSettings.Initialize(CommonServices);


            var content = new ImageContent();
            content.Initialize(new ImageContentParameters(file,
                _settings,
                cropResizeSettings,
                dimensionSettings,
                mipSettings,
                alphaSettings,
                imageData,
                GraphicsContext.Graphics.VideoAdapter,
                GraphicsContext.Graphics.FormatSupport,
                services));

            return content;
        }

        /// <summary>Function to provide clean up for the plugin.</summary>
        protected override void OnShutdown()
        {
            if ((_settings.WriteSettingsCommand != null) && (_settings.WriteSettingsCommand.CanExecute(null)))
            {
                _settings.WriteSettingsCommand.Execute(null);
            }

            ViewFactory.Unregister<IImageContent>();

            base.OnShutdown();
        }

        /// <summary>Function to provide initialization for the plugin.</summary>
        /// <remarks>This method is only called when the plugin is loaded at startup.</remarks>
        protected override void OnInitialize()
        {
            ViewFactory.Register<IImageContent>(() => new ImageEditorView());
            (_codecs, _settings) = SharedDataFactory.GetSharedData(ContentPlugInService, CommonServices);
        }

        /// <summary>Function to determine if the content plugin can open the specified file.</summary>
        /// <param name="file">The content file to evaluate.</param>
        /// <param name="fileManager">The content file manager.</param>
        /// <returns>
        ///   <b>true</b> if the plugin can open the file, or <b>false</b> if not.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="file" />, or the <paramref name="fileManager"/> parameter is <b>null</b>.</exception>
        public bool CanOpenContent(IContentFile file, IContentFileManager fileManager)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (fileManager == null)
            {
                throw new ArgumentNullException(nameof(fileManager));
            }

            using (Stream stream = file.OpenRead())
            {
                if (!_ddsCodec.IsReadable(stream))
                {
                    return false;
                }

                IGorgonImageInfo metadata = _ddsCodec.GetMetaData(stream);

                // We won't be supporting 1D images in this editor.
                if ((metadata.ImageType == ImageType.Image1D) || (metadata.ImageType == ImageType.Unknown))
                {
                    return false;
                }

                UpdateFileMetadataAttributes(file.Metadata.Attributes);
                return true;
            }
        }

        /// <summary>
        /// Function to retrieve the small icon for the content plug in.
        /// </summary>
        /// <returns>An image for the small icon.</returns>
        public Image GetSmallIcon() => Resources.image_16x16;

        /// <summary>Function to retrieve a thumbnail for the content.</summary>
        /// <param name="contentFile">The content file used to retrieve the data to build the thumbnail with.</param>
        /// <param name="fileManager">The content file manager.</param>
        /// <param name="outputFile">The output file for the thumbnail data.</param>
        /// <param name="cancelToken">The token used to cancel the thumbnail generation.</param>
        /// <returns>A <see cref="T:Gorgon.Graphics.Imaging.IGorgonImage"/> containing the thumbnail image data.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="contentFile" />, <paramref name="fileManager" />, or the <paramref name="outputFile" /> parameter is <b>null</b>.</exception>
        public async Task<IGorgonImage> GetThumbnailAsync(IContentFile contentFile, IContentFileManager fileManager, FileInfo outputFile, CancellationToken cancelToken)
        {
            if (contentFile == null)
            {
                throw new ArgumentNullException(nameof(contentFile));
            }

            if (fileManager == null)
            {
                throw new ArgumentNullException(nameof(fileManager));
            }

            if (outputFile == null)
            {
                throw new ArgumentNullException(nameof(outputFile));
            }

            // If the content is not a DDS image, then leave it.
            if ((!contentFile.Metadata.Attributes.TryGetValue(ImageContent.CodecAttr, out string codecName))
                || (string.IsNullOrWhiteSpace(codecName))
                || (!string.Equals(codecName, _ddsCodec.GetType().FullName, StringComparison.OrdinalIgnoreCase)))
            {
                return null;
            }

            if (!outputFile.Directory.Exists)
            {
                outputFile.Directory.Create();
                outputFile.Directory.Refresh();
            }

            IGorgonImageCodec pngCodec = new GorgonCodecPng();

            (IGorgonImage thumbImage, bool needsConversion) = await Task.Run(() => LoadThumbNailImage(pngCodec, outputFile, contentFile, cancelToken));

            if ((thumbImage == null) || (cancelToken.IsCancellationRequested))
            {
                return null;
            }

            if (!needsConversion)
            {
                return thumbImage;
            }

            // We need to switch back to the main thread here to render the image, otherwise things will break.
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                const float maxSize = 256;
                float scale = (maxSize / thumbImage.Width).Min(maxSize / thumbImage.Height);
                RenderThumbnail(ref thumbImage, scale);

                if (cancelToken.IsCancellationRequested)
                {
                    return null;
                }

                // We're done on the main thread, we can switch to another thread to write the image.
                Cursor.Current = Cursors.Default;

                await Task.Run(() => pngCodec.SaveToFile(thumbImage, outputFile.FullName), cancelToken);

                if (cancelToken.IsCancellationRequested)
                {
                    return null;
                }

                contentFile.Metadata.Attributes[CommonEditorConstants.ThumbnailAttr] = outputFile.Name;
                return thumbImage;
            }
            catch (Exception ex)
            {
                CommonServices.Log.Print($"[ERROR] Cannot create thumbnail for '{contentFile.Path}'", LoggingLevel.Intermediate);
                CommonServices.Log.LogException(ex);
                return null;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        /// <summary>Function to retrieve the icon used for new content creation.</summary>
        /// <returns>An image for the icon.</returns>
        public Image GetNewIcon() => null;
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the ImageEditorPlugIn class.</summary>
        public ImageEditorPlugIn()
            : base(Resources.GORIMG_DESC) => SmallIconID = Guid.NewGuid();
        #endregion
    }
}
