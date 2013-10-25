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
using Microsoft.Phone.Controls;

using SurveillanceCamera.Model;

namespace SurveillanceCamera
{
    public partial class AlarmHistoryPage : PhoneApplicationPage
    {

        public Logs Logs
        {
            get;
            set;
        }

        public AlarmHistoryPage()
        {
            InitializeComponent();

            this.Logs = new Logs();
            listBox.ItemsSource = this.Logs;
        }

        override protected void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (State.ContainsKey("LogPageModel"))
            {
                Logs pageData = (Logs)State["LogPageModel"];
                this.Logs = pageData;
                State.Remove("LogPageModel");
            }

            base.OnNavigatedTo(e);
        }

        override protected void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            // If this is a back navigation, the page will be discarded, so there
            // is no need to save state.
            if (e.NavigationMode != System.Windows.Navigation.NavigationMode.Back)
            {
                State["LogPageModel"] = this.Logs;
            }

            base.OnNavigatingFrom(e);
        }

        private void ApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            // Show info page
            NavigationService.Navigate(new Uri("/InfoPage.xaml", UriKind.RelativeOrAbsolute));
        }

        private void ApplicationBarMenuItem_Click_2(object sender, EventArgs e)
        {
            // Clear logs
            this.Logs.ClearLogs();
        }

    }
}