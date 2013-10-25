/*
 * Copyright (c) 2012 Nokia Corporation.
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;

namespace SurveillanceCamera.UserControls
{
    /*
     * Message popup that asks Ovi account id from the user
     * */
    public partial class OviAccountId : System.Windows.Controls.UserControl
    {
        private string _username;
        private Popup _popupForNote = null;

        public delegate void OviUsernameReceived(string username, bool save);
        public event OviUsernameReceived OviUsernameReceivedEvent = null;

        /*
         * Constructor
         * */
        public OviAccountId(double width, double height, string defaultUsername)
        {
            InitializeComponent();
            if (_popupForNote==null)
                _popupForNote = new Popup();
            _popupForNote.HorizontalOffset = 0;
            _popupForNote.VerticalOffset = 0;
            _popupForNote.Child = this;
            _popupForNote.IsOpen = false;
            accountIdTextBox.Text = defaultUsername;
            this.Width = width;
            this.Height = height;
        }

        public void SetDefaulUsername(string name)
        {
            accountIdTextBox.Text = name;
        }

        /*
         * Shows the popup
         * */
        public void Show()
        {
            _popupForNote.IsOpen = true;
        }

        public void Hide()
        {
            _popupForNote.IsOpen = false;
        }

        /*
         * Is Popup visible or not
         * */
        public bool IsVisible()
        {
            if (_popupForNote != null)
                return _popupForNote.IsOpen;
            else
                return false;
        }

        /*
         * Closes popup and sends given account id to the listener
         * */
        private void activateBtn_Click(object sender, RoutedEventArgs e)
        {
            if (accountIdTextBox.Text.Trim().Length > 0)
            {
                _popupForNote.IsOpen = false;
                _username = accountIdTextBox.Text.Trim();
                if (OviUsernameReceivedEvent != null)
                    OviUsernameReceivedEvent(_username, this.saveDataCheckBox.IsChecked.Value);
            }
        }

        private void accountIdTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb.Text.Length > 0)
                this.activateBtn.IsEnabled = true;
            else
                this.activateBtn.IsEnabled = false;
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            _popupForNote.IsOpen = false;
            _username = "";
            if (OviUsernameReceivedEvent != null)
                OviUsernameReceivedEvent(_username, false);
        }


    }
}
