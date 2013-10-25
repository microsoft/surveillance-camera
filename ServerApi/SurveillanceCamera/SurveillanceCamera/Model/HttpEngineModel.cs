/*
 * Copyright (c) 2012 Nokia Corporation.
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

namespace SurveillanceCamera.Model
{
    /*
     * Data class for serialization
     * http://msdn.microsoft.com/en-us/library/ms733127.aspx
     * */
    [DataContract]
    public class HttpEngineModel
    {
        [DataMember]
        public string Realm
        {
            get;
            set;
        }

        [DataMember]
        public string Nonce
        {
            get;
            set;
        }

        [DataMember]
        public string Qop
        {
            get;
            set;
        }

        [DataMember]
        public int Nc
        {
            get;
            set;
        }

        [DataMember]
        public string Cnonce
        {
            get;
            set;
        }

        [DataMember]
        public DateTime CnonceDate
        {
            get;
            set;
        }
    }
}
