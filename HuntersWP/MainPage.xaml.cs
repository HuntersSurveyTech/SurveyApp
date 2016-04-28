using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using HuntersWP.Pages;
using HuntersWP.Services;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using HuntersWP.Resources;
using Netmera;

namespace HuntersWP
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (StateService.CurrentUserId > 0 && false)
            {
                ExNavigationService.Navigate<AdressesPage>();
            }
            else
            {
                ExNavigationService.Navigate<LoginPage>();
            }


        }

    }
}