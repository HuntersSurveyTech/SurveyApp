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
            var type = (SurveyType)cmbType.SelectedItem;
            if (type == null) return;
           // if (type.Id == Guid.Empty) return;

            if (type.Name != StateService.CurrentAddress.Type && type.Id != Guid.Empty)
            {
                StateService.CurrentAddress.Type = type.Name;
                StateService.CurrentAddress.TypeUpdated = true;




            }

            StateService.CurrentAddress.IsAlreadyCheckedPropertyType = true;
            await new DbService().Save(StateService.CurrentAddress, ESyncStatus.NotSynced);

            ExNavigationService.Navigate<QuestionsPage>();

        }


        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back || e.NavigationMode == NavigationMode.Reset)
            {
                if (StateService.CurrentAddress.IsAlreadyCheckedPropertyType || StateService.IsQA)
                {
                    ExNavigationService.GoBack();
                    return;
                }

                var type = (SurveyType)cmbType.SelectedItem;

                if ( type != null && StateService.CurrentAddress.Type == type.Name)
                {
                    var typesBack = await new DbService().GetSurveyTypes(StateService.CurrentCustomer.CustomerSurveyID);
                    typesBack = typesBack.Where(x => x.Name != StateService.CurrentAddress.Type).ToList();
                    typesBack.Insert(0, new SurveyType { Name = "Select Type", Id = Guid.Empty });
                    cmbType.ItemsSource = typesBack;
                    cmbType.SelectedItem = cmbType.Items.First();
                }
                tbCurrentType.Text = StateService.CurrentAddress.Type;
                return;
            }

            var addressId = NavigationContext.QueryString["addressId"];

            var address = await new DbService().Find<Address>(addressId);

            if (address == null)
            {
                MessageBox.Show("Address was deleted after sync");
                ExNavigationService.GoBack();
                return;
            }


            if (address.RemoveDataAfterSync)
            {
                MessageBox.Show("Address will be removed after sync");
                ExNavigationService.GoBack();
                return;
            }

            StateService.CurrentAddress = address;


            if (address.IsAlreadyCheckedPropertyType || StateService.IsQA)
            {
                ExNavigationService.Navigate<QuestionsPage>();
                return;
            }


            tbCurrentType.Text = StateService.CurrentAddress.Type;
            var types = await new DbService().GetSurveyTypes(StateService.CurrentCustomer.CustomerSurveyID);

            types = types.Where(x => x.Name != StateService.CurrentAddress.Type).ToList();

            types.Insert(0,new SurveyType{Name = "Select Type",Id = Guid.Empty});
            cmbType.ItemsSource = types;



            //cmbType.SelectedItem = types.FirstOrDefault(x => x.Name == StateService.CurrentAddress.Type);
        }

        private async void ApplicationBarMenuItem_OnClick(object sender, EventArgs e)
        {
            StateService.ProgressIndicatorService.Show("Clearing address");

            var address = StateService.CurrentAddress;
            address.IsCompleted = false;
            address.IsAlreadyCheckedPropertyType = false;

            await new DbService().Save(address, ESyncStatus.NotSynced);

            var elems = await new DbService().GetSurvelemsByAddressUPRN(address.UPRN);

            foreach (var item in elems)
            {
                await new DbService().Delete(item);
            }

            var medias = await new DbService().GetRichMediasAddressUPRN(address.UPRN);

            foreach (var item in medias)
            {

                using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    iso.DeleteFile(item.FileName);
                }

                await new DbService().Delete(item);
            }

            StateService.ProgressIndicatorService.Hide();
            MessageBox.Show("Cleared");
        }
    }
}