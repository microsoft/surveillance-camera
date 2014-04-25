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

namespace SurveillanceCamera.Model
{
    /*
     * Page data class for serialization
     * http://msdn.microsoft.com/en-us/library/ms733127.aspx
     * */
    [DataContract]
    public class InfoPageModel
    {
        [DataMember]
        public double ScrollViewerPosition
        {
            get;
            set;
        }
    }
}
