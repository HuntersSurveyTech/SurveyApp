using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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
    public partial class QuestionsPage
    {
        public QuestionsPage()
        {
            InitializeComponent();
        }

        private string addressId;


        ApplicationBarMenuItem btnCopyTo
        {
            get
            {
                return this.ApplicationBar.MenuItems[0] as ApplicationBarMenuItem;
            }
        }

        private List<QuestionGroup> _questionGroups = new List<QuestionGroup>();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {


            if (e.NavigationMode == NavigationMode.Back || e.NavigationMode == NavigationMode.Reset)
            {
                if (_questionGroups.Any())
                {
                    foreach (var g in _questionGroups)
                    {
                        g.Complete = await new DbService().FindIsQuestionGroupCompleted(g.Name,StateService.CurrentAddress.UPRN);
                        g.NotifyOfPropertyChange(() => g.Complete);
                    }

                    if (_questionGroups.All(x => x.Complete))
                    {
                        StateService.CurrentAddress.Complete = true;
                        await new DbService().Save(StateService.CurrentAddress, ESyncStatus.NotSynced);
                        btnCopyTo.IsEnabled = true;
                    }

                    lstGroupss.SelectedItem = null;
                    return;
                }
            }

            //cmbTypess.ItemsSource = new List<TextValueItem>
            //{
            //    new TextValueItem("All", EQuestionGroupStatus.All),
            //    new TextValueItem("Completed", EQuestionGroupStatus.Complete),
            //    new TextValueItem("Incompleted", EQuestionGroupStatus.Incomplete),
            //};


            var address = StateService.CurrentAddress;
      
            btnCopyTo.IsEnabled = address.Complete;

            ttbAdress.Text = address.FullAddress + " (UPRN: " + address.UPRN + ") ";

            await LoadQuestions(EQuestionGroupStatus.All);

            
         // cmbTypess.SelectedIndex = 0;
            //cmbTypess.SelectionChanged -= cmbType_SelectionChanged;
            //cmbTypess.SelectionChanged += cmbType_SelectionChanged;

    

        }


        async Task LoadQuestions(EQuestionGroupStatus status)
        {
            IsBusy = true;
            _questionGroups.Clear();

            var address = StateService.CurrentAddress;
             var surveyTypes = await new DbService().FindSurveyType(address.Type);

            if (surveyTypes != null)
            {

                var questions = await new DbService().GetQuestions(surveyTypes.Value);

                var groupped = questions.GroupBy(x => x.Main_Element).ToList();

  
                foreach (var g in groupped)
                {
                    if (g.Key == "SECONDARY") continue;
                    var gr = new QuestionGroup(){Name = g.Key,Questions = new List<Question>(g)};
                    gr.Complete = await new DbService().FindIsQuestionGroupCompleted(gr.Name,address.UPRN);
                    _questionGroups.Add(gr);
                }

                if (_questionGroups.All(x => x.Complete))
                {
                    address.Complete = true;
                    await new DbService().Save(address, ESyncStatus.NotSynced);
                    btnCopyTo.IsEnabled = true;
                }

                _questionGroups = _questionGroups.OrderBy(x => x.Questions.First().Question_Order).ToList();

                if (status == EQuestionGroupStatus.All)
                {
                    lstGroupss.ItemsSource = _questionGroups;
                }
                else
                {
                    lstGroupss.ItemsSource = _questionGroups.Where(x => x.Complete == (status == EQuestionGroupStatus.Complete)).ToList();
                }

            }

            IsBusy = false;
        
        }

        private void LstGroups_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems.Count == 0) return;

            var g = (QuestionGroup)e.AddedItems[0];

            StateService.CurrentQuestionGroup = g;

            ExNavigationService.Navigate<ElementsPage>();
        }


        private void CopyToAddressClick(object sender, EventArgs e)
        {
           
            ExNavigationService.Navigate<CopyToAdressesPage>("addressId",StateService.CurrentAddress.AddressID);

        }



        private async void BtnAll_OnClick(object sender, RoutedEventArgs e)
        {
            await LoadQuestions(EQuestionGroupStatus.All);

        }

        private async void BtnCompleted_OnClick(object sender, RoutedEventArgs e)
        {
            await LoadQuestions(EQuestionGroupStatus.Complete);

        }

        private async void BtnIncompleted_OnClick(object sender, RoutedEventArgs e)
        {
            await LoadQuestions(EQuestionGroupStatus.Incomplete);

        }
    }
}