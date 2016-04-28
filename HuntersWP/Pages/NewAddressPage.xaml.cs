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
            cmbType.ItemsSource = await new DbService().GetSurveyTypes();
#if DEBUG
            cmbType.SelectedIndex = 0;
            tbBlockUPRN.Text = "1";
            tbBuildingName.Text = "1";
            tbFlatNo.Text = "@#$%&*()-\\\\!;:'\\?/.,?:,^[]{}<>€£¥|~_=+-•.,•_±¬·";
            tbPostCode.Text = "1";
            tbStreetName.Text = @"/?''::!;\\-)(*&%$#@.,-^[]{}<>€£¥|~_=+-••_~¬·±≠_~¦¥£€≥≤}{][√@#¢₹¤‰&*[<]~\\";
            tbStreetNo.Text = "1";
            tbUPRN.Text = "2";
#endif

        }

        private async void ApplicationBarIconButton_OnClick(object sender, EventArgs e)
        {
            if (cmbType.SelectedItem == null) return;

            IsBusy = true;

            var a = new Address();
            a.IsCreatedOnClient = true;
            a.AddressID = Guid.NewGuid().ToString();
            a.UPRN = string.Format("{0}-{1}", tbUPRN.Text,a.AddressID);
            a.FlatNo = tbFlatNo.Text;
            a.Postcode = tbPostCode.Text;
            a.StreetName = tbStreetName.Text;
            a.StreetNo = tbStreetNo.Text;
            a.BlockUPRN = string.Format("{0}-{1}",tbBlockUPRN.Text,a.AddressID);
            a.BuildingName = tbBuildingName.Name;

         
            a.Identity = a.AddressID;
            a.Type = (cmbType.SelectedItem as SurveyTypes).Name;

            a.AddressLine1 = string.Format("{0} {1}, {2}",a.StreetName,a.StreetNo,a.BuildingName);
            

            
            a.CustomerSurveyID = StateService.CurrentCustomer.CustomerSurveyID;
            a.CustomerID = StateService.CurrentCustomer.CustomerID;

            a.Surveyor = StateService.CurrentUserId.ToString();
            a.FullAddress = string.Format("{0}, {1}",a.AddressLine1,a.Type);


            await new DbService().Save(a, ESyncStatus.NotSynced);

            IsBusy = true;

            ExNavigationService.GoBack();
        }
    }
}