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
using System.Windows.Controls.Primitives;
using System.IO.IsolatedStorage;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System.Windows.Resources;
using System.Windows.Threading;
using System.Threading;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Phone.Shell;
using System.Windows.Data;

using SurveillanceCamera.Model;
using SurveillanceCamera.UserControls;
using System.Windows.Media.Imaging;


namespace SurveillanceCamera
{

    public partial class MainPage : PhoneApplicationPage
    {
        // Nokia Notifications API
        private NotifServerApi _notifServerApi = null;

        // Popup control
        private OviAccountId _oviAccoutPopupContent = null;

        // Screensaver
        private ScreenSaver _screensaverControl = null;

        // Sounds
        private SoundEffect _alarmSound = null;
        private SoundEffect _waitAlarmSound = null;
        private SoundEffect _radioSound = null;
        private SoundEffect _radioSound2 = null;
        private SoundEffect _radioSound3 = null;
        private SoundEffect _radioSound4 = null;
        private DispatcherTimer _radioTimer = null;
        private int _radioSoundIndex = 0;

        private DispatcherTimer _nextAlarmDelayTimer = null;
        private int _nextAlarmDelayTicks = 50;

        private DispatcherTimer _startSeekingDelayTimer = null;
        private int _startSeekingIndex = 3;


        private double _activityOldValue = 0;

        // Alarm history model
        public Logs LogsModel
        {
            get;
            set;
        }

        // This page model
        public MainPageModel MainPageModel
        {
            get;
            set;
        }

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            LogsModel = new Logs();
            MainPageModel = new MainPageModel();

            _notifServerApi = App.NotifServerApi;

            SetNotificationEnabled(false);
            UiStateToStopped(true);

            // DataContexts
            this.ActivitySlider.DataContext = CameraViewfinder.CameraData;
            this.ActivitySlider.Style = Application.Current.Resources["CustomSliderStyle_Yellow"] as Style;
            this.CameraTresholdSlider.DataContext = CameraViewfinder.CameraData;

            // Custom screensaver
            _screensaverControl = new ScreenSaver(this.LayoutRoot, this.ApplicationBar);

            // Read Ovi accout id from isolated storage
            string usernameFromStore;
            if (ReadUsername(out usernameFromStore))
                MainPageModel.OviAccountId = usernameFromStore;

            // Load sounds
            StreamResourceInfo alarmStream = Application.GetResourceStream(
                new Uri("SoundFiles/26173__wim__sirene-06080401.wav", UriKind.RelativeOrAbsolute));
            _alarmSound = SoundEffect.FromStream(alarmStream.Stream);

            StreamResourceInfo waitAlarmSound = Application.GetResourceStream(
                new Uri("SoundFiles/31841__hardpcm__chip001.wav", UriKind.RelativeOrAbsolute));
            _waitAlarmSound = SoundEffect.FromStream(waitAlarmSound.Stream);

            StreamResourceInfo radioStream = Application.GetResourceStream(
                new Uri("SoundFiles/30335__erh__radio-noise-2.wav", UriKind.RelativeOrAbsolute));
            _radioSound = SoundEffect.FromStream(radioStream.Stream);

            StreamResourceInfo radioStream2 = Application.GetResourceStream(
                new Uri("SoundFiles/30623__erh__do-it-now-2.wav", UriKind.RelativeOrAbsolute));
            _radioSound2 = SoundEffect.FromStream(radioStream2.Stream);

            StreamResourceInfo radioStream3 = Application.GetResourceStream(
                new Uri("SoundFiles/27878__inequation__walkietalkie-eot.wav", UriKind.RelativeOrAbsolute));
            _radioSound3 = SoundEffect.FromStream(radioStream3.Stream);

            StreamResourceInfo radioStream4 = Application.GetResourceStream(
                new Uri("SoundFiles/34383__erh__walk-away.wav", UriKind.RelativeOrAbsolute));
            _radioSound4 = SoundEffect.FromStream(radioStream4.Stream);
        }

        /*
         * Store page state
         * */
        private void StorePageState()
        {
            MainPageModel.CameraThreshold = CameraViewfinder.CameraData.Treshold;
            State["MainPageModel"] = MainPageModel;
        }

