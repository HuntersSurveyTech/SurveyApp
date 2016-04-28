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
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Security.Cryptography;
using Hammock.Authentication.OAuth;
using Hammock;

namespace Netmera
{
    internal class Twitter
    {
        public static String ApiHost = "https://api.twitter.com";
        public static String OAuthUrl = "https://api.twitter.com/oauth";
        public static String RequestToken = "/request_token";
        public static String AccessToken = "/access_token";
        public static String Authorize = "/authorize";
        public static String VerifyCredentials = "/1/account/verify_credentials.json?skip_status=true";
        public static String RequestUrl = OAuthUrl + RequestToken;
        public static String AccessUrl = OAuthUrl + AccessToken;
        public static String AuthorizeUrl = OAuthUrl + Authorize;
        public static string VerifyCredentialsUrl = ApiHost + VerifyCredentials;

        public static String _consumerKey;
        public static String _consumerSecret;

        private static string _oAuthSecret;
        private static string _oAuthToken;

        private static string _secretToken;
        private static string _accessToken;

        private static string _verifier;

        public static string _oAuthVersion = "1.0";

        public Twitter(String consumerKey, String consumerSecret)
        {
            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
        }

        public String getConsumerKey()
        {
            return _consumerKey;
        }

        public String getConsumerSecret()
        {
            return _consumerSecret;
        }

        public String getAccessToken()
        {
            return _accessToken;
        }

        public void setAccessToken(String accessToken)
        {
            _accessToken = accessToken;
        }

        public String getSecretToken()
        {
            return _secretToken;
        }

        public void setSecretToken(String secretToken)
        {
            _secretToken = secretToken;
        }

        public bool isSessionValid()
        {
            return getAccessToken() != null && getSecretToken() != null;
        }

        public void getTwitterLoginUri(Action<Uri, Exception> callback)
        {
            try
            {
                var credentials = new OAuthCredentials
                {
                    Type = OAuthType.RequestToken,
                    SignatureMethod = OAuthSignatureMethod.HmacSha1,
                    ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                    ConsumerKey = _consumerKey,
                    ConsumerSecret = _consumerSecret,
                    Version = _oAuthVersion,
                    //CallbackUrl = _callbackUri
                };

                var client = new RestClient
                {
                    Authority = OAuthUrl,
                    Credentials = credentials,
                    HasElevatedPermissions = true
                };

                var request = new RestRequest
                {
                    Path = RequestToken
                };
                client.BeginRequest(request, new RestCallback((req, resp, userstate) =>
                {
                    _oAuthToken = getQueryParameter(resp.Content, "oauth_token");
                    _oAuthSecret = getQueryParameter(resp.Content, "oauth_token_secret");

                    if (!String.IsNullOrEmpty(_oAuthToken))
                    {
                        var authorizeUrl = AuthorizeUrl + "?oauth_token=" + _oAuthToken + "&oauth_token_secret=" + _oAuthSecret;

                        if (callback != null)
                            callback(new Uri(authorizeUrl), null);
                    }
                    else
                    {
                        if (callback != null)
                            callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_TW_ERROR, "Error occured while getting twitter login URI."));
                    }
                }));
            }
            catch (Exception)
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_TW_ERROR, "Error occured while getting twitter login URI."));
            }
        }

        public void getTwitterPin(String pinHtml, Action<String, Exception> callback)
        {
            String[] codeHtml = Regex.Split(pinHtml, "<code>");
            if (codeHtml.Length > 1)
            {
                String pin = Regex.Split(codeHtml[1], "</code>")[0];
                if (!String.IsNullOrEmpty(pin))
                {
                    _verifier = pin;
                    if (callback != null)
                        callback(pin, null);
                }
                else
                    if (callback != null)
                        callback(null, null);
            }
            else
                if (callback != null)
                    callback(null, null);
        }

        public void getTwitterAccessToken(Uri uri, Action<String, Exception> callback)
        {
            try
            {
                var credentials = new OAuthCredentials
                {
                    Type = OAuthType.AccessToken,
                    SignatureMethod = OAuthSignatureMethod.HmacSha1,
                    ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                    ConsumerKey = _consumerKey,
                    ConsumerSecret = _consumerSecret,
                    Token = _oAuthToken,
                    TokenSecret = _oAuthSecret,
                    Verifier = _verifier
                };

                var client = new RestClient
                {
                    Authority = OAuthUrl,
                    Credentials = credentials,
                    HasElevatedPermissions = true
                };

                var request = new RestRequest
                {
                    Path = AccessToken
                };

                bool once = false;

                client.BeginRequest(request, new RestCallback((req, resp, userstate) =>
                {
                    if (!once)
                    {
                        once = true;
                        _accessToken = getQueryParameter(resp.Content, "oauth_token").Trim();
                        _secretToken = getQueryParameter(resp.Content, "oauth_token_secret").Trim();
                        String UserId = getQueryParameter(resp.Content, "user_id");
                        String ScreenName = getQueryParameter(resp.Content, "screen_name");
                        //if (!String.IsNullOrEmpty(_accessToken))
                        {
                            if (callback != null)
                                callback(_accessToken, null);
                        }
                        //else
                        //{
                        //    if (callback != null)
                        //        //callback(_accessToken, new NetmeraException(NetmeraException.ErrorCode.EC_TW_ERROR, "Error occured while getting twitter access token."));
                        //        callback(null, null);
                        //}
                    }
                }));
            }
            catch (Exception)
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_TW_ERROR, "Error occured while getting twitter access token."));
            }
        }

        public void getTwitterUserInfo(Action<String, Exception> callback)
        {
            try
            {
                var credentials = new OAuthCredentials
                {
                    Type = OAuthType.ProtectedResource,
                    SignatureMethod = OAuthSignatureMethod.HmacSha1,
                    ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                    ConsumerKey = _consumerKey,
                    ConsumerSecret = _consumerSecret,
                    Token = _accessToken,
                    TokenSecret = _secretToken,
                    Version = "1.0",
                };

                var request = new RestRequest
                {
                    Credentials = credentials,
                    Path = VerifyCredentials
                };
                var client = new RestClient
                {
                    Authority = ApiHost,
                    HasElevatedPermissions = true
                };
                client.BeginRequest(request, new RestCallback((req, resp, userstate) =>
                {
                    if (resp.StatusCode != HttpStatusCode.OK)
                    {
                        if (callback != null)
                            callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_TW_ERROR, "Error occured while getting twitter user info."));
                    }
                    else
                    {
                        if (callback != null)
                            callback(resp.Content, null);
                    }
                }));
            }
            catch (Exception)
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_TW_ERROR, "Error occured while getting twitter user info."));
            }
        }

        private static string getQueryParameter(string input, string parameterName)
        {
            foreach (string item in input.Split('&'))
            {
                var parts = item.Split('=');
                if (parts[0] == parameterName)
                {
                    return parts[1];
                }
            }
            return String.Empty;
        }
    }
}
