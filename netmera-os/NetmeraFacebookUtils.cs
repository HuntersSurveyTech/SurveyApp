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
using System.Text;
using Microsoft.Phone.Controls;
using System.Windows.Threading;
using Newtonsoft.Json.Linq;
using System.Windows.Navigation;
using Facebook;
using System.Windows.Controls.Primitives;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text.RegularExpressions;
using System.Threading;

namespace Netmera
{
    /// <summary>
    /// It uses Facebook account for user operations
    /// </summary>
    public class NetmeraFacebookUtils
    {
        private static Facebook facebook;

        /// <summary>
        /// Initiliazes Netmera Facebook utilities with application Id
        /// </summary>
        /// <param name="appId">Id of the Facebook application</param>
        public static void initialize(String appId)
        {
            facebook = new Facebook(appId);
            FacebookSession.restore(facebook);
        }

        /// <summary>
        /// Logs a user into the registered application with Facebook account without Facebook permissions.
        /// </summary>
        /// <param name="callback">Method to be called when login operation finishes</param>
        public static void login(Action<NetmeraUser, Exception> callback)
        {
            login(new String[0], callback);
        }

        /// <summary>
        /// Logs a user into the registered application with Facebook account with Facebook permissions.
        /// </summary>
        /// <param name="permissionArray">Permission array</param>
        /// <param name="callback">Method to be called when login operation finishes</param>
        public static void login(String[] permissionArray, Action<NetmeraUser, Exception> callback)
        {
            String securityToken = NetmeraClient.getSecurityToken();
            if (securityToken != null && securityToken.Trim() != "")
            {
                Popup popup = new Popup();
                LoginUserControl control = new LoginUserControl();
                control.btnCancel.Click += new RoutedEventHandler((s, e) =>
                {
                    NetmeraUser user = null;
                    user = NetmeraUser.getCurrentUser();
                    if (callback != null)
                        callback(user, null);
                });
                try
                {
                    NetmeraUser.clearSocialSessions((logout, ex) =>
                    {
                        if (logout && ex == null)
                        {
                            control.webBrowser.Loaded += new RoutedEventHandler((s, e) =>
                            {
                                facebook.getFacebookLoginUri(permissionArray, (loginUri, ex1) =>
                                {
                                    if (loginUri != null && ex1 == null)
                                    {
                                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                                        {
                                            control.webBrowser.Navigate(loginUri);
                                        });
                                    }
                                    else
                                    {
                                        if (callback != null)
                                            callback(null, ex1);
                                    }
                                });
                            });

                            control.webBrowser.LoadCompleted += new LoadCompletedEventHandler((s, e) =>
                            {
                                String ss = control.webBrowser.SaveToString();
                                facebook.getFacebookAccessToken(e.Uri, (accessToken, ex2) =>
                                {
                                    if (!String.IsNullOrEmpty(accessToken))
                                    {
                                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                                        {
                                            popup.IsOpen = false;
                                            getFacebookUser((user, ex3) =>
                                            {
                                                if (user != null && ex3 == null)
                                                {
                                                    FacebookSession.save(facebook);
                                                }
                                                callback(user, ex3);
                                            });
                                        });
                                    }
                                    else if (ex2 != null)
                                    {
                                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                                        {
                                            popup.IsOpen = false;
                                            if (callback != null)
                                                callback(null, ex2);
                                        });
                                    }
                                });
                            });
                            Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                popup.Child = control;
                                popup.IsOpen = true;
                            });
                        }
                        else
                        {
                            if (callback != null)
                                callback(null, ex);
                        }
                    });
                }
                catch (Exception)
                {
                    if (callback != null)
                        callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_FB_ERROR, "Error occured while logging in facebook."));
                }
            }
            else
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_API_KEY_NOT_FOUND, "You didn't set your api key. Please use NetmeraClient.init(apiKey)."));
            }
        }

        private static void getFacebookUser(Action<NetmeraUser, Exception> callback)
        {
            try
            {
                facebook.getFacebookUserInfo((data, ex) =>
                {
                    if (ex == null)
                    {
                        JObject jsonObject = JObject.Parse(data);

                        String fbId = jsonObject.Value<String>(NetmeraConstants.Facebook_Id);
                        String nickname = jsonObject.Value<String>(NetmeraConstants.Facebook_Username);
                        String firstName = jsonObject.Value<String>(NetmeraConstants.Facebook_Firstname);
                        String lastName = jsonObject.Value<String>(NetmeraConstants.Facebook_Lastname);
                        String email = null;

                        if (jsonObject[NetmeraConstants.Facebook_Email] != null)
                        {
                            email = jsonObject.Value<String>(NetmeraConstants.Facebook_Email);
                        }
                        registerAndLogin(fbId, nickname, firstName, lastName, email, callback);
                    }
                    else
                    {
                        if (callback != null)
                            callback(null, ex);
                    }
                });
            }
            catch (Exception)
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_FB_ERROR, "Error occured while getting facebook user."));
            }
        }

        private static void registerAndLogin(String fbId, String nickname, String name, String surname, String email, Action<NetmeraUser, Exception> callback)
        {
            NetmeraUser user = new NetmeraUser();

            RequestItem loginFbUserUserReqItem = new RequestItem();
            loginFbUserUserReqItem.setFbId(fbId);
            loginFbUserUserReqItem.setNickname(nickname);
            loginFbUserUserReqItem.setName(name);
            loginFbUserUserReqItem.setSurname(surname);
            loginFbUserUserReqItem.setEmail(email);

            NetmeraUser.facebookRegister(loginFbUserUserReqItem, (json, ex) =>
            {
                if (json == null || ex != null)
                {
                    if (callback != null)
                        callback(null, ex);
                }
                else
                {
                    try
                    {
                        user = NetmeraUser.setCurrentUser(json);
                        if (callback != null)
                            callback(user, ex);
                    }
                    catch (NetmeraException e)
                    {
                        if (callback != null)
                            callback(null, e);
                    }
                }
            });
        }

        /// <summary>
        /// Facebook user logged out from the application.
        /// </summary>
        /// <param name="callback">Method to be called when logout operation finishes, which has a boolean parameter showing logout is successful or not </param>
        public static void logout(Action<Boolean, Exception> callback)
        {
            NetmeraUser.logout();
            clearFacebookSession(callback);
        }

        internal static void clearFacebookSession(Action<Boolean, Exception> callback)
        {
            if (facebook != null && facebook.isSessionValid())
            {
                facebook.setAccessToken(null);
            }
            FacebookSession.clear();
            clearFacebookCookies(callback);
        }

        //it clears cookies by posting logout form inside html. A synchronous way would be much better
        private static void clearFacebookCookies(Action<Boolean, Exception> callback)
        {
            try
            {
                bool isFirstCallback = true;
                Popup popup = new Popup();
                LoginUserControl control = new LoginUserControl();
                control.Visibility = Visibility.Collapsed;
                control.webBrowser.Loaded += new RoutedEventHandler((o, e) =>
                {
                    control.webBrowser.Navigate(new Uri("http://www.facebook.com/logout.php"));
                });

                control.webBrowser.LoadCompleted += new LoadCompletedEventHandler((s, e) =>
                {
                    String html = control.webBrowser.SaveToString();

                    if (popup.Visibility == Visibility.Visible && html.Contains("id=\"logout_form\""))
                    {
                        control.webBrowser.InvokeScript("eval", "document.forms['logout_form'].submit();");
                        popup.IsOpen = false;
                        popup.IsOpen = true;
                    }
                    else
                    {
                        if (isFirstCallback)
                        {
                            isFirstCallback = false;
                            if (callback != null)
                                callback(true, null);
                        }
                    }
                });

                popup.Child = control;
                popup.IsOpen = true;
            }
            catch (Exception)
            {
                if (callback != null)
                    callback(false, new NetmeraException(NetmeraException.ErrorCode.EC_FB_ERROR, "Error occured while clearing facebook cookies."));
            }
        }

        /// <summary>
        /// Request to Facebook API
        /// </summary>
        /// <param name="path">Request path</param>
        /// <param name="callback">Method to be called when request finishes</param>
        public static void request(String path, Action<String, NetmeraException> callback)
        {
            request(path, null, null, callback);
        }

        /// <summary>
        /// Request to Facebook API
        /// </summary>
        /// <param name="path">Request path</param>
        /// <param name="mapParams">Request parameters</param>
        /// <param name="callback">Method to be called when request finishes</param>
        public static void request(String path, Dictionary<String, String> mapParams, Action<String, NetmeraException> callback)
        {
            request(path, mapParams, null, callback);
        }

        /// <summary>
        /// Request to Facebook API
        /// </summary>
        /// <param name="path">Request path</param>
        /// <param name="method">Request method</param>
        /// <param name="callback">Method to be called when request finishes</param>
        public static void request(String path, String method, Action<String, NetmeraException> callback)
        {
            request(path, null, method, callback);
        }

        /// <summary>
        /// Request to Facebook API
        /// </summary>
        /// <param name="path">Request path</param>
        /// <param name="mapParams">Request parameters</param>
        /// <param name="method">Request method</param>
        /// <param name="callback">Method to be called when request finishes</param>
        public static void request(String path, Dictionary<String, String> mapParams, String method, Action<String, NetmeraException> callback)
        {
            if (facebook != null && facebook.isSessionValid())
            {
                try
                {
                    FacebookClient fb = new FacebookClient(facebook.getAccessToken());

                    var parameters = new Dictionary<String, Object>();
                    if (mapParams != null && mapParams.Count != 0)
                    {
                        foreach (var key in mapParams.Keys)
                        {
                            parameters.Add(key, mapParams[key]);
                        }
                    }

                    if (!String.IsNullOrEmpty(method) && method.ToUpper().Trim() == "POST")
                    {
                        fb.PostCompleted += (o, e) =>
                        {
                            if (e.Error != null)
                            {
                                if (callback != null)
                                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_FB_ERROR, e.Error.Message));
                            }
                            else
                            {
                                var result = (IDictionary<String, Object>)e.GetResultData();
                                callback(result.ToString(), null);
                            }
                        };
                        fb.PostAsync(path, parameters);
                    }
                    else
                    {
                        fb.GetCompleted += (o, e) =>
                        {
                            if (e.Error != null)
                            {
                                if (callback != null)
                                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_FB_ERROR, e.Error.Message));
                            }
                            else
                            {
                                var result = (IDictionary<String, Object>)e.GetResultData();
                                callback(result.ToString(), null);
                            }
                        };
                        fb.GetAsync(path, parameters);
                    }
                }
                catch
                {
                    if (callback != null)
                        callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_FB_ERROR, "Request error occured."));
                }
            }
            else
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_FB_ERROR, "Must first login."));
            }
        }
    }
}
