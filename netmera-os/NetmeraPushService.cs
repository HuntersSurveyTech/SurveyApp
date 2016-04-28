using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using Microsoft.Phone.Notification;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Microsoft.Phone.Controls;
using System.Windows.Navigation;
using System.Threading;
using System.Text.RegularExpressions;

namespace Netmera
{
    /// <summary>
    ///  This is the main class to manage push notifications in Netmera. With the help
    ///  of this class, you can register/unregister devices into different groups to get notification.
    /// </summary>
    public static class NetmeraPushService
    {
        private static HttpNotificationChannel Channel;
        private static string Service_Name = "netmera";

        private static List<String> Groups;
        private static String Channel_Name;
        private static NetmeraGeoLocation Device_Location;

        private static bool richPush = false;
        private static String richPushId;

        /// <summary>
        /// Registers device for Toast Notifications
        /// </summary>
        /// <param name="channelName">Channel name to be registered</param>
        /// <param name="callback">Method to be called just after registration</param>
        public static void register(String channelName, Action<Exception> callback)
        {
            NetmeraDeviceDetail device = new NetmeraDeviceDetail();
            NetmeraPushService.register(channelName, device, callback);
        }

        /// <summary>
        /// Registers device to specified groups, if <seealso cref="NetmeraDeviceDetail"/> groups are set; otherwise registers to broadcast group
        /// </summary>
        /// <param name="channelName">Channel name to be registered</param>
        /// <param name="deviceDetail"><seealso cref="NetmeraDeviceDetail"/> object keeping device details</param>
        /// <param name="callback">Method to be called just after registration</param>
        public static void register(String channelName, NetmeraDeviceDetail deviceDetail, Action<Exception> callback)
        {
            Channel_Name = channelName;
            if (deviceDetail != null)
            {
                Groups = deviceDetail.getDeviceGroups();
                Device_Location = deviceDetail.getDeviceLocation();
            }

            Channel = HttpNotificationChannel.Find(Channel_Name);
            if (Channel == null)
            {
                Channel = new HttpNotificationChannel(Channel_Name, Service_Name);
                Channel.ChannelUriUpdated += (s, e) =>
                {
                    ChannelUriUpdated(s, e, callback);
                };

                try
                {
                    Channel.Open();
                }
                catch (InvalidOperationException ex)
                {
                    if (callback != null)
                        callback(new NetmeraException(NetmeraException.ErrorCode.EC_IO_EXCEPTION, "Maximum channel number have already been opened in this device." + ex.Message));
                }
            }
            else
            {
                //RegisterForNotifications(callback);
                RegisterWithNotificationService(callback);
            }
        }

        /// <summary>
        /// Unregisters device
        /// </summary>
        /// <param name="channelName">Channel name to be unregistered</param>
        /// <param name="callback">Method to be called just after unregister</param>
        public static void unregister(String channelName, Action<Exception> callback)
        {
            NetmeraDeviceDetail device = new NetmeraDeviceDetail();
            NetmeraPushService.unregister(channelName, device, callback);
        }

        /// <summary>
        /// Unregisters device from specified groups, if <seealso cref="NetmeraDeviceDetail"/> groups are set; otherwise unregisters from broadcast group
        /// </summary>
        /// <param name="channelName">Channel name to be unregistered</param>
        /// <param name="deviceDetail"><seealso cref="NetmeraDeviceDetail"/> object keeping device details</param>
        /// <param name="callback">>Method to be called just after unregister</param>
        public static void unregister(String channelName, NetmeraDeviceDetail deviceDetail, Action<Exception> callback)
        {
            Channel_Name = channelName;
            if (deviceDetail != null)
            {
                Groups = deviceDetail.getDeviceGroups();
                Device_Location = deviceDetail.getDeviceLocation();
            }

            Channel = HttpNotificationChannel.Find(Channel_Name);
            if (Channel != null)
            {
                UnregisterFromNotificationService(ex =>
                {
                    if (callback != null)
                        callback(ex);
                    //if (ex != null)
                    //    callback(ex);
                    //else
                    //    Channel.Close();
                });
            }
            else
            {
                if (callback != null)
                    callback(new NetmeraException(NetmeraException.ErrorCode.EC_PUSH_DEVICE_NOT_REGISTERED, "Unregister failed since such a channel not found!"));
            }
        }

