using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
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
    public partial class CheckAddressTypePage
    {
        public CheckAddressTypePage()
        {
            InitializeComponent();


        }

        private async void ApplicationBarIconButton_OnClick(object sender, EventArgs e)
        {
            var type = (SurveyTypes)cmbType.SelectedItem;

            if (type.Identity == null) return;

            if (type.Name != StateService.CurrentAddress.Type && type.Identity != "0")
            {
                StateService.CurrentAddress.Type = type.Name;
                StateService.CurrentAddress.PTUpdated = true;

                await new DbService().Save(StateService.CurrentAddress, ESyncStatus.NotSynced);


            }

            ExNavigationService.Navigate<QuestionsPage>();

        }


        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back || e.NavigationMode == NavigationMode.Reset)
            {
                var type = (SurveyTypes)cmbType.SelectedItem;

                if ( type != null && StateService.CurrentAddress.Type == type.Name)
                {
                    var typesBack = await new DbService().GetSurveyTypes();
                    typesBack = typesBack.Where(x => x.Name != StateService.CurrentAddress.Type).ToList();
                    typesBack.Insert(0, new SurveyTypes { Name = "Select Type", Identity = "0" });
                    cmbType.ItemsSource = typesBack;
                    cmbType.SelectedItem = cmbType.Items.First();
                }
                tbCurrentType.Text = StateService.CurrentAddress.Type;
                return;
            }

            var addressId = NavigationContext.QueryString["addressId"];

            var address = await new DbService().Find<Address>(addressId);

            StateService.CurrentAddress = address;

            tbCurrentType.Text = StateService.CurrentAddress.Type;
            var types= await new DbService().GetSurveyTypes();

            types = types.Where(x => x.Name != StateService.CurrentAddress.Type).ToList();

            types.Insert(0,new SurveyTypes{Name = "Select Type",Identity = "0"});
            cmbType.ItemsSource = types;



            //cmbType.SelectedItem = types.FirstOrDefault(x => x.Name == StateService.CurrentAddress.Type);
        }

        private async void ApplicationBarMenuItem_OnClick(object sender, EventArgs e)
        {
            await new MyNetmeraClient().ClearAddress();
        }
    }
}