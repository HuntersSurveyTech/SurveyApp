using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using HuntersWP.Db;
using HuntersWP.Models;
using HuntersWP.ServiceReference;
using HuntersWP.Services;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using SQLite;
using Option = HuntersWP.Models.Option;
using QAAddress = HuntersWP.Models.QAAddress;
using QAAddressComment = HuntersWP.Models.QAAddressComment;
using Question = HuntersWP.Models.Question;

namespace HuntersWP.Pages
{
    public partial class LoginPage
    {
        public LoginPage()
        {
            InitializeComponent();

            versionText.Text = "v. " + Helpers.GetAppVersion();
           // tbLogin.Text = "Karl Palmer";
           //tbPassword.Password = "3272";
        }


        private int _optionsQuestionsCheckCount = 0;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

#if DEBUG
            tbLogin.Text = "Phil Summerson";
            tbPassword.Password = "6379";
#endif

            ExNavigationService.ClearNavigationHistory();

            tbNotConnected.Visibility = Helpers.IsNetworkAvailable ? Visibility.Collapsed : Visibility.Visible;
            for (int i = 0; i < 2; i++)
            {
                var edit = ApplicationBar.MenuItems[i] as ApplicationBarMenuItem;
                edit.IsEnabled = Helpers.IsNetworkAvailable;
            }

            if (Helpers.IsNetworkAvailable & StateService.CurrentUserId != Guid.Empty)
            {
                CheckOptionsQuestions();
            }
            else if (StateService.CurrentUserId == Guid.Empty)
            {
                SetTbAndLoginButton(true);
            }
            else
            {
                SetTbAndLoginButton(StateService.LoginEnabled);
            }

        }

        private async void CheckOptionsQuestions()
        {
            return;
            if (StateService.CurrentUserId == Guid.Empty) return;
            if (_optionsQuestionsCheckCount > 2)
            {
                MessageBox.Show("Can not get Options or Questions count from Internet, please try again later.");
                SetTbAndLoginButton(false);
                _optionsQuestionsCheckCount = 0;
                StateService.ProgressIndicatorService.Hide();
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Enabled;
                return;
            }

            StateService.ProgressIndicatorService.Show("Checking Questions and Options");
            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;

            var customers = await new DbService().GetCustomers();

            var netmeraQuestionCount = await new DataLoaderService().CountQuestions(customers);
            var netmeraOptionsCount = await new DataLoaderService().CountOptions(customers);
            if (netmeraOptionsCount == -1 || netmeraQuestionCount == -1)
            {
                MessageBox.Show("Error while chekcing Options and Questions from Internet.");
                _optionsQuestionsCheckCount += 1;
                CheckOptionsQuestions();
                return;
            }
            var dbQuestionCount = await new DbService().Count<Question>();
            var dbOptionsCount = await new DbService().Count<Option>();
            if (dbQuestionCount != netmeraQuestionCount || dbOptionsCount != netmeraOptionsCount)
            {
                SetTbAndLoginButton(false);
            }
            else
            {
                SetTbAndLoginButton(true);
                _optionsQuestionsCheckCount = 0;
            }
            StateService.ProgressIndicatorService.Hide();
            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Enabled;
        }

        private void SetTbAndLoginButton(bool b)
        {
            if (b)
            {
                tbNotUpdated.Visibility = Visibility.Collapsed;
                btnLogin.IsEnabled = true;
                StateService.LoginEnabled = true;
            }
            else
            {
                tbNotUpdated.Visibility = Visibility.Visible;
                btnLogin.IsEnabled = false;
                StateService.LoginEnabled = false;
            }
        }

