using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Windows.Storage;
using Windows.Storage.Streams;
using Coding4Fun.Toolkit.Controls;
using HuntersWP.Db;
using HuntersWP.Models;
using HuntersWP.Services;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework.Media;

namespace HuntersWP.Pages
{
    public partial class ElementsPage
    {
        public ElementsPage()
        {
            InitializeComponent();

            QATextBlock.Visibility = StateService.IsQA ? Visibility.Visible : Visibility.Collapsed;

            if (StateService.IsQA)
            {
                var item = new ApplicationBarMenuItem("Add QA comment");
                item.Click += addQACommentitem_Click;
                ApplicationBar.MenuItems.Add(item);
            }
        }

        async void addQACommentitem_Click(object sender, EventArgs e)
        {
            var comment = await new DbService().FindQaAddressComment(StateService.CurrentAddress.Id, _currentQuestion.Question_Ref);

            if (comment == null)
            {
                comment = new QAAddressComment() { Id = Guid.NewGuid(), IsCreatedOnClient = true, QuestionRef = _currentQuestion.Question_Ref, AddressId = StateService.CurrentAddress.Id };
            }

            var prompt = new InputPrompt();
            prompt.Value = comment.Text;
            prompt.Title = "QA comment";

            prompt.Completed += async (o, args) =>
            {
                if (args.PopUpResult == PopUpResult.Ok)
                {
                    comment.Text = args.Result;

                    await new DbService().Save(comment, ESyncStatus.NotSynced);
                }
            };
            prompt.Show();
        }

        private GestureListener gestureListener;

        Stack<string> _questionsStack = new Stack<string>();
        List<string> _confirmations = new List<string>();

        private List<Question> _questions;
        private Question _currentQuestion;
        private Survelem _currentAnswer;

        private List<PropertyInfo> _propertyInfos;
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            _propertyInfos = typeof(Survelem).GetProperties().ToList();

            gestureListener = GestureService.GetGestureListener(this);
            gestureListener.Flick += this.GlFlick;

            if (e.NavigationMode == NavigationMode.Back)
            {
                return;
            }



            cmb1.SelectionChanged += cmb1_SelectionChanged;

            _questions = StateService.CurrentQuestionGroup.Questions;


            if (_questions == null) return;

            _currentQuestion = _questions.First();

            tb1ForCmb1.TextChanged += tb1ForCmb1_TextChanged;

            await LoadQuestionData();
        }

        void tb1ForCmb1_TextChanged(object sender, TextChangedEventArgs e)
        {
            _shownRangeWarning = false;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            tb1ForCmb1.TextChanged -= tb1ForCmb1_TextChanged;

            if (gestureListener != null)
            {
                gestureListener.Flick -= this.GlFlick;
                gestureListener = null;
            }
        }

