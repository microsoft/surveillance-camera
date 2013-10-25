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
using System.Threading;
using System.Windows.Threading;
using Microsoft.Phone.Shell;

namespace SurveillanceCamera.UserControls
{
    /*
     * Custom screensaver for the application that is used only on MainPage.xaml
     * Let camera to run all the time. Application is not deactivated under system idle screen.
     * Screensaver is dismissed when user touch the MainPage
     * */
    public partial class ScreenSaver : System.Windows.Controls.UserControl
    {
        private Grid _parentLayoutRoot;
        private DispatcherTimer _timer = null;
        const int SCREENSAVER_TIMER_SECONDS = 60;
        private IApplicationBar _applicationBar = null;

        public ScreenSaver(Grid parentLayoutRoot, IApplicationBar appBar)
        {
            InitializeComponent();
            // MainPage.xaml layoutroot
            _parentLayoutRoot = parentLayoutRoot;
            _applicationBar = appBar;
            this.LayoutRoot.Visibility = System.Windows.Visibility.Collapsed;
        }

        /*
         * Count down time to show screensaver
         * */
        public void StartScreenSaverTimer()
        {
            Hide();
            if (_timer != null)
            {
                _timer.Stop();
            }
            else
            {
                _timer = new DispatcherTimer();
                _timer.Interval = new TimeSpan(0, 0, SCREENSAVER_TIMER_SECONDS);
                _timer.Tick += new EventHandler(ScreenSaverTimer_Tick);
            }
            _timer.Start();
        }

        public void StopScreenSaverTimer()
        {
            Hide();

            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
            }
        }

        /*
         * Time to show sceensaver
         * */
        private void ScreenSaverTimer_Tick(object o, EventArgs sender)
        {
            if (_timer != null)
                _timer.Stop();
            Show();
        }

        private void Show()
        {
            if (LayoutRoot.Visibility == System.Windows.Visibility.Collapsed)
            {
                _applicationBar.IsVisible = false;
                _parentLayoutRoot.Children.Add(this);
                this.ScreenSaverImage.SetValue(Canvas.LeftProperty, 0.0);
                this.ScreenSaverImage.SetValue(Canvas.TopProperty, 0.0);
                this.LayoutRoot.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void Hide()
        {
            if (this.LayoutRoot.Visibility == System.Windows.Visibility.Visible)
            {
                this.LayoutRoot.Visibility = System.Windows.Visibility.Collapsed;
                _parentLayoutRoot.Children.Remove(this);
                _applicationBar.IsVisible = true;
            }
        }

        /*
         * User touched the MainPage.xaml
         * Hide sceensaver and start counting again
         * */
        private void LayoutRoot_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            StartScreenSaverTimer();
        }
    }
}
