using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Net.Browser;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Xml.Linq;
using HuntersWP.Db;
using HuntersWP.Models;
using HuntersWP.Services;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Info;
using Microsoft.Phone.Shell;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Netmera;
using Newtonsoft.Json;
using SQLite;

namespace HuntersWP.Pages
{
    public partial class LoginPage
    {
        public LoginPage()
        {
            InitializeComponent();

            versionText.Text = "v. " + App.GetVersion();
            //tbLogin.Text = "AmarH";
            //tbPassword.Password = "1234qwer";
        }


        private int _optionsQuestionsCheckCount = 0;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ExNavigationService.ClearNavigationHistory();

            tbNotConnected.Visibility = Helpers.IsNetworkAvailable ? Visibility.Collapsed : Visibility.Visible;
            for (int i = 0; i < 2; i++)
            {
                var edit = ApplicationBar.MenuItems[i] as ApplicationBarMenuItem;
                edit.IsEnabled = Helpers.IsNetworkAvailable;                
            }

            if (Helpers.IsNetworkAvailable & StateService.CurrentUserId != 0)
            {
                CheckOptionsQuestions();
            }
            else if (StateService.CurrentUserId == 0)
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
            if (StateService.CurrentUserId == 0) return;
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

            var netmeraQuestionCount = await new MyNetmeraClient().Count<Question>(new NetmeraService("Questions"));
            var netmeraOptionsCount = await new MyNetmeraClient().Count<Option>(new NetmeraService("Options"));
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


            ApiResponse<List<Surveyor>> login = null;
            if (!Helpers.IsNetworkAvailable && StateService.Login == tbLogin.Text && StateService.Password == tbPassword.Password)
            {
                login = new ApiResponse<List<Surveyor>>() { Data = new List<Surveyor>() { new Surveyor() { id = StateService.CurrentUserId } }, IsSuccess = true };
                MessageBox.Show("Working in Offline mode. Data will be synched when internet connection becomes available");
            }
            else
            {
                login = await new MyNetmeraClient().Login(tbLogin.Text, tbPassword.Password);

            }


