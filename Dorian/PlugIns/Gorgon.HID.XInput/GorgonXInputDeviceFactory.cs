#region MIT.
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
// Created: Friday, July 15, 2011 6:22:54 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using GorgonLibrary.Collections;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.HID;
using XI = SlimDX.XInput;
using Forms = System.Windows.Forms;

namespace GorgonLibrary.HID.XInput
{
	/// <summary>
	/// Object representing the main interface to the input library.
	/// </summary>
	internal class GorgonXInputDeviceFactory
		: GorgonInputDeviceFactory
	{
		#region Methods.
		/// <summary>
		/// Function to enumerate the pointing devices on the system.
		/// </summary>
		/// <returns>A list of pointing device names.</returns>
		protected override GorgonNamedObjectReadOnlyCollection<GorgonInputDeviceName> EnumeratePointingDevices()
		{
			return new GorgonNamedObjectReadOnlyCollection<GorgonInputDeviceName>(false, new GorgonXInputDeviceName[] { });
		}

		/// <summary>
		/// Function to enumerate the keyboard devices on the system.
		/// </summary>
		/// <returns>A list of keyboard device names.</returns>
		protected override GorgonNamedObjectReadOnlyCollection<GorgonInputDeviceName> EnumerateKeyboardDevices()
		{
			return new GorgonNamedObjectReadOnlyCollection<GorgonInputDeviceName>(false, new GorgonXInputDeviceName[] { });
		}

		/// <summary>
		/// Function to enumerate the joystick devices attached to the system.
		/// </summary>
		/// <returns>A list of joystick device names.</returns>
		protected override GorgonNamedObjectReadOnlyCollection<GorgonInputDeviceName> EnumerateJoysticksDevices()
		{
			XI.UserIndex[] controllerIndex = ((XI.UserIndex[])Enum.GetValues(typeof(XI.UserIndex))).OrderBy((item) => item).ToArray();
			XI.Controller[] controllers = new XI.Controller[controllerIndex.Length];
			IList<GorgonXInputDeviceName> devices = null;

			devices = new List<GorgonXInputDeviceName>();

			// Enumerate all controllers.
			for (int i = 0; i < controllerIndex.Length; i++)
			{
				XI.Capabilities caps = default(XI.Capabilities);

				controllers[i] = new XI.Controller(controllerIndex[i]);
				if (controllers[i].IsConnected)
				{
					if (controllerIndex[i] != XI.UserIndex.Any)
					{
						caps = controllers[i].GetCapabilities(XI.DeviceQueryType.Any);
						devices.Add(new GorgonXInputDeviceName(string.Format("{0}", i + 1) + ": XInput " + caps.Subtype.ToString() + " Controller", caps.Subtype.ToString(), "XInput_" + controllerIndex[i].ToString(), controllers[i], i));
					}
				}
				else
					devices.Add(new GorgonXInputDeviceName(string.Format("{0}", i + 1) + ": XInput Disconnected Controller ", "Disconnected Controller", "XInput_" + controllerIndex[i].ToString(), controllers[i], i));
			}			
				
			return new GorgonNamedObjectReadOnlyCollection<GorgonInputDeviceName>(false, devices.OrderBy((item) => item.Name));
		}

		/// <summary>
		/// Function to enumerate device types for which there is no class wrapper and will return data in a custom property collection.
		/// </summary>
		/// <returns>
		/// A list of custom HID types.
		/// </returns>
		protected override GorgonNamedObjectReadOnlyCollection<GorgonInputDeviceName> EnumerateCustomHIDs()
		{
			return new GorgonNamedObjectReadOnlyCollection<GorgonInputDeviceName>(false, new GorgonXInputDeviceName[] { });
		}

		/// <summary>
		/// Function to create a custom HID interface.
		/// </summary>
		/// <param name="hidName">A <see cref="GorgonLibrary.HID.GorgonInputDeviceName">GorgonDeviceName</see> object containing the HID information.</param>
		/// <param name="window">Window to bind with.</param>
		/// <returns>
		/// A new custom HID interface.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">The <paramRef name="hidName"/> is NULL.</exception>
		protected override GorgonCustomHID CreateCustomHIDImpl(GorgonInputDeviceName hidName, Forms.Control window)
		{
			throw new NotImplementedException("This plug-in only contains XBOX 360 controller devices.");
		}

		/// <summary>
		/// Function to create a keyboard interface.
		/// </summary>
		/// <param name="keyboardName">Name of the keyboard device to create.</param>
		/// <param name="window">Window to bind with.</param>
		/// <returns>A new keyboard interface.</returns>
		/// <remarks>Passing NULL for <paramref name="keyboardName"/> will use the system keyboard.
		/// <para>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationWindow">Gorgon application window</see>.</para></remarks>
		protected override GorgonKeyboard CreateKeyboardImpl(GorgonInputDeviceName keyboardName, Forms.Control window)
		{
			throw new NotImplementedException("This plug-in only contains XBOX 360 controller devices.");
		}

		/// <summary>
		/// Function to create a pointing device interface.
		/// </summary>
		/// <param name="pointingDeviceName">Name of the pointing device device to create.</param>
		/// <param name="window">Window to bind with.</param>
		/// <returns>A new pointing device interface.</returns>
		/// <remarks>Passing NULL for <paramref name="pointingDeviceName"/> will use the system pointing device.
		/// <para>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationWindow">Gorgon application window</see>.</para>
		/// </remarks>
		protected override GorgonPointingDevice CreatePointingDeviceImpl(GorgonInputDeviceName pointingDeviceName, Forms.Control window)
		{
			throw new NotImplementedException("This plug-in only contains XBOX 360 controller devices.");
		}

		/// <summary>
		/// Function to create a joystick interface.
		/// </summary>
		/// <param name="joystickName">A <see cref="GorgonLibrary.HID.GorgonInputDeviceName">GorgonDeviceName</see> object containing the joystick information.</param>
		/// <param name="window">Window to bind with.</param>
		/// <returns>A new joystick interface.</returns>
		/// <remarks>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationWindow">Gorgon application window</see>.</remarks>
		/// <exception cref="System.ArgumentNullException">The <paramRef name="joystickName"/> is NULL.</exception>
		protected override GorgonJoystick CreateJoystickImpl(GorgonInputDeviceName joystickName, Forms.Control window)
		{
			GorgonJoystick joystick = null;
			GorgonXInputDeviceName deviceName = joystickName as GorgonXInputDeviceName;

			if (deviceName == null)
				throw new ArgumentNullException("joystickName");

			joystick = new XInputController(this, deviceName.Index, joystickName.Name, window, deviceName.Controller);
			joystick.Enabled = true;

			return joystick;			
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonXInputDeviceFactory"/> class.
		/// </summary>
		public GorgonXInputDeviceFactory()
			: base("Gorgon.XInput")
		{
		}
		#endregion
	}
}
