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
// Created: Wednesday, October 12, 2011 6:38:14 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
using GI = SlimDX.DXGI;
using D3D = SlimDX.Direct3D11;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Native;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A swap chain used to display graphics to a window.
	/// </summary>
	/// <remarks>The swap chain is used to display data to the <see cref="GorgonLibrary.Graphics.GorgonVideoOutput">video output</see>, or it can be used as a shader input.
	/// <para>Swap chains embedded into child controls (a panel, group box, etc...) will not be able to switch to full screen mode and will automatically revert to windowed mode.</para>
	/// <para>Multiple swap chains can be set to full screen on different video outputs.  When setting up for multiple video outputs in full screen, ensure that the window
	/// for the extra video output is located on the monitor attached to that video output.  Failure to do so will keep the mode from switching.
	/// </para>	
	/// <para>
	/// Note that due to a known limitation on Windows 7, it is not currently possible to switch to full screen on multiple outputs on <see cref="GorgonLibrary.Graphics.GorgonVideoDevice">multiple video devices</see>.  
	/// One possible workaround is to create a full screen borderless window on the secondary device and use that as a "fake" full screen mode.  If this workaround
	/// is applied, then it is suggested to disable the Desktop Windows Compositor.  To disable the compositor, see this link http://msdn.microsoft.com/en-us/library/aa969510.aspx.
	/// </para>	
	/// <para>
	/// It is recommended that user switching be avoided when in a multiple monitor scenario because, as of this writing, the application will not gracefully recover.
	/// </para>
	/// <para>If the window loses focus and the swap chain is in full screen, it will revert to windowed mode.  The swap chain will attempt to reacquire full screen mode when it regains focus.  
	/// This functionality can be disabled with the <see cref="P:GorgonLibrary.Graphics.ResetFullscreenOnFocus">GorgonGraphics.ResetFullscreenOnFocus</see> property if it does not suit the needs of the 
	/// developer.</para>
	/// </remarks>
	public class GorgonSwapChain
		: GorgonNamedObject, IDisposable
	{
		#region Variables.
		private Form _parentForm = null;							// Parent form for our window.
		private bool _disposed = false;								// Flag to indicate that the object was disposed.
		private bool _wasWindowed = false;							// Flag to indicate that the window was in windowed mode because of a transition.
		private IEnumerable<GorgonSwapChain> _swapChains = null;	// A list of other full screen swap chains.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the DXGI swap chain interface.
		/// </summary>
		internal GI.SwapChain GISwapChain
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the D3D render target interface.
		/// </summary>
		internal D3D.RenderTargetView D3DRenderTarget
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the D3D view data.
		/// </summary>
		internal D3D.Viewport D3DView
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the video output that the swap chain is operating on.
		/// </summary>
		public GorgonVideoOutput VideoOutput
		{
			get
			{
				if (GISwapChain == null)
					return null;

				GI.Output d3doutput = GISwapChain.ContainingOutput;

				var videoOutput = (from output in Graphics.VideoDevice.Outputs
								   where output.Handle == d3doutput.Description.MonitorHandle
								   select output).SingleOrDefault();

				return videoOutput;
			}
		}

		/// <summary>
		/// Property to return the settings for this swap chain.
		/// </summary>
		public GorgonSwapChainSettings Settings
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the graphics interface that owns this object.
		/// </summary>
		public GorgonGraphics Graphics
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the Deactivate event of the _parentForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void _parentForm_Deactivate(object sender, EventArgs e)
		{			
			if ((GISwapChain == null) || (!Graphics.ResetFullscreenOnFocus))
				return;

			if ((Graphics.InitialSwapChain != null) && (Graphics.InitialSwapChain != this))
			{
				Graphics.InitialSwapChain._parentForm_Deactivate(this, e);
				return;
			}
			else
			{
				// If there is no initial swap chain, then make this one the initial swap chain since it is the first with focus.
				if (Graphics.InitialSwapChain == null)
					Graphics.InitialSwapChain = this;
			}

			// Get the forms being used.
			var applicationStillActive = from swapChain in _swapChains
										 let form = swapChain.Settings.Window as Form
										 where form != null
										 select form;

			// If we've just switched to another full screen form in the application, then we don't care.
			foreach (var winForm in applicationStillActive)
			{
				if (Form.ActiveForm == winForm)
					return;
			}

			// Find out if any of our swap chains gained focus.
			if ((!_wasWindowed) && (GISwapChain.IsFullScreen))
				_wasWindowed = true;
		}

		/// <summary>
		/// Handles the Activated event of the _parentForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void _parentForm_Activated(object sender, EventArgs e)
		{
			if ((GISwapChain == null) || (!Graphics.ResetFullscreenOnFocus))
				return;

			if ((Graphics.InitialSwapChain != null) && (Graphics.InitialSwapChain != this))
			{
				Graphics.InitialSwapChain._parentForm_Activated(this, e);
				return;
			}
			else
			{
				// If there is no initial swap chain, then make this one the initial swap chain since it is the first with focus.
				if (Graphics.InitialSwapChain == null)
					Graphics.InitialSwapChain = this;
			}

			if ((_wasWindowed) && (!GISwapChain.IsFullScreen))
			{
				_wasWindowed = false;
				UpdateSettings(false);

				// Restore the other swap chains.
				foreach (var swapChain in _swapChains)
				{
					if (swapChain != this)
						swapChain.UpdateSettings(false);
				}
			}
		}

		/// <summary>
		/// Function to release any resources bound to the swap chain.
		/// </summary>
		private void ReleaseResources()
		{
			if (D3DRenderTarget != null)
			{
				Gorgon.Log.Print("GorgonSwapChain '{0}': Releasing D3D11 render target view...", Diagnostics.GorgonLoggingLevel.Intermediate, Name);
				D3DRenderTarget.Dispose();
			}
			D3DRenderTarget = null;
		}

		/// <summary>
		/// Function to create any resources bound to the swap chain.
		/// </summary>
		private void CreateResources()
		{			
			D3D.Texture2D texture = null;

			if (D3DRenderTarget != null)
				ReleaseResources();

			Gorgon.Log.Print("GorgonSwapChain '{0}': Creating D3D11 render target view...", Diagnostics.GorgonLoggingLevel.Intermediate, Name);

			try
			{
				texture = D3D.Texture2D.FromSwapChain<D3D.Texture2D>(GISwapChain, 0);
				texture.DebugName = "SwapChain '" + Name + "' Texture2D";
				D3DRenderTarget = new D3D.RenderTargetView(Graphics.VideoDevice.D3DDevice, texture);
				D3DRenderTarget.DebugName = "SwapChain '" + Name + "' D3DRenderTargetView";
				D3DView = new D3D.Viewport(0, 0, Settings.VideoMode.Width, Settings.VideoMode.Height);
			}
			finally
			{
				if (texture != null)
					texture.Dispose();
			}
		}

		/// <summary>
		/// Function to resize the back buffers.
		/// </summary>
		private void ResizeBuffers()
		{
			ReleaseResources();
			GI.SwapChainFlags flags = GI.SwapChainFlags.AllowModeSwitch;

			GISwapChain.ResizeBuffers(Settings.BufferCount, Settings.VideoMode.Width, Settings.VideoMode.Height, (GI.Format)Settings.VideoMode.Format, flags);
			CreateResources();
		}

		/// <summary>
		/// Handles the Resize event of the Window control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void Window_Resize(object sender, EventArgs e)
		{
			// Only do this if the size has changed, if we're just restoring the window, then don't bother.
			if (((_parentForm.WindowState != FormWindowState.Minimized) && (GISwapChain != null)) && (Settings.Window.ClientSize.Width > 0) && (Settings.Window.ClientSize.Height > 0))
			{
				// Resize the video mode.
				Settings.VideoMode = new GorgonVideoMode(Settings.Window.ClientSize, Settings.VideoMode);
				ResizeBuffers();
			}
		}
		
		/// <summary>
		/// Function to update the fullscreen/windowed mode state.
		/// </summary>
		private void ModeStateUpdate()
		{
			SlimDX.Result result = GI.ResultCode.Success;

			GI.ModeDescription mode = GorgonVideoMode.Convert(Settings.VideoMode);

			GISwapChain.ResizeTarget(mode);

			try
			{
				if (!Settings.IsWindowed)
					result = GISwapChain.SetFullScreenState(true, VideoOutput.GIOutput);
				else
					result = GISwapChain.SetFullScreenState(false, null);
			}
			catch (SlimDX.SlimDXException sdEx)
			{
				if (sdEx.ResultCode == GI.ResultCode.ModeChangeInProgress)
				{
					Gorgon.Log.Print("GorgonSwapChain '{0}': Could not switch to full screen mode because the device was busy switching to full screen on another output.", GorgonLoggingLevel.All, Name);
				}
				else
				{
					if (string.Compare(sdEx.ResultCode.Name, "DXGI_ERROR_NOT_CURRENTLY_AVAILABLE", true) == 0)
						Gorgon.Log.Print("GorgonSwapChain '{0}': Could not switch to full screen mode because the device is not currently available.  Possible causes are:  .", GorgonLoggingLevel.All, Name);
					else
						throw sdEx;
				}
			}

			ResizeBuffers();
		}

		/// <summary>
		/// Function to force clean up.
		/// </summary>
		private void CleanUp()
		{
			if (Graphics.InitialSwapChain == this)
				Graphics.InitialSwapChain = null;

			_parentForm.Activated -= new EventHandler(_parentForm_Activated);
			_parentForm.Deactivate -= new EventHandler(_parentForm_Deactivate);
			Settings.Window.Resize -= new EventHandler(Window_Resize);

			ReleaseResources();

			Gorgon.Log.Print("GorgonSwapChain '{0}': Removing D3D11 swap chain...", Diagnostics.GorgonLoggingLevel.Simple, Name);
			if (GISwapChain != null)
			{
				// Always go to windowed mode before destroying the swap chain.
				GISwapChain.SetFullScreenState(false, null);
				GISwapChain.Dispose();
			}
			if (Graphics != null)
				Graphics.RemoveTrackedObject(this);					

			GISwapChain = null;
		}

		/// <summary>
		/// Function to perform updating of the swap chain settings.
		/// </summary>
		/// <param name="graphics">Gorgon graphics interface to use.</param>
		/// <param name="settings">Settings to change.</param>
		internal static void ValidateSwapChainSettings(GorgonGraphics graphics, GorgonSwapChainSettings settings)
		{
			D3D.Device d3dDevice = null;
			IntPtr monitor = IntPtr.Zero;
			GorgonVideoOutput output = null;

			if (graphics.VideoDevice == null)
				throw new GorgonException(GorgonResult.CannotCreate, "Cannot create the swap chain, no video device was selected.");

			// Default to using the default Gorgon application window.
			if (settings.Window == null)
			{
				settings.Window = Gorgon.ApplicationForm;

				// No application window, then we're out of luck.
				if (settings.Window == null)
					throw new ArgumentException("No window to bind with the swap chain.", "settings");
			}

			// Force to windowed mode if we're binding to a child control on a form.
			if (!(settings.Window is Form))
				settings.IsWindowed = true;

			monitor = Win32API.GetMonitor(settings.Window);		// Get the monitor that the window is on.

			// Find the video output for the window.
			output = (from videoDevice in graphics.VideoDevices
					  from videoOutput in videoDevice.Outputs
					  where videoOutput.Handle == monitor
					  select videoOutput).SingleOrDefault();

			if (output == null)
				throw new GorgonException(GorgonResult.CannotCreate, "Could not find the video output for the specified window.");

			// Get the Direct 3D device instance.
			d3dDevice = graphics.VideoDevice.D3DDevice;

			// If we've not defined a video mode, determine the best mode to use.
			GorgonVideoMode stagedMode = settings.VideoMode;

			// Fill in any missing settings.
			if (stagedMode.Width == 0)
				stagedMode.Width = settings.Window.ClientSize.Width;
			if (stagedMode.Height == 0)
				stagedMode.Width = settings.Window.ClientSize.Height;
			if (stagedMode.Format == GorgonBufferFormat.Unknown)
				stagedMode.Format = output.DefaultVideoMode.Format;
			if ((stagedMode.RefreshRateDenominator == 0) || (stagedMode.RefreshRateNumerator == 0))
			{
				stagedMode.RefreshRateNumerator = output.DefaultVideoMode.RefreshRateNumerator;
				stagedMode.RefreshRateDenominator = output.DefaultVideoMode.RefreshRateDenominator;
			}					

			// If going full screen, ensure that whatever mode we've chosen can be used, otherwise go to the closest match.
			if (!settings.IsWindowed)
			{
				// Check to ensure that no other swap chain is on the video output if we're going to full screen mode.
				var swapChainCount = (from item in graphics.TrackedObjects
									  let swapChain = item as GorgonSwapChain
									  where ((swapChain != null) && (swapChain.VideoOutput == output) && (!swapChain.Settings.IsWindowed) && (swapChain.Settings.Window != settings.Window))
									  select item).Count();

				if (swapChainCount > 0)
					throw new GorgonException(GorgonResult.CannotCreate, "There is already a full screen swap chain active on the video output '" + output.Name + "'.");

				var modeCount = (from mode in output.VideoModes
								 where mode == stagedMode
								 select mode).Count();

				// We couldn't find the mode in the list, find the nearest match.
				if (modeCount == 0)
					stagedMode = output.FindMode(stagedMode);
			}
			else
			{
				// We don't need a refresh rate for windowed mode.
				stagedMode = new GorgonVideoMode(stagedMode.Width, stagedMode.Height, stagedMode.Format);
			}
					
			// Ensure that the selected video format can be used.
			if (!graphics.VideoDevice.SupportsDisplayFormat(stagedMode.Format))
				throw new ArgumentException("Cannot use the format '" + stagedMode.Format.ToString() + "' for display on the video device '" + graphics.VideoDevice.Name + "'.");

			settings.VideoMode = stagedMode;

			// Check multi sampling levels.
			if (settings.SwapEffect == SwapEffect.Sequential)
				settings.MultiSample = new GorgonMultiSampling(1, 0);

			int quality = graphics.VideoDevice.GetMultiSampleQuality(settings.VideoMode.Format, settings.MultiSample.Count);

			// Ensure that the quality of the sampling does not exceed what the card can do.
			if ((settings.MultiSample.Quality >= quality) || (settings.MultiSample.Quality < 0))
				throw new ArgumentException("Video device '" + graphics.VideoDevice.Name + "' does not support multisampling with a count of '" + settings.MultiSample.Count.ToString() + "' and a quality of '" + settings.MultiSample.Quality.ToString() + " with a format of '" + settings.VideoMode.Format + "'");

			// Force 2 buffers for discard.
			if ((settings.BufferCount < 2) && (settings.SwapEffect == SwapEffect.Discard))
				settings.BufferCount = 2;

			// Perform window handling.
			settings.Window.Visible = true;
			settings.Window.Enabled = true;
		}

		/// <summary>
		/// Function to intialize the swap chain.
		/// </summary>
		internal void Initialize()
		{
			GI.SwapChainFlags flags = GI.SwapChainFlags.AllowModeSwitch;
			GI.SwapChainDescription d3dSettings = new GI.SwapChainDescription();

			// Resize the window to match requested mode size.
			if ((_parentForm == Settings.Window) && (Settings.IsWindowed))
				_parentForm.ClientSize = new Size(Settings.VideoMode.Width, Settings.VideoMode.Height);

			_swapChains = Graphics.GetFullscreenSwapChains();

			if (Graphics.InitialSwapChain == null)
				Graphics.InitialSwapChain = this;

			_parentForm.Activated += new EventHandler(_parentForm_Activated);			
			_parentForm.Deactivate += new EventHandler(_parentForm_Deactivate);			

			d3dSettings.BufferCount = Settings.BufferCount;
			d3dSettings.Flags = flags;

			d3dSettings.IsWindowed = true;
			d3dSettings.ModeDescription = GorgonVideoMode.Convert(Settings.VideoMode);
			d3dSettings.OutputHandle = Settings.Window.Handle;
			d3dSettings.SampleDescription = GorgonMultiSampling.Convert(Settings.MultiSample);
			d3dSettings.SwapEffect = GorgonSwapChainSettings.Convert(Settings.SwapEffect);

			if ((Settings.Flags & SwapChainUsageFlags.RenderTarget) == SwapChainUsageFlags.RenderTarget)
				d3dSettings.Usage = GI.Usage.RenderTargetOutput;

			if ((Settings.Flags & SwapChainUsageFlags.ShaderInput) == SwapChainUsageFlags.ShaderInput)
				d3dSettings.Usage |= GI.Usage.ShaderInput;

			Gorgon.Log.Print("GorgonSwapChain '{0}': Creating D3D11 swap chain...", Diagnostics.GorgonLoggingLevel.Simple, Name);
			GISwapChain = new GI.SwapChain(Graphics.GIFactory, Graphics.VideoDevice.D3DDevice, d3dSettings);
			GISwapChain.DebugName = Name + " DXGISwapChain";

			// Due to a bug with winforms and DXGI, we have to manually handle transitions ourselves.
			Graphics.GIFactory.SetWindowAssociation(Settings.Window.Handle, GI.WindowAssociationFlags.IgnoreAll);

			if (!Settings.IsWindowed)
				flags |= GI.SwapChainFlags.NonPrerotated;

			if (!Settings.IsWindowed)
				ModeStateUpdate();
			CreateResources();

			Settings.Window.Resize += new EventHandler(Window_Resize);
		}

		/// <summary>
		/// Function to update the settings for the swap chain.
		/// </summary>
		/// <param name="mode">New video mode to use.</param>
		/// <exception cref="System.ArgumentException">Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.Window">GorgonSwapChainSettings.Window</see> property is NULL (Nothing in VB.Net), and the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application window</see> is NULL.
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonVideoMode.Format">GorgonSwapChainSettings.VideoMode.Format</see> property cannot be used by the video device for displaying data.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.MultSamples.Quality">GorgonSwapChainSettings.MultiSamples.Quality</see> property is higher than what the video device can support.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the video output could not be determined from the window.
		/// <para>-or-</para>
		/// <para>Thrown when the swap chain is going to full screen mode and another swap chain is already on the video output.</para>
		/// </exception>
		public void UpdateSettings(GorgonVideoMode mode)
		{
			UpdateSettings(mode, Settings.IsWindowed, Settings.BufferCount);
		}

		/// <summary>
		/// Function to update the settings for the swap chain.
		/// </summary>
		/// <param name="isWindowed">TRUE to use windowed mode, FALSE to use full screen mode.</param>
		/// <exception cref="System.ArgumentException">Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.Window">GorgonSwapChainSettings.Window</see> property is NULL (Nothing in VB.Net), and the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application window</see> is NULL.
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonVideoMode.Format">GorgonSwapChainSettings.VideoMode.Format</see> property cannot be used by the video device for displaying data.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.MultSamples.Quality">GorgonSwapChainSettings.MultiSamples.Quality</see> property is higher than what the video device can support.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the video output could not be determined from the window.
		/// <para>-or-</para>
		/// <para>Thrown when the swap chain is going to full screen mode and another swap chain is already on the video output.</para>
		/// </exception>
		public void UpdateSettings(bool isWindowed)
		{
			UpdateSettings(Settings.VideoMode, isWindowed, Settings.BufferCount);
		}

		/// <summary>
		/// Function to update the settings for the swap chain.
		/// </summary>
		/// <param name="mode">New video mode to use.</param>
		/// <param name="isWindowed">TRUE to use windowed mode, FALSE to use full screen mode.</param>
		/// <exception cref="System.ArgumentException">Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.Window">GorgonSwapChainSettings.Window</see> property is NULL (Nothing in VB.Net), and the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application window</see> is NULL.
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonVideoMode.Format">GorgonSwapChainSettings.VideoMode.Format</see> property cannot be used by the video device for displaying data.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.MultSamples.Quality">GorgonSwapChainSettings.MultiSamples.Quality</see> property is higher than what the video device can support.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the video output could not be determined from the window.
		/// <para>-or-</para>
		/// <para>Thrown when the swap chain is going to full screen mode and another swap chain is already on the video output.</para>
		/// </exception>
		public void UpdateSettings(GorgonVideoMode mode, bool isWindowed)
		{
			UpdateSettings(Settings.VideoMode, isWindowed, Settings.BufferCount);
		}

		/// <summary>
		/// Function to update the settings for the swap chain.
		/// </summary>
		/// <param name="mode">New video mode to use.</param>
		/// <param name="isWindowed">TRUE to switch to windowed mode, FALSE to switch to full screen.</param>
		/// <param name="bufferCount">Number of back buffers.</param>
		/// <remarks>If the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.SwapEffect">SwapEffect</see> for the swap chain is set to discard, then the <paramref name="bufferCount"/> must be greater than 1.</remarks>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.Window">GorgonSwapChainSettings.Window</see> property is NULL (Nothing in VB.Net), and the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application window</see> is NULL.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonVideoMode.Format">GorgonSwapChainSettings.VideoMode.Format</see> property cannot be used by the video device for displaying data.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.MultSamples.Quality">GorgonSwapChainSettings.MultiSamples.Quality</see> property is higher than what the video device can support.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the video output could not be determined from the window.
		/// <para>-or-</para>
		/// <para>Thrown when the swap chain is going to full screen mode and another swap chain is already on the video output.</para>
		/// </exception>
		public void UpdateSettings(GorgonVideoMode mode, bool isWindowed, int bufferCount)
		{
			if (GISwapChain == null)
				return;

			// Assign the new settings.
			_wasWindowed = true;
			Settings.IsWindowed = isWindowed;			
			Settings.VideoMode = mode;
			Settings.BufferCount = bufferCount;

			// Validate and modify the settings as appropriate.
			ValidateSwapChainSettings(Graphics, Settings);

			ModeStateUpdate();
			
			// Ensure our window is the proper size.
			if ((_parentForm == Settings.Window) && (Settings.IsWindowed) && ((Settings.VideoMode.Width != Settings.Window.ClientSize.Width) || (Settings.VideoMode.Height != Settings.Window.ClientSize.Height)))
				Settings.Window.ClientSize = new Size(Settings.VideoMode.Width, Settings.VideoMode.Height);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonSwapChain"/> class.
		/// </summary>
		/// <param name="graphics">Graphics interface that owns this swap chain.</param>
		/// <param name="name">The name of the swap chain.</param>
		/// <param name="settings">Settings for the swap chain.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		internal GorgonSwapChain(GorgonGraphics graphics, string name, GorgonSwapChainSettings settings)
			: base(name)
		{
			if (graphics == null)
				throw new ArgumentNullException("graphics");

			Settings = settings;
			Graphics = graphics;			

			// Get the parent form for our window.
			_parentForm = Gorgon.GetTopLevelForm(settings.Window);
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
					CleanUp();

				Graphics = null;
				_disposed = true;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