        /// <summary>
        /// Gets whether the device is registered with the given channel
        /// </summary>
        /// <param name="channelName">Channel name</param>
        /// <returns>True if the device is registered; otherwise it returns false</returns>
        public static bool isRegistered(String channelName)
        {
            Channel_Name = channelName;
            Channel = HttpNotificationChannel.Find(Channel_Name);
            return Channel != null;
        }

        /// <summary>
        /// Retrieves the all groups of all registered devices.
        /// </summary>
        /// <param name="callback">Method to be called just after getting device groups</param>
        public static void getDeviceGroups(Action<List<String>, Exception> callback)
        {
            Dictionary<string, object> postParameters = new Dictionary<string, object>();
            postParameters.Add(NetmeraConstants.Netmera_Push_Apikey, NetmeraClient.securityToken);

            String url = NetmeraConstants.Netmera_Domain_Url + NetmeraConstants.Netmera_Push_Server_Url + NetmeraConstants.Netmera_Push_Get_Device_Groups;

            if (callback != null)
                NetmeraHttpUtils.getDeviceGroups(url, postParameters, callback);
        }

        private static void ChannelUriUpdated(object sender, NotificationChannelUriEventArgs e, Action<Exception> callback)
        {
            Channel = HttpNotificationChannel.Find(Channel_Name);
            if (Channel != null)
            {
                //if (!channel.IsShellTileBound)
                //{
                //    // you can register the phone application to recieve tile images from remote servers [this is optional]
                //    var uris = new Collection<Uri>(Allowed_Domains);
                //    channel.BindToShellTile(uris);
                //    //channel.BindToShellTile();
                //}

                if (!Channel.IsShellToastBound)
                {
                    Channel.BindToShellToast();
                }

                RegisterForNotifications(callback);
                //RegisterWithNotificationService(callback);
            }
            else
            {
                if (callback != null)
                    callback(new NetmeraException(NetmeraException.ErrorCode.EC_PUSH_DEVICE_NOT_REGISTERED, "Channel URI update failed"));
            }
        }

        private static void RegisterForNotifications(Action<Exception> callback)
        {
            string uri = Channel.ChannelUri.AbsoluteUri;
            RegisterWithNotificationService(callback);
            //Channel.ShellToastNotificationReceived += (s, e) => Deployment.Current.Dispatcher.BeginInvoke(() =>
            //{
            //});
            //Channel.HttpNotificationReceived += (s, e) => Deployment.Current.Dispatcher.BeginInvoke(RAWRECEIVED);
            //Channel.ErrorOccurred += (s, e) => Deployment.Current.Dispatcher.BeginInvoke(ERROROCCURED);
        }

        private static void RegisterWithNotificationService(Action<Exception> callback)
        {
            String groupString = null;

            if (Groups != null)
            {
                groupString = JsonConvert.SerializeObject(Groups);
            }

            Dictionary<string, object> postParameters = new Dictionary<string, object>();
            postParameters.Add(NetmeraConstants.Netmera_Push_Registration_Id, Channel.ChannelUri.ToString());
            postParameters.Add(NetmeraConstants.Netmera_Push_Channel, NetmeraConstants.Netmera_Push_Type_Wp);
            postParameters.Add(NetmeraConstants.Netmera_Push_Apikey, NetmeraClient.securityToken);
            postParameters.Add(NetmeraConstants.Netmera_Push_Device_Groups, groupString);
            if (Device_Location != null)
            {
                postParameters.Add(NetmeraConstants.Netmera_Push_Latitude_Params, Device_Location.getLatitude());
                postParameters.Add(NetmeraConstants.Netmera_Push_Longitude_Params, Device_Location.getLongitude());
            }

            String url = NetmeraConstants.Netmera_Domain_Url + NetmeraConstants.Netmera_Push_Server_Url + NetmeraConstants.Netmera_Push_Register;

            NetmeraHttpUtils.registerUnregisterPush(url, postParameters, ex =>
            {
                if (ex != null)
                {
                    unregister(Channel_Name, null);
                    if (callback != null)
                        callback(new NetmeraException(NetmeraException.ErrorCode.EC_PUSH_DEVICE_NOT_REGISTERED, "Registration failed."));
                }
                else if (callback != null)
                    callback(ex);
            });
        }

