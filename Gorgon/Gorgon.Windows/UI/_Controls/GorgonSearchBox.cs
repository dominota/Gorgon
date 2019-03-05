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
// Created: December 12, 2018 10:52:42 AM
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

namespace Gorgon.Windows.UI
{
    /// <summary>
    /// A textbox for searching.
    /// </summary>
    public partial class GorgonSearchBox : UserControl
    {
        #region Events.
        /// <summary>
        /// The event triggered when a search is initiated.
        /// </summary>
        [Category("Behavior"), Description("Event triggered when a search is initiated.")]
        public event EventHandler<GorgonSearchEventArgs> Search;
        #endregion

        #region Properties.
        /// <summary>This property is not relevant for this class.</summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Padding Padding
        {
            get => base.Padding;
            set => base.Padding = value;
        }
        
        /// <summary>Gets or sets the current text in the <see cref="T:System.Windows.Forms.TextBox" />.</summary>
        /// <returns>The text displayed in the control.</returns>
        /// <filterpriority>1</filterpriority>
        public override string Text
        {
            get => base.Text;
            set
            {
                TextSearch.Text = base.Text = value;
                TextSearch.SelectionLength = 0;
                TextSearch.SelectionStart = 0;
            }
        }
        #endregion

        #region Methods.
        /// <summary>Handles the TextChanged event of the TextSearch control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void TextSearch_TextChanged(object sender, EventArgs e)
        {
            if ((!Focused) && (string.IsNullOrEmpty(TextSearch.Text)))
            {
                ButtonClearSearch.Visible = false;
            }
            else
            {
                ButtonClearSearch.Visible = true;
            }

            base.Text = TextSearch.Text;
        }

        /// <summary>Handles the Enter event of the TextSearch control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void TextSearch_Enter(object sender, EventArgs e)
        {
            ButtonClearSearch.Visible = TextSearch.Text.Length > 0;
            OnGotFocus(EventArgs.Empty);
        }

        /// <summary>
        /// Function to trigger to the <see cref="Search"/> event.
        /// </summary>
        /// <param name="e">The event parameters.</param>
        protected virtual void OnSearch(GorgonSearchEventArgs e)
        {
            EventHandler<GorgonSearchEventArgs> handler = Search;
            handler?.Invoke(this, e);
        }

        /// <summary>Handles the KeyUp event of the TextSearch control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void TextSearch_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    OnSearch(new GorgonSearchEventArgs(TextSearch.Text));
                    break;
                case Keys.Escape:
                    ButtonClearSearch.PerformClick();
                    break;
            }
        }

        /// <summary>Handles the Click event of the ButtonClearSearch control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonClearSearch_Click(object sender, EventArgs e)
        {
            TextSearch.Text = string.Empty;
            OnSearch(new GorgonSearchEventArgs(string.Empty));
            SelectNextControl(this, true, true, true, true);
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.Control.BackColorChanged"/> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            TextSearch.BackColor = BackColor;
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.Control.ForeColorChanged"/> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnForeColorChanged(EventArgs e)
        {
            base.OnForeColorChanged(e);
            TextSearch.ForeColor = ForeColor;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.ForeColorChanged"/> event when the <see cref="P:System.Windows.Forms.Control.ForeColor"/> property value of the control's container changes.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnParentForeColorChanged(EventArgs e)
        {
            base.OnParentForeColorChanged(e);
            TextSearch.ForeColor = ForeColor;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.BackColorChanged"/> event when the <see cref="P:System.Windows.Forms.Control.BackColor"/> property value of the control's container changes.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnParentBackColorChanged(EventArgs e)
        {
            base.OnParentBackColorChanged(e);
            TextSearch.BackColor = BackColor;
        }
        #endregion

        #region Constructor.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Windows.UI.GorgonSearchBox"/> class.</summary>
        public GorgonSearchBox() =>InitializeComponent();
        #endregion
    }
}