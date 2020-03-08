﻿#region MIT
// 
// Gorgon.
// Copyright (C) 2020 Michael Winsor
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
// Created: March 6, 2020 1:30:22 PM
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
using Gorgon.Editor.UI.Controls;
using Gorgon.Editor.UI;
using Gorgon.Editor.ImageEditor.Fx;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// The settings for the sharpen effect.
    /// </summary>
    internal partial class FxSharpenSettings 
        : EditorSubPanelCommon, IDataContext<IFxSharpen>
    {
        #region Properties.
        /// <summary>Property to return the data context assigned to this view.</summary>
        public IFxSharpen DataContext
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>Handles the ValueChanged event of the NumericBlurAmount control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void NumericSharpenAmount_ValueChanged(object sender, EventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            int currentValue = (int)NumericSharpenAmount.Value;

            if (currentValue == DataContext.Amount)
            {
                return;
            }

            DataContext.Amount = currentValue;
        }

        /// <summary>Function to submit the change.</summary>
        protected override void OnSubmit()
        {
            base.OnSubmit();

            if ((DataContext?.OkCommand == null) || (!DataContext.OkCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.OkCommand.Execute(null);
        }

        /// <summary>Function to cancel the change.</summary>
        protected override void OnCancel()
        {
            base.OnCancel();

            if ((DataContext?.CancelCommand == null) || (!DataContext.CancelCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.CancelCommand.Execute(null);
        }

        /// <summary>Handles the PropertyChanged event of the DataContext control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IFxSharpen.Amount):
                    NumericSharpenAmount.Value = DataContext.Amount;
                    break;
            }
        }

        /// <summary>
        /// Function to unassign the events from the data context.
        /// </summary>
        private void UnassignEvents()
        {
            if (DataContext == null)
            {
                return;
            }

            DataContext.PropertyChanged -= DataContext_PropertyChanged;
        }

        /// <summary>
        /// Function to initialize the control from the data context.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        private void InitializeFromDataContext(IFxSharpen dataContext)
        {
            if (dataContext == null)
            {
                NumericSharpenAmount.ValueChanged -= NumericSharpenAmount_ValueChanged;
                NumericSharpenAmount.Value = 1;
                NumericSharpenAmount.ValueChanged += NumericSharpenAmount_ValueChanged;
                return;
            }

            NumericSharpenAmount.Value = dataContext.Amount;
        }

        /// <summary>Function to assign a data context to the view as a view model.</summary>
        /// <param name="dataContext">The data context to assign.</param>
        /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
        public void SetDataContext(IFxSharpen dataContext)
        {
            UnassignEvents();

            InitializeFromDataContext(dataContext);

            DataContext = dataContext;

            if (dataContext == null)
            {
                return;
            }

            DataContext.PropertyChanged += DataContext_PropertyChanged;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="FxSharpenSettings"/> class.</summary>
        public FxSharpenSettings() => InitializeComponent();
        #endregion
    }
}