        private static void UnregisterFromNotificationService(Action<Exception> callback)
        {
            String channelUri = Channel.ChannelUri.ToString();
            String groupString = null;

            if (Groups != null)
            {
                groupString = JsonConvert.SerializeObject(Groups);
            }
            if (Groups == null || Groups.Count == 0)
            {
                Channel.Close();
            }

            Dictionary<string, object> postParameters = new Dictionary<string, object>();
            postParameters.Add(NetmeraConstants.Netmera_Push_Registration_Id, channelUri);
            postParameters.Add(NetmeraConstants.Netmera_Push_Channel, NetmeraConstants.Netmera_Push_Type_Wp);
            postParameters.Add(NetmeraConstants.Netmera_Push_Apikey, NetmeraClient.securityToken);
            postParameters.Add(NetmeraConstants.Netmera_Push_Device_Groups, groupString);

            String url = NetmeraConstants.Netmera_Domain_Url + NetmeraConstants.Netmera_Push_Server_Url + NetmeraConstants.Netmera_Push_Unregister;

            NetmeraHttpUtils.registerUnregisterPush(url, postParameters, ex =>
            {
                if (ex != null)
                {
                    if (callback != null)
                        callback(new NetmeraException(NetmeraException.ErrorCode.EC_PUSH_DEVICE_NOT_REGISTERED, "Unregister failed."));
                }
                else if (callback != null)
                    callback(ex);
            });
        }