        void GlFlick(object sender, FlickGestureEventArgs e)
        {
            if (e.Direction == System.Windows.Controls.Orientation.Horizontal)
            {
                if (Math.Abs(e.HorizontalVelocity) < 500) return;

                if (e.HorizontalVelocity > 0)
                {
                    PreviousClick(null, EventArgs.Empty);
                }
                else
                {
                    NextClick(null, EventArgs.Empty);
                }
            }
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
            _shownRangeWarning = false;

            tb1ForCmb1.InputScope = new InputScope();

            var media = await new DbService().FindMedia(_currentQuestion.Question_Ref, StateService.CurrentAddress.UPRN);

            tbImage.Visibility = media != null ? Visibility.Visible : Visibility.Collapsed;

            var options1 = await new DbService().GetFirstLevelOptions(_currentQuestion.Question_Ref, StateService.CurrentCustomer.CustomerSurveyID);

            _currentAnswer = await new DbService().FindAnswer(_currentQuestion.Question_Ref, StateService.CurrentAddress.UPRN);

            if (_currentAnswer != null)
            {
                tbComment.Text = _currentAnswer.COMMENT ?? "";
            }
            else
            {
                tbComment.Text = "";
            }

            options1.Insert(0, new Option { DisplayText = "Answer required", Id = Guid.Empty });
            cmb1.ItemsSource = options1;

            if (_currentAnswer != null)
            {
                var o1 = options1.FirstOrDefault(x => x.Id == _currentAnswer.OptionID);

                if (o1 != null)
                    cmb1.SelectedItem = o1;
            }

            if (options1.Any() && cmb1.SelectedItem == null)
                cmb1.SelectedIndex = 0;

            if (options1.Count > 1)
            {
                cmb1Panel.Visibility = Visibility.Visible;
                tb1ForCmb1Panel.Visibility = Visibility.Collapsed;
                tb1ForCmb1.Text = "";
            }
            else
            {
                tb1ForCmb1Panel.Visibility = Visibility.Visible;
                if (_currentQuestion.isDate)
                {
                    dt1ForCmb1.Visibility = Visibility.Visible;

                    tb1ForCmb1.Visibility = Visibility.Collapsed;
                }
                else
                {
                    dt1ForCmb1.Visibility = Visibility.Collapsed;

                    tb1ForCmb1.Visibility = Visibility.Visible;
                }

                if (_currentAnswer != null)
                {
                    if (_currentQuestion.isDate)
                    {
                        tb1ForCmb1.Text = "";
                        if (!string.IsNullOrEmpty(_currentAnswer.Freetext))
                        {
                            DateTime date;

                            if (DateTime.TryParseExact(_currentAnswer.Freetext, Constants.DateTimeFormat, CultureInfo.CurrentUICulture, DateTimeStyles.None, out date))
                            {
                                dt1ForCmb1.Value = date;
                            }
                            else
                            {
                                dt1ForCmb1.Value = null;
                            }


                        }
                    }
                    else
                    {
                        dt1ForCmb1.Value = null;
                        tb1ForCmb1.Text = _currentAnswer.Freetext ?? "";
                    }
                }

                else
                {
                    tb1ForCmb1.Text = "";
                    dt1ForCmb1.Value = null;
                }

                if (!string.IsNullOrEmpty(_currentQuestion.LookAtRange) &&
                    _currentQuestion.LookAtRange.ToLower() == "true")
                {
                    InputScope numberScope = new InputScope();
                    InputScopeName numberScopeName = new InputScopeName();
                    numberScopeName.NameValue = InputScopeNameValue.NumberFullWidth;
                    numberScope.Names.Add(numberScopeName);

                    tb1ForCmb1.InputScope = numberScope;
                }

                tb1ForCmb1Panel.Visibility = Visibility.Visible;
                tb1ForCmb1.BorderThickness = new Thickness(3);
                tb1ForCmb1.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 100, 160, 200));
                cmb1Panel.Visibility = Visibility.Collapsed;

            }

            cmb2.IsEnabled = true;

            tbQuestionRef.Text = string.Format("{0} {1}", _currentQuestion.Main_Element, _currentQuestion.Unit);
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

                var options2 = await new DbService().GetSecondLevelOptions(_currentQuestion.Question_Ref, StateService.CurrentCustomer.CustomerSurveyID);



                if (options2.Any())
                {
                    options2.Insert(0, new Option { DisplayText = "Answer required", Id = Guid.Empty });
                    cmb2.ItemsSource = options2;

                    cmb2.Visibility = Visibility.Visible;

                    if (_currentAnswer != null)
                    {
                        var o2 = options2.FirstOrDefault(x => x.Id == _currentAnswer.OptionID2ndry);

                        if (o2 != null)
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

                //todo: Ulas disabled to deafult qRef to be *, and now sending actual Question_Ref of the current question to GetSurvelMaps method. We need to check if this effects anywhere else. 

                //var qRef = "*";

                //todo: Ulas commented out the following if clause in order to enable secondary questions for HHRSRS and RDSAP. Need to check and see if this causes any problem with the first project data. We need to check if this effects anywhere else.

                //if (_currentQuestion.SurveyType.Contains("HHSRS") || _currentQuestion.SurveyType.Contains("RDSAP"))
                //{
                //    qRef = _currentQuestion.Question_Ref;
                //}

                var survelemMaps = await new DbService().GetSurvelemMaps(_currentQuestion.Question_Ref, StateService.CurrentCustomer.CustomerSurveyID, _currentQuestion.SurveyType);


                foreach (var survelemMap in survelemMaps)
                {
                    var p = new StackPanel() { Width = 200 };
                    var b = new TextBlock() { Text = survelemMap.Question_Heading };
                    var tb = new TextBox() { Tag = survelemMap.SqName, Width = 180 };


                    b.Margin = new Thickness(25, 0, 0, -8);
                    b.Foreground = new SolidColorBrush(Color.FromArgb(255, 219, 70, 154));
                    b.FontWeight = FontWeights.Bold;
                    tb.BorderThickness = new Thickness(3);
                    tb.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 100, 160, 200));

                    InputScope numberScope = new InputScope();
                    InputScopeName numberScopeName = new InputScopeName();
                    numberScopeName.NameValue = InputScopeNameValue.NumberFullWidth;
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

                    if (_currentAnswer != null)
                    {
                        var value = _propertyInfos.First(x => x.Name == survelemMap.SqName).GetValue(_currentAnswer);
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


            var r = await SaveData();

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
                    //group complete

                    var group =
                        await
                            new DbService().FindAddressQuestionGroupStatus(StateService.CurrentAddress.Id,
                                _questions[0].Main_Element);

                    if (group == null)
                    {
                        group = new AddressQuestionGroupStatus() { AddressId = StateService.CurrentAddress.Id, Group = _questions[0].Main_Element };
                    }

                    group.IsCompleted = true;

                    if (!StateService.IsQA)
                        await new DbService().Save(group, ESyncStatus.NotSynced);

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

            if ((option != null && option.Id != Guid.Empty) || !string.IsNullOrEmpty(tb1ForCmb1.Text))
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

        async Task SaveToMediaLibrary(string fileName, Stream stream, bool tryAgain = true)
        {
            stream.Seek(0, SeekOrigin.Begin);

            //StorageFolder appFolder = await KnownFolders.PicturesLibrary.CreateFolderAsync("Hunters", CreationCollisionOption.OpenIfExists);

            //StorageFile myfile = await appFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

            //using (var outputStream = await myfile.OpenAsync(FileAccessMode.ReadWrite))
            //{
            //    DataWriter dataWriter = new DataWriter(outputStream); 
            //    dataWriter.WriteBytes(Helpers.ReadFully(stream));
            //}

            //return;
            bool saved = false;

         
            using (MediaLibrary library = new MediaLibrary())
            {
                try
                {
                    library.SavePicture(fileName, stream);
                    saved  = true;
                }
                catch (Exception Ex)
                {
                    saved = false;

                }
            }

            if (!saved && tryAgain)
            {
                SaveToMediaLibrary(fileName,stream,false);
            }
           
        }

        async void photo_Completed(object sender, PhotoResult e)
        {
            (sender as CameraCaptureTask).Completed -= photo_Completed;
            var stream = e.ChosenPhoto;

            if (stream == null) return;

            IsBusy = true;
            var addQAText = "";

            if (StateService.IsQA) addQAText = "_QA";

            var optionId = "";

            if (cmb1.SelectedItem != null)
            {
                optionId = (cmb1.SelectedItem as Option).Id.ToString();
            }

            var fileName = string.Format("{0}_{1}{2}{3}.jpg", StateService.CurrentAddress.UPRN, _currentQuestion.Question_Ref,
                optionId != (Guid.Empty).ToString() ? ("_" + optionId) : "", addQAText);

            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var file = iso.OpenFile(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    await stream.CopyToAsync(file);
                    file.Flush();
                }
            }

            await SaveToMediaLibrary(fileName, stream);

            var option = (cmb1.SelectedItem as Option);

            var media = new RichMedia
            {
                CustomerID = _currentQuestion.CustomerID,
                CustomerSurveyID = _currentQuestion.CustomerSurveyID,
                Comments = tbComment.Text,
                FileName = fileName,
                Question_Ref = _currentQuestion.Question_Ref,
                Option_ID = option.Id != Guid.Empty ? (Guid?)option.Id : null,
                UPRN = StateService.CurrentAddress.UPRN,
                IsCreatedOnClient = true,
                IsCreatePDF = _currentQuestion.createPDF || option.createPDF,
                WatermarkText = string.Format("{0}[@n@]{1}[@n@]{2}", StateService.CurrentAddress.UPRN, _currentQuestion.Question_Heading, DateTime.Now.ToString("dd MMM yyyy HH:mm:ss"))

            };

            media.PDFFileName = media.IsCreatePDF ?
                    string.Format("{0}_{1}_{2}.pdf", StateService.CurrentAddress.UPRN, "HUNT",
                        DateTime.Now.ToString("ddMMyyyyHHmmss"))
                    : "";


            await new DbService().Save(media, ESyncStatus.NotSynced);

            IsBusy = false;

            tbImage.Visibility = Visibility.Visible;

        }

        private bool _shownRangeWarning = false;

        async Task<bool> SaveData()
        {
            if (tb1ForCmb1Panel.Visibility == Visibility.Visible && string.IsNullOrEmpty(tb1ForCmb1.Text) && dt1ForCmb1.Value == null)
                return false;

            if (cmb1.Items.Count > 1 && cmb1.SelectedIndex <= 0) return false;

            if (_currentQuestion.NeedToHaveMedia)
            {
                var media = await new DbService().FindMedia(_currentQuestion.Question_Ref, StateService.CurrentAddress.UPRN);

                if (media == null)
                {
                    MessageBox.Show("This question requires to take a photo. Please take a photo to go to the next question.");
                    return false;
                }
            }

            foreach (StackPanel child in panelTextBoxes.Children)
            {
                var item = child.Children[1] as TextBox;

                if (item != null && string.IsNullOrEmpty(item.Text) && item.IsEnabled)
                {
                    MessageBox.Show("Please answer all questions");
                    return false;
                }
            }

            if (!string.IsNullOrEmpty(tb1ForCmb1.Text) && !_shownRangeWarning)
            {
                if (!string.IsNullOrEmpty(_currentQuestion.LookAtRange) && _currentQuestion.LookAtRange.ToLower() == "true")
                {
                    double value = 0;

                    if (!double.TryParse(tb1ForCmb1.Text, out value))
                    {
                        _shownRangeWarning = true;
                        MessageBox.Show("Value should be numeric");
                        return false;
                    }


                    if (!string.IsNullOrEmpty(_currentQuestion.AnswerRangeFrom))
                    {
                        var low = double.Parse(_currentQuestion.AnswerRangeFrom);
                        if (value < low)
                        {
                            _shownRangeWarning = true;
                            MessageBox.Show(_currentQuestion.AnswerRangeTextF ?? "AnswerRangeTextF");
                            return false;
                        }
                    }
                    if (!string.IsNullOrEmpty(_currentQuestion.AnswerRangeTo))
                    {
                        var top = double.Parse(_currentQuestion.AnswerRangeTo);
                        if (value > top)
                        {
                            _shownRangeWarning = true;
                            MessageBox.Show(_currentQuestion.AnswerRangeTextT ?? "AnswerRangeTextT");
                            return false;
                        }
                    }

                }
            }

            if (_currentAnswer == null)
            {
                _currentAnswer = new Survelem()
                {
                    UPRN = StateService.CurrentAddress.UPRN,
                    DateOfSurvey = DateTime.Now.ToString(Constants.DateTimeFormat),

                    Question_Ref = _currentQuestion.Question_Ref,
                    BuildingType = StateService.CurrentAddress.Type,
                    CustomerID = _currentQuestion.CustomerID,
                    CustomerSurveyID = _currentQuestion.CustomerSurveyID,
                    IsCreatedOnClient = true,


                };
            }

            if (!string.IsNullOrEmpty(tb1ForCmb1.Text))
            {
                _currentAnswer.Freetext = tb1ForCmb1.Text;
            }
            else if (dt1ForCmb1.Value != null)
            {
                _currentAnswer.Freetext = dt1ForCmb1.Value.Value.ToString(Constants.DateTimeFormat,CultureInfo.InvariantCulture);

            }
            _currentAnswer.COMMENT = tbComment.Text;

            var o1 = (Option)cmb1.SelectedItem;

            // o1.Choose = true;

            if (o1.ConfirmationRequired && !_confirmations.Contains(o1.Id.ToString()))
            {
                _confirmations.Add(o1.Id.ToString());
                var mb = MessageBox.Show(string.Format("Are you sure you want to select {0}?", o1.Text), "Warning", MessageBoxButton.OKCancel);
                if (mb == MessageBoxResult.Cancel) return false;
            }

            if (o1.NeedToHaveMedia)
            {
                var media = await new DbService().FindMedia(_currentQuestion.Question_Ref, StateService.CurrentAddress.UPRN);

                if (media == null)
                {
                    MessageBox.Show("This answer requires to take a photo. Please take a photo to go to the next question.");
                    return false;
                }
            }

            _currentAnswer.OptionID = o1.Id != Guid.Empty ? (Guid?)o1.Id : null;


            if (cmb2.Items.Count > 1 && cmb2.IsEnabled)
            {
                if (cmb2.SelectedIndex <= 0 && cmb2.IsEnabled) return false;

                var o2 = cmb2.SelectedItem as Option;

                if (o2.ConfirmationRequired && !_confirmations.Contains(o1.Id.ToString()))
                {
                    _confirmations.Add(o2.Id.ToString());
                    var mb = MessageBox.Show(string.Format("Are you sure you want to select {0}?", o2.Text), "Warning", MessageBoxButton.OKCancel);
                    if (mb == MessageBoxResult.Cancel) return false;
                }

                if (o2.NeedToHaveMedia)
                {
                    var media = await new DbService().FindMedia(_currentQuestion.Question_Ref, StateService.CurrentAddress.UPRN);

                    if (media == null)
                    {
                        MessageBox.Show("Please make a photo");
                        return false;
                    }
                }

                //o2.Choose = true;

                _currentAnswer.OptionID2ndry = o2.Id != Guid.Empty ? (Guid?)o2.Id : null;
                // await new DbService().Save(o2, ESyncStatus.NotSynced);
            }


            if (_currentQuestion.Apply_2nd_Question == "YES" && panelTextBoxes.Children.Any())
            {
                foreach (StackPanel child in panelTextBoxes.Children)
                {
                    var item = child.Children[1] as TextBox;

                    if (item == null) continue;

                    var name = item.Tag.ToString();


                    var props = typeof(Survelem).GetProperties();

                    var prop = props.FirstOrDefault(x => x.Name == name);

                    if (prop == null)
                    {
                        throw new Exception("Not found property: " + name);
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

            if (!StateService.IsQA)
                await new DbService().Save(_currentAnswer, ESyncStatus.NotSynced);

            var address = StateService.CurrentAddress;

            if (!address.HasStartedToSurveyed)
            {
                address.HasStartedToSurveyed = true;
                if (!StateService.IsQA)
                    await new DbService().Save(address, ESyncStatus.NotSynced);
            }

            return true;

        }
    }
}