        /*
         * Restore page state
         * */
        private void RestorePageState()
        {
            if (State.ContainsKey("MainPageModel"))
            {
                MainPageModel = (MainPageModel)State["MainPageModel"];
                CameraViewfinder.CameraData.Treshold = MainPageModel.CameraThreshold;
                this.CameraTresholdSlider.Value = MainPageModel.CameraThreshold;

                SetNotificationEnabled(MainPageModel.NotificationEnabled);
                UiStateToStopped(!MainPageModel.NotificationEnabled);

                State.Remove("MainPageModel");
            }
        }


        /*
         * Radio sound playing timer tick
         * */
        private void RadioTimer_Tick(object o, EventArgs sender)
        {
            _radioSoundIndex++;

            FrameworkDispatcher.Update();

            if (_radioSoundIndex == 1)
                _radioSound.Play();
            else if (_radioSoundIndex == 2)
                _radioSound2.Play();
            else if (_radioSoundIndex == 3)
                _radioSound3.Play();
            else if (_radioSoundIndex == 4)
                _radioSound4.Play();

            if (_radioSoundIndex >= 4)
                _radioSoundIndex = 0;

        }

        /*
         * Camera noticed movement. Show alert
         * */
        private void CameraViewfinder_AlertEvent()
        {
            if (MainPageModel.NotificationEnabled)
            {
                if (_nextAlarmDelayTimer == null)
                {
                    // Play alert
                    FrameworkDispatcher.Update();
                    _alarmSound.Play();

                    // Show alert user control
                    this.CameraViewfinder.ShowAlert();

                    // Send "Alert" notification into service
                    string message = "ALERT " + LogsModel.GetNow();
                    SendNotification(message);

                    // Count down to time for allowing next alarm sending
                    _nextAlarmDelayTicks = (int)this.ActivitySlider.Maximum;
                    _nextAlarmDelayTimer = new DispatcherTimer();
                    _nextAlarmDelayTimer.Interval = new TimeSpan(0, 0, 0, 0, 10 * 1000 / _nextAlarmDelayTicks);
                    _nextAlarmDelayTimer.Tick += new EventHandler(AlarmDelay_Tick);

                    // Update slider style to Orange
                    this.ActivitySlider.Style = Application.Current.Resources["CustomSliderStyle_Orange"] as Style;

                    // Remove DataContext from ActivitySlider
                    this.ActivitySlider.DataContext = null;
                    this.ActivitySlider.Value = this.ActivitySlider.Maximum;

                    // Store alarm to history log
                    LogsModel.AddAlarmToLog();

                    _nextAlarmDelayTimer.Start();
                }
            }
        }

        /*
         * Count down to time for allowing next alarm sending
         * */
        private void AlarmDelay_Tick(object sender, EventArgs e)
        {
            _nextAlarmDelayTicks--;

            // Change ActivitySlider, there is no binding enabled
            this.ActivitySlider.Value -= 1;

            if (_nextAlarmDelayTicks <= 1)
            {
                // Delay went
                if (_nextAlarmDelayTimer != null)
                    _nextAlarmDelayTimer.Stop();
                _nextAlarmDelayTimer = null; // Do not comment this

                // Enable DataContext back
                this.ActivitySlider.DataContext = CameraViewfinder.CameraData;
                // Update slider style back to Yellow
                this.ActivitySlider.Style = Application.Current.Resources["CustomSliderStyle_Yellow"] as Style;
            }
        }

        /*
         * Open xaml page
         * */
        override protected void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            // Try to restore page state
            RestorePageState();

            // Handle events
            _notifServerApi.RequestDoneEvent += new NotifServerApi.RequestDoneDelegate(RequestDoneEvent);
            CameraViewfinder.AlertEvent += new CameraViewfinder.Alert(CameraViewfinder_AlertEvent);

            // Radio sound playing timer
            _radioTimer = new DispatcherTimer();
            _radioTimer.Interval = new TimeSpan(0, 0, 30);
            _radioTimer.Tick += new EventHandler(RadioTimer_Tick);
            _radioTimer.Start();

