﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GorgonLibrary;
using GorgonLibrary.UI;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.PlugIns;
using GorgonLibrary.Collections;
using GorgonLibrary.Graphics;

namespace Tester_Graphics
{
	public partial class Form1 : Form
	{
		GorgonDeviceWindowSettings settings = null;
		GorgonMultiHeadSettings multiHead;
		GorgonGraphics _gfx = null;
		GorgonDeviceWindow _dev = null;
		GorgonDeviceWindow _dev2 = null;
		private bool _running = true;
		GorgonTimer _timer = new GorgonTimer(true);
		Form2 form2 = null;

		private bool Idle(GorgonFrameRate timing)
		{			
			Text = "FPS: " + timing.FPS.ToString() + " DT:" + timing.FrameDelta.ToString();

/*			while (_timer.Milliseconds < GorgonTimer.FpsToMilliseconds(30.0f))
			{
			}*/

			_timer.Reset();

			if ((_dev != null) && (_running))
				_dev.RunTest(timing.FrameDelta);

			if ((_dev2 != null) && (_running))
				_dev2.RunTest(timing.FrameDelta);

			return true;
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			try
			{
				if (e.KeyCode == Keys.F1)
				{
					settings.IsWindowed = !_dev.Settings.IsWindowed;
					_dev.UpdateSettings();
				}

				if (e.KeyCode == Keys.F)
				{
					settings.Width = 1024;
					settings.Height = 768;
					_dev.UpdateSettings();
				}

				if (e.KeyCode == Keys.Space)
					_running = !_running;

				if (e.KeyCode == Keys.Back)
				{
					_dev.Dispose();
					_dev = null;
				}
			}
			catch (Exception ex)
			{
				GorgonException.Catch(ex, () => GorgonDialogs.ErrorBox(this, ex));
				Close();
			}
		}

		protected override void OnLoad(EventArgs e)
		{			
			base.OnLoad(e);

			int? quality = null;

			try 
			{
				this.panelDX.Visible = false;
				Gorgon.Initialize(this);

				Gorgon.PlugIns.SearchPaths.Add(@"..\..\..\..\PlugIns\bin\debug");
				Gorgon.PlugIns.LoadPlugInAssembly("Gorgon.Graphics.D3D9.dll");

				GorgonFrameRate.UseHighResolutionTimer = false;

				Gorgon.UnfocusedSleepTime = 10;
				Gorgon.AllowBackground = true;				

				ClientSize = new System.Drawing.Size(640, 480);
				_gfx = GorgonGraphics.CreateGraphics("GorgonLibrary.Graphics.GorgonD3D9");

				form2 = new Form2();				
				form2.Show();
				form2.ClientSize = new System.Drawing.Size(640, 480);
				form2.Location = new Point(Screen.AllScreens[1].Bounds.Width / 2 + Screen.AllScreens[1].Bounds.Left, Screen.AllScreens[1].Bounds.Height / 2 + Screen.AllScreens[1].Bounds.Top);
				form2.FormClosing += new FormClosingEventHandler(form2_FormClosing);

				GorgonMSAALevel[] antiAliasLevels = new[] { GorgonMSAALevel.NonMasked };//(GorgonMSAALevel[])(Enum.GetValues(typeof(GorgonMSAALevel)));
				for (int i = antiAliasLevels.Length - 1; i >= 0; i--)
				{
					quality = _gfx.VideoDevices[0].GetMultiSampleQuality(antiAliasLevels[i], GorgonBufferFormat.X8_R8G8B8_UIntNormal, true);
					if (quality != null)
						break;
				}

				
				multiHead = new GorgonMultiHeadSettings(_gfx.VideoDevices[0], new[] {
						new GorgonDeviceWindowSettings(this)
						{
							IsWindowed = false,
							DepthStencilFormat = GorgonBufferFormat.D16_UIntNormal,
							MSAAQualityLevel = (quality != null ? new GorgonMSAAQualityLevel(GorgonMSAALevel.NonMasked, quality.Value) : new GorgonMSAAQualityLevel(GorgonMSAALevel.None, 0))
						},
						new GorgonDeviceWindowSettings(form2)
						{
							IsWindowed = true,
							DepthStencilFormat = GorgonBufferFormat.D16_UIntNormal,
							MSAAQualityLevel = (quality != null ? new GorgonMSAAQualityLevel(GorgonMSAALevel.NonMasked, quality.Value) : new GorgonMSAAQualityLevel(GorgonMSAALevel.None, 0))
						}
					});

				IEnumerable<GorgonDeviceWindow> windows = _gfx.CreateMultiHeadDeviceWindows("MultiHead", multiHead);
				_dev = windows.ElementAt(0);
				_dev.SetupTest();
				_dev2 = windows.ElementAt(1);
				_dev2.SetupTest();

/*				settings = new GorgonDeviceWindowSettings()
				{
					//DisplayMode = new GorgonVideoMode(640, 480, GorgonBufferFormat.X8_R8G8B8_UIntNormal),
					IsWindowed = true,
					DepthStencilFormat = GorgonBufferFormat.D16_UIntNormal					
				};

				

				_dev = _gfx.CreateDeviceWindow("Test", settings);
				_dev.SetupTest();

				_dev2 = _gfx.CreateDeviceWindow("Test2", new GorgonDeviceWindowSettings(form2)
					{						
						IsWindowed = true,
						DepthStencilFormat = GorgonBufferFormat.D16_UIntNormal
					});
				_dev2.SetupTest();*/
				
				Gorgon.Go(Idle);
			}
			catch (Exception ex)
			{
				GorgonException.Catch(ex, () => GorgonDialogs.ErrorBox(this, ex));
				Application.Exit();				
			}

		}

		void form2_FormClosing(object sender, FormClosingEventArgs e)
		{
			_dev2.Dispose();
			_dev2 = null;
			form2 = null;
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			try
			{
				/*if (_dev != null)
					_dev.Dispose();

				if (_gfx != null)
					_gfx.Dispose();*/

				Gorgon.Terminate();
			}
			catch (Exception ex)
			{
				GorgonException.Catch(ex, () => GorgonDialogs.ErrorBox(this, ex));
			}
		}

		public Form1()
		{
			InitializeComponent();
		}
	}
}
