/*
 * Copyright (c) 2012-2014 Microsoft Mobile.
 * */

using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace SurveillanceCamera.Model
{
    /*
     * Page data class for serialization
     * http://msdn.microsoft.com/en-us/library/ms733127.aspx
     * */
    [DataContract]
    public class MainPageModel : INotifyPropertyChanged
    {
        private bool _oviAccountPopupShowed;
        private double _cameraTreshold;
        private string _oviAccountId="";
        private bool _authenticated;
        private bool _notificationEnabled;
        
        [DataMember]
        public bool NotificationEnabled
        {
            get { return _notificationEnabled; }
            set
            {
                if (_notificationEnabled == value) return;
                _notificationEnabled = value;
                NotifyPropertyChanged("NotificationEnabled");
            }
        }

        [DataMember]
        public bool Authenticated
        {
            get { return _authenticated; }
            set
            {
                if (_authenticated == value) return;
                _authenticated = value;
                NotifyPropertyChanged("Authenticated");
            }
        }

        [DataMember]
        public string OviAccountId
        {
            get { return _oviAccountId; }
            set
            {
                if (_oviAccountId == value) return;
                _oviAccountId = value;
                NotifyPropertyChanged("OviAccountId");
            }
        }

        [DataMember]
        public double CameraThreshold
        {
            get { return _cameraTreshold; }
            set
            {
                if (_cameraTreshold == value) return;
                _cameraTreshold = value;
                NotifyPropertyChanged("CameraThreshold");
            }
        }

        [DataMember]
        public bool OviAccoutPopupShowed
        {
            get { return _oviAccountPopupShowed; }
            set
            {
                if (_oviAccountPopupShowed == value) return;
                _oviAccountPopupShowed = value;
                NotifyPropertyChanged("OviAccoutPopupShowed");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
