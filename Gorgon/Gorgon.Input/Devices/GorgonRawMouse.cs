﻿#region MIT
// 
// Gorgon
// Copyright (C) 2015 Michael Winsor
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
// Created: Wednesday, September 9, 2015 8:21:31 PM
// 
#endregion

using System;
using System.Drawing;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Input.Properties;
using Gorgon.Math;
using Gorgon.Native;
using Gorgon.Timing;

namespace Gorgon.Input
{
    /// <summary>
    /// Provides events and state for mouse data returned from Raw Input.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This allows a user to read mouse data from Raw Input for all mouse devices, or an individual device depending on what is passed to the constructor. 
    /// </para>
    /// <para>
    /// The raw input mouse is primarily meant for use in cases where the built-in Windows cursor ballistics interfere with tracking. Because of how Raw Input works, the mouse may behave strangely compared 
    /// to how the usual mouse events deliver data. For example, in a WinForms <see cref="Control.MouseMove"/> event, the position of the mouse is relative to the client area of the window receiving the message. 
    /// In the Raw Input scheme, there is no concept or knowledge of a window. The data sent from the device to Raw Input will be in the context of raw data for that device (in most cases, this would be a 
    /// mickey). 
    /// </para>
    /// <para>
    /// Because there is no concept of a window (or screen dimensions either) for Raw Input, the data is usually received as relative. That is, the values received from raw input are vectors based on the last 
    /// known position of the mouse. In some cases this data may be absolute (e.g. a touch device), and in this case the position is given in the coordinates for the device. To see these values, an application 
    /// can read the <see cref="Position"/> or <see cref="RelativePositionOffset"/> properties.
    /// </para>
    /// <para>
    /// <note type="tip">
    /// <para>
    /// This object is ideal for things like rotating a camera, as cursor ballistics may cause strange behaviour when moving. It is not recommended to use this object for things like a GUI as cursor ballistics 
    /// are not applied, and the positioning is not relative to any window or control. For those scenarios, the standard windows event mechanism is preferred.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    public class GorgonRawMouse
        : IGorgonRawInputDevice, IRawInputDeviceData<GorgonRawMouseData>, IGorgonMouse
    {
        #region Variables.
        // Range that a double click is valid within.
        private Size _doubleClickSize;
        // Mouse horizontal and vertical position.
        private Point _position;
        // Mouse wheel position.
        private int _wheel;
        // Constraints for the pointing device position.
        private Rectangle _positionConstraint;
        // Constraints for the pointing device wheel.
        private GorgonRange _wheelConstraint;
        // The delay, in milliseconds, between clicks for a double click event.
        private int _doubleClickDelay;
        // The number of times a button was fully clicked.
        private int _clickCount;
        // The recorded position for a double click.
        private Point _doubleClickPosition;
        // The button used for a double click.
        private MouseButtons _doubleClickButton;
        // The timer used for a double click.
        private readonly IGorgonTimer _doubleClickTimer = GorgonTimerQpc.SupportsQpc() ? (IGorgonTimer)new GorgonTimerQpc() : new GorgonTimerMultimedia();
        // The absolute position of the wheel.
        private int _wheelPosition;
        // The last known position for the mouse.
        private Point? _lastPosition;
        // A synchronization object for multiple threads.
        private static readonly object _syncLock = new object();
        // The device handle.
        private readonly IntPtr _deviceHandle;
        #endregion

        #region Events.
        /// <summary>
        /// Event triggered when the mouse is moved.
        /// </summary>
        public event EventHandler<GorgonMouseEventArgs> MouseMove;

        /// <summary>
        /// Event triggered when a mouse button is held down.
        /// </summary>
        public event EventHandler<GorgonMouseEventArgs> MouseButtonDown;

        /// <summary>
        /// Event triggered when a mouse button is released.
        /// </summary>
        public event EventHandler<GorgonMouseEventArgs> MouseButtonUp;

        /// <summary>
        /// Event triggered when a mouse wheel (if present) is moved.
        /// </summary>
        public event EventHandler<GorgonMouseEventArgs> MouseWheelMove;

        /// <summary>
        /// Event triggered when a double click is performed on a mouse button.
        /// </summary>
        public event EventHandler<GorgonMouseEventArgs> MouseDoubleClicked;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the type of device.
        /// </summary>
        RawInputType IGorgonRawInputDevice.DeviceType => RawInputType.Mouse;

        /// <summary>
        /// Property to return the handle for the device.
        /// </summary>
        IntPtr IGorgonRawInputDevice.Handle => _deviceHandle;

        /// <summary>
        /// Property to return the HID usage code for this device.
        /// </summary>
        HIDUsage IGorgonRawInputDevice.DeviceUsage => HIDUsage.Mouse;

        /// <summary>
        /// Property to set or return the last reported relative position movement offset.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value represents the number of units that the mouse moved since its last <see cref="IGorgonMouse.MouseMove"/> event.
        /// </para>
        /// <para>
        /// Users should reset this value when they are done with it. Otherwise, it will not be reset until the next <see cref="IGorgonMouse.MouseWheelMove"/> event.
        /// </para>
        /// <para>
        /// Raw input devices that use relative tracking will update this value as-is. That is, no calculation from a prior position is done inside of this object except to update the <see cref="Position"/> 
        /// property. For devices with absolute tracking, this value is calculated from the last known position. Due to this, the relative amount on the first read may be <c>0, 0</c> because there is no 
        /// point of reference to derive the relative offset from. 
        /// </para>
        /// </remarks>
        public Point RelativePositionOffset
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the last reported relative wheel movement delta.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value represents the number of units that the mouse wheel moved since its last <see cref="IGorgonMouse.MouseWheelMove"/> event.
        /// </para>
        /// <para>
        /// Users should reset this value when they are done with it. Otherwise, it will not be reset until the next <see cref="IGorgonMouse.MouseWheelMove"/> event.
        /// </para>
        /// </remarks>
        public int RelativeWheelDelta
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return information about this mouse.
        /// </summary>
        public IGorgonMouseInfo Info
        {
            get;
        }

        /// <summary>
        /// Property to set or return the delay between button clicks, in milliseconds, for a double click event.
        /// </summary>
        public int DoubleClickDelay
        {
            get => _doubleClickDelay;
            set
            {
                if (value < 0)
                {
                    value = 0;
                }

                _doubleClickDelay = value;
            }
        }

        /// <summary>
        /// Property to set or return whether the mouse cursor is visible.
        /// </summary>
        /// <remarks>
        /// This is a convenience property that will show or hide the mouse cursor. It has an advantage over <see cref="Cursor.Show"/> in that it will keep the mouse cursor visibility reference count 
        /// at a constant value. 
        /// </remarks>
        public static bool CursorVisible
        {
            get => UserApi.IsCursorVisible() == CursorInfoFlags.CursorShowing;
            set
            {
                lock (_syncLock)
                {
                    bool isVisible = UserApi.IsCursorVisible() == CursorInfoFlags.CursorShowing;

                    if (value != isVisible)
                    {
                        ShowMouseCursor(value);
                    }

                    ShowMouseCursor(value);
                }
            }
        }

        /// <summary>
        /// Property to set or return the <see cref="Rectangle"/> used to constrain the mouse <see cref="IGorgonMouse.Position"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This will constrain the value of the <see cref="IGorgonMouse.Position"/> within the specified <see cref="Rectangle"/>. This means that a cursor positioned at 320x200 with a region located at 330x210 with a width 
        /// and height of 160x160 will make the <see cref="IGorgonMouse.Position"/> property return 330x210. If the cursor was positioned at 500x400, the <see cref="IGorgonMouse.Position"/> property would return 480x360.
        /// </para>
        /// <para>
        /// Passing <see cref="Rectangle.Empty"/> to this property will remove the constraint on the position.
        /// </para>
        /// <para>
        /// <note type="warning">
        /// <para>
        /// This will constrain the mouse cursor to the area defined. If more than one mouse device is being used, the mouse cursor will always be constrained to the last <see cref="GorgonRawMouse"/> that set 
        /// this value. In such cases, it is best to turn off the mouse cursor and handle drawing of a mouse cursor manually.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        public Rectangle PositionConstraint
        {
            get => _positionConstraint;
            set
            {
                _positionConstraint = value;
                _position = ConstrainPositionData(_position);
            }
        }

        /// <summary>
        /// Property to set or return the <see cref="GorgonRange"/> used to constrain the mouse <see cref="IGorgonMouse.WheelPosition"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If a mouse wheel exists on the device, this will constrain the value of the <see cref="IGorgonMouse.WheelPosition"/> within the specified <see cref="GorgonRange"/>. This means that a wheel with a position of  
        /// 160, with a constraint of 180-190 will make the <see cref="IGorgonMouse.WheelPosition"/> property return 180.
        /// </para>
        /// <para>
        /// Passing <see cref="GorgonRange.Empty"/> to this property will remove the constraint on the position.
        /// </para>
        /// </remarks>
        public GorgonRange WheelConstraint
        {
            get => _wheelConstraint;
            set
            {
                _wheelConstraint = value;
                ConstrainWheelData();
            }
        }

        /// <summary>
        /// Property to set or return the <see cref="Size"/> of the area, in pixels, surrounding the cursor that represents a valid double click area.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When this value is set, and a mouse button is double clicked, this value is checked to see if the mouse <see cref="IGorgonMouse.Position"/> falls within -<c>value.</c><see cref="Size.Width"/> to <c>value.</c><see cref="Size.Width"/>, 
        /// and -<c>value.</c><see cref="Size.Height"/> to <c>value.</c><see cref="Size.Height"/> on the second click. If the <see cref="IGorgonMouse.Position"/> is within this area, then the double click event will be triggered. Otherwise, it will 
        /// not.
        /// </para>
        /// <para>
        /// Passing <see cref="Size.Empty"/> to this property will disable double clicking.
        /// </para>
        /// </remarks>
        public Size DoubleClickSize
        {
            get => _doubleClickSize;
            set => _doubleClickSize = new Size(value.Width.Abs(), value.Height.Abs());
        }

        /// <summary>
        /// Property to set or return the position of the mouse.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When the object is first created, this value is set to <c>0, 0</c> and is updated when the mouse moves. For devices with relative tracking, this value is accumulated based on the <see cref="RelativePositionOffset"/> 
        /// returned from Raw Input. If the device uses absolute tracking, then this value is the actual position received from the device.
        /// </para>
        /// <para>
        /// Raw Input has no concept of screen dimensions, or window dimensions. And because of this, the values reflected here may seem strange, and will not be constrained to the client area of the active window. 
        /// The user is responsible for handling where the mouse is located in relation to the screen or window if that is desired. However, in those cases, it is much better to use the events of the window to 
        /// handle mouse movement tracking.
        /// </para>
        /// <para>
        /// This property is affected by the <see cref="IGorgonMouse.PositionConstraint"/> value.
        /// </para>
        /// </remarks>
        public Point Position
        {
            get => _position;
            set => _position = ConstrainPositionData(value);
        }

        /// <summary>
        /// Property to set or return the pointing device wheel position.
        /// </summary>
        /// <remarks>
        /// This property is affected by the <see cref="IGorgonMouse.WheelConstraint"/> value.
        /// </remarks>
        public int WheelPosition
        {
            get => _wheelPosition;
            set
            {
                _wheelPosition = value;
                ConstrainWheelData();
            }
        }

        /// <summary>
        /// Property to set or return the pointing device button(s) that are currently down.
        /// </summary>
        public MouseButtons Buttons
        {
            get;
            set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to initiate a double click.
        /// </summary>
        /// <param name="button">The button that was clicked.</param>
        private void BeginDoubleClick(MouseButtons button)
        {
            if ((_doubleClickButton != MouseButtons.None) && (button != _doubleClickButton))
            {
                _doubleClickTimer.Reset();
                _clickCount = 0;
                return;
            }

            if (_clickCount > 0)
            {
                return;
            }

            _doubleClickTimer.Reset();
            _doubleClickPosition = Position;
            _doubleClickButton = button;
        }


        /// <summary>
        /// Function to handle mouse down button events.
        /// </summary>
        /// <param name="buttonState">Button state to evaluate.</param>
        /// <returns>The current button that was held down.</returns>
        private MouseButtons HandleButtonDownEvents(MouseButtonState buttonState)
        {
            MouseButtons button = MouseButtons.None;

            if (_doubleClickTimer.Milliseconds > DoubleClickDelay)
            {
                _doubleClickTimer.Reset();
                _clickCount = 0;
            }

            if ((buttonState & MouseButtonState.ButtonLeftDown) == MouseButtonState.ButtonLeftDown)
            {
                button = MouseButtons.Left;
                BeginDoubleClick(button);
            }

            if ((buttonState & MouseButtonState.ButtonRightDown) == MouseButtonState.ButtonRightDown)
            {
                button = MouseButtons.Right;
                BeginDoubleClick(button);
            }

            if ((buttonState & MouseButtonState.ButtonMiddleDown) == MouseButtonState.ButtonMiddleDown)
            {
                button = MouseButtons.Middle;
                BeginDoubleClick(button);
            }

            if ((buttonState & MouseButtonState.Button4Down) == MouseButtonState.Button4Down)
            {
                button = MouseButtons.Button4;
                BeginDoubleClick(button);
            }

            if ((buttonState & MouseButtonState.Button5Down) == MouseButtonState.Button5Down)
            {
                button = MouseButtons.Button5;
                BeginDoubleClick(button);
            }

            Buttons |= button;

            return button;
        }

        /// <summary>
        /// Function to handle mouse up button events.
        /// </summary>
        /// <param name="buttonState">Button state to evaluate.</param>
        /// <returns>The current button that was released.</returns>
        private MouseButtons HandleButtonUpEvents(MouseButtonState buttonState)
        {
            MouseButtons button = MouseButtons.None;

            if ((buttonState & MouseButtonState.ButtonLeftUp) == MouseButtonState.ButtonLeftUp)
            {
                button = MouseButtons.Left;
            }

            if ((buttonState & MouseButtonState.ButtonRightUp) == MouseButtonState.ButtonRightUp)
            {
                button = MouseButtons.Right;
            }

            if ((buttonState & MouseButtonState.ButtonMiddleUp) == MouseButtonState.ButtonMiddleUp)
            {
                button = MouseButtons.Middle;
            }

            if ((buttonState & MouseButtonState.Button4Up) == MouseButtonState.Button4Up)
            {
                button = MouseButtons.Button4;
            }

            if ((buttonState & MouseButtonState.Button5Up) == MouseButtonState.Button5Up)
            {
                button = MouseButtons.Button5;
            }

            // If no button was released, then exit.
            if (button == MouseButtons.None)
            {
                return button;
            }

            var doubleClickArea = new Rectangle(_doubleClickPosition.X - (DoubleClickSize.Width / 2),
                                                      _doubleClickPosition.Y - (DoubleClickSize.Height / 2),
                                                      DoubleClickSize.Width,
                                                      DoubleClickSize.Height);

            if ((!doubleClickArea.Contains(Position)) || (_doubleClickButton != button) || (_doubleClickTimer.Milliseconds > DoubleClickDelay))
            {
                _doubleClickTimer.Reset();
                _clickCount = 0;
            }
            else
            {
                ++_clickCount;
            }

            Buttons &= ~button;

            return button;
        }

        /// <summary>
        /// Function to handle a mouse movement event.
        /// </summary>
        /// <param name="mouseWheelDelta">The delta indicating the direction and amount that the mouse wheel moved by.</param>
        private void HandleMouseWheelMove(short mouseWheelDelta)
        {
            if (mouseWheelDelta == 0)
            {
                return;
            }

            RelativeWheelDelta = mouseWheelDelta;
            WheelPosition += mouseWheelDelta;
        }

        /// <summary>
        /// Function to handle the mouse movement event.
        /// </summary>
        /// <param name="x">The last relative horizontal position for the mouse.</param>
        /// <param name="y">The last relative vertical position for the mouse.</param>
        /// <param name="relative"><b>true</b> if the mouse movement is relative, <b>false</b> if absolute.</param>
        /// <returns><b>true</b> if the mouse moved, <b>false</b> if not.</returns>
        private bool HandleMouseMove(int x, int y, bool relative)
        {
            Point newPosition;

            if (relative)
            {
                RelativePositionOffset = new Point(x, y);
                newPosition = new Point(_position.X + x, _position.Y + y);
            }
            else
            {
                newPosition = new Point(x, y);

                if (_lastPosition != null)
                {
                    RelativePositionOffset = new Point((newPosition.X - _lastPosition.Value.X), ((newPosition.Y - _lastPosition.Value.Y)));
                }
            }

            newPosition = ConstrainPositionData(newPosition);

            _lastPosition = newPosition;

            if ((newPosition.X == _position.X) && (newPosition.Y == _position.Y))
            {
                return false;
            }

            Position = newPosition;

            return true;
        }

        /// <summary>
        /// Function to show the mouse cursor or hide it.
        /// </summary>
        /// <param name="show"><b>true</b> to show the mouse cursor, <b>false</b> to hide it.</param>
        private static void ShowMouseCursor(bool show)
        {
            CursorInfoFlags isVisible = UserApi.IsCursorVisible();

            // If the cursor is suppressed, then we're using a touch interface.
            // So we'll not acknowledge requests to show the cursor in that case.
            if ((isVisible == CursorInfoFlags.Suppressed)
                || ((isVisible == CursorInfoFlags.CursorShowing) && (show))
                || ((isVisible == CursorInfoFlags.CursorHidden) && (!show)))
            {
                return;
            }

            if (show)
            {
                while (UserApi.ShowCursor(true) < 0)
                {
                }

                return;
            }

            while (UserApi.ShowCursor(false) > -1)
            {
            }
        }

        /// <summary>
        /// Function to constrain the mouse position data to the supplied ranges.
        /// </summary>
        /// <param name="position">The position to constrain.</param>
        /// <returns>The constrained position.</returns>
        private Point ConstrainPositionData(Point position)
        {
            if (_positionConstraint.IsEmpty)
            {
                return position;
            }

            // Limit positioning.
            if (position.X < _positionConstraint.X)
            {
                RelativePositionOffset = new Point(0, RelativePositionOffset.Y);
                position.X = _positionConstraint.X;
            }

            if (position.Y < _positionConstraint.Y)
            {
                RelativePositionOffset = new Point(RelativePositionOffset.X, 0);
                position.Y = _positionConstraint.Y;
            }

            if (position.X > _positionConstraint.Right)
            {
                RelativePositionOffset = new Point(0, RelativePositionOffset.Y);
                position.X = _positionConstraint.Right;
            }

            if (position.Y <= _positionConstraint.Bottom)
            {
                return position;
            }

            RelativePositionOffset = new Point(RelativePositionOffset.X, 0);
            position.Y = _positionConstraint.Bottom;

            return position;
        }

        /// <summary>
        /// Function to constrain the mouse wheel data to the supplied range.
        /// </summary>
        private void ConstrainWheelData()
        {
            if (_wheelConstraint == GorgonRange.Empty)
            {
                return;
            }

            // Limit wheel.
            if (_wheel < _wheelConstraint.Minimum)
            {
                _wheel = _wheelConstraint.Minimum;
            }

            if (_wheel > _wheelConstraint.Maximum)
            {
                _wheel = _wheelConstraint.Maximum;
            }
        }

        /// <summary>
        /// Function to process the Gorgon raw input data into device state data and appropriate events.
        /// </summary>
        /// <param name="rawInputData">The data to process.</param>
        void IRawInputDeviceData<GorgonRawMouseData>.ProcessData(ref GorgonRawMouseData rawInputData)
        {
            // Gather the event information.
            MouseButtons downButtons = MouseButtons.None;
            MouseButtons upButtons = MouseButtons.None;

            bool wasMoved = HandleMouseMove(rawInputData.Position.X, rawInputData.Position.Y, rawInputData.IsRelative);

            if (rawInputData.MouseWheelDelta != 0)
            {
                HandleMouseWheelMove(rawInputData.MouseWheelDelta);
            }

            // If there's a button event, then process it.
            if (rawInputData.ButtonState != MouseButtonState.None)
            {
                downButtons = HandleButtonDownEvents(rawInputData.ButtonState);
                upButtons = HandleButtonUpEvents(rawInputData.ButtonState);
            }

            // Trigger button events.
            if (downButtons != MouseButtons.None)
            {
                MouseButtonDown?.Invoke(this, new GorgonMouseEventArgs(downButtons, Buttons, _position, _wheelPosition, RelativePositionOffset, RelativeWheelDelta, _clickCount, !rawInputData.IsRelative));
            }

            if (upButtons != MouseButtons.None)
            {
                var e = new GorgonMouseEventArgs(upButtons, Buttons, _position, _wheelPosition, RelativePositionOffset, RelativeWheelDelta, _clickCount, !rawInputData.IsRelative);

                MouseButtonUp?.Invoke(this, e);

                if ((_clickCount > 0) && ((_clickCount % 2) == 0))
                {
                    MouseDoubleClicked?.Invoke(this, e);
                }
            }

            // Trigger move events.
            if (rawInputData.MouseWheelDelta != 0)
            {
                MouseWheelMove?.Invoke(this, new GorgonMouseEventArgs(Buttons, MouseButtons.None, _position, _wheelPosition, RelativePositionOffset, RelativeWheelDelta, 0, !rawInputData.IsRelative));
            }

            if (wasMoved)
            {
                MouseMove?.Invoke(this, new GorgonMouseEventArgs(Buttons, MouseButtons.None, _position, _wheelPosition, RelativePositionOffset, RelativeWheelDelta, 0, !rawInputData.IsRelative));
            }
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonRawMouse" /> class.
        /// </summary>
        /// <param name="mouseInfo">[Optional] A <see cref="IGorgonMouseInfo"/> value to determine which device to use..</param>
        /// <exception cref="InvalidCastException">Thrown if the <paramref name="mouseInfo"/> is not the expected type.</exception>
        /// <remarks>
        /// When the <paramref name="mouseInfo"/> is set to <b>null</b>, the system mouse (that is, all mice attached to the computer) will be used. No differentiation between 
        /// mouse devices is made. To specify an individual mouse, pass an appropriate <see cref="IGorgonMouseInfo"/> obtained from the <see cref="GorgonRawInput.EnumerateMice"/> method.
        /// </remarks>
        public GorgonRawMouse(IGorgonMouseInfo mouseInfo = null)
        {
            var info = mouseInfo as RawMouseInfo;

            if (mouseInfo == null)
            {
                mouseInfo = info = new RawMouseInfo(IntPtr.Zero,
                                                    "System Mouse",
                                                    "Mouse",
                                                    Resources.GORINP_RAW_DESC_SYS_MOUSE,
                                                    new RID_DEVICE_INFO_MOUSE
                                                    {
                                                        dwId = 0,
                                                        dwNumberOfButtons = SystemInformation.MouseButtons,
                                                        dwSampleRate = 0
                                                    });
            }

            _deviceHandle = info?.Handle ?? throw new InvalidCastException(string.Format(Resources.GORINP_RAW_ERR_INVALID_DEVICE_INFO_TYPE, mouseInfo.GetType().FullName, GetType().FullName));

            Info = mouseInfo;
        }
        #endregion

    }
}