            // Authenticate user into Service if needed
            if (!MainPageModel.Authenticated && !MainPageModel.OviAccoutPopupShowed)
            {
                // Open login popup
                ShowOviAccountPopup();
                MainPageModel.OviAccoutPopupShowed = true;
            }
            else
            {
                _screensaverControl.StartScreenSaverTimer();
            }

            // Start camera
            EnableCamera(true);

            base.OnNavigatedTo(e);
        }

        /*
         * Leave xaml page
         * */
        override protected void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            _screensaverControl.StopScreenSaverTimer();

            // Disable notification
            SetNotificationEnabled(false);
            UiStateToStopped(true);

            // If this is a back navigation, the page will be discarded, so there
            // is no need to save state.
            if (e.NavigationMode != System.Windows.Navigation.NavigationMode.Back)
            {
                StorePageState();
            }

            // Stop monitoring countdown timer
            if (_startSeekingDelayTimer != null)
                _startSeekingDelayTimer.Stop();
            InformationNote.HideMessage();

            // Disable camera
            EnableCamera(false);

            // Stop radio playing loop
            if (_radioTimer != null)
                _radioTimer.Stop();
            _radioTimer = null;

            // Free events handling
            CameraViewfinder.AlertEvent -= new CameraViewfinder.Alert(CameraViewfinder_AlertEvent);
            _notifServerApi.RequestDoneEvent -= new NotifServerApi.RequestDoneDelegate(RequestDoneEvent);

            base.OnNavigatingFrom(e);
        }

        /*
         * Handle device Back key press
         * */
        override protected void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            // If Ovi account popup is open, cancel BackKey navigation
            // and close only the popup
            if (_oviAccoutPopupContent != null && _oviAccoutPopupContent.IsVisible())
            {
                e.Cancel = true;
                _oviAccoutPopupContent.Hide();
                OviUsernameReceivedEvent("", false);
            }
            base.OnBackKeyPress(e);
        }


        /*
         * Show popup for asking Ovi account id from the user
         * */
        private void ShowOviAccountPopup()
        {
            _screensaverControl.StopScreenSaverTimer();
            
            if (_oviAccoutPopupContent == null)
            {
                _oviAccoutPopupContent = new OviAccountId(App.Current.Host.Content.ActualWidth, 
                    App.Current.Host.Content.ActualHeight, 
                    MainPageModel.OviAccountId);
                _oviAccoutPopupContent.OviUsernameReceivedEvent += new OviAccountId.OviUsernameReceived(OviUsernameReceivedEvent);
            }
            _oviAccoutPopupContent.SetDefaulUsername(MainPageModel.OviAccountId);
            this.ApplicationBar.IsVisible = false;
            _oviAccoutPopupContent.Show();
        }

        /*
         * Ovi account id reveiced from popup
         * */
        private void OviUsernameReceivedEvent(string username, bool save)
        {
            this.ApplicationBar.IsVisible = true;

            _screensaverControl.StartScreenSaverTimer();

            MainPageModel.OviAccountId = username;

            // Account was no given
            if (MainPageModel.OviAccountId.Length < 1)
            {
                MainPageModel.Authenticated = false;
                // Disable notification
                SetNotificationEnabled(false);
                UiStateToStopped(true);
                // Store username into isolated storage if wanted
                StoreUsername(MainPageModel.OviAccountId, save);
                InformationNote.ShowMessage("No Nokia Account ID given. Cannot send notifications.");
                return;
            }

            // Add "@ovi.com" at the end of account
            if (username.Contains("@"))
            {
                MainPageModel.OviAccountId = username.Remove(username.IndexOf("@"));
                MainPageModel.OviAccountId = MainPageModel.OviAccountId+"@ovi.com";
            }
            else
            {
                MainPageModel.OviAccountId = MainPageModel.OviAccountId+"@ovi.com";
            }

            // Store username into isolated storage if wanted
            StoreUsername(MainPageModel.OviAccountId, save);

            // Login into Notification Service if network exists
            if (isNetwork())
            {
                // Make HTTPS digest authentication into service
                if (!MainPageModel.Authenticated)
                    AuthenticateByPing();
            }
            else
            {
                ShowNoNetwork();
            }
        }

        /*
         * Application does not have GSM or WLAN network.
         * Show message about that and disable notification sending
         * Request to authenticate
         * */
        private void ShowNoNetwork()
        {
            InformationNote.ShowMessage("No network. Enable network and try again");

            // Request new authentication
            MainPageModel.Authenticated = false;

            // Disable notification
            SetNotificationEnabled(false);
            UiStateToStopped(true);
        }

        /*
         * Make HTTPS digest authentication into Notifications service
         * */
        private void AuthenticateByPing()
        {
            InformationNote.ShowStaticMessage("Connecting...");
            _notifServerApi.RequestPing(App.ServiceId, App.ServiceSecret, NotifServerApi.PING_URI);
        }

        /*
         * Send notification into service
         * */
        private void SendNotification(string notificationMsg)
        {
            _notifServerApi.RequestSendNotification(
                App.ServiceId,
                App.ServiceSecret,
                NotifServerApi.NOTIF_URI + MainPageModel.OviAccountId,
                App.AppId,
                notificationMsg);
        }


        /*
         * HTTPS request done. Handle response
         * */
        private void RequestDoneEvent(RequestState requestState)
        {
            Dispatcher.BeginInvoke(() =>
            {
                switch (requestState.HttpEngineState)
                {
                    case HttpEngineState.EAuthenticate:
                        {
                            if (requestState.RequestPassed)
                            {
                                // Authenticated first time
                                if (!MainPageModel.Authenticated)
                                {
                                    if (_startSeekingDelayTimer == null || !_startSeekingDelayTimer.IsEnabled)
                                        InformationNote.ShowMessage("Connected");
                                }
                                MainPageModel.Authenticated = true;
                            }
                            break;
                        }
                    case HttpEngineState.ESendNotification:
                        {
                            if (requestState.RequestPassed)
                                InformationNote.ShowMessage("Alert sent");
                            else
                                InformationNote.ShowMessage("Sending alert failed!");
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }

                // General error
                if (!requestState.RequestPassed)
                {
                    ShowNoNetwork();
                }

            });
        }

        /*
         * Enable or disable camera
         * */
        private void EnableCamera(bool enable)
        {
            if (enable)
                CameraViewfinder.StartCamera();
            else
                CameraViewfinder.StopCamera();
        }

        /*
         * Store username into isolated storage
         * */
        private void StoreUsername(string username, bool save)
        {
            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
            settings.Remove("ovi_username");
            if (save)
                settings.Add("ovi_username", username);
            settings.Save();
        }

        /*
         * Read username from isolated storage
         * */
        private bool ReadUsername(out string username)
        {
            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
            return settings.TryGetValue<string>("ovi_username", out username);
        }

        /*
         * Camera treshold slider changed
         * */
       private void CameraTresholdSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
       {
            double value = e.NewValue;
            if (CameraViewfinder != null)
                CameraViewfinder.CameraData.Treshold = (int)value;
        }


        /*
         * Does device have network connection enabled?
         * */
        private bool isNetwork()
        {
            return NetworkInterface.GetIsNetworkAvailable();
        }

        /*
         * Show info page
         * */
        private void ApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/InfoPage.xaml", UriKind.RelativeOrAbsolute));
        }

        /*
         * Clear logs
         * */
        private void ApplicationBarMenuItem_Click_1(object sender, EventArgs e)
        {
            LogsModel.ClearLogs();
        }

        /*
         * Show log
         * */
        private void ApplicationBarMenuItem_Click_2(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AlarmHistoryPage.xaml", UriKind.RelativeOrAbsolute));
        }

        /*
         * Play or Stop icon
         * */
        private void ShowPlayImage(bool show)
        {
            BitmapImage myImage = new BitmapImage();
            string imagePath;
            if (show)
                imagePath = "/Images/play.png";
            else
                imagePath = "/Images/Stop.png";
            myImage.UriSource = new Uri(imagePath, UriKind.RelativeOrAbsolute);
            isMonitoringImage.Source = myImage;
        }

        /*
         * Stop monitoring
         * */
        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_startSeekingDelayTimer != null)
                _startSeekingDelayTimer.Stop();
            InformationNote.HideMessage();

            SetNotificationEnabled(false);
            UiStateToStopped(true);
        }

        /*
         * Start monitoring
         * Count down from 3 to 0 before starting monitoring
         * */
        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            bool allowStart = true;

            // Network and Ovi account id exists?
            if (isNetwork())
            {
                // Ovi account exists?
                if (MainPageModel.OviAccountId.Length < 1)
                {
                    ShowOviAccountPopup();
                    InformationNote.ShowMessage("Enter Nokia Account ID of the receiving user and try again.");
                    allowStart = false;
                }
                // Authenticated?
                else if (!MainPageModel.Authenticated)
                {
                    AuthenticateByPing();
                }
            }
            else
            {
                // No network!
                ShowNoNetwork();
                allowStart = false;
            }

            if (!allowStart)
                return;

            FrameworkDispatcher.Update();
            _radioSound2.Play();

            // Start countdown
            if (_startSeekingDelayTimer != null)
            {
                _startSeekingDelayTimer.Stop();
            }
            else
            {
                _startSeekingDelayTimer = new DispatcherTimer();
                _startSeekingDelayTimer.Interval = new TimeSpan(0, 0, 1);
                _startSeekingDelayTimer.Tick += new EventHandler(SeekingDelayTimer_Tick);
            }
            _startSeekingIndex = 3;
            _startSeekingDelayTimer.Start();

            string message = String.Format("Monitoring starts in {0}...", _startSeekingIndex);
            InformationNote.ShowStaticMessage(message);
            UiStateToStopped(false);
        }

        /*
         * Countdown from 3 to 0 before starting monitoring
         * */
        private void SeekingDelayTimer_Tick(object sender, EventArgs e)
        {
            _startSeekingIndex--;

            string message = String.Format("Monitoring starts in {0}...", _startSeekingIndex);
            InformationNote.UpdateMessageText(message);

            if (_startSeekingIndex <= 0)
            {
                _startSeekingDelayTimer.Stop();

                InformationNote.HideMessage();
                SetNotificationEnabled(true);

                InformationNote.ShowMessage("Monitoring");
            }
        }


        /*
         * Open Nokia account popup
         * */
        private void ApplicationBarMenuItem_Click_3(object sender, EventArgs e)
        {
            ShowOviAccountPopup();
        }

        /*
         * Notification sending enabled
         * */
        private void SetNotificationEnabled(bool enabled)
        {
            if (enabled && MainPageModel.Authenticated)
            {
                MainPageModel.NotificationEnabled = true;
            }
            else
            {
                MainPageModel.NotificationEnabled = false;
            }
        }

        /*
         * UI states for STOP and PLAY
         * */
        private void UiStateToStopped(bool stopped)
        {
            if (stopped)
            {
                // Stop bnt disabled
                ShowPlayImage(false);
                this.StopBtn.IsEnabled = false;
                this.StartBtn.IsEnabled = true;
            }
            else
            {
                // Stop btn enabled
                ShowPlayImage(true);
                this.StopBtn.IsEnabled = true;
                this.StartBtn.IsEnabled = false;
            }
        }

        /*
         * Updata slider color
         * */
        private void ActivitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // If 10s alarm countdown in running do not change slider color here
            if (_nextAlarmDelayTimer != null)
                return;

            Slider slider = sender as Slider;
            if (slider == null || CameraTresholdSlider == null)
                return;

            // Ignore duplicate values
            if (_activityOldValue == slider.Value)
                return;

            _activityOldValue = slider.Value;

            double fixedValue = slider.Value / CameraData.MULTIPLIER;
            if (fixedValue >= CameraTresholdSlider.Value)
            {
                // Update slider style to Orange
                this.ActivitySlider.Style = Application.Current.Resources["CustomSliderStyle_Orange"] as Style;
            }
            else
            {
                // Update slider style to Yellow
                this.ActivitySlider.Style = Application.Current.Resources["CustomSliderStyle_Yellow"] as Style;
            }
        }


    }
}