        /// <summary>
        /// Gets device details if registered
        /// </summary>
        /// <param name="channelName">Channel name the device registered</param>
        /// <param name="callback">Method to be called just after getting details</param>
        public static void getDeviceDetail(String channelName, Action<NetmeraDeviceDetail, Exception> callback)
        {
            Channel_Name = channelName;
            Channel = HttpNotificationChannel.Find(Channel_Name);
            if (Channel != null)
            {
                NetmeraDeviceDetail deviceDetail = new NetmeraDeviceDetail(Channel.ChannelUri.ToString());

                Dictionary<string, object> postParameters = new Dictionary<string, object>();
                postParameters.Add(NetmeraConstants.Netmera_Push_Registration_Id, deviceDetail.regId);
                postParameters.Add(NetmeraConstants.Netmera_Push_Apikey, NetmeraClient.securityToken);

                String url = NetmeraConstants.Netmera_Domain_Url + NetmeraConstants.Netmera_Push_Server_Url + NetmeraConstants.Netmera_Push_Device_Details;

                NetmeraHttpUtils.getDeviceDetails(url, postParameters, deviceDetail, callback);
            }
            else if (callback != null)
                callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_PUSH_DEVICE_NOT_REGISTERED, "A channel with such a name is not registered"));
        }

        /// <summary>
        /// Checks whether a rich push is captured when the application is started or not.
        /// </summary>
        /// <returns>Whether it is rich push or not</returns>
        public static bool isRichPush()
        {
            return richPush && !String.IsNullOrEmpty(richPushId);
        }

        /// <summary>
        /// Get the required parameters to handle rich push. After that, you can check whether a rich push has received or not.
        /// </summary>
        /// <param name="page">Application page to check the rich push. You should put <code>this</code> here</param>
        public static void checkRichPush(PhoneApplicationPage page)
        {
            IDictionary<string, string> queryString = page.NavigationContext.QueryString;
            if (queryString.ContainsKey(NetmeraConstants.Netmera_Push_Type) && queryString[NetmeraConstants.Netmera_Push_Type] == "RICH")
            {
                richPush = true;
            }
            if (richPush && queryString.ContainsKey(NetmeraConstants.Netmera_Rich_Push_Id))
            {
                richPushId = queryString[NetmeraConstants.Netmera_Rich_Push_Id].ToString();
                richPush = !String.IsNullOrEmpty(richPushId);
            }
        }

        /// <summary>
        /// Handles rich push. That is, it gives you a WebBrowser object filled with rich push HTML content.
        /// </summary>
        /// <param name="callback"></param>
        public static void handleRichPush(Action<WebBrowser, Exception> callback)
        {
            //richPush = true;
            //richPushId = "5113c5e4e4b0533d1f2d5374";
            if (richPush && richPushId != null)
            {
                JObject userJson = new JObject();
                NetmeraUser user = NetmeraUser.getCurrentUser();
                if (user != null)
                {
                    userJson.Add("name", user.getName());
                    userJson.Add("surname", user.getSurname());
                }

                getRichPushContent((pushContent, ex) =>
                {
                    resetRichPush();

                    if (ex != null)
                    {
                        if (callback != null)
                            callback(null, ex);
                    }
                    else
                    {
                        if (pushContent.Value<String>("message") == "OK")
                        {
                            pushContent = pushContent.Value<JObject>("result");
                            String pushData = pushContent.Value<String>(NetmeraConstants.Netmera_Rich_Push_Html);
                            JObject clientJson = pushContent.Value<JObject>(NetmeraConstants.Netmera_Rich_Push_Client_Json);
                            String userString = userJson.ToString();
                            String clientString = clientJson.ToString();

                            Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                WebBrowser webBrowser = new WebBrowser();
                                webBrowser.IsScriptEnabled = true;
                                webBrowser.LoadCompleted += new LoadCompletedEventHandler((s, e) =>
                                {
                                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                                    {
                                        String html = webBrowser.SaveToString();
                                        clientString = Regex.Replace(clientString, "\r", "");
                                        clientString = Regex.Replace(clientString, "\n", "");
                                        userString = Regex.Replace(userString, "\r", "");
                                        userString = Regex.Replace(userString, "\n", "");
                                        String jscript = "var customJson='" + clientString + "'; var currentUser='" + userString + "'; customJson=JSON.parse(customJson); currentUser=JSON.parse(currentUser); getClientParams(customJson,currentUser);";
                                        try
                                        {
                                            webBrowser.InvokeScript("eval", jscript);
                                            if (callback != null)
                                                callback(webBrowser, null);
                                        }
                                        catch (Exception)
                                        {
                                            if (callback != null)
                                                callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_RICH_PUSH_JAVASCRIPT_ERROR, "Javascript error occured. Make sure that you put the proper Javascript code in your push message."));
                                        }
                                    });
                                });
                                webBrowser.NavigateToString(pushData);
                            });
                        }
                        else
                        {
                            if (callback != null)
                                callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_RICH_PUSH_CONTENT_ERROR, "Rich push -HTML- content is not retrieved properly."));
                        }
                    }
                });
            }
            else
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_RICH_PUSH_ID_EMPTY, "Rich push id is empty. Please check rich push data with the function 'NetmeraPushService.checkRichPush()..' and then 'NetmeraPushService.isRichPush().."));
            }
        }

        private static void getRichPushContent(Action<JObject, Exception> callback)
        {
            Dictionary<string, object> nvp = new Dictionary<string, object>();
            JObject parameters = new JObject();
            parameters.Add(NetmeraConstants.Netmera_Push_Apikey, NetmeraClient.securityToken);
            parameters.Add(NetmeraConstants.Netmera_Push_Id, richPushId);

            nvp.Add(NetmeraConstants.Netmera_Push_Notification, parameters);

            String url = NetmeraConstants.Netmera_Domain_Url + NetmeraConstants.Netmera_Push_Server_Url + NetmeraConstants.Rest_Version + NetmeraConstants.Netmera_Push_Get_Push_Content;

            NetmeraHttpUtils.getRichPushContent(url, nvp, callback);
        }

        private static void resetRichPush()
        {
            richPushId = null;
            richPush = false;
        }
    }
}
