/*
 * Copyright (c) 2012-2014 Microsoft Mobile.
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using SurveillanceCamera.Model;

namespace SurveillanceCamera
{
    public partial class App : Application
    {
        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }


        /* *******************************************************************
         * Nokia Notifications API
         * Needed information to connect into service:
         * */
        // Your service id
        public static string ServiceId = "dn.notification.demo.com";
        // Your application id
        public static string AppId = "com.demo.notification.dn";
        // Your service secret
        // For test env
        //public static string ServiceSecret = "8qGib0GElhd/aGmjtjRIZCzg+IPXDRvlYB//M0KVxrM=";
        // For production env
        public static string ServiceSecret = "J3/XyJKjWPnhtqX7deYzLWErUHHRcTN9vADLHWfY93g=";
        // *******************************************************************

        // API for communicate with Nokia Notifications Service
        public static NotifServerApi NotifServerApi
        {
            get;
            set;
        }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions. 
            UnhandledException += Application_UnhandledException;

            // Nokia Notifications API handler
            NotifServerApi = new NotifServerApi();

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            // Disable idle mode for to allow camera to run all the time
            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Application_Launching");
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Application_Activated");
            if (e.IsApplicationInstancePreserved)
            {
                // No need to read state when application activated from dromant state
                return;
            }
            // Application activated from tombstoned state
            // Check to see if the key for the application state data is in the State dictionary.
            RestoreAppState();
        }

        /*
         * Store application global data
         * */
        private void StoreAppState()
        {
            // HttpEngine state
            HttpEngineModel engineData = NotifServerApi.EngineData();
            PhoneApplicationService.Current.State["HttpEngineModel"] = engineData;
        }

        /*
         * Restore application global data
         * */
        private void RestoreAppState()
        {
            if (PhoneApplicationService.Current.State.ContainsKey("HttpEngineModel"))
            {
                if (PhoneApplicationService.Current.State.ContainsKey("HttpEngineData"))
                {
                    // HttpEngine state
                    HttpEngineModel engineData = PhoneApplicationService.Current.State["HttpEngineModel"] as HttpEngineModel;
                    NotifServerApi.RestoreEngineData(engineData);
                }
            }
        }


        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Application_Deactivated");

            // Store application state when it is deactivated
            StoreAppState();
            
            // Note that application state is not stored when it is closed
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Application_Closing");
            // No storing states
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion
    }
}