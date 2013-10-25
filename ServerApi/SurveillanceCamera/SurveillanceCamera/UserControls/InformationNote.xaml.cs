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
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using System.ComponentModel;

namespace SurveillanceCamera.UserControls
{

    /*
     * Animated information note
     * */
    public partial class InformationNote : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {
        private Popup _popupForNote = null;
        private DispatcherTimer _timer = null;
        Storyboard _storyBoard = new Storyboard();

        private int _animCounter = 2;

        static private InformationNote _staticNote = null;

        public bool DoNotHide { get; set; }

        /*
         * Constructor
         * */
        public InformationNote() 
        {
            InitializeComponent();
            _popupForNote = new Popup();
            _popupForNote.HorizontalOffset = 0;
            _popupForNote.VerticalOffset = -100;
            _popupForNote.Child = this;
            _popupForNote.IsOpen = false;
            _popupForNote.DataContext = this;

            _storyBoard.Completed += new EventHandler(StoryBoard_Completed);
            this.LayoutRoot.Resources.Add("myStoryboard", _storyBoard);
        }

        private string _message;
        public string Message
        {
            get { return _message; }
            set
            {
                if (_message == value) return;
                _message = value;
                NotifyPropertyChanged("Message");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }


        /*
         * Shows the message
         * */
        public void ShowNote(string message)
        {
            if (!IsBusy())
            {
                StopShowing();
            }

            this.Width = App.Current.Host.Content.ActualWidth;
            Message = message;
            _popupForNote.HorizontalOffset = 0;
            _popupForNote.VerticalOffset = -100;
            _popupForNote.IsOpen = true;

            DoNotHide = false;

            ShowAnimation();
        }

        private void ShowStaticNote(string message)
        {
            ShowNote(message);
            DoNotHide = true;
        }

        private void StopShowing()
        {
            _storyBoard.Stop();
            _storyBoard.Children.Clear();

            if (_timer != null)
                _timer.Stop();

            _popupForNote.IsOpen = false;
        }

        /*
         * Starts animation
         * */
        public void ShowAnimation()
        {
            Duration duration = new Duration(TimeSpan.FromSeconds(0.5)); 
            _storyBoard.Duration = duration;

            DoubleAnimation doubleAnimation = new DoubleAnimation();
            doubleAnimation.From = -100;
            doubleAnimation.To = 0;
            doubleAnimation.Duration = duration;

            _storyBoard.Stop();
            _storyBoard.Children.Clear();
            _storyBoard.Children.Add(doubleAnimation);
            Storyboard.SetTarget(doubleAnimation, _popupForNote);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(Popup.VerticalOffsetProperty));

            _animCounter = 2;

            _storyBoard.Begin();
        }

        /*
         * Hides messages
         * */
        private void HideAnimation()
        {
            Duration duration = new Duration(TimeSpan.FromSeconds(0.5));
            _storyBoard.Duration = duration;

            DoubleAnimation doubleAnimation = new DoubleAnimation();
            doubleAnimation.From = 0;
            doubleAnimation.To = -100;
            doubleAnimation.Duration = duration;

            _storyBoard.Stop();
            _storyBoard.Children.Clear();
            _storyBoard.Children.Add(doubleAnimation);
            Storyboard.SetTarget(doubleAnimation, _popupForNote);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(Popup.VerticalOffsetProperty));

            _storyBoard.Begin();
        }

        /*
         * Animation completes
         * */
        private void StoryBoard_Completed(object sender, EventArgs e)
        {
            if (DoNotHide)
                return;
            
            _animCounter--;

            if (_animCounter == 1)
            {
                if (_timer != null)
                {
                    _timer.Stop();
                }
                else
                {
                    _timer = new DispatcherTimer();
                    _timer.Interval = new TimeSpan(0, 0, 4);
                    _timer.Tick += new EventHandler(WaitTimer_Tick);
                }
                _timer.Start();
            }
            else
            {
                HideNote();
            }


        }

        private bool IsBusy()
        {
            if (_timer != null || (_popupForNote != null && _popupForNote.IsOpen))
                return true;
            else
                return false;
        }

        /*
         * Animation sleep timer completes
         * */
        private void WaitTimer_Tick(object o, EventArgs sender)
        {
            if (_timer != null)
                _timer.Stop();

            HideAnimation();
        }

        /*
         * Hide note without animation
         * */
        public void HideNote()
        {
            _popupForNote.IsOpen = false;
        }

        /*
         * Show message
         * */
        static public void ShowMessage(string message)
        {
            if (_staticNote==null)
                _staticNote = new InformationNote();
            _staticNote.ShowNote(message);
        }

        /*
         * Show message all the time
         * */
        static public void ShowStaticMessage(string message)
        {
            if (_staticNote == null)
                _staticNote = new InformationNote();
            _staticNote.ShowStaticNote(message);
        }

        /*
         * Updates message text
         * */
        static public void UpdateMessageText(string newMessage)
        {
            if (_staticNote != null)
                _staticNote.Message = newMessage;
        }

        /*
         * Hide message
         * */
        static public void HideMessage()
        {
            if (_staticNote != null)
                _staticNote.HideNote();
        }

    }
}