        private async void ApplicationBarIconButton_OnClick(object sender, EventArgs e)
        {
            if (IsBusy)


                IsBusy = true;
            StateService.ProgressIndicatorService.Show("Logging in...");

            var offLineMode = false;

            LoginReply login = null;
            if (!Helpers.IsNetworkAvailable && StateService.Login == tbLogin.Text && StateService.Password == tbPassword.Password)
            {
                login = new LoginReply() { UserId = StateService.CurrentUserId, IsSuccess = true };
                offLineMode = true;
                Helpers.LogEvent("Login", new Dictionary<string, string> { { "UserId", StateService.CurrentUserId.ToString() }, { "AppVersion", Helpers.GetAppVersion() } });
                MessageBox.Show("Working in Offline mode. Data will be synched when internet connection becomes available");
            }
            else
            {
                login = await new MyNetmeraClient().Login(tbLogin.Text, tbPassword.Password);

            }


            if (ApiResponseProcessor.Execute(login))
            {
           

                StateService.CurrentUserType = (ESurveyorType)login.Type;

                Guid userId = login.UserId.Value;

                Helpers.LogEvent("Login", new Dictionary<string, string> { { "UserId", userId.ToString() }, { "AppVersion", Helpers.GetAppVersion() } });

                if (StateService.PreviousUserId != userId)
                {
                    if (SyncEngine.IsSyncing)
                    {
                        MessageBox.Show("Syncing now, wait");
                        StateService.ProgressIndicatorService.Hide();
                        IsBusy = false;
                        return;
                    }

                    if (StateService.CurrentUserId == Guid.Empty)
                    {
                        StateService.CurrentUserId = userId;
                    }

                    PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;


                    StateService.ProgressIndicatorService.Hide();
                    StateService.ProgressIndicatorService.Show("Syncing previous user data");

                    await SyncEngine.Sync(true);
                    PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;

                    StateService.ProgressIndicatorService.Hide();
                    StateService.ProgressIndicatorService.Show("Clearing database");

                    await new DbService().ClearDb();

                    if (StateService.IsQA)
                    {
                        var myQaAddresses =
                            DataLoaderService.Convert<QAAddress, ServiceReference.QAAddress>(login.QaAddresses);

                        await new DbService().Save(myQaAddresses);

                        var myQaAddressesComments =
    DataLoaderService.Convert<QAAddressComment, ServiceReference.QAAddressComment>(login.QaAddressComments);

                        await new DbService().Save(myQaAddressesComments);

                    }


                    StateService.ProgressIndicatorService.Hide();
                    StateService.ProgressIndicatorService.Show("Loading data");

                    bool isError = false;
                    StateService.CurrentUserTimeStamp = login.Timestamp;
                    StateService.CurrentUserId = userId;
                    try
                    {
                        await new MyNetmeraClient().DownloadUserData();
                    }
                    catch (Exception exc)
                    {
                        isError = true;
                        Helpers.LogException(exc, "DownloadUserData");
                        MessageBox.Show(string.IsNullOrEmpty(exc.Message) ? "Error while loading data. Try again" : exc.Message);
                    }

                    if (isError)
                    {
                        await new MyNetmeraClient().ClearDb();
                    }

                    StateService.ProgressIndicatorService.Hide();
                    PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Enabled;

                    if (isError)
                    {
                        return;
                    }

                    StateService.PreviousUserId = userId;

                    StateService.Login = tbLogin.Text;
                    StateService.Password = tbPassword.Password;


                }
                else
                {
                    bool isError = false;
                    //Process timestamp
                    if (!offLineMode && StateService.CurrentUserTimeStamp != login.Timestamp)
                    {
                        StateService.ProgressIndicatorService.Hide();
                        StateService.ProgressIndicatorService.Show("Refreshing user data");

                        try
                        {
                            await new MyNetmeraClient().RefreshUserData();
                            StateService.CurrentUserTimeStamp = login.Timestamp;
                        }
                        catch (Exception exc)
                        {
                            isError = true;
                            Helpers.LogException(exc, "RefreshUserData");
                            MessageBox.Show(string.IsNullOrEmpty(exc.Message) ? "Error while refreshing data. Try again" : exc.Message);
                        }


                    }

                    try
                    {
                        if (!isError)
                        {
                            StateService.ProgressIndicatorService.Hide();
                            StateService.ProgressIndicatorService.Show("Processing address move");
                            await new MyNetmeraClient().RefreshAddressMoves(login.AddressMoves);
                        }
                      
                    }
                    catch (Exception exc)
                    {
                        isError = true;
                        Helpers.LogException(exc, "RefreshAddressMoves");
                        MessageBox.Show(string.IsNullOrEmpty(exc.Message) ? "Error while processing address moves. Try again" : exc.Message);
                    }

                    //update address properties
                    try
                    {

                        StateService.ProgressIndicatorService.Hide();
                        StateService.ProgressIndicatorService.Show("Refreshing addresses properties");

                        if(!offLineMode)
                            await new MyNetmeraClient().RefreshAddressesProperties();
                    }
                    catch (Exception exc)
                    {
                        isError = true;
                        Helpers.LogException(exc, "RefreshAddressesProperties");
                        MessageBox.Show(string.IsNullOrEmpty(exc.Message) ? "Error while refreshing data. Try again" : exc.Message);
                    }
                    //

                    if (isError)
                    {
                        StateService.ProgressIndicatorService.Hide();
                        IsBusy = false;
                        return;
                    }

                   


                    SyncEngine.Sync();



                }

                SyncEngine.InitializeTimer();
                ExNavigationService.Navigate<SelectCustomersPage>();
            }
            StateService.ProgressIndicatorService.Hide();
            IsBusy = false;

        }

