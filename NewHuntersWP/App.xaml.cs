using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Navigation;
using Windows.Foundation.Metadata;
using BugSense;
using HuntersWP.Controls;
using HuntersWP.Db;
using HuntersWP.Models;
using HuntersWP.Services;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using HuntersWP.Resources;
using Microsoft.ApplicationInsights;

namespace HuntersWP
{
    public partial class App : Application
    {
        /// <summary>
        /// Allows tracking page views, exceptions and other telemetry through the Microsoft Application Insights service.
        /// </summary>
        public static TelemetryClient TelemetryClient;

        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public static PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            TelemetryClient = new TelemetryClient();

            

            // Global handler for uncaught exceptions.
            UnhandledException += Application_UnhandledException;

            // Standard XAML initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            // Language display initialization
            InitializeLanguage();

            BugSenseHandler.Instance.InitAndStartSession(this, "w8c3c4e1");
            //DevRainErrorHandler.Initialize(this, 20012);

          //  ((SolidColorBrush)App.Current.Resources["PhoneChromeBrush"]).Color = Color.FromArgb(255,245,245,245);
            ((SolidColorBrush)App.Current.Resources["PhoneRadioCheckBoxCheckBrush"]).Color = Color.FromArgb(255, 219, 70, 154);
            ((SolidColorBrush)App.Current.Resources["PhoneForegroundBrush"]).Color = Color.FromArgb(255,100,160,200);



            
            AutoMapper.Mapper.CreateMap<SurvelemMap, ServiceReference.SurvelemMap>();
            AutoMapper.Mapper.CreateMap<ServiceReference.SurvelemMap, SurvelemMap>();
            
            AutoMapper.Mapper.CreateMap<Question, ServiceReference.Question>();
            AutoMapper.Mapper.CreateMap<ServiceReference.Question, Question>();

            AutoMapper.Mapper.CreateMap<Option, ServiceReference.Option>();
            AutoMapper.Mapper.CreateMap<ServiceReference.Option, Option>();
      
            AutoMapper.Mapper.CreateMap<RichMedia, ServiceReference.RichMedia>();
            AutoMapper.Mapper.CreateMap<ServiceReference.RichMedia, RichMedia>();

            AutoMapper.Mapper.CreateMap<SurveyType, ServiceReference.SurveyType>();
            AutoMapper.Mapper.CreateMap<ServiceReference.SurveyType, SurveyType>();

            AutoMapper.Mapper.CreateMap<Customer, ServiceReference.Customer>();
            AutoMapper.Mapper.CreateMap<ServiceReference.Customer, Customer>();

            AutoMapper.Mapper.CreateMap<Address, ServiceReference.Address>();
            AutoMapper.Mapper.CreateMap<ServiceReference.Address, Address>();
            
            AutoMapper.Mapper.CreateMap<Survelem, ServiceReference.Survelem>();
            AutoMapper.Mapper.CreateMap<ServiceReference.Survelem, Survelem>();


            AutoMapper.Mapper.CreateMap<AddressStatus, ServiceReference.AddressStatus>();
            AutoMapper.Mapper.CreateMap<ServiceReference.AddressStatus, AddressStatus>();

            
            //AutoMapper.Mapper.CreateMap<SurvelemSecond, ServiceReference.SurvelemSecond>();
            //AutoMapper.Mapper.CreateMap<ServiceReference.SurvelemSecond, SurvelemSecond>();

            
            AutoMapper.Mapper.CreateMap<QAAddressComment, ServiceReference.QAAddressComment>();
            AutoMapper.Mapper.CreateMap<ServiceReference.QAAddressComment, QAAddressComment>();


            AutoMapper.Mapper.CreateMap<QAAddress, ServiceReference.QAAddress>();
            AutoMapper.Mapper.CreateMap<ServiceReference.QAAddress, QAAddress>();


            AutoMapper.Mapper.CreateMap<AddressQuestionGroupStatus, ServiceReference.AddressQuestionGroupStatus>();
            AutoMapper.Mapper.CreateMap<ServiceReference.AddressQuestionGroupStatus, AddressQuestionGroupStatus>();

            //var url = "http://huntersservicedemo.azurewebsites.net";
            //var url = "http://huntersservicetest.azurewebsites.net";
         //  var url = "http://testhunters.azurewebsites.net";
            var url = "http://huntersdatamigrate.azurewebsites.net";

