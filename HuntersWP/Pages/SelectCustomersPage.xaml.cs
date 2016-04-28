using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using HuntersWP.Db;
using HuntersWP.Models;
using HuntersWP.Services;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace HuntersWP.Pages
{
    public partial class SelectCustomersPage {
        public SelectCustomersPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                ExNavigationService.GoBack();
                return;
            }


            IsBusy = true;
            var customers = await new DbService().GetCustomers();
            IsBusy = false;
            if (customers.Count == 1)
            {
                StateService.CurrentCustomer = customers[0];
                ExNavigationService.Navigate<AdressesPage>();
                return;
            }

            lstCustomers.ItemsSource = customers;
        }

        private void LstCustomers_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;

            var a = (Customer)e.AddedItems[0];

            StateService.CurrentCustomer = a;

            ExNavigationService.Navigate<AdressesPage>();
        }
    }
}