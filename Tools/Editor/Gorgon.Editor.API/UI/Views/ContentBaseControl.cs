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
// Created: October 29, 2018 4:12:28 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gorgon.Editor.Rendering;
using Gorgon.Graphics.Core;
using Gorgon.UI;
using ComponentFactory.Krypton.Toolkit;
using ComponentFactory.Krypton.Ribbon;
using Gorgon.Editor.Content;

namespace Gorgon.Editor.UI.Views
{
    /// <summary>
    /// The base control used to render content.
    /// </summary>
    public partial class ContentBaseControl 
        : EditorBaseControl, IRendererControl
    {
        #region Variables.
        // The swap chain for the control.
        private GorgonSwapChain _swapChain;
        // The data context for the editor context.
        private IEditorContent _dataContext;
        #endregion

        #region Events.
        /// <summary>
        /// Event triggered when a drag enter operation is to be bubbled up to its parent.
        /// </summary>
        [Category("Drag Drop"), Description("Notifies the parent control that the drag enter event has been passed to it from this control.")]
        public event EventHandler<DragEventArgs> BubbleDragEnter;
        /// <summary>
        /// Event triggered when a drag enter operation is to be bubbled up to its parent.
        /// </summary>
        [Category("Drag Drop"), Description("Notifies the parent control that the drag drop event has been passed to it from this control.")]
        public event EventHandler<DragEventArgs> BubbleDragDrop;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the idle method for rendering on the control.
        /// </summary>
        /// <remarks>
        /// The <see cref="Stop"/> method must be called prior to switching idle methods.
        /// </remarks>
        protected Func<bool> IdleMethod
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the ribbon for the content view.
        /// </summary>
        [Browsable(false)]
        public KryptonRibbon Ribbon
        {
            get;
            protected set;
        }

        /// <summary>
        /// Property to set or return the control that will be rendered into using a <see cref="GorgonSwapChain"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Plug in developers should set this in the IDE designer to set up a swap chain for rendering when this control is created.
        /// </para>
        /// <para>
        /// If this property is assigned after control creation, the <see cref="SetupGraphics(IGraphicsContext)"/> method must be called again for it to take effect.
        /// </para>
        /// <para>
        /// If this value is set to <b>null</b>, then no swap chain will be created and the <see cref="SwapChain"/> property will be set to <b>null</b>.
        /// </para>
        /// </remarks>
        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always), Category("Rendering"), Description("Sets or returns the custom control to use for Gorgon rendering.")]
        public Control RenderControl
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the panel that will be used for presentation of the content.
        /// </summary>
        [Browsable(false)]
        public KryptonPanel PresentationPanel => PanelPresenter;

        /// <summary>Property to return the graphics context.</summary>
        [Browsable(false)]
        public IGraphicsContext GraphicsContext
        {
            get;
            private set;
        }

        /// <summary>Property to return the swap chain assigned to the control.</summary>
        [Browsable(false)]
        public GorgonSwapChain SwapChain => _swapChain;
        #endregion

        #region Methods.
        /// <summary>Handles the Click event of the ButtonClose control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The [EventArgs] instance containing the event data.</param>
        private void ButtonClose_Click(object sender, EventArgs e)
        {
            if ((_dataContext?.CloseContentCommand == null) || (!_dataContext.CloseContentCommand.CanExecute(null)))
            {
                return;
            }

            _dataContext.CloseContentCommand.Execute(null);
        }

        /// <summary>Handles the PropertyChanged event of the DataContext control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The [PropertyChangedEventArgs] instance containing the event data.</param>
        private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IEditorContent.CloseContentCommand):
                    ButtonClose.Visible = _dataContext.CloseContentCommand != null;

                    if (!PanelContentName.Visible)
                    {
                        SetContentName(_dataContext);
                    }
                    return;
                case nameof(IEditorContent.ContentState):
                case nameof(IEditorContent.File):
                    SetContentName(_dataContext);
                    break;
            }

            OnPropertyChanged(e);
        }

        /// <summary>Handles the PropertyChanging event of the DataContext control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The [PropertyChangingEventArgs] instance containing the event data.</param>
        private void DataContext_PropertyChanging(object sender, PropertyChangingEventArgs e) => OnPropertyChanging(e);

        /// <summary>
        /// Function to initialize the view from the current data context.
        /// </summary>
        /// <param name="dataContext">The data context being assigned.</param>
        private void InitializeFromDataContext(IEditorContent dataContext)
        {
            if (dataContext == null)
            {
                ResetDataContext();
                return;
            }

            ButtonClose.Visible = dataContext.CloseContentCommand != null;
            SetContentName(dataContext);
        }

        /// <summary>
        /// Function to shut down the view.
        /// </summary>
        private void Shutdown()
        {
            UnassignEvents();

            Stop();

            OnShutdown();

            // Return the swap chain to the pool.
            if (_swapChain != null)
            {
                GraphicsContext.ReturnSwapPresenter(ref _swapChain);
            }
        }

        /// <summary>
        /// Function to assign the current content name.
        /// </summary>
        /// <param name="dataContext">The data context to use.</param>
        private void SetContentName(IEditorContent dataContext)
        {
            if (string.IsNullOrWhiteSpace(dataContext?.File?.Name))
            {
                LabelHeader.Text = string.Empty;
                if (dataContext?.CloseContentCommand == null)
                {
                    PanelContentName.Visible = false;
                }
                else
                {
                    PanelContentName.Visible = true;
                }
                return;
            }

            LabelHeader.Text = $"{dataContext.File.Name}{(dataContext.ContentState == ContentState.Unmodified ? string.Empty : "*")}";
            PanelContentName.Visible = true;
        }

        /// <summary>
        /// Function to bubble up the drag enter event up to the main project window.
        /// </summary>
        /// <param name="e">The drag event parameters.</param>
        /// <remarks>
        /// <para>
        /// Implementors can use this method to notify the parent of this control that a drag enter event is being passed on from this control.
        /// </para>
        /// </remarks>
        protected virtual void OnBubbleDragEnter(DragEventArgs e)
        {
            EventHandler<DragEventArgs> handler = BubbleDragEnter;
            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Function to bubble up the drag drop event up to the main project window.
        /// </summary>
        /// <param name="e">The drag event parameters.</param>
        /// <remarks>
        /// <para>
        /// Implementors can use this method to notify the parent of this control that a drag drop event is being passed on from this control.
        /// </para>
        /// </remarks>
        protected virtual void OnBubbleDragDrop(DragEventArgs e)
        {
            EventHandler<DragEventArgs> handler = BubbleDragDrop;
            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Function called when a property is changing on the data context.
        /// </summary>
        /// <param name="e">The event parameters.</param>
        /// <remarks>
        /// <para>
        /// Implementors should override this method in order to handle a property change notification from their data context.
        /// </para>
        /// </remarks>
        protected virtual void OnPropertyChanging(PropertyChangingEventArgs e)
        {

        }

        /// <summary>
        /// Function called when a property is changed on the data context.
        /// </summary>
        /// <param name="e">The event parameters.</param>
        /// <remarks>
        /// <para>
        /// Implementors should override this method in order to handle a property change notification from their data context.
        /// </para>
        /// </remarks>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {

        }

        /// <summary>
        /// Function to unassign events for the data context.
        /// </summary>
        protected virtual void UnassignEvents()
        {
            if (_dataContext == null)
            {
                return;
            }

            _dataContext.PropertyChanging -= DataContext_PropertyChanging;
            _dataContext.PropertyChanged -= DataContext_PropertyChanged;
        }

        /// <summary>
        /// Function called when the view should be reset by a <b>null</b> data context.
        /// </summary>
        protected virtual void ResetDataContext()
        {
            if (_dataContext == null)
            {
                return;
            }

            ButtonClose.Visible = false;
            SetContentName(null);
        }

        /// <summary>
        /// Function to assign the data context to this object.
        /// </summary>
        /// <param name="dataContext">The data context to assign.</param>
        /// <remarks>
        /// <para>
        /// Applications must call this method when setting their own data context. Otherwise, some functionality will not work.
        /// </para>
        /// </remarks>
        protected void SetDataContext(IEditorContent dataContext)
        {
            UnassignEvents();

            InitializeFromDataContext(dataContext);
            _dataContext = dataContext;

            if (_dataContext == null)
            {
                return;
            }

            _dataContext.PropertyChanged += DataContext_PropertyChanged;
            _dataContext.PropertyChanging += DataContext_PropertyChanging;
        }

        /// <summary>
        /// Function to allow user defined setup of the graphics context with this control.
        /// </summary>
        /// <param name="context">The context being assigned.</param>
        /// <param name="swapChain">The swap chain assigned to the <see cref="RenderControl"/>.</param>
        protected virtual void OnSetupGraphics(IGraphicsContext context, GorgonSwapChain swapChain)
        {
        }

        /// <summary>
        /// Function called to shut down the view and perform any clean up required (including user defined graphics objects).
        /// </summary>
        /// <remarks>
        /// <para>
        /// Plug in developers do not need to clean up the <see cref="SwapChain"/> as it will be returned to the swap chain pool automatically.
        /// </para>
        /// </remarks>
        protected virtual void OnShutdown()
        {
        }

        /// <summary>
        /// Function to begin rendering on the control.
        /// </summary>
        public void Start() => GorgonApplication.IdleMethod = IdleMethod;

        /// <summary>
        /// Function to cease rendering on the control.
        /// </summary>
        public void Stop() => GorgonApplication.IdleMethod = null;

        /// <summary>
        /// Function to initialize the graphics context for the control.
        /// </summary>
        /// <param name="context">The graphics context to use.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="context"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// If the <see cref="RenderControl"/> property is assigned on control creation, then a primary swap chain will be created for that control and provided via the <see cref="SwapChain"/> property.
        /// </para>
        /// </remarks>
        public void SetupGraphics(IGraphicsContext context)
        {
            // This should not be executing when designing.
            if (IsDesignTime)
            {
                return;
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // If we've made no change, then do nothing.
            if ((context == GraphicsContext) && (_swapChain != null) && (_swapChain.Window == RenderControl))
            {
                return;
            }

            if (_swapChain != null)
            {
                GraphicsContext.ReturnSwapPresenter(ref _swapChain);
            }

            GorgonSwapChain swapChain = null;

            // If we've defined a render control, then lease a swap chain from the swap chain pool.
            if ((context != null) && (RenderControl != null))
            {
                swapChain = context.LeaseSwapPresenter(RenderControl);
            }            

            OnSetupGraphics(context, swapChain);
            GraphicsContext = context;
            _swapChain = swapChain;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the ContentBaseControl class.</summary>
        public ContentBaseControl() => InitializeComponent();
        #endregion
    }
}