            StateService.CurrentServiceUri = url;

            // Show graphics profiling information while debugging.
            if (Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                //Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode,
                // which shows areas of a page that are handed off to GPU with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Prevent the screen from turning off while under the debugger by disabling
                // the application's idle detection.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }

        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            BugSenseHandler.Instance.LogException(e.ExceptionObject, "Entity", e.ExceptionObject.GetType().Name);

            Helpers.LogException(e.ExceptionObject, "Application_UnhandledException");
            //new ApiResponse<SystemException>() { IsSuccess = false, Message = "Error while processing request. Try again", Exception = e.ExceptionObject };
            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
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

            // Handle reset requests for clearing the backstack
            RootFrame.Navigated += CheckForResetNavigation;

            new DbService().Initialize();
            StateService.ProgressIndicatorService = new ProgressIndicatorService();

            //test key
            
            

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        ////test
        ////public const string KEY = "ZGoweEpuVTlOVEl6TWpjeU1HWmxOR0l3WWpjMFlUQm1NbVUwT1RsbUptRTlhSFZ1ZEdWeWMyTnZiR3hsWTNSaGNIQW0=";

        ////production
        //public const string KEY = "ZGoweEpuVTlOVEl6TWpjeU1HWmxOR0l3WWpjMFlUQm1NbVUwT1RsbUptRTlhSFZ1ZEdWeWMzZHBibkJvYjI1bEpn";

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        private void CheckForResetNavigation(object sender, NavigationEventArgs e)
        {
            // If the app has received a 'reset' navigation, then we need to check
            // on the next navigation to see if the page stack should be reset
            if (e.NavigationMode == NavigationMode.Reset)
                RootFrame.Navigated += ClearBackStackAfterReset;
        }

        private void ClearBackStackAfterReset(object sender, NavigationEventArgs e)
        {
            // Unregister the event so it doesn't get called again
            RootFrame.Navigated -= ClearBackStackAfterReset;

            // Only clear the stack for 'new' (forward) and 'refresh' navigations
            if (e.NavigationMode != NavigationMode.New && e.NavigationMode != NavigationMode.Refresh)
                return;

            // For UI consistency, clear the entire page stack
            while (RootFrame.RemoveBackEntry() != null)
            {
                ; // do nothing
            }
        }

        #endregion

        // Initialize the app's font and flow direction as defined in its localized resource strings.
        //
        // To ensure that the font of your application is aligned with its supported languages and that the
        // FlowDirection for each of those languages follows its traditional direction, ResourceLanguage
        // and ResourceFlowDirection should be initialized in each resx file to match these values with that
        // file's culture. For example:
        //
        // AppResources.es-ES.resx
        //    ResourceLanguage's value should be "es-ES"
        //    ResourceFlowDirection's value should be "LeftToRight"
        //
        // AppResources.ar-SA.resx
        //     ResourceLanguage's value should be "ar-SA"
        //     ResourceFlowDirection's value should be "RightToLeft"
        //
        // For more info on localizing Windows Phone apps see http://go.microsoft.com/fwlink/?LinkId=262072.
        //

  
        private void InitializeLanguage()
        {
            try
            {
                // Set the font to match the display language defined by the
                // ResourceLanguage resource string for each supported language.
                //
                // Fall back to the font of the neutral language if the Display
                // language of the phone is not supported.
                //
                // If a compiler error is hit then ResourceLanguage is missing from
                // the resource file.
                RootFrame.Language = XmlLanguage.GetLanguage(AppResources.ResourceLanguage);

                // Set the FlowDirection of all elements under the root frame based
                // on the ResourceFlowDirection resource string for each
                // supported language.
                //
                // If a compiler error is hit then ResourceFlowDirection is missing from
                // the resource file.
                FlowDirection flow = (FlowDirection)Enum.Parse(typeof(FlowDirection), AppResources.ResourceFlowDirection);
                RootFrame.FlowDirection = flow;
            }
            catch
            {
                // If an exception is caught here it is most likely due to either
                // ResourceLangauge not being correctly set to a supported language
                // code or ResourceFlowDirection is set to a value other than LeftToRight
                // or RightToLeft.

                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }

                throw;
            }
        }

    
    }

}