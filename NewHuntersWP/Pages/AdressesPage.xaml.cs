using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using HuntersWP.Db;
using HuntersWP.Models;
using HuntersWP.Services;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

namespace HuntersWP.Pages
{
    public partial class AdressesPage
    {
        public AdressesPage()
        {
            InitializeComponent();
            this.Loaded += AdressesPage_Loaded;
        }

        private List<Address> _allAddresses;

        async void AdressesPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadAddresses(EAddressStatus.All);
            RefreshSyncStatus(SynTextBlock);
            QATextBlock.Visibility = StateService.IsQA ? Visibility.Visible : Visibility.Collapsed;
            //SynTextBlock.DataContext = SyncStatusText;
        }

        async Task LoadAddresses(EAddressStatus status)
        {
            IsBusy = true;
            var adresses = await new DbService().GetAdresses(StateService.CurrentCustomer);

            if (status == EAddressStatus.InComplete)
            {
                adresses = adresses.Where(x => !x.IsCompleted && x.HasStartedToSurveyed).ToList();


            }
            else if (status == EAddressStatus.Surveyed)
            {
                adresses = adresses.Where(x => x.IsCompleted).ToList();
            }
            else if (status == EAddressStatus.ToSurvey)
            {
                adresses = adresses.Where(x => !x.IsCompleted && !x.HasStartedToSurveyed).ToList();

            }

            _allAddresses = new List<Address>(adresses);

            lstAdresses.ItemsSource = adresses;

            //foreach (var address in adresses)
            //{
            //    address.AddressLine1 += " (" + address.Type + ")";
            //}

            Filter();

            IsBusy = false;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //ExNavigationService.ClearNavigationHistory();

            if (e.NavigationMode == NavigationMode.Back)
            {
                Filter();
                return;
            }

            //e.NavigationMode == NavigationMode.New;
            
            ExNavigationService.RemoveQuestionsPage();

            //cmbType.ItemsSource = new List<TextValueItem>
            //{
            //    new TextValueItem("All", EAddressStatus.All),
            //    new TextValueItem("InComplete", EAddressStatus.InComplete),
            //    new TextValueItem("Surveyed", EAddressStatus.Surveyed),
            //    new TextValueItem("ToSurvey", EAddressStatus.ToSurvey)
            //};

            //cmbType.SelectedIndex = 0;
            //cmbType.SelectionChanged -= cmbType_SelectionChanged;
            //cmbType.SelectionChanged += cmbType_SelectionChanged;

        }

        //async void cmbType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    var i = (TextValueItem)cmbType.SelectedItem;

        //    await LoadAddresses((EAddressStatus) i.Value);
        //}


        private void LstAdresses_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;

            var a = (Address)e.AddedItems[0];

            ExNavigationService.Navigate<CheckAddressTypePage>("addressId", a.Id);

        }

        private void AddClick(object sender, EventArgs e)
        {
            ExNavigationService.Navigate<NewAddressPage>();
        }

        void Filter()
        {
            if (_allAddresses == null) return;

            var s = tbSearch.Text;

            if (string.IsNullOrEmpty(s))
            {
                lstAdresses.ItemsSource = _allAddresses;
            }
            else
            {
                var adresses = new List<Address>(_allAddresses);

                lstAdresses.ItemsSource = adresses.Where(x => x.FullAddress.ToUpper().Contains(s.ToUpper())).ToList();

            }
        }

        private void TbSearch_OnKeyUp(object sender, KeyEventArgs e)
        {
            Filter();
        }



        private async void ApplicationBarIconButton_OnClick(object sender, EventArgs e)
        {
            await SyncEngine.Sync();

            RefreshSyncStatus(SynTextBlock);
        }

    //    private async void BtnAll_OnClick(object sender, RoutedEventArgs e)
    //    {

    //        await LoadAddresses(EAddressStatus.All);
    //    }

        private async void BtnIncomplete_OnClick(object sender, RoutedEventArgs e)
        {
            await LoadAddresses(EAddressStatus.InComplete);

        }

        private async void BtnSurveyed_OnClick(object sender, RoutedEventArgs e)
        {
            await LoadAddresses(EAddressStatus.Surveyed);


        }

        private async void BtnToSurvey_OnClick(object sender, RoutedEventArgs e)
        {
            await LoadAddresses(EAddressStatus.ToSurvey);

        }
    }
}