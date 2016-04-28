using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public partial class CopyToAdressesPage
    {
        public CopyToAdressesPage()
        {
            InitializeComponent();
        }

        private List<Address> _allAddresses;
        private Address _address;
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            IsBusy = true;

            var addressId = NavigationContext.QueryString["addressId"];

            _address = await new DbService().Find<Address>(addressId);

            var addresses = await new DbService().GetAddressesForCopyTo(_address.Type);

            _allAddresses = new List<Address>(addresses);

            lstAdresses.ItemsSource = addresses;


            IsBusy = false;
        }

        void Filter()
        {
            var s = tbSearch.Text;

            if (string.IsNullOrEmpty(s))
            {
                lstAdresses.ItemsSource = _allAddresses;
            }
            else
            {
                var adresses = new List<Address>(lstAdresses.ItemsSource as List<Address>);

                lstAdresses.ItemsSource = adresses.Where(x => x.FullAddress.ToUpper().Contains(s.ToUpper())).ToList();

            }
        }

        private void TbSearch_OnKeyUp(object sender, KeyEventArgs e)
        {
            Filter();
        }



        private async void ApplicationBarIconButton_OnClick(object sender, EventArgs e)
        {
            var items = lstAdresses.ItemsSource as List<Address>;
            var selected = items.Where(x => x.IsSelected).ToList();


            if (selected.Count == 0)
            {
                MessageBox.Show("No addresses selected");
                return;
            }

            if (IsBusy) return;

            IsBusy = true;

            StateService.ProgressIndicatorService.Show("Processing");


            var survelems = await new DbService().GetSurvelemsByAddressUPRN(_address.UPRN);


            List<string> notCompletedGroups = new List<string>();
            foreach (var a in selected)
            {
                bool canAdressComplete = true;
                foreach (var o in survelems)
                {

                    var q = await new DbService().FindQuestion(o.Question_Ref);

                    if (q != null)
                    {
                        if (q.ExcludeFromClone)
                        {
                            canAdressComplete = false;
                            notCompletedGroups.Add(q.Main_Element);
                            continue;
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Not found question: " + o.Question_Ref);
                    }
                    var n = new Survelem() { id = Guid.NewGuid().ToString(),IsCreatedOnClient = true};
                    n.BuildingType = o.BuildingType;
                    n.COMMENT = o.COMMENT;
                    n.CustomerID = o.CustomerID;
                    n.CustomerSurveyID = o.CustomerSurveyID;
                    n.DateOfSurvey = o.DateOfSurvey;
                    n.Freetext = o.Freetext;
                    n.OptionID = o.OptionID;
                    n.OptionID2ndry = o.OptionID2ndry;
                    n.Question_Ref = o.Question_Ref;
                    n.SpotPriceFF = o.SpotPriceFF;
                   

                    n.SqN1 = o.SqN1;
                    n.SqN10 = o.SqN10;
                    n.SqN11 = o.SqN11;
                    n.SqN12 = o.SqN12;
                    n.SqN13 = o.SqN13;
                    n.SqN14 = o.SqN14;
                    n.SqN15 = o.SqN15;
                    n.SqN2 = o.SqN2;
                    n.SqN3 = o.SqN3;
                    n.SqN4 = o.SqN4;
                    n.SqN5 = o.SqN5;
                    n.SqN6 = o.SqN6;
                    n.SqN7 = o.SqN7;
                    n.SqN8 = o.SqN8;
                    n.SqN9 = o.SqN9;
                    
                    n.SqT1 = o.SqT1;
                    n.SqT10 = o.SqT10;
                    n.SqT11 = o.SqT11;
                    n.SqT12 = o.SqT12;
                    n.SqT13 = o.SqT13;
                    n.SqT14 = o.SqT14;
                    n.SqT15 = o.SqT15;
                    n.SqT2 = o.SqT2;
                    n.SqT3 = o.SqT3;
                    n.SqT4 = o.SqT4;
                    n.SqT5 = o.SqT5;
                    n.SqT6 = o.SqT6;
                    n.SqT7 = o.SqT7;
                    n.SqT8 = o.SqT8;
                    n.SqT9 = o.SqT9;


                    n.Identity = n.id;
                    n.UPRN = a.UPRN;

                    await new DbService().Save(n, ESyncStatus.NotSynced);

                    if(q !=null && !notCompletedGroups.Contains(q.Main_Element))
                        new ApplicationSettingsService().SetSetting(a.UPRN + "." + q.Main_Element, true);
                }

                if(canAdressComplete)
                    a.Complete = true;

                a.CopiedFrom = _address.UPRN;

                await new DbService().Save(a, ESyncStatus.NotSynced);
            }

         

            StateService.ProgressIndicatorService.Hide();
            IsBusy =false;

            MessageBox.Show("Addresses copied");

            ExNavigationService.GoBack();

        }
    }
}