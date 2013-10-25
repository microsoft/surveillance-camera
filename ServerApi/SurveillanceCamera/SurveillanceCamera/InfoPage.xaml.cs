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
   
    public partial class InfoPage : PhoneApplicationPage
    {
        public InfoPage()
        {
            InitializeComponent();
            /*

            infoText.Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aenean sed dapibus tellus. Phasellus at dictum lorem. Aenean ante lacus, volutpat quis blandit ut, tincidunt vitae turpis. Maecenas vel aliquam arcu. Cras vitae varius tellus. Duis orci est, venenatis ac lacinia a, congue a mauris. Mauris interdum, lorem a blandit bibendum, arcu magna faucibus leo, a ultrices mi sem sit amet arcu. Cras porttitor nisl ut elit dictum vehicula."+
            "\n"+
            "\n" +
            "Suspendisse sodales tellus in eros venenatis pellentesque. Nunc tellus dolor, ullamcorper at vestibulum pharetra, suscipit vitae tellus. Cras at libero in purus luctus tincidunt. Aenean ornare, odio a accumsan congue, ligula dolor placerat diam, in fringilla nisi tortor id enim. Phasellus fringilla tempor faucibus. Mauris a velit vitae ante ornare iaculis. Praesent lacinia interdum gravida. Ut in erat ut dui facilisis auctor. Integer eget lacus ligula, non porttitor purus. Curabitur scelerisque porttitor orci, a varius elit vulputate quis. Vivamus quis erat nec lacus posuere malesuada non at felis. Quisque feugiat gravida justo." +
            "\n" +
            "\n" +
            "Maecenas augue nisl, mollis vel congue non, lacinia a ante. Cras vel tincidunt erat. Ut massa metus, dictum eu varius vel, egestas quis justo. Sed sit amet consequat tortor. Maecenas in purus eros, quis mollis quam. Aliquam vel orci velit. Donec sit amet massa eu ligula laoreet elementum. Pellentesque eu ipsum risus. Fusce placerat adipiscing nunc, quis gravida elit volutpat vel. Sed tempor mi at erat molestie sed convallis diam congue." +
            "\n" +
            "\n" +
            "In elementum, neque nec ullamcorper placerat, lacus dui condimentum magna, quis lobortis arcu dui id metus. Maecenas dictum lectus at orci vestibulum sed dapibus nulla mattis. Nulla vehicula sollicitudin molestie. Nulla ullamcorper eros sed sapien mattis facilisis. Integer est felis, pulvinar eget condimentum non, placerat in elit. Suspendisse potenti. Nullam ut tellus libero. Donec quis libero in ipsum auctor aliquet congue id mauris. Mauris mollis libero in enim molestie mollis. Maecenas vitae mi feugiat tortor gravida egestas nec id purus. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos." +
            "\n" +
            "\n" +
            "Aenean iaculis arcu in urna tincidunt congue. Sed dictum viverra tortor, non volutpat turpis aliquam id. Curabitur bibendum, mauris id vulputate commodo, libero leo aliquam sapien, et blandit est lectus eu purus. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Suspendisse placerat turpis sed lectus faucibus quis semper velit pharetra. Integer elit sem, interdum quis sodales ut, mollis non nisi. Donec suscipit pulvinar adipiscing.";
            */

            /*
            infoText.Text += "Surveillance Camera for Silverlight and Qt demonstrates the usage of Nokia's Notifications API. ";
            infoText.Text += "The Notifications API lets you add real-time push notifications to your client applications. ";
            infoText.Text += "Windows Phone application uses Service API to send push notifications to Symbian device. ";
            infoText.Text += "The Windows Phone side of the example is implemented using Silverlight and the Symbian side of ";
            infoText.Text += "the example is implemented using Qt Quick.";
            */




        }

        override protected void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (State.ContainsKey("InfoPageModel"))
            {
                InfoPageModel pageData = (InfoPageModel)State["InfoPageModel"];
                    
                // TODO: Bug, does not scroll to given position
                this.scrollViewer.UpdateLayout();
                this.scrollViewer.ScrollToVerticalOffset(pageData.ScrollViewerPosition);

                State.Remove("InfoPageModel");
            }

            base.OnNavigatedTo(e);
        }

        override protected void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            // If this is a back navigation, the page will be discarded, so there
            // is no need to save state.
            if (e.NavigationMode != System.Windows.Navigation.NavigationMode.Back)
            {
                InfoPageModel pageData = new InfoPageModel();
                pageData.ScrollViewerPosition = this.scrollViewer.VerticalOffset;
                State["InfoPageModel"] = pageData;
            }

            base.OnNavigatingFrom(e);
        }

    }
}