        private async void BtnUploadDb_OnClick(object sender, EventArgs eventArgs)
        {
            StateService.ProgressIndicatorService.Show("Uploading db");
            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                iso.CopyFile(DbService.DB_NAME, DbService.DB_NAME_COPY, true);
                using (var file = iso.OpenFile(DbService.DB_NAME_COPY, FileMode.Open, FileAccess.Read))
                {
                    //var data = new byte[file.Length];

                    //await file.ReadAsync(data, 0, data.Length);

                    try
                    {
                        await UploadToCloud(file);
                        MessageBox.Show("Done uploading");
                    }
                    catch (Exception ex)
                    {
                        Helpers.LogException(ex, "UploadToCloud");
                        MessageBox.Show(ex.Message,"Error while uploading db",MessageBoxButton.OK);
                    }

                    finally
                    {
                        file.Close();

                        StateService.ProgressIndicatorService.Hide();
                        PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Enabled;
                    }




                }
            }




        }

        private static async Task<string> UploadToCloud(Stream s)
        {
            var account =
                new CloudStorageAccount(
                    new StorageCredentials("hunterscollect",
                        "kgn29dcfazCtxIT8UZpPIUuFX6MiLaFSl11Vu4HPa2V42j5Gk7WtZA/KV4gdrvPvU+evRaRLXDJ6r7Eva4CIXQ=="),
                    true);


            var client = account.CreateCloudBlobClient();

            var container = client.GetContainerReference("hunters-db");
            //await container.CreateIfNotExistsAsync();

            //await container.SetPermissionsAsync(new BlobContainerPermissions() { PublicAccess = BlobContainerPublicAccessType.Off });
            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;

            var blob = container.GetBlockBlobReference(Guid.NewGuid().ToString() + "_v2db_" + StateService.CurrentUserId + ".sqlite3");

            await blob.UploadFromStreamAsync(s);
            //var size = 500;
            //int i = 0;
            //var transferred = 0;
            //while (i < data.Length)
            //{
            //    var transfer = data.Length - i < size ? data.Length - i : size;
            //    transferred += transfer;
            //    await blob.UploadFromByteArrayAsync(data, i, transfer);
            //    await blob.UploadFr
            //    i += size;
            //}


            //  Debug.WriteLine(transferred);
            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Enabled;
            return blob.Uri.ToString();
        }

        private async void BtnUpdateQuestionsAndOptions_OnClick(object sender, EventArgs eventArgs)
        {
            if (StateService.CurrentUserId == Guid.Empty) return;

            StateService.ProgressIndicatorService.Show("Updating questions and options");
            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;

            try
            {
                await new MyNetmeraClient().UpdateQuestionsAndOptions();
                MessageBox.Show(string.Format("Updated questions ({0}) and options ({1})", await new DbService().Count<Question>(), await new DbService().Count<Option>()));


            }
            catch (DataLoadException exc)
            {
                MessageBox.Show(string.IsNullOrEmpty(exc.Message) ? "Error while loading data. Try again" : exc.Message);
            }

            StateService.ProgressIndicatorService.Hide();
            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Enabled;
            CheckOptionsQuestions();

        }
    }
}