            if (ApiResponseProcessor.Execute(login))
            {

                var userId = login.Data.First().id;

                if (StateService.PreviousUserId != userId)
                {
                    if (SyncEngine.IsSyncing)
                    {
                        MessageBox.Show("Syncing now, wait");
                        StateService.ProgressIndicatorService.Hide();
                        IsBusy = false;
                        return;
                    }

                    PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;


                    StateService.ProgressIndicatorService.Hide();
                    StateService.ProgressIndicatorService.Show("Syncing previous user data");

                    await SyncEngine.Sync(true);
                    PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;

                    StateService.ProgressIndicatorService.Hide();
                    StateService.ProgressIndicatorService.Show("Clearing database");

                    await new DbService().ClearDb();


                    StateService.ProgressIndicatorService.Hide();
                    StateService.ProgressIndicatorService.Show("Loading data");


                    var r = await new MyNetmeraClient().SyncUserData(userId);

                    StateService.ProgressIndicatorService.Hide();
                    PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Enabled;

                    if (!r.IsSuccess)
                    {
                        MessageBox.Show(string.IsNullOrEmpty(r.Message) ? "Error while loading data. Try again" : r.Message);
                        return;
                    }

                    StateService.CurrentUserId = userId;
                    StateService.PreviousUserId = userId;

                    StateService.Login = tbLogin.Text;
                    StateService.Password = tbPassword.Password;


                }
                else
                {
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
                iso.CopyFile("db1.sqlite", "db1copy.sqlite",true);
                using (var file = iso.OpenFile("db1copy.sqlite", FileMode.Open, FileAccess.Read))
                {
                    //var data = new byte[file.Length];

                    //await file.ReadAsync(data, 0, data.Length);

                    try
                    {
                        await UploadToCloud(file);
                        MessageBox.Show("Done uploading");
                    }
                    catch (Exception exc)
                    {
                        DevRainErrorHandler.Log("Fail db uploading",exc);
                        MessageBox.Show("Error while uploading db");
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
                    new StorageCredentials("hunters",
                        "hSY6n6KoHvqTfk6NByrZODMlsqWfrA9g030I+Pr9QAzt5NK2vmh0KTzRiydq1kCkFu6Ot3lKAty+30EwjJA2+A=="),
                    true);


            var client = account.CreateCloudBlobClient();

            var container = client.GetContainerReference("hunters-db");
            //await container.CreateIfNotExistsAsync();

            //await container.SetPermissionsAsync(new BlobContainerPermissions() { PublicAccess = BlobContainerPublicAccessType.Off });
            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;

            var blob = container.GetBlockBlobReference(Guid.NewGuid().ToString() + "_" + StateService.CurrentUserId + ".sqlite3");

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
            if (StateService.CurrentUserId == 0) return;

            StateService.ProgressIndicatorService.Show("Updating questions and options");
            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;

            var response = await new MyNetmeraClient().UpdateQuestionsAndOptions(StateService.CurrentUserId);

            if (!response.IsSuccess)
            {
                MessageBox.Show(string.IsNullOrEmpty(response.Message) ? "Error while loading data. Try again" : response.Message);
            
            }
            else
            {
                MessageBox.Show(string.Format("Updated questions ({0}) and options ({1})", await new DbService().Count<Question>(), await new DbService().Count<Option>()));
            }

       
            StateService.ProgressIndicatorService.Hide();
            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Enabled;
            CheckOptionsQuestions();

        }
    }

    public class DevRainErrorHandler
    {
        private const String CrashDirectoryName = "DevRainCrashLogs";
        private static int _appId;
        private static bool _initialized;


        public static void Initialize(Application application, int appId)
        {
            if (_initialized)
            {
                throw new Exception("DevRainErrorHandler: Already initialized");
            }

            _appId = appId;
            application.UnhandledException -= Current_UnhandledException;
            application.UnhandledException += Current_UnhandledException;

            _initialized = true;

            HandleCrashes();
        }


        public static void Log(string message, Exception exception)
        {
            if (!_initialized) return;

            Task.Factory.StartNew(async () =>
            {
                ProcessException(message, exception);
                HandleCrashes();
            });
        }

        static async void Current_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            ProcessException(e.ExceptionObject.Message, e.ExceptionObject);
            //HandleCrashes();
        }

        static void ProcessException(string message, Exception e)
        {
            var messageBuilder = new StringBuilder();
            messageBuilder.Append(message);
            messageBuilder.AppendLine();
            messageBuilder.Append(e.Message);

            var item = new DataLogItem()
            {
                DateTimeTicks = DateTime.UtcNow.Ticks,
                Description = CreateHeader(),
                AppVersion = DeviceHelper.GetAppVersion().ToString(),
                Message = messageBuilder.ToString(),
                StackTrace = CreateStackTrace(e),
                Type = (byte)EDataItemType.Error
            };


            SaveLog(JsonConvert.SerializeObject(item));
        }

        static String CreateHeader()
        {
            var builder = new StringBuilder();
            builder.AppendFormat("Package: {0}", Application.Current.GetType().Namespace);
            builder.AppendLine();
            builder.AppendFormat("Version: {0}", DeviceHelper.GetAppVersion().ToString());
            builder.AppendLine();
            builder.AppendFormat("OS: Windows Phone {0}", Environment.OSVersion.Version.ToString());
            builder.AppendLine();
            builder.AppendFormat("Manufacturer: {0}", DeviceHelper.GetDeviceManufacturer());
            builder.AppendLine();
            builder.AppendFormat("Model: {0}", DeviceHelper.GetDeviceType());

            return builder.ToString();
        }

        static String CreateStackTrace(Exception exception)
        {
            var builder = new StringBuilder();
            builder.Append(exception.GetType().ToString());
            builder.Append(": ");
            builder.Append(string.IsNullOrEmpty(exception.Message) ? "No reason" : exception.Message);
            builder.AppendLine();
            builder.Append(string.IsNullOrEmpty(exception.StackTrace) ? "  at unknown location" : exception.StackTrace);

            var inner = exception.InnerException;
            if ((inner != null) && (!string.IsNullOrEmpty(inner.StackTrace)))
            {
                builder.AppendLine();
                builder.AppendLine("Inner Exception");
                builder.Append(inner.StackTrace);
            }

            return builder.ToString().Trim();
        }

        private static void SaveLog(String log)
        {
            try
            {
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (!store.DirectoryExists(CrashDirectoryName))
                    {
                        store.CreateDirectory(CrashDirectoryName);
                    }

                    var filename = string.Format("crash{0}.log", Guid.NewGuid());
                    using (var stream = store.CreateFile(Path.Combine(CrashDirectoryName, filename)))
                    {
                        using (var writer = new StreamWriter(stream))
                        {
                            writer.Write(log);
                        }
                        stream.Close();
                    }
                }
            }
            catch
            {
                // Ignore all exceptions
            }
        }

        private static bool _isProcessingCrashes;
        static readonly object _locker = new object();


        async static void HandleCrashes()
        {
            lock (_locker)
            {
                if (_isProcessingCrashes) return;
                _isProcessingCrashes = true;
            }
            try
            {
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (store.DirectoryExists(CrashDirectoryName))
                    {
                        var filenames = store.GetFileNames(CrashDirectoryName + "\\crash*.log");

                        if (filenames.Length > 0)
                        {
                            await SendCrashes(store, filenames);
                        }

                    }
                }
                _isProcessingCrashes = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                // Ignore all exceptions
                _isProcessingCrashes = false;
            }
        }

        private static async Task SendCrashes(IsolatedStorageFile store, string[] filenames)
        {
            foreach (var filename in filenames)
            {
                try
                {

                    await SendWebRequest(store, filename);


                }
                catch (Exception)
                {
                    store.DeleteFile(Path.Combine(CrashDirectoryName, filename));
                }
            }
        }

        private static void DeleteCrashes(IsolatedStorageFile store, string filename)
        {
            store.DeleteFile(Path.Combine(CrashDirectoryName, filename));
        }

        async static Task<object> SendWebRequest(IsolatedStorageFile store, string filename)
        {
            TaskCompletionSource<object> result = new TaskCompletionSource<object>();


            var log = "";
            using (Stream fileStream = store.OpenFile(Path.Combine(CrashDirectoryName, filename), FileMode.Open))
            {
                using (var reader = new StreamReader(fileStream))
                {
                    log = reader.ReadToEnd();
                }
                fileStream.Close();
            }

            var item = JsonConvert.DeserializeObject<DataLogItem>(log);


            var body = string.Format("Message={0}&DateTimeTicks={1}&AppVersion={2}&ApiKey={3}&StackTrace={4}&Description={5}&Type={6}",
                item.Message, item.DateTimeTicks, item.AppVersion, _appId, item.StackTrace, item.Description, item.Type);


            var request = WebRequestCreator.ClientHttp.Create(new Uri("http://devrainerrorservice.cloudapp.net/api/data"));
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            request.BeginGetRequestStream(requestResult =>
            {
                try
                {
                    var stream = request.EndGetRequestStream(requestResult);
                    var byteArray = Encoding.UTF8.GetBytes(body);
                    stream.Write(byteArray, 0, body.Length);
                    stream.Close();

                    request.BeginGetResponse(responseResult =>
                    {
                        var deleteCrashes = true;
                        try
                        {
                            var response = request.EndGetResponse(responseResult);
                        }
                        catch (WebException e)
                        {
                            if ((e.Status == WebExceptionStatus.ConnectFailure) ||
                                (e.Status == WebExceptionStatus.ReceiveFailure) ||
                                (e.Status == WebExceptionStatus.SendFailure) ||
                                (e.Status == WebExceptionStatus.Timeout) ||
                                (e.Status == WebExceptionStatus.UnknownError))
                            {
                                deleteCrashes = false;
                            }
                        }
                        catch (Exception)
                        {
                        }
                        finally
                        {
                            if (deleteCrashes)
                            {
                                DeleteCrashes(store, filename);
                            }
                            result.SetResult(null);
                        }
                    }, null);
                }
                catch (Exception)
                {
                    result.SetResult(null);
                }
            }, null);


            return await result.Task;
        }
    }

    public enum EDataItemType
    {
        Error,
        Event
    }

    public class DataLogItem
    {
        public byte Type { get; set; }
        public string AppVersion { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public long DateTimeTicks { get; set; }
        public string Description { get; set; }
    }

    public class DeviceHelper
    {
        public static string GetAssemblyFileVersion()
        {
            var assembly = Assembly.GetCallingAssembly();
            var versionString = assembly.GetCustomAttributes(false)
                .OfType<AssemblyFileVersionAttribute>()
                .First()
                .Version;

            return versionString;
        }


        public static Version GetAppVersion()
        {
            try
            {
                var data = ApplicationManifestHelper.Read();

                Version version;
                if (Version.TryParse(data.Version, out version))
                {
                    return version;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return default(Version);

            //try
            //{
            //	var doc = XDocument.Load("WMAppManifest.xml");
            //	var xAttribute = doc.Descendants("App").First().Attribute("Version");
            //	if (xAttribute != null)
            //	{
            //		var version = xAttribute.Value;
            //		if (!string.IsNullOrEmpty(version))
            //		{
            //			Version result;
            //			if (Version.TryParse(version, out result))
            //			{
            //				return result;
            //			}
            //		}
            //	}
            //}
            //// ReSharper disable EmptyGeneralCatchClause
            //catch
            //// ReSharper restore EmptyGeneralCatchClause
            //{
            //}
            //return default(Version);
        }

        public static long GetTimestamp(DateTime dateTime)
        {
            long ticks = dateTime.Ticks - new DateTime(1970, 1, 1).Ticks;
            ticks /= 10000000; //Convert windows ticks to seconds
            return ticks;
        }

        public static TimeSpan GetTimeSpan(long timestamp)
        {
            timestamp *= 10000000;

            timestamp += new DateTime(1970, 1, 1).Ticks;


            return TimeSpan.FromTicks(timestamp);
        }

        public static PhoneApplicationFrame GetCurrentApplicationFrame()
        {
            var frame = Application.Current.RootVisual as PhoneApplicationFrame;
            return frame;
        }

        public static PhoneApplicationPage GetCurrentPage()
        {
            var frame = GetCurrentApplicationFrame();
            var startPage = frame.Content as PhoneApplicationPage;

            return startPage;
        }

        public static PageOrientation GetCurrentOrientation()
        {
            return GetCurrentPage().Orientation;
        }

        public static string GetDeviceUniqueID()
        {
            try
            {
                byte[] result = null;
                object uniqueId;
                if (DeviceExtendedProperties.TryGetValue("DeviceUniqueId", out uniqueId))
                    result = (byte[])uniqueId;

                return Convert.ToBase64String(result);
            }
            catch
            {
                return string.Empty;
            }
        }




        public static string GetDeviceManufacturer()
        {
            try
            {
                return DeviceExtendedProperties.GetValue("DeviceManufacturer").ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string GetDeviceType()
        {
            try
            {
                return DeviceExtendedProperties.GetValue("DeviceName").ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string GetOsVersion()
        {
            return string.Format("WP {0}", System.Environment.OSVersion.Version);
        }

        public static bool IsLowLevelWP8Device()
        {
            var t = GetDeviceType();

            if (t == null) return false;

            return t.Contains("520") || t.Contains("525") || t.Contains("620") || t.Contains("625") || t.Contains("720") ||
                   t.Contains("725");
        }


        public static bool IsWP8
        {
            get { return Environment.OSVersion.Version >= WP8TargetedVersion; }
        }

        private static Version WP8TargetedVersion = new Version(8, 0);


        //public static IPAddress GetIpAddress()
        //{
        //	List<string> ipAddresses = new List<string>();

        //	var hostnames = NetworkInformation.GetHostNames();
        //	foreach (var hn in hostnames)
        //	{
        //		if (hn.IPInformation != null)
        //		{
        //			string ipAddress = hn.DisplayName;
        //			ipAddresses.Add(ipAddress);
        //		}
        //	}

        //	IPAddress address = IPAddress.Parse(ipAddresses[0]);
        //	return address;
        //}
    }

    public static class ApplicationManifestHelper
    {
        public static ManifestData Read()
        {
            var data = new ManifestData();
            var manifestXml = XElement.Load("WMAppManifest.xml");
            var appElement = manifestXml.Descendants("App").FirstOrDefault();

            if (appElement != null)
            {
                data.ProductId = (string)appElement.Attribute("ProductID");
                data.Title = (string)appElement.Attribute("Title");
                data.RuntimeType = (string)appElement.Attribute("RuntimeType");
                data.Version = (string)appElement.Attribute("Version");
                data.Genre = (string)appElement.Attribute("Genre");
                data.Author = (string)appElement.Attribute("Author");
                data.Description = (string)appElement.Attribute("Description");
                data.Publisher = (string)appElement.Attribute("Publisher");
            }

            appElement = manifestXml.Descendants("PrimaryToken").FirstOrDefault();

            if (appElement != null)
            {
                data.TokenId = (string)appElement.Attribute("TokenID");
            }

            return data;
        }
    }

    public class ManifestData
    {
        public string TokenId;
        public string Genre;
        public string Author;
        public string Description;
        public string Publisher;
        public string Title;
        public string Version;
        public string RuntimeType;
        public string ProductId;

        public string DisplayVersion
        {
            get
            {
                if (string.IsNullOrEmpty(Version)) return string.Empty;
                return Version.Substring(0, Version.IndexOf(".", StringComparison.Ordinal) + 2);
            }
        }
    }
}