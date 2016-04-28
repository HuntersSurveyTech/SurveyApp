using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
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


        ApplicationBarMenuItem btnCompleteQaAdrress
        {
            get
            {
                return this.ApplicationBar.MenuItems[1] as ApplicationBarMenuItem;
            }
        }

        private List<QuestionGroup> _questionGroups = new List<QuestionGroup>();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            btnCompleteQaAdrress.IsEnabled = StateService.IsQA;

            if (e.NavigationMode == NavigationMode.Back || e.NavigationMode == NavigationMode.Reset)
            {
                if (_questionGroups.Any())
                {
                    //foreach (var gr in _questionGroups)
                    //{
                    //    gr.IsCompleted =
                    //        await
                    //            new DbService().FindIfQuestionGroupIsCompleted(StateService.CurrentAddress.Id, gr.Name);

                    //    gr.NotifyOfPropertyChange(() => gr.IsCompleted);
                    //}


                    Task.Factory.StartNew(() =>
                    {
                        UpdateAddressStatus(_questionGroups,true);
                    });


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

            var dbAddress = await new DbService().FindAddress(address.Id);

            if (dbAddress == null)
            {
                MessageBox.Show("Address was deleted after sync");
                ExNavigationService.GoBack();
                return;
            }
      
            btnCopyTo.IsEnabled = address.IsCompleted && !StateService.IsQA;

            ttbAdress.Text = address.FullAddress + " (UPRN: " + address.UPRN + ") ";

            await LoadQuestions(EQuestionGroupStatus.All);

            
         // cmbTypess.SelectedIndex = 0;
            //cmbTypess.SelectionChanged -= cmbType_SelectionChanged;
            //cmbTypess.SelectionChanged += cmbType_SelectionChanged;

    

        }


        async Task LoadQuestions(EQuestionGroupStatus status)
        {
            if (IsBusy) return;

            IsBusy = true;
            _questionGroups.Clear();

            var address = StateService.CurrentAddress;
            var customer = StateService.CurrentCustomer;
            var surveyType = await new DbService().FindSurveyType(address.Type, customer.CustomerSurveyID);

            if (surveyType != null)
            {

                var questions = await new DbService().GetQuestions(surveyType.Value,customer.CustomerSurveyID);

                var groupped = questions.GroupBy(x => x.Main_Element).ToList();

  
                foreach (var g in groupped)
                {
                    if (g.Key == "SECONDARY") continue;
                    var gr = new QuestionGroup(){Name = g.Key,Questions = new List<Question>(g)};
                    gr.IsCompleted = await new DbService().FindIfQuestionGroupIsCompleted(address.Id, gr.Name);
                  
                    _questionGroups.Add(gr);
                }

               _questionGroups = _questionGroups.OrderBy(x => x.Questions.First().Question_Order).ToList();

                if (status == EQuestionGroupStatus.All)
                {
                    lstGroupss.ItemsSource = _questionGroups;
                }
                else
                {
                    lstGroupss.ItemsSource = _questionGroups.Where(x => x.IsCompleted == (status == EQuestionGroupStatus.Complete)).ToList();
                }

                Task.Factory.StartNew(() =>
                {
                    UpdateAddressStatus(_questionGroups);
                });

            }
            else
            {
                IsBusy = false;
            }


        
        }

        async Task UpdateAddressStatus(List<QuestionGroup> groups, bool updateStatus = false)
        {
            var status = await new DbService().FindAddressStatus(StateService.CurrentAddress.Id);

            if (status == null || status.IsDeletedOnClient)
            {
                status = new AddressStatus()
                {
                    AddressId = StateService.CurrentAddress.Id,
                };
            }

            var count = 0;

            foreach (var group in groups)
            {
                foreach (var q in group.Questions)
                {
                    var r =
                        await
                            new DbService().FindIfQuestionIsCompleted(q.Question_Ref, StateService.CurrentAddress.UPRN);

                    if (r)
                    {
                        count ++;
                    }
                }
            }
            status.CompletedQuestionsCount = count;
            status.IsCompleted = groups.All(x => x.IsCompleted);

            if (!StateService.IsQA)
                await new DbService().Save(status, ESyncStatus.NotSynced);

            StateService.CurrentAddress.IsCompleted = status.IsCompleted;
            if (status.IsCompleted)
            {
                StateService.CurrentAddress.IsLoadToPhone = status.IsCompleted;
            }

            if (!StateService.IsQA)
                await new DbService().Save(StateService.CurrentAddress, ESyncStatus.NotSynced);


            Dispatcher.BeginInvoke(async () =>
            {
                if (StateService.CurrentAddress.IsCompleted)
                {
                    btnCopyTo.IsEnabled = true && !StateService.IsQA;
                }


                if (updateStatus)
                {
                    foreach (var group in groups)
                    {
                        group.IsCompleted = await
                            new DbService().FindIfQuestionGroupIsCompleted(StateService.CurrentAddress.Id, group.Name);

                        group.NotifyOfPropertyChange(() => group.IsCompleted);
                    }
                }

                IsBusy = false;
            });


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
           
            ExNavigationService.Navigate<CopyToAdressesPage>("addressId",StateService.CurrentAddress.Id);

        }

        private async void ClearAddressClick(object sender, EventArgs e)
        {
            await new MyNetmeraClient().ClearAddress();

            ExNavigationService.Navigate<AdressesPage>();
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

        private async void CompleteQaAddress(object sender, EventArgs e)
        {
            var qaAddress = await new DbService().FindQaAddress(StateService.CurrentAddress.Id);

            qaAddress.IsCompleted = true;

            await new DbService().Save(qaAddress, ESyncStatus.NotSynced);


        }
    }
}