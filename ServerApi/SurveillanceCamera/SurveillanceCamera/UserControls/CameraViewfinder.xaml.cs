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

using Microsoft.Devices;
using System.Windows.Media.Imaging;
using System.Threading;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Threading;

namespace SurveillanceCamera.UserControls
{
    /*
     * Camera data class for binding
     * */
    public class CameraData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public static int MULTIPLIER = 5;

        public CameraData(int tres, int activ)
        {
            _treshold = tres;
            _activity = activ;
        }

        private double _treshold;
        public double Treshold
        {
            get { return _treshold; }
            set {
                _treshold = value;
                NotifyPropertyChanged("Treshold");
            }
        }

        private double _activity;
        public double Activity
        {
            get { return _activity; }
            set {
                _activity = value * MULTIPLIER;
                NotifyPropertyChanged("Activity");
            }
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                if (Deployment.Current.Dispatcher.CheckAccess())
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
                else
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                        });
                }

            }
        }
    }

    /*
     * Get frames from camera
     * */
    public partial class CameraViewfinder : System.Windows.Controls.UserControl
    {
        private PhotoCamera _camera = null;
        private Thread _thread = null;
        private bool _threadPleaseExit;
        private int[] previousFrame = null;

        public delegate void Alert();
        public event Alert AlertEvent = null;

        private const int FRAME_DIFFER_VALUE = 75 * 1000;

        private DispatcherTimer _alertShowingTimer = null;
        private int _alertShowingCounter = 0;

        public CameraData CameraData
        {
            get; set;
        }

        public CameraViewfinder()
        {
            InitializeComponent();

            CameraData = new CameraData(5,0);
        }

        public void StartCamera()
        {
            if (_camera == null)
            {
                _camera = new PhotoCamera(CameraType.Primary);
                _camera.Initialized += new EventHandler<CameraOperationCompletedEventArgs>(CameraInitialized);
                viewfinderBrush.SetSource(_camera);
            }
        }

        public void StopCamera()
        {
            if (_camera != null)
            {
                // Notify the background worker to stop processing.
                _threadPleaseExit = true;

                // Dispose camera to minimize power consumption and to expedite shutdown.
                _camera.Dispose();
                _camera = null;

                _thread = null;
            }
        }

        public void ShowAlert()
        {
            // Show alert image blinking
            AlertImage.Visibility = System.Windows.Visibility.Collapsed;
            if (_alertShowingTimer == null)
            {
                _alertShowingTimer = new DispatcherTimer();
                _alertShowingTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
                _alertShowingTimer.Tick += new EventHandler(AlertShowingTimer_Tick);
            }
            _alertShowingTimer.Stop();
            _alertShowingCounter = 0;
            _alertShowingTimer.Start();
        }

        private void AlertShowingTimer_Tick(object sender, EventArgs e)
        {
            _alertShowingCounter++;
            if (_alertShowingCounter < 21)
            {
                if (AlertImage.Visibility == System.Windows.Visibility.Collapsed)
                    AlertImage.Visibility = System.Windows.Visibility.Visible;
                else
                    AlertImage.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                AlertImage.Visibility = System.Windows.Visibility.Collapsed;
                (sender as DispatcherTimer).Stop();
            }
        }


        /*
         * Camera started
         * Start reading frames from the camera
         * */
        private void CameraInitialized(object sender, CameraOperationCompletedEventArgs e)
        {
            if (_camera != null)
            {
                System.Diagnostics.Debug.WriteLine("Camera started");
                _camera.FlashMode = FlashMode.Off;

                Dispatcher.BeginInvoke(() =>
                {
                    // Set the orientation of the viewfinder.
                    this.viewfinderBrushTransformation.Angle = _camera.Orientation;
                });

                // Start the background worker thread that processes the camera preview buffer frames.
                _threadPleaseExit = false;
                _thread = new Thread(ReadFramesBackgroundWorker);
                _thread.Start();
            }
        }

        /*
         * Read camera frames in separate thread
         * */
        private void ReadFramesBackgroundWorker()
        {
            var bufferLayout = _camera.YCbCrPixelLayout;
            int[] currentFrame = new int[bufferLayout.RequiredBufferSize];
            while (!_threadPleaseExit)
            {
                // Get the current preview buffer from the camera
                _camera.GetPreviewBufferArgb32(currentFrame);

                // Does previous and current frame differ?
                if (FrameDiffer(currentFrame))
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        if (AlertEvent != null)
                            AlertEvent();
                    });
                }

                // Sleep 
                Thread.Sleep(new TimeSpan(0, 0, 0, 0, 300));
            }
        }

        /*
         * Compares frames. Try to find difference between frames
         * */
        private bool FrameDiffer(int[] currentFrame)
        {
            if (previousFrame != null && previousFrame.Length == currentFrame.Length)
            {
                int redFromCurrentFrame = 0;
                int redFromPrevFrame = 0;
                for (int i = 0; i < currentFrame.Length; i++)
                {
                    redFromCurrentFrame += ((currentFrame[i] >> 0) & 0x000000ff);
                    redFromPrevFrame += ((previousFrame[i] >> 0) & 0x000000ff);
                    //int r = ((currentFrame[i] >> 0) & 0x000000ff);
                    //int g = ((currentFrame[i] >> 8) & 0x000000ff);
                    //int b = ((currentFrame[i] >> 16) & 0x000000ff);
                    //int a = ((currentFrame[i] >> 24) & 0x000000ff);
                }
                
                // Frames differ amount
                int framesDiff = Math.Abs(redFromCurrentFrame - redFromPrevFrame);

                // Activity calculation
                int activity = framesDiff / FRAME_DIFFER_VALUE;
                if (activity < 0)
                    activity = 0;
                else if (activity > 11)
                    activity = 10;

                // Update model
                CameraData.Activity = activity;

                // Is frame enought differ and is activity more that treshold?
                if (framesDiff > CameraData.Treshold * FRAME_DIFFER_VALUE)
                {
                    if (activity >= CameraData.Treshold)
                    {
                        System.Array.Copy(currentFrame, previousFrame, currentFrame.Length);
                        // Different frame
                        return true;
                    }
                }
            }
            else
            {
                previousFrame = new int[currentFrame.Length];
            }
            System.Array.Copy(currentFrame, previousFrame, currentFrame.Length);
            // Same frame
            return false;
        }

    }
}
