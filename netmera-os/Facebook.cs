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
using Facebook;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Netmera
{
    internal class Facebook
    {
        private static String _appId;
        private static String _accessToken;

        public Facebook(String appId)
        {
            _appId = appId;
        }

        public String getAccessToken()
        {
            return _accessToken;
        }

        public void setAccessToken(String accessToken)
        {
            _accessToken = accessToken;
        }

        public bool isSessionValid()
        {
            return getAccessToken() != null;
        }

        public void getFacebookLoginUri(String[] permissions, Action<Uri, Exception> callback)
        {
            try
            {
                String permissionString = String.Empty;
                if (permissions.Length > 0)
                    permissionString = String.Join(",", permissions);

                FacebookOAuthClient fboc = new FacebookOAuthClient();
                Dictionary<String, Object> parameters = new Dictionary<String, Object>();
                parameters["client_id"] = _appId;
                parameters["redirect_uri"] = "https://www.facebook.com/connect/login_success.html";
                parameters["response_type"] = "token";
                parameters["display"] = "touch";

                // add the 'scope' only if we have extendedPermissions.
                if (!String.IsNullOrEmpty(permissionString))
                {
                    // A comma-delimited list of permissions
                    parameters["scope"] = permissionString;
                }

                Uri uri = fboc.GetLoginUrl(parameters);
                if (callback != null)
                    callback(uri, null);
            }
            catch (Exception)
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_FB_ERROR, "Error occured while getting facebook login URI."));
            }
        }

        public void getFacebookAccessToken(Uri uri, Action<String, Exception> callback)
        {
            try
            {
                FacebookOAuthClient fboc = new FacebookOAuthClient();
                FacebookOAuthResult oauthResult;
                if (!fboc.TryParseResult(uri, out oauthResult))
                {
                    callback(null, null);
                }
                else
                {
                    if (oauthResult.IsSuccess)
                    {
                        _accessToken = oauthResult.AccessToken;
                        if (callback != null)
                            callback(_accessToken, null);
                    }
                    else
                    {
                        if (callback != null)
                            callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_FB_ERROR, oauthResult.ErrorDescription));
                    }
                }
            }
            catch (Exception)
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_FB_ERROR, "Error occured while getting facebook access token."));
            }
        }

        public void getFacebookUserInfo(Action<String, Exception> callback)
        {
            try
            {
                FacebookClient fb = new FacebookClient(_accessToken);
                fb.GetCompleted += (o, e) =>
                {
                    if (e.Error != null)
                    {
                        if (callback != null)
                            callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_FB_ERROR, e.Error.Message));
                    }

                    var result = (IDictionary<String, Object>)e.GetResultData();
                    if (result == null)
                    {
                        if (callback != null)
                            callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_FB_ERROR, "User info coming from Facebook is empty!"));
                    }
                    else
                    {
                        if (callback != null)
                            callback(result.ToString(), null);
                    }
                };
                fb.GetAsync("me");
            }
            catch (Exception)
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_FB_ERROR, "Error occured while getting facebook user info."));
            }
        }

    }
}
