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
using System.Windows.Controls.Primitives;
using Microsoft.Phone.Controls;
using System.IO;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Windows.Navigation;
using System.Linq;
using System.Text.RegularExpressions;
using Hammock;
using Hammock.Authentication.OAuth;
using System.Xml;
using Newtonsoft.Json.Linq;
using System.IO.IsolatedStorage;
using Hammock.Web;
using System.Threading;

namespace Netmera
{
    /// <summary>
    /// It uses Twitter account for user operations
    /// </summary>
    public class NetmeraTwitterUtils
    {
        private static Twitter twitter;
       // private static int count = 0;

        /// <summary>
        /// Initiliazes twitter object
        /// </summary>
        /// <param name="consumerKey">Consumer key of the twitter applicaton</param>
        /// <param name="consumerSecret"></param>
        public static void initialize(String consumerKey, String consumerSecret)
        {
            twitter = new Twitter(consumerKey, consumerSecret);
            TwitterSession.restore(twitter);
        }

        /// <summary>
        /// Login with Twitter account
        /// </summary>
        /// <param name="callback">Method to be called when login operation finishes</param>
        public static void login(Action<NetmeraUser, Exception> callback)
        {
            String securityToken = NetmeraClient.getSecurityToken();
            if (securityToken != null && securityToken.Trim() != "")
            {
                Popup popup = new Popup();
                LoginUserControl control = new LoginUserControl();
                control.webBrowser.Source = null;
                control.btnCancel.Click += new RoutedEventHandler((s, e) =>
                {
                    if (callback != null)
                        callback(null, null);
                });
                try
                {
                    NetmeraUser.clearSocialSessions((logout, ex) =>
                    {
                        if (logout && ex == null)
                        {
                            control.webBrowser.Loaded += new RoutedEventHandler((s, e) =>
                            {
                                twitter.getTwitterLoginUri((loginUri, ex1) =>
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
                                if (e.Uri.AbsoluteUri == Twitter.AuthorizeUrl)
                                {
                                    String pinHtml = control.webBrowser.SaveToString();

                                    twitter.getTwitterPin(pinHtml, (pin, ex1) =>
                                    {
                                        if (!String.IsNullOrEmpty(pin))
                                        {
                                            twitter.getTwitterAccessToken(e.Uri, (accessToken, ex2) =>
                                            {
                                                Deployment.Current.Dispatcher.BeginInvoke(() =>
                                                {
                                                    popup.IsOpen = false;
                                                    if (ex2 != null)
                                                    {
                                                        if (callback != null)
                                                            callback(null, ex2);
                                                    }
                                                    else
                                                    {
                                                        if (!String.IsNullOrEmpty(accessToken))
                                                        {
                                                            getTwitterUser((user, exc) =>
                                                            {
                                                                if (user != null && exc == null)
                                                                {
                                                                    TwitterSession.save(twitter);
                                                                }
                                                                callback(user, exc);
                                                            });
                                                        }
                                                        else
                                                        {
                                                            login(callback);
                                                        }
                                                    }
                                                });
                                            });
                                        }
                                    });
                                }
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
                catch (Exception e)
                {
                    if (callback != null)
                        callback(null, e);
                }
            }
            else
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_API_KEY_NOT_FOUND, "You didn't set your api key. Please use NetmeraClient.init(apiKey)."));
            }
        }

        private static void getTwitterUser(Action<NetmeraUser, Exception> callback)
        {
            try
            {
                twitter.getTwitterUserInfo((data, ex) =>
                {
                    if (ex == null)
                    {
                        JObject jsonObject = JObject.Parse(data);

                        String twId = jsonObject.Value<String>(NetmeraConstants.Twitter_Id);
                        String screenName = jsonObject.Value<String>(NetmeraConstants.Twitter_Screenname);
                        String fullName = jsonObject.Value<String>(NetmeraConstants.Twitter_Name);
                        String firstName = null;
                        String lastName = null;

                        if (fullName.Contains(" "))
                        {
                            int lastWhiteSpaceIndex = fullName.LastIndexOf(" ");
                            firstName = fullName.Substring(0, lastWhiteSpaceIndex);
                            lastName = fullName.Substring(lastWhiteSpaceIndex + 1);
                        }
                        else
                        {
                            firstName = fullName;
                        }
                        registerAndLogin(twId, screenName, firstName, lastName, callback);
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
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_TW_ERROR, "Error occured while getting twitter user."));
            }
        }

        private static void registerAndLogin(String twId, String nickname, String name, String surname, Action<NetmeraUser, Exception> callback)
        {
            NetmeraUser user = new NetmeraUser();

            RequestItem loginTwUserUserReqItem = new RequestItem();
            loginTwUserUserReqItem.setTwId(twId);
            loginTwUserUserReqItem.setNickname(nickname);
            loginTwUserUserReqItem.setName(name);
            loginTwUserUserReqItem.setSurname(surname);

            NetmeraUser.twitterRegister(loginTwUserUserReqItem, (json, ex) =>
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
        /// Twitter user logged out from the application.
        /// </summary>
        /// <param name="callback">Method to be called when logout operation finishes, which has a boolean parameter showing logout is successful or not </param>
        public static void logout(Action<Boolean, Exception> callback)
        {
            NetmeraUser.logout();
            clearTwitterSession(callback);
        }

        internal static void clearTwitterSession(Action<Boolean, Exception> callback)
        {
            if (twitter != null && twitter.isSessionValid())
            {
                twitter.setAccessToken(null);
                twitter.setSecretToken(null);
                TwitterSession.clear();
                clearTwitterCookies(callback);
            }
            else
            {
                if (callback != null)
                    callback(true, null);
            }
        }

        //it clears cookies by posting logout form inside html. A synchronous way would be much better
        private static void clearTwitterCookies(Action<Boolean, Exception> callback)
        {
            bool newUri = true;
            try
            {
                Popup popup = new Popup();
                LoginUserControl control = new LoginUserControl();
                control.Visibility = Visibility.Collapsed;
                control.webBrowser.Loaded += new RoutedEventHandler((o, e) =>
                {
                    twitter.getTwitterLoginUri((loginUri, ex) =>
                    {
                        newUri = true;
                        if (loginUri != null && ex == null)
                        {
                            Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                control.webBrowser.Navigate(loginUri);
                            });
                        }
                        else
                        {
                            if (callback != null)
                                callback(false, new NetmeraException(NetmeraException.ErrorCode.EC_TW_ERROR, "Error occured while clearing twitter cookies."));
                        }
                    });
                });
                control.webBrowser.LoadCompleted += new LoadCompletedEventHandler((s, e) =>
                {
                    if (newUri)
                    {
                        if (popup.Visibility == Visibility.Visible && control.webBrowser.SaveToString().Contains("signout"))
                        {
                            control.webBrowser.InvokeScript("eval", "var formsArray = document.getElementsByTagName('form'); for (i=0; i<formsArray.length; i++) {if(formsArray[i].getAttribute('action')== '/intent/session'){formsArray[i].submit();} }");
                            popup.IsOpen = false;
                            popup.IsOpen = true;
                            newUri = false;
                        }
                        else
                        {
                            popup.IsOpen = false;
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
                    callback(false, new NetmeraException(NetmeraException.ErrorCode.EC_TW_ERROR, "Error occured while clearing twitter cookies."));
            }
        }

        /// <summary>
        /// Sends request to Twitter
        /// </summary>
        /// <param name="url">Request url</param>
        /// <param name="callback">Method to be called when request finishes</param>
        public static void request(String url, Action<String, NetmeraException> callback)
        {
            request(url, null, null, callback);
        }

        /// <summary>
        /// Sends request to Twitter
        /// </summary>
        /// <param name="url">Request url</param>
        /// <param name="mapParams">Request parameters</param>
        /// <param name="callback">Method to be called when request finishes</param>
        public static void request(String url, Dictionary<String, String> mapParams, Action<String, NetmeraException> callback)
        {
            request(url, mapParams, null, callback);
        }

        /// <summary>
        /// Sends request to Twitter
        /// </summary>
        /// <param name="url">Request url</param>
        /// <param name="method">Request method</param>
        /// <param name="callback">Method to be called when request finishes</param>
        public static void request(String url, String method, Action<String, NetmeraException> callback)
        {
            request(url, null, method, callback);
        }

        /// <summary>
        /// Sends request to Twitter
        /// </summary>
        /// <param name="url">Request url</param>
        /// <param name="mapParams">Request parameters</param>
        /// <param name="method">Request method</param>
        /// <param name="callback">Method to be called when request finishes</param>
        public static void request(String url, Dictionary<String, String> mapParams, String method, Action<String, NetmeraException> callback)
        {
            if (twitter != null && twitter.isSessionValid())
            {
                try
                {
                    var credentials = new OAuthCredentials
                    {
                        Type = OAuthType.ProtectedResource,
                        SignatureMethod = OAuthSignatureMethod.HmacSha1,
                        ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                        ConsumerKey = twitter.getConsumerKey(),
                        ConsumerSecret = twitter.getConsumerSecret(),
                        Token = twitter.getAccessToken(),
                        TokenSecret = twitter.getSecretToken(),
                        Version = "1.0",
                    };

                    var request = new RestRequest();
                    request.Credentials = credentials;
                    if (!String.IsNullOrEmpty(url))
                        request.Path = url.Replace("http://", "https://").Replace(Twitter.ApiHost, "");
                    if (!String.IsNullOrEmpty(method) && method.ToUpper().Trim() == "POST")
                        request.Method = WebMethod.Post;

                    if (mapParams != null && mapParams.Count != 0)
                    {
                        foreach (var key in mapParams.Keys)
                        {
                            request.AddParameter(key, mapParams[key]);
                        }
                    }

                    var client = new RestClient
                    {
                        Authority = Twitter.ApiHost,
                        HasElevatedPermissions = true
                    };

                    client.BeginRequest(request, new RestCallback((req, resp, userstate) =>
                    {
                        if (resp.StatusCode != HttpStatusCode.OK)
                        {
                            JObject json = JObject.Parse(resp.Content);
                            if (json["error"] != null)
                            {
                                if (callback != null)
                                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_TW_ERROR, json.Value<String>("error")));
                            }
                            else
                            {
                                if (callback != null)
                                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_TW_ERROR, "Request error occured."));
                            }
                        }
                        else
                        {
                            if (callback != null)
                                callback(resp.Content, null);
                        }
                    }));
                }
                catch
                {
                    if (callback != null)
                        callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_TW_ERROR, "Request error occured."));
                }
            }
            else
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_TW_ERROR, "Must first login."));
            }
        }
    }
}