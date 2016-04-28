using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using HuntersWP.Db;
using HuntersWP.Models;
using HuntersWP.Services;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace HuntersWP.Pages
{
    public partial class NewAddressPage 
    {
        public NewAddressPage()
        {
            InitializeComponent();

            Load();
        }

        void tb_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                newAddressPage.Focus();
            }
        }

        async void Load()
        {
            cmbType.ItemsSource = await new DbService().GetSurveyTypes(StateService.CurrentCustomer.CustomerSurveyID);
#if DEBUG
            cmbType.SelectedIndex = 0;
            tbUPRN.Text = "1";
            tbAddress.Text = "2";
#endif

        }

        private async void ApplicationBarIconButton_OnClick(object sender, EventArgs e)
        {
            if (cmbType.SelectedItem == null) return;

            IsBusy = true;

            var a = new Address();
            a.IsCreatedOnClient = true;
            a.UPRN = string.Format("{0}-{1}", tbUPRN.Text,a.Id);
            a.AddressLine1 = tbAddress.Text;
         
            a.Type = (cmbType.SelectedItem as SurveyType).Name;

            

            
            a.CustomerSurveyID = StateService.CurrentCustomer.CustomerSurveyID;
            a.CustomerID = StateService.CurrentCustomer.CustomerID;

            a.SurveyorId = StateService.CurrentUserId;
            a.FullAddress = string.Format("{0}, {1}",a.AddressLine1,a.Type);

            if (!StateService.IsQA)
                await new DbService().Save(a, ESyncStatus.NotSynced);

            IsBusy = true;

            ExNavigationService.GoBack();
        }
    }
}