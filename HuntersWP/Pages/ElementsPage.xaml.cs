using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using HuntersWP.Db;
using HuntersWP.Models;
using HuntersWP.Services;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

namespace HuntersWP.Pages
{
    public partial class ElementsPage 
    {
        public ElementsPage()
        {
            InitializeComponent();
        }

        Stack<string> _questionsStack = new Stack<string>();
        List<string> _confirmations = new List<string>();

        private List<Question> _questions;
        private Question _currentQuestion;
        private Survelem _currentAnswer;
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back) return;

            cmb1.SelectionChanged += cmb1_SelectionChanged;

            _questions = StateService.CurrentQuestionGroup.Questions;
            

            if(_questions == null) return;

            _currentQuestion = _questions.First();

            await LoadQuestionData();
        }


        void cmb1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;

            var item = e.AddedItems[0] as Option;

            cmb2.IsEnabled = !item.Disable2nd;

            if (_currentQuestion.Apply_2nd_Question == "YES" && panelTextBoxes.Children.Any())
            {
                foreach (StackPanel child in panelTextBoxes.Children)
                {
                    var txtBox = child.Children[1] as TextBox;
                    txtBox.IsEnabled = !item.Disable2nd;
                }
            }

        }

   
        async Task LoadQuestionData()
        {

            var media = await new DbService().FindMedia(_currentQuestion.Question_Ref, StateService.CurrentAddress.UPRN);

            tbImage.Visibility = media != null ? Visibility.Visible : Visibility.Collapsed;

            var options1 = await new DbService().GetFirstLevelOptions(_currentQuestion.Question_Ref);

            _currentAnswer = await new DbService().FindAnswer(_currentQuestion.Question_Ref,StateService.CurrentAddress.UPRN);

            if (_currentAnswer != null)
            {
                tbComment.Text = _currentAnswer.COMMENT;
            }
            else
            {
                tbComment.Text = "";
            }

            options1.Insert(0,new Option{DisplayText = "Answer required",Identity = "0"});
            cmb1.ItemsSource = options1;

            if (_currentAnswer != null)
            {
                var o1 = options1.FirstOrDefault(x => x.OptionId == _currentAnswer.OptionID);

                if(o1 != null)
                    cmb1.SelectedItem = o1;
            }

            if(options1.Any() && cmb1.SelectedItem == null)
                cmb1.SelectedIndex = 0;

            if (options1.Count > 1)
            {
                cmb1Panel.Visibility = Visibility.Visible;
                tb1ForCmb1Panel.Visibility = Visibility.Collapsed;
                tb1ForCmb1.Text = "";
            }
            else
            {
                if (_currentAnswer != null)
                    tb1ForCmb1.Text = _currentAnswer.Freetext ?? "";
                else
                {
                    tb1ForCmb1.Text = "";
                }

                tb1ForCmb1Panel.Visibility = Visibility.Visible;
                tb1ForCmb1.BorderThickness =new Thickness(3);
                tb1ForCmb1.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 100, 160, 200));
                cmb1Panel.Visibility = Visibility.Collapsed;

            }

            cmb2.IsEnabled = true;

            tbQuestionRef.Text = string.Format("{0} {1}",_currentQuestion.Main_Element ,_currentQuestion.Unit);
            tbQuestionHeading.Text = _currentQuestion.Question_Heading;

            tbCurrentQNmber.Text = (_questions.IndexOf(_currentQuestion) + 1).ToString();
            tbQuestionCount.Text = _questions.Count.ToString();


            foreach (StackPanel child in panelTextBoxes.Children)
            {
                var tb = child.Children[1] as TextBox;
                tb.KeyUp -= tb_KeyUp;
            }

            panelTextBoxes.Children.Clear();
            if (_currentQuestion.Apply_2nd_Question == "YES")
            {
                panel2.Visibility = Visibility.Visible;
      

                tbComment.Visibility = Visibility.Visible;
                tbQuestionNum.Visibility = Visibility.Visible;
                panelTextBoxes.Visibility = Visibility.Visible;

                var options2 = await new DbService().GetSecondLevelOptions(_currentQuestion.Question_Ref);



                if (options2.Any())
                {
                    options2.Insert(0, new Option {DisplayText = "Answer required", Identity = "0"});
                    cmb2.ItemsSource = options2;

                    cmb2.Visibility = Visibility.Visible;

                    if (_currentAnswer != null)
                    {
                        var o2 = options2.FirstOrDefault(x => x.OptionId == _currentAnswer.OptionID2ndry);

                        if(o2 != null)
                            cmb2.SelectedItem = o2;
                    }

                    if (options2.Any() && cmb2.SelectedItem == null)
                        cmb2.SelectedIndex = 0;


                }
                else
                {
                    cmb2.Visibility = Visibility.Collapsed;
                }

                var address = StateService.CurrentAddress;

               // var surveyTypes = await new DbService().FindSurveyType(address.Type);

                var qRef = "*";
                if (_currentQuestion.SurveyType.Contains("HHSRS") || _currentQuestion.SurveyType.Contains("RDSAP"))
                {
                    qRef = _currentQuestion.Question_Ref;
                }

                var survelemMaps = await new DbService().GetSurvelemMaps(qRef);


                foreach (var survelemMap in survelemMaps)
                {
                    var p = new StackPanel(){Width = 200};
                    var b = new TextBlock() {Text = survelemMap.Question_Heading};
                    var tb = new TextBox() {Tag =survelemMap.SqName,Width = 180};
                    

                    b.Margin=new Thickness(25,0,0,-8);
                    b.Foreground=new SolidColorBrush(Color.FromArgb(255,219,70,154)) ;
                    b.FontWeight= FontWeights.Bold;
                    tb.BorderThickness = new Thickness(3);
                    tb.BorderBrush= new SolidColorBrush(Color.FromArgb(255,100,160,200));

                    InputScope numberScope = new InputScope();
                    InputScopeName numberScopeName = new InputScopeName();
                    numberScopeName.NameValue = InputScopeNameValue.CurrencyAmountAndSymbol;
                    numberScope.Names.Add(numberScopeName);

                    InputScope textScope = new InputScope();
                    InputScopeName textScopeName = new InputScopeName();
                    textScopeName.NameValue = InputScopeNameValue.AlphanumericFullWidth;
                    textScope.Names.Add(textScopeName);

                    if (survelemMap.SqName.ToLower().StartsWith("sqn"))
                    {
                        tb.InputScope = numberScope;
                    }
                    else if (survelemMap.SqName.ToLower().StartsWith("sqt"))
                    {
                        tb.InputScope = textScope;
                    }

                    var props = typeof(Survelem).GetProperties();

                    if (_currentAnswer != null)
                    {
                        var value = props.First(x => x.Name == survelemMap.SqName).GetValue(_currentAnswer);
                        if (value != null)
                        {
                            tb.Text = value.ToString();
                        }
                        if (cmb1.SelectedIndex != 0)
                        {
                            var selectedO = cmb1.SelectedItem as Option;
                            tb.IsEnabled = !selectedO.Disable2nd;
                        }
                    }

                    p.Children.Add(b);
                    p.Children.Add(tb);

                    tb.KeyUp += tb_KeyUp;

                    panelTextBoxes.Children.Add(p);
                }
            }
            else
            {
                panel2.Visibility = Visibility.Collapsed;
                cmb2.Visibility = Visibility.Collapsed;

                tbComment.Visibility = Visibility.Collapsed;
                tbQuestionNum.Visibility = Visibility.Collapsed;
                panelTextBoxes.Visibility = Visibility.Collapsed;
            }
        }

        void tb_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                page.Focus();
            }
        }



        private async void PreviousClick(object sender, EventArgs e)
        {
            btnPrevious.IsEnabled = false;
          
            var r = await SaveData();

            if (!r)
            {
                btnPrevious.IsEnabled = true;
                return;
            }

            if (_questions.IndexOf(_currentQuestion) == 0)
            {
                btnPrevious.IsEnabled = true;
                return;
            }


            var questionRef = _questionsStack.Pop();


            _currentQuestion = _questions.First(x => x.Question_Ref == questionRef);
            
            await LoadQuestionData();
            btnPrevious.IsEnabled = true;
        }
        
        
        //protected override async void OnBackKeyPress(CancelEventArgs e)
        //{

        //    await SaveData();
        //    base.OnBackKeyPress(e);
        //}

        ApplicationBarIconButton btnNext
        {
            get { return (this.ApplicationBar.Buttons[1] as ApplicationBarIconButton); }
        }

        ApplicationBarIconButton btnPrevious
        {
            get { return (this.ApplicationBar.Buttons[0] as ApplicationBarIconButton); }
        }

        private async void NextClick(object sender, EventArgs e)
        {
            btnNext.IsEnabled = false;

        
            var r =await SaveData();

            if (!r)
            {
                btnNext.IsEnabled = true;
                return;
            }


            var previousQuestion = _currentQuestion;


            var o1 = (Option)cmb1.SelectedItem;

            if (string.IsNullOrEmpty(o1.Jumpto))
            {
                if (_questions.IndexOf(_currentQuestion) == _questions.Count - 1)
                {
                    //address complete
                    new ApplicationSettingsService().SetSetting(StateService.CurrentAddress.UPRN+"."+StateService.CurrentQuestionGroup.Name,true);

                    ExNavigationService.GoBack();
                    btnNext.IsEnabled = true;
  return;
                }
                _questionsStack.Push(_currentQuestion.Question_Ref);
                _currentQuestion = _questions[_questions.IndexOf(_currentQuestion) + 1];
         
            }
            else
            {
                _questionsStack.Push(_currentQuestion.Question_Ref);
                _currentQuestion = _questions.FirstOrDefault(x => x.Question_Ref == o1.Jumpto);

                if (_currentQuestion == null)
                {
                    MessageBox.Show("Question not found for jump to, REF = " + o1.Jumpto);
                    _questionsStack.Pop();
                    _currentQuestion = previousQuestion;
                    await LoadQuestionData();
                    btnNext.IsEnabled = true;
                    return;
                }

            }



            await LoadQuestionData();
            btnNext.IsEnabled = true;
        }

        private async void SubmitClick(object sender, EventArgs e)
        {
            NextClick(sender, e);

        }

        private void PhotoClick(object sender, EventArgs e)
        {
            if (IsBusy) return;
            var option = cmb1.SelectedItem as Option;
            if ((option != null && option.OptionId != null) || !string.IsNullOrEmpty(tb1ForCmb1.Text))
            {
                var photo = new CameraCaptureTask();
                photo.Completed += photo_Completed;
                photo.Show();
            }
            else
            {
                MessageBox.Show("Please select an answer to the 1st question before taking a photo");
            }

        }

        async void photo_Completed(object sender, PhotoResult e)
        {
            (sender as CameraCaptureTask).Completed -= photo_Completed;
            var stream = e.ChosenPhoto;

            if (stream == null) return;

            IsBusy = true;

            var fileName = string.Format("{0}{1}{2}", StateService.CurrentAddress.UPRN, _currentQuestion.Question_Ref,
                (cmb1.SelectedItem as Option).OptionId);

            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var file = iso.OpenFile(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    await stream.CopyToAsync(file);
                    file.Flush();
                }
            }

            var media = new RichMedia
            {
                CustomerID = _currentQuestion.CustomerID,
                CustomerSurveyID = _currentQuestion.CustomerSurveyID,
                Comments = tbComment.Text,
                FileName = fileName ,
                Question_Ref = _currentQuestion.Question_Ref,
                Option_ID = (cmb1.SelectedItem as Option).OptionId,
                ID = Guid.NewGuid().ToString(),
                UPRN = StateService.CurrentAddress.UPRN,
                IsCreatedOnClient = true

            };
            media.Identity = media.ID;


            await new DbService().Save(media, ESyncStatus.NotSynced);

            IsBusy = false;

            tbImage.Visibility = Visibility.Visible;

        }

        async Task<bool> SaveData()
        {
            if (tb1ForCmb1Panel.Visibility == Visibility.Visible && string.IsNullOrEmpty(tb1ForCmb1.Text))
                return false;

            if (cmb1.Items.Count > 1 && cmb1.SelectedIndex <=0 ) return false;

            foreach (StackPanel child in panelTextBoxes.Children)
            {
                var item = child.Children[1] as TextBox;

                if (item != null && string.IsNullOrEmpty(item.Text) && item.IsEnabled)
                {
                    MessageBox.Show("Please answer all questions");
                    return false;
                }
            }

            if (_currentAnswer == null)
            {
                _currentAnswer = new Survelem()
                {
                    UPRN = StateService.CurrentAddress.UPRN,
                    DateOfSurvey = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                    id = Guid.NewGuid().ToString(),
                    Question_Ref = _currentQuestion.Question_Ref,
                    BuildingType = StateService.CurrentAddress.Type,
                    CustomerID = _currentQuestion.CustomerID,
                    CustomerSurveyID = _currentQuestion.CustomerSurveyID,
                    IsCreatedOnClient = true

                };
            }

            if (!string.IsNullOrEmpty(tb1ForCmb1.Text))
            {
                _currentAnswer.Freetext = tb1ForCmb1.Text;
            }

            _currentAnswer.COMMENT = tbComment.Text;

            var o1 = (Option)cmb1.SelectedItem;

           // o1.Choose = true;

            if (o1.ConfirmationRequired && !_confirmations.Contains(o1.Identity))
            {
                _confirmations.Add(o1.Identity);
                var mb = MessageBox.Show(string.Format("Are you sure you want to select {0}?",o1.Text), "Warning", MessageBoxButton.OKCancel);
                if (mb == MessageBoxResult.Cancel) return false;
            }

            _currentAnswer.OptionID = o1.OptionId;

            
            if (cmb2.Items.Count > 1 && cmb2.IsEnabled)
            {
                if (cmb2.SelectedIndex <= 0 && cmb2.IsEnabled) return false;

                var o2 = cmb2.SelectedItem as Option;

                if (o2.ConfirmationRequired && !_confirmations.Contains(o1.Identity))
                {
                    _confirmations.Add(o2.Identity);
                    var mb = MessageBox.Show(string.Format("Are you sure you want to select {0}?", o2.Text), "Warning", MessageBoxButton.OKCancel);
                    if (mb == MessageBoxResult.Cancel) return false;
                }
                //o2.Choose = true;

                _currentAnswer.OptionID2ndry = o2.OptionId;
               // await new DbService().Save(o2, ESyncStatus.NotSynced);
            }


            if (_currentQuestion.Apply_2nd_Question == "YES" && panelTextBoxes.Children.Any())
            {
                foreach (StackPanel child in panelTextBoxes.Children)
                {
                    var item = child.Children[1] as TextBox;

                    if(item == null) continue;

                    var name = item.Tag.ToString();

                    var props = typeof (Survelem).GetProperties();

                    var prop = props.FirstOrDefault(x => x.Name == name);

                    if (prop == null)
                    {
                      throw  new Exception("Not found property: " + name);
                    }

                    if (item.IsEnabled)
                    {
                        prop.SetValue(_currentAnswer, item.Text);
                    }
                    else
                    {
                        prop.SetValue(_currentAnswer, "");
                    }

                }
            }
           // await new DbService().Save(o1, ESyncStatus.NotSynced);


            _currentAnswer.Identity = _currentAnswer.id;

            await new DbService().Save(_currentAnswer, ESyncStatus.NotSynced);

            QuestionStatus questionStatus = await new DbService().FindQuestionStatus(_currentQuestion.Question_Ref,StateService.CurrentAddress.UPRN);

            if (questionStatus == null)
            {
                questionStatus = new QuestionStatus() {Identity = Guid.NewGuid().ToString(),QuestionRef = _currentQuestion.Question_Ref,UPRN = StateService.CurrentAddress.UPRN};
            }
            questionStatus.Completed = true;

            await new DbService().Save(questionStatus);


            var address = StateService.CurrentAddress;
            address.HasStartedToSurveyed = true;
            await new DbService().Save(address, ESyncStatus.NotSynced);

            return true;

        }
    }
}