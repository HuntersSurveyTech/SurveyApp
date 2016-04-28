using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Collections.Generic;

namespace Netmera
{
    internal static class NetmeraHttpUtils
    {
        public static void sendHttp(String url, String method, Action<JObject, Exception> callback)
        {
            sendHttpWithJson(url, null, method, callback);
        }

        public static void sendHttpWithJson(String url, String jsonCall, String method, Action<JObject, Exception> callback)
        {
            StringBuilder strBuild = new StringBuilder(url);
            strBuild.Append("&").Append(NetmeraConstants.Netmera_SDK_Params).Append(NetmeraConstants.Netmera_SDK_Value);
            strBuild.Append("&").Append(NetmeraConstants.Netmera_SDKVERSION_Params).Append(NetmeraConstants.Netmera_SDKVERSION_Value);

            HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;
            request.AllowReadStreamBuffering = true;
            request.Method = method;

            if (method == NetmeraConstants.Http_Method_Post)
                request.ContentType = NetmeraConstants.Http_Content_Type;

            //it is for json call (add json body to the request. e.g. for user update operation)
            if (!String.IsNullOrEmpty(jsonCall))
            {
                request.BeginGetRequestStream(ar2 =>
                {
                    try
                    {
                        using (var streamRequest = (ar2.AsyncState as HttpWebRequest).EndGetRequestStream(ar2))
                        using (var writer = new StreamWriter(streamRequest))
                        {
                            writer.Write(jsonCall);
                            writer.Flush();
                            writer.Close();
                        }
                        getResponseWithRequest(request, callback);
                    }
                    catch (Exception ex)
                    {
                        // throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_REQUEST, ex);
                        if (callback != null)
                            callback(null, ex);
                    }
                }, request);
            }
            //for calls without json
            else
            {
                getResponseWithRequest(request, callback);
            }
        }

        public static void sendHttpwithReturn(String url, String method, Action<bool, JObject, Exception> callback)
        {
            sendHttpWithJsonwithReturn(url, null, method, callback);

        }

        public static void sendHttpWithJsonwithReturn(String url, String jsonCall, String method, Action<bool, JObject, Exception> callback)
        {

            StringBuilder strBuild = new StringBuilder(url);
            strBuild.Append("&").Append(NetmeraConstants.Netmera_SDK_Params).Append(NetmeraConstants.Netmera_SDK_Value);
            strBuild.Append("&").Append(NetmeraConstants.Netmera_SDKVERSION_Params).Append(NetmeraConstants.Netmera_SDKVERSION_Value);

            HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;
            request.AllowReadStreamBuffering = true;
            request.Method = method;

            if (method == NetmeraConstants.Http_Method_Post)
                request.ContentType = NetmeraConstants.Http_Content_Type;

            //it is for json call (add json body to the request. e.g. for user update operation)
            if (!String.IsNullOrEmpty(jsonCall))
            {
                request.BeginGetRequestStream(ar2 =>
                {
                    try
                    {
                        using (var streamRequest = (ar2.AsyncState as HttpWebRequest).EndGetRequestStream(ar2))
                        using (var writer = new StreamWriter(streamRequest))
                        {
                            writer.Write(jsonCall);
                            writer.Flush();
                            writer.Close();
                        }
                        getResponseWithRequestwithReturn(request, callback);
                    }
                    catch (Exception ex)
                    {
                        // throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_REQUEST, ex);
                        if (callback != null)
                            callback(false, null, ex);
                    }
                }, request);
            }
            //for calls without json
            else
            {
                getResponseWithRequestwithReturn(request, callback);
            }

        }

        /// <summary>
        /// Carries out BeginGetResponse operation of .NET in a more useful manner for Netmera and escapes code duplicates
        /// </summary>
        /// <param name="request">Http web request</param>
        /// <param name="callback">Callback</param>
        private static void getResponseWithRequestwithReturn(HttpWebRequest request, Action<bool, JObject, Exception> callback)
        {
            bool flag = false;
            request.BeginGetResponse(ar =>
            {
                try
                {
                    using (var response = (ar.AsyncState as HttpWebRequest).EndGetResponse(ar) as HttpWebResponse)
                    using (var streamResponse = response.GetResponseStream())
                    using (var streamRead = new StreamReader(streamResponse))
                    {
                        var responseString = streamRead.ReadToEnd();
                        var success = response.StatusCode == HttpStatusCode.OK;
                        if (responseString.ToString().IndexOf("true") != -1)
                        {
                            flag = true;
                        }
                        if (callback != null)
                            callback(flag, JObject.Parse(responseString), null);

                    }
                    NetmeraClient.finish();
                }
                catch (WebException ex)
                {
                    //throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_REQUEST, ex);
                    if (callback != null)
                        callback(flag, null, ex);
                }
            }, request);



        }

        public static void registerUnregisterPush(String url, Dictionary<string, object> postParameters, Action<Exception> callback)
        {

            string userAgent = "Someone";

            MultipartFormData.MultipartFormDataPost(url, userAgent, postParameters, (response, ex) =>
            {
                if (response != null && ex == null)
                {
                    int statusCode = (int)response.StatusCode;

                    if (statusCode == 200)
                    {
                        Stream responseStream = response.GetResponseStream();
                        if (responseStream != null)
                        {
                            StreamReader responseStreamReader = new StreamReader(responseStream);
                            string responseStr = responseStreamReader.ReadToEnd();

                            JObject json = JObject.Parse(responseStr);
                            if (json.Value<int>("code") == 1000)
                            {
                                json = json.Value<JObject>("result");

                                Dictionary<Netmera.BasePush.PushChannel, NetmeraPushDetail> result = new Dictionary<Netmera.BasePush.PushChannel, NetmeraPushDetail>();

                                if (callback != null)
                                    callback(null);
                            }
                            else
                            {
                                String error = json.Value<String>("message");
                                if (callback != null)
                                    callback(new NetmeraException(NetmeraException.ErrorCode.EC_PUSH_ERROR, error));
                            }
                        }
                        else
                        {
                            if (callback != null)
                                callback(new NetmeraException(NetmeraException.ErrorCode.EC_NULL_EXCEPTION, "Response entity is null while registering/unregistering notifications"));
                        }
                    }
                    else
                    {
                        if (callback != null)
                            callback(new NetmeraException(NetmeraException.ErrorCode.EC_HTTP_PROTOCOL_EXCEPTION, "Http exception occurred while registering/unregistering notifications"));
                    }
                }
                else
                {
                    if (callback != null)
                        callback(new NetmeraException(NetmeraException.ErrorCode.EC_NULL_EXCEPTION, "Response is null while registering/unregistering notifications"));
                }
            });
        }

        public static void getDeviceGroups(String url, Dictionary<string, object> postParameters, Action<List<String>, Exception> callback)
        {
            List<String> result = new List<string>();

            string userAgent = "Someone";

            MultipartFormData.MultipartFormDataPost(url, userAgent, postParameters, (response, ex) =>
            {
                if (response != null && ex == null)
                {
                    int statusCode = (int)response.StatusCode;

                    if (statusCode == 200)
                    {
                        Stream responseStream = response.GetResponseStream();
                        if (responseStream != null)
                        {
                            StreamReader responseStreamReader = new StreamReader(responseStream);
                            string responseStr = responseStreamReader.ReadToEnd();

                            JObject json = JObject.Parse(responseStr);
                            if (json.Value<int>("code") == 1000)
                            {
                                JArray jsonArray = json.Value<JArray>("result");

                                for (int i = 0; i < jsonArray.Count; i++)
                                {
                                    JObject childJObject = jsonArray.Value<JObject>(i);
                                    String deviceGroup = childJObject.Value<String>("groupName");
                                    result.Add(deviceGroup);
                                }


                                if (callback != null)
                                    callback(result, null);
                            }
                            else
                            {
                                String error = json.Value<String>("message");
                                if (callback != null)
                                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_PUSH_ERROR, error));
                            }
                        }
                        else
                        {
                            if (callback != null)
                                callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_NULL_EXCEPTION, "Response entity is null while getting device groups"));
                        }
                    }
                    else
                    {
                        if (callback != null)
                            callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_HTTP_PROTOCOL_EXCEPTION, "Http exception occurred while getting device groups"));
                    }
                }
                else
                {
                    if (callback != null)
                        callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_NULL_EXCEPTION, "Response is null while getting device groups"));
                }
            });
        }

        public static void getDeviceDetails(String url, Dictionary<string, object> postParameters, NetmeraDeviceDetail deviceDetail, Action<NetmeraDeviceDetail, Exception> callback)
        {
            List<String> result = new List<string>();

            string userAgent = "Someone";

            MultipartFormData.MultipartFormDataPost(url, userAgent, postParameters, (response, ex) =>
            {
                if (response != null && ex == null)
                {
                    int statusCode = (int)response.StatusCode;

                    if (statusCode == 200)
                    {
                        Stream responseStream = response.GetResponseStream();
                        if (responseStream != null)
                        {
                            StreamReader responseStreamReader = new StreamReader(responseStream);
                            string responseStr = responseStreamReader.ReadToEnd();

                            JObject json = JObject.Parse(responseStr);
                            if (json.Value<int>("code") == 1000)
                            {
                                if (json.Value<JObject>("result") != null)
                                {
                                    json = json.Value<JObject>("result");
                                    JArray jsonArray = json.Value<JArray>(NetmeraConstants.Netmera_Push_Device_Groups);

                                    //to be edited
                                    //deviceDetail.regId = json.ToString();

                                    for (int i = 0; i < jsonArray.Count; i++)
                                    {
                                        JObject childJobject = jsonArray.Value<JObject>(i);
                                        String deviceGroup = childJobject.Value<String>("groupName");
                                        result.Add(deviceGroup);
                                    }
                                    deviceDetail.setDeviceGroups(result);

                                    double lat = json.Value<double>(NetmeraConstants.Netmera_Push_Latitude_Params);
                                    double lng = json.Value<double>(NetmeraConstants.Netmera_Push_Longitude_Params);

                                    if (lat != 0 && lng != 0)
                                    {
                                        deviceDetail.setDeviceLocation(new NetmeraGeoLocation(lat, lng));
                                    }

                                    String regId = json.Value<String>(NetmeraConstants.Netmera_Push_Registration_Id);

                                    if (!string.IsNullOrEmpty(regId))
                                    {
                                        deviceDetail.setRegId(regId);
                                    }

                                    if (callback != null)
                                        callback(deviceDetail, null);
                                }
                            }
                            else
                            {
                                String error = json.Value<String>("message");
                                if (callback != null)
                                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_PUSH_ERROR, error));
                            }
                        }
                        else
                        {
                            if (callback != null)
                                callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_NULL_EXCEPTION, "Response entity is null while getting details of the device"));
                        }
                    }
                    else
                    {
                        if (callback != null)
                            callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_HTTP_PROTOCOL_EXCEPTION, "Http exception occurred while getting details of the device"));
                    }
                }
                else
                {
                    if (callback != null)
                        callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_NULL_EXCEPTION, "Response is null while getting details of the device"));
                }
            });
        }

        public static void sendPushMessage(String url, Dictionary<string, object> postParameters, Action<Dictionary<Netmera.BasePush.PushChannel, NetmeraPushDetail>, Exception> callback)
        {
            string userAgent = "Someone";

            MultipartFormData.MultipartFormDataPost(url, userAgent, postParameters, (response, ex) =>
            {
                if (response != null)
                {
                    int statusCode = (int)response.StatusCode;

                    if (statusCode == 200)
                    {
                        Stream responseStream = response.GetResponseStream();
                        if (responseStream != null)
                        {
                            StreamReader responseStreamReader = new StreamReader(responseStream);
                            string responseStr = responseStreamReader.ReadToEnd();

                            JObject json = JObject.Parse(responseStr);
                            if (json.Value<int>("code") == 1000)
                            {
                                json = json.Value<JObject>("result");

                                Dictionary<Netmera.BasePush.PushChannel, NetmeraPushDetail> result = new Dictionary<Netmera.BasePush.PushChannel, NetmeraPushDetail>();
                                if (json[NetmeraConstants.Netmera_Push_Type_Android] != null)
                                {
                                    NetmeraPushDetail androidDetail = new NetmeraPushDetail();
                                    JObject androidJson = json.Value<JObject>(NetmeraConstants.Netmera_Push_Type_Android);
                                    if (androidJson.Value<int>("code") == 1000)
                                    {
                                        androidDetail = parsePushResponse(androidJson);
                                    }
                                    else
                                    {
                                        String error = androidJson.Value<String>("message");
                                        androidDetail.setError(error);
                                    }
                                    result.Add(Netmera.BasePush.PushChannel.android, androidDetail);
                                }
                                if (json[NetmeraConstants.Netmera_Push_Type_Ios] != null)
                                {
                                    NetmeraPushDetail iosDetail = new NetmeraPushDetail();
                                    JObject iosJson = json.Value<JObject>(NetmeraConstants.Netmera_Push_Type_Ios);
                                    if (iosJson.Value<int>("code") == 1000)
                                    {
                                        iosDetail = parsePushResponse(iosJson);
                                    }
                                    else
                                    {
                                        String error = iosJson.Value<String>("message");
                                        iosDetail.setError(error);
                                    }
                                    result.Add(Netmera.BasePush.PushChannel.ios, iosDetail);
                                }
                                if (json[NetmeraConstants.Netmera_Push_Type_Wp] != null)
                                {
                                    NetmeraPushDetail wpDetail = new NetmeraPushDetail();
                                    JObject wpJson = json.Value<JObject>(NetmeraConstants.Netmera_Push_Type_Wp);
                                    if (wpJson.Value<int>("code") == 1000)
                                    {
                                        wpDetail = parsePushResponse(wpJson);
                                    }
                                    else
                                    {
                                        String error = wpJson.Value<String>("message");
                                        wpDetail.setError(error);
                                    }
                                    result.Add(Netmera.BasePush.PushChannel.wp, wpDetail);
                                }
                                if (callback != null)
                                    callback(result, null);
                            }
                            else
                            {
                                String error = json.Value<String>("message");
                                if (callback != null)
                                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_PUSH_ERROR, error));
                            }
                        }
                        else
                        {
                            if (callback != null)
                                callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_NULL_EXCEPTION, "Response entity is null while sending notification to devices"));
                        }
                    }
                    else
                    {
                        if (callback != null)
                            callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_HTTP_PROTOCOL_EXCEPTION, "Http exception occurred while sending notification to devices"));
                    }
                }
                else
                {
                    if (callback != null)
                        callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_NULL_EXCEPTION, "Response is null while sending notification to devices"));
                }
            });
        }

        /// <summary>
        /// Carries out BeginGetResponse operation of .NET in a more useful manner for Netmera and escapes code duplicates
        /// </summary>
        /// <param name="request">Http web request</param>
        /// <param name="callback">Callback</param>
        private static void getResponseWithRequest(HttpWebRequest request, Action<JObject, Exception> callback)
        {
            request.BeginGetResponse(ar =>
            {
                try
                {
                    using (var response = (ar.AsyncState as HttpWebRequest).EndGetResponse(ar) as HttpWebResponse)
                    using (var streamResponse = response.GetResponseStream())
                    using (var streamRead = new StreamReader(streamResponse))
                    {
                        var responseString = streamRead.ReadToEnd();
                        var success = response.StatusCode == HttpStatusCode.OK;

                        if (callback != null)
                            callback(JObject.Parse(responseString), null);
                    }
                    NetmeraClient.finish();
                }
                catch (WebException ex)
                {
                    //throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_REQUEST, ex);
                    if (callback != null)
                        callback(null, ex);
                }
            }, request);
        }

        //public static void createBulkContent(JObject paramsObject, Action<JObject, Exception> callback)
        //{
        //    //security token is stored in a temporary variable because createActionToken resets the security token
        //    //inside NetmeraClien due to the method sendHttp which contains NetmeraClient.finish();
        //    String securityToken = NetmeraClient.getSecurityToken();

        //    if (securityToken != null && securityToken.Trim() != "")
        //    {
        //        long contentNameRnd = currentTimeMillis();

        //        StringBuilder strBuild = new StringBuilder()
        //            .Append(NetmeraConstants.Netmera_Domain_Url)
        //            .Append(NetmeraConstants.Netmera_Domain_Rpc_Url);
        //        //.Append(NetmeraConstants.Netmera_CreateBulkContent_Url)
        //        //.Append(NetmeraConstants.SecurityToken_Params).Append("=").Append(securityToken).Append("&")
        //        //.Append(NetmeraConstants.ContentType_Params).Append("=").Append(NetmeraConstants.Default_ContentType).Append("&")
        //        //.Append(NetmeraConstants.ContentName_Params).Append("=").Append(contentNameRnd).Append("&")
        //        //.Append(NetmeraConstants.ContentPrivacy_Params).Append("=").Append(paramsObject[NetmeraConstants.ContentPrivacy_Params]).Append("&")
        //        //.Append(NetmeraConstants.Path_Params).Append("=").Append(NetmeraConstants.Default_ParentPath).Append("&")
        //        //.Append(NetmeraConstants.Service_Params).Append("=").Append(NetmeraConstants.Default_Service).Append("&")
        //        //.Append(NetmeraConstants.Action_Params).Append("=").Append(NetmeraConstants.Create_Action).Append("&")
        //        //.Append(NetmeraConstants.Content_Params);
        //        //.Append("=").Append(JsonConvert.SerializeObject(paramsObject));
        //        String strBody = JsonConvert.SerializeObject(paramsObject);

        //        sendHttp(strBuild.ToString(), NetmeraConstants.Http_Method_Post, (o, e) =>
        //        {
        //            JObject responseHttp = (JObject)o[NetmeraConstants.Entry_Params];
        //            if (callback != null)
        //                callback(responseHttp, e);
        //        });
        //    }
        //    else
        //    {
        //        throw new NetmeraException(NetmeraException.ErrorCode.EC_API_KEY_NOT_FOUND, "You didn't set your api key. Please use NetmeraClient.init(apiKey).");
        //    }
        //}

        public static void createContent(JObject paramsObject, Action<JObject, Exception> callback)
        {
            String securityToken = NetmeraClient.getSecurityToken();
            if (securityToken != null && securityToken.Trim() != "")
            {
                long contentNameRnd = currentTimeMillis();

                StringBuilder strBuild = new StringBuilder()
                        .Append(NetmeraConstants.Netmera_Domain_Url)
                        .Append(NetmeraConstants.Netmera_Domain_Rest_Url)
                        .Append(NetmeraConstants.Netmera_CreateContent_Url)
                        .Append(NetmeraConstants.SecurityToken_Params).Append("=").Append(securityToken).Append("&")
                        .Append(NetmeraConstants.ContentType_Params).Append("=").Append(NetmeraConstants.Default_ContentType).Append("&")
                        .Append(NetmeraConstants.ContentName_Params).Append("=").Append(contentNameRnd).Append("&")
                        .Append(NetmeraConstants.ContentPrivacy_Params).Append("=").Append(paramsObject[NetmeraConstants.ContentPrivacy_Params]).Append("&")
                        .Append(NetmeraConstants.Path_Params).Append("=").Append(NetmeraConstants.Default_ParentPath).Append("&")
                        .Append(NetmeraConstants.Service_Params).Append("=").Append(NetmeraConstants.Default_Service).Append("&")
                        .Append(NetmeraConstants.Action_Params).Append("=").Append(NetmeraConstants.Create_Action).Append("&")
                        .Append(NetmeraConstants.Content_Params).Append("=").Append(JsonConvert.SerializeObject(paramsObject));

                sendHttp(strBuild.ToString(), NetmeraConstants.Http_Method_Post, (o, e) =>
                {
                    if (o == null)
                    {
                        if (callback != null)
                            callback(null, e);
                    }
                    else
                    {
                        JObject responseHttp = (JObject)o[NetmeraConstants.Entry_Params];
                        if (callback != null)
                            callback(responseHttp, e);
                    }
               
                });
            }
            else
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_API_KEY_NOT_FOUND, "You didn't set your api key. Please use NetmeraClient.init(apiKey)."));
            }
        }

        public static void createBulkContent(JObject paramsObject, Action<JObject, Exception> callback)
        {
            String securityToken = NetmeraClient.getSecurityToken();
            if (securityToken != null && securityToken.Trim() != "")
            {
                long contentNameRnd = currentTimeMillis();

                StringBuilder strBuild = new StringBuilder()
                    .Append(NetmeraConstants.Netmera_Domain_Url)
                    .Append(NetmeraConstants.Netmera_Domain_Rpc_Url)
                    //.Append(NetmeraConstants.Netmera_CreateContent_Url)
                    .Append(NetmeraConstants.SecurityToken_Params).Append("=").Append(securityToken);//.Append("&")
                        //.Append(NetmeraConstants.ContentType_Params).Append("=").Append(NetmeraConstants.Default_ContentType).Append("&")
                        //.Append(NetmeraConstants.ContentName_Params).Append("=").Append(contentNameRnd).Append("&")
                        //.Append(NetmeraConstants.ContentPrivacy_Params).Append("=").Append(paramsObject[NetmeraConstants.ContentPrivacy_Params]).Append("&")
                        //.Append(NetmeraConstants.Path_Params).Append("=").Append(NetmeraConstants.Default_ParentPath).Append("&")
                        //.Append(NetmeraConstants.Service_Params).Append("=").Append(NetmeraConstants.Default_Service).Append("&")
                        //.Append(NetmeraConstants.Action_Params).Append("=").Append(NetmeraConstants.Create_Action).Append("&")
                        //.Append(NetmeraConstants.Content_Params).Append("=").Append(JsonConvert.SerializeObject(paramsObject));
                string strBody = JsonConvert.SerializeObject(paramsObject);

                sendHttpWithJson(strBuild.ToString(), strBody,NetmeraConstants.Http_Method_Post, (o, e) =>
                {
                    if (o == null)
                    {
                        if (callback != null)
                            callback(null, e);
                    }
                    else
                    {
                        JObject responseHttp = (JObject)o[NetmeraConstants.Entry_Params];
                        if (callback != null)
                            callback(responseHttp, e);
                    }
             
                });
            }
            else
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_API_KEY_NOT_FOUND, "You didn't set your api key. Please use NetmeraClient.init(apiKey)."));
            }
        }



        public static void updateContent(JObject paramsObject, String path, Action<JObject, Exception> callback)
        {
            String securityToken = NetmeraClient.getSecurityToken();
            if (securityToken != null && securityToken.Trim() != "")
            {
                StringBuilder strBuild = new StringBuilder()
                .Append(NetmeraConstants.Netmera_Domain_Url)
                .Append(NetmeraConstants.Netmera_Domain_Rest_Url)
                .Append(NetmeraConstants.Netmera_UpdateContent_Url)
                .Append(NetmeraConstants.SecurityToken_Params).Append("=").Append(securityToken).Append("&")
                .Append(NetmeraConstants.ContentType_Params).Append("=").Append(NetmeraConstants.Default_ContentType).Append("&")
                .Append(NetmeraConstants.Path_Params).Append("=").Append(path).Append("&")
                .Append(NetmeraConstants.ContentPrivacy_Params).Append("=").Append(paramsObject[NetmeraConstants.ContentPrivacy_Params]).Append("&")
                .Append(NetmeraConstants.Service_Params).Append("=").Append(NetmeraConstants.Default_Service).Append("&")
                .Append(NetmeraConstants.Action_Params).Append("=").Append(NetmeraConstants.Update_Action).Append("&")
                .Append(NetmeraConstants.Content_Params).Append("=").Append(JsonConvert.SerializeObject(paramsObject));

                sendHttp(strBuild.ToString(), NetmeraConstants.Http_Method_Post, (o, e) =>
                {
                    if (o != null)
                    {
                        JObject responseHttp = (JObject)o[NetmeraConstants.Entry_Params];
                        if (callback != null)
                            callback(responseHttp, e);
                    }
                    else
                    {
                        if (callback != null)
                            callback(null, e);
                    }
                });
            }
            else
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_API_KEY_NOT_FOUND, "You didn't set your api key. Please use NetmeraClient.init(apiKey)."));
            }
        }

        public static void updateBulkContent(JObject paramsObject, Action<JObject, Exception> callback)
        {
            String securityToken = NetmeraClient.getSecurityToken();
            if (securityToken != null && securityToken.Trim() != "")
            {
                StringBuilder strBuild = new StringBuilder()
                    .Append(NetmeraConstants.Netmera_Domain_Url)
                    .Append(NetmeraConstants.Netmera_Domain_Rpc_Url)
                    //.Append(NetmeraConstants.Netmera_UpdateContent_Url)
                    .Append(NetmeraConstants.SecurityToken_Params).Append("=").Append(securityToken);//.Append("&")
                //.Append(NetmeraConstants.ContentType_Params).Append("=").Append(NetmeraConstants.Default_ContentType).Append("&")
                //.Append(NetmeraConstants.Path_Params).Append("=").Append(path).Append("&")
                //.Append(NetmeraConstants.ContentPrivacy_Params).Append("=").Append(paramsObject[NetmeraConstants.ContentPrivacy_Params]).Append("&")
                //.Append(NetmeraConstants.Service_Params).Append("=").Append(NetmeraConstants.Default_Service).Append("&")
                //.Append(NetmeraConstants.Action_Params).Append("=").Append(NetmeraConstants.Update_Action).Append("&")
                //.Append(NetmeraConstants.Content_Params).Append("=").Append(JsonConvert.SerializeObject(paramsObject));

                string strBody = JsonConvert.SerializeObject(paramsObject);
                sendHttpWithJson(strBuild.ToString(), strBody,NetmeraConstants.Http_Method_Post, (o, e) =>
                {
                    if (o == null)
                    {
                        if (callback != null)
                            callback(null, e);
                    }
                    else
                    {
                        JObject responseHttp = (JObject)o[NetmeraConstants.Entry_Params];
                        if (callback != null)
                            callback(responseHttp, e);
                    }
             
                });
            }
            else
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_API_KEY_NOT_FOUND, "You didn't set your api key. Please use NetmeraClient.init(apiKey)."));
            }
        }

        public static void deleteContent(String path, Action<bool, Exception> callback)
        {

            String securityToken = NetmeraClient.getSecurityToken();
            if (securityToken != null && securityToken.Trim() != "")
            {
                StringBuilder strBuild = new StringBuilder()
                    .Append(NetmeraConstants.Netmera_Domain_Url)
                    .Append(NetmeraConstants.Netmera_Domain_Rest_Url)
                    .Append(NetmeraConstants.Netmera_RemoveContent_Url)
                    .Append(NetmeraConstants.SecurityToken_Params).Append("=").Append(securityToken).Append("&")
                    .Append(NetmeraConstants.Path_Params).Append("=").Append(path);

                sendHttpwithReturn(strBuild.ToString(), NetmeraConstants.Http_Method_Post, (returnValue, o, ex) =>
                {
                    if (callback != null)
                        callback(returnValue, ex);

                });
            }
            else
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_API_KEY_NOT_FOUND, "You didn't set your api key. Please use NetmeraClient.init(apiKey).");
            }

        }

        public static void search(RequestItem item, Action<JObject, Exception> callback)
        {
            String securityToken = NetmeraClient.getSecurityToken();
            if (securityToken != null && securityToken.Trim() != "")
            {
                StringBuilder strBuild = new StringBuilder()
                    .Append(NetmeraConstants.Netmera_Domain_Url)
                    .Append(NetmeraConstants.Netmera_Domain_Rest_Url)
                    .Append(NetmeraConstants.Netmera_SearchContent_Url)
                    .Append(NetmeraConstants.SecurityToken_Params).Append("=").Append(securityToken).Append("&")
                    .Append(NetmeraConstants.Path_Params).Append("=").Append(NetmeraConstants.Default_ParentPath).Append("&")
                    .Append(NetmeraConstants.ContentType_Params).Append("=").Append(NetmeraConstants.Default_ContentType).Append("&")
                    .Append(NetmeraConstants.CustomCondition_Params).Append("=").Append(item.customCondition).Append("&")
                    .Append(NetmeraConstants.SearchText_Params).Append("=").Append(item.searchText).Append("&")
                    .Append(NetmeraConstants.Max_Params).Append("=").Append(item.max).Append("&")
                    .Append(NetmeraConstants.Page_Params).Append("=").Append(item.page).Append("&")
                    .Append(NetmeraConstants.SortBy_Params).Append("=").Append(item.sortBy).Append("&")
                    .Append(NetmeraConstants.SortOrder_Params).Append("=").Append(item.sortOrder).Append("&");

                sendHttp(strBuild.ToString(), NetmeraConstants.Http_Method_Post, (o, ex) =>
                {
                    if (callback != null)
                        callback(o, ex);
                });
            }
            else
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_API_KEY_NOT_FOUND, "You didn't set your api key. Please use NetmeraClient.init(apiKey)."));
            }
        }

        public static void searchUser(RequestItem item, Action<JObject, Exception> callback)
        {
            String securityToken = NetmeraClient.getSecurityToken();
            if (securityToken != null && securityToken.Trim() != "")
            {
                StringBuilder strBuild = new StringBuilder()
                    .Append(NetmeraConstants.Netmera_Domain_Url)
                    .Append(NetmeraConstants.Netmera_Domain_Rest_Url)
                    .Append(NetmeraConstants.Netmera_PeopleSearch_Url)
                    .Append(NetmeraConstants.SecurityToken_Params).Append("=").Append(securityToken).Append("&")
                    .Append(NetmeraConstants.Path_Params).Append("=").Append(item.path).Append("&")
                    .Append(NetmeraConstants.SearchText_Params).Append("=").Append(item.searchText).Append("&")
                    .Append(NetmeraConstants.CustomCondition_Params).Append("=").Append(item.customCondition).Append("&")
                    .Append(NetmeraConstants.Max_Params).Append("=").Append(item.max).Append("&")
                    .Append(NetmeraConstants.Page_Params).Append("=").Append(item.page).Append("&")
                    .Append(NetmeraConstants.SortBy_Params).Append("=").Append(item.sortBy).Append("&")
                    .Append(NetmeraConstants.SortOrder_Params).Append("=").Append(item.sortOrder).Append("&")
                    .Append(NetmeraConstants.Filter_Params).Append("=").Append(item.filterBy).Append("&")
                    .Append(NetmeraConstants.FilterValue_Params).Append("=").Append(item.filterValue).Append("&")
                    .Append(NetmeraConstants.FilterOperation_Params).Append("=").Append(item.filterOperation).Append("&");

                sendHttp(strBuild.ToString(), NetmeraConstants.Http_Method_Post, (o, ex) =>
                {
                    if (callback != null)
                        callback(o, ex);
                });
            }
            else
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_API_KEY_NOT_FOUND, "You didn't set your api key. Please use NetmeraClient.init(apiKey)."));
            }
        }

        public static void get(RequestItem item, Action<JObject, Exception> callback)
        {
            String securityToken = NetmeraClient.getSecurityToken();
            if (securityToken != null && securityToken.Trim() != "")
            {
                StringBuilder strBuild = new StringBuilder()
                   .Append(NetmeraConstants.Netmera_Domain_Url)
                   .Append(NetmeraConstants.Netmera_Domain_Rest_Url)
                   .Append(NetmeraConstants.Netmera_GetContent_Url)
                   .Append(NetmeraConstants.SecurityToken_Params).Append("=").Append(securityToken).Append("&")
                   .Append(NetmeraConstants.Path_Params).Append("=").Append(item.path);

                sendHttp(strBuild.ToString(), NetmeraConstants.Http_Method_Get, (o, ex) =>
                {
                    if (callback != null)
                        callback(o, ex);
                });
            }
            else
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_API_KEY_NOT_FOUND, "You didn't set your api key. Please use NetmeraClient.init(apiKey)."));
            }
        }

        public static void locationSearch(RequestItem item, Action<JObject, Exception> callback)
        {
            String securityToken = NetmeraClient.getSecurityToken();
            if (securityToken != null && securityToken.Trim() != "")
            {
                StringBuilder strBuild = new StringBuilder()
                    .Append(NetmeraConstants.Netmera_Domain_Url)
                    .Append(NetmeraConstants.Netmera_Domain_Rest_Url)
                    .Append(NetmeraConstants.Netmera_LocationSearchContent_Url)
                    .Append(NetmeraConstants.SecurityToken_Params).Append("=").Append(securityToken).Append("&")
                    .Append(NetmeraConstants.Path_Params).Append("=").Append(NetmeraConstants.Default_ParentPath).Append("&")
                    .Append(NetmeraConstants.ContentType_Params).Append("=").Append(NetmeraConstants.Default_ContentType).Append("&")
                    .Append(NetmeraConstants.CustomCondition_Params).Append("=").Append(item.customCondition).Append("&")
                    .Append(NetmeraConstants.SearchText_Params).Append("=").Append(item.searchText).Append("&")
                    .Append(NetmeraConstants.Max_Params).Append("=").Append(item.max).Append("&")
                    .Append(NetmeraConstants.Page_Params).Append("=").Append(item.page).Append("&")
                    .Append(NetmeraConstants.SortBy_Params).Append("=").Append(item.sortBy).Append("&")
                    .Append(NetmeraConstants.SortOrder_Params).Append("=").Append(item.sortOrder).Append("&")
                    .Append(NetmeraConstants.LocationSearchType_Params).Append("=").Append(item.locationSearchType).Append("&")
                    .Append(NetmeraConstants.LocationSearchField_Params).Append("=").Append(item.locationSearchField + NetmeraConstants.LocationField_Suffix).Append("&")
                    .Append(NetmeraConstants.LocationLatitude_Params).Append("=").Append(item.latitudes).Append("&")
                    .Append(NetmeraConstants.LocationLongitude_Params).Append("=").Append(item.longitudes).Append("&")
                    .Append(NetmeraConstants.LocationDistance_Params).Append("=").Append(item.distance).Append("&");

                sendHttp(strBuild.ToString(), NetmeraConstants.Http_Method_Post, (o, ex) =>
                {
                    if (callback != null)
                        callback(o, ex);
                });
            }
            else
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_API_KEY_NOT_FOUND, "You didn't set your api key. Please use NetmeraClient.init(apiKey)."));
            }
        }

        public static void registerUser(RequestItem item, Action<JObject, Exception> callback)
        {
            String securityToken = NetmeraClient.getSecurityToken();
            if (securityToken != null && securityToken.Trim() != "")
            {
                StringBuilder strBuild = new StringBuilder()
                    .Append(NetmeraConstants.Netmera_Domain_Url)
                    .Append(NetmeraConstants.Netmera_Domain_Rest_Url)
                    .Append(NetmeraConstants.Netmera_RegisterUser_Url)
                    .Append(NetmeraConstants.SecurityToken_Params).Append("=").Append(securityToken).Append("&")
                    .Append(NetmeraConstants.Netmera_UserEmail).Append("=").Append(item.getEmail()).Append("&")
                    .Append(NetmeraConstants.Netmera_UserPassword).Append("=").Append(item.getPassword()).Append("&")
                    .Append(NetmeraConstants.Netmera_UserNickname).Append("=").Append(item.getNickname()).Append("&")
                    .Append(NetmeraConstants.Netmera_UserName).Append("=").Append(item.getName()).Append("&")
                    .Append(NetmeraConstants.Netmera_UserSurname).Append("=").Append(item.getSurname());

                sendHttp(strBuild.ToString(), NetmeraConstants.Http_Method_Post, (o, e) =>
                {
                    if (callback != null)
                        callback(o, e);
                });
            }
            else
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_API_KEY_NOT_FOUND, "You didn't set your api key. Please use NetmeraClient.init(apiKey)."));
            }
        }

        public static void profileUpdate(RequestItem item, Action<JObject, Exception> callback)
        {
            String securityToken = NetmeraClient.getSecurityToken();
            if (securityToken != null && securityToken.Trim() != "")
            {
                StringBuilder strBuildForJson = new StringBuilder()
                    .Append("{\"" + NetmeraConstants.Params_Params + "\":{")
                    .Append("\"" + NetmeraConstants.Netmera_UserEmail + "\" : \"" + item.getEmail() + "\"").Append(",")
                    .Append("\"" + NetmeraConstants.Netmera_UserNickname + "\" : \"" + item.getNickname() + "\"").Append(",")
                    .Append("\"" + NetmeraConstants.Netmera_UserName + "\" : \"" + item.getName() + "\"").Append(",")
                    .Append("\"" + NetmeraConstants.Netmera_UserSurname + "\" : \"" + item.getSurname() + "\"")
                    .Append(" },\"" + NetmeraConstants.Method_Params + "\":\"" + NetmeraConstants.Netmera_PeopleProfileUpdate_Method + "\"}");


                StringBuilder strBuildForUrl = new StringBuilder()
                    .Append(NetmeraConstants.Netmera_Domain_Url)
                    .Append(NetmeraConstants.Netmera_Domain_Rpc_Url)
                    .Append(NetmeraConstants.SecurityToken_Params).Append("=").Append(securityToken);

                sendHttpWithJson(strBuildForUrl.ToString(), strBuildForJson.ToString(), NetmeraConstants.Http_Method_Post, (o, ex) =>
                {
                    if (callback != null)
                        callback(o, ex);
                });
            }
            else
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_API_KEY_NOT_FOUND, "You didn't set your api key. Please use NetmeraClient.init(apiKey)."));
            }
        }

        public static void accountUpdate(RequestItem item, Action<JObject, Exception> callback)
        {
            String securityToken = NetmeraClient.getSecurityToken();
            if (securityToken != null && securityToken.Trim() != "")
            {
                StringBuilder strBuildForJson = new StringBuilder()
                   .Append("{\"" + NetmeraConstants.Params_Params + "\":{")
                   .Append("\"" + NetmeraConstants.Netmera_UserEmail + "\" : \"" + item.getEmail() + "\"").Append(",")
                   .Append("\"" + NetmeraConstants.Netmera_UserPassword + "\" : \"" + item.getPassword() + "\"").Append(",")
                   .Append("\"" + NetmeraConstants.Netmera_UserName + "\" : \"" + item.getName() + "\"").Append(",")
                   .Append("\"" + NetmeraConstants.Netmera_UserSurname + "\" : \"" + item.getSurname() + "\"")
                   .Append(" },\"" + NetmeraConstants.Method_Params + "\":\"" + NetmeraConstants.Netmera_PeopleAccountUpdate_Method + "\"}");


                StringBuilder strBuildForUrl = new StringBuilder()
                    .Append(NetmeraConstants.Netmera_Domain_Url)
                    .Append(NetmeraConstants.Netmera_Domain_Rpc_Url)
                    .Append(NetmeraConstants.SecurityToken_Params).Append("=").Append(securityToken);

                sendHttpWithJson(strBuildForUrl.ToString(), strBuildForJson.ToString(), NetmeraConstants.Http_Method_Post, (o, ex) =>
                {
                    if (callback != null)
                        callback(o, ex);
                });
            }
            else
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_API_KEY_NOT_FOUND, "You didn't set your api key. Please use NetmeraClient.init(apiKey)."));
            }
        }

        public static void loginAsGuest(Action<JObject, Exception> callback)
        {
            String securityToken = NetmeraClient.getSecurityToken();
            if (securityToken != null && securityToken.Trim() != "")
            {
                StringBuilder strBuildForJson = new StringBuilder()
                   .Append("{\"" + NetmeraConstants.Params_Params + "\":{")
                   .Append(" },\"" + NetmeraConstants.Method_Params + "\":\"" + NetmeraConstants.Netmera_LoginGuestUser_Method + "\"}");

                StringBuilder strBuildForUrl = new StringBuilder()
                   .Append(NetmeraConstants.Netmera_Domain_Url)
                   .Append(NetmeraConstants.Netmera_Domain_Rpc_Url)
                   .Append(NetmeraConstants.SecurityToken_Params).Append("=").Append(securityToken);

                sendHttpWithJson(strBuildForUrl.ToString(), strBuildForJson.ToString(), NetmeraConstants.Http_Method_Post, (o, ex) =>
                {
                    if (callback != null)
                        callback(o, ex);
                });
            }
            else
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_API_KEY_NOT_FOUND, "You didn't set your api key. Please use NetmeraClient.init(apiKey)."));
            }
        }

        public static void login(RequestItem item, Action<JObject, Exception> callback)
        {
            String securityToken = NetmeraClient.getSecurityToken();
            if (securityToken != null && securityToken.Trim() != "")
            {
                StringBuilder strBuildForJson = new StringBuilder()
                   .Append("{\"" + NetmeraConstants.Params_Params + "\":{")
                   .Append("\"" + NetmeraConstants.Netmera_UserEmail + "\" : \"" + item.getEmail() + "\"").Append(",")
                   .Append("\"" + NetmeraConstants.Netmera_UserPassword + "\" : \"" + item.getPassword() + "\"")
                   .Append(" },\"" + NetmeraConstants.Method_Params + "\":\"" + NetmeraConstants.Netmera_LoginUser_Method + "\"}");

                StringBuilder strBuildForUrl = new StringBuilder()
                   .Append(NetmeraConstants.Netmera_Domain_Url)
                   .Append(NetmeraConstants.Netmera_Domain_Rpc_Url)
                   .Append(NetmeraConstants.SecurityToken_Params).Append("=").Append(securityToken);

                sendHttpWithJson(strBuildForUrl.ToString(), strBuildForJson.ToString(), NetmeraConstants.Http_Method_Post, (o, ex) =>
                {
                    if (callback != null)
                        callback(o, ex);
                });
            }
            else
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_API_KEY_NOT_FOUND, "You didn't set your api key. Please use NetmeraClient.init(apiKey)."));
            }
        }

        public static void facebookRegisterUser(RequestItem item, Action<JObject, Exception> callback)
        {
            String securityToken = NetmeraClient.getSecurityToken();
            if (securityToken != null && securityToken.Trim() != "")
            {
                StringBuilder strBuild = new StringBuilder()
                    .Append(NetmeraConstants.Netmera_Domain_Url)
                    .Append(NetmeraConstants.Netmera_Domain_Rest_Url)
                    .Append(NetmeraConstants.Rest_Facebook_Login_Url)
                    .Append(NetmeraConstants.SecurityToken_Params).Append("=").Append(securityToken).Append("&")
                    .Append(NetmeraConstants.Netmera_UserFbId).Append("=").Append(item.getFbId()).Append("&")
                    .Append(NetmeraConstants.Netmera_UserNickname).Append("=").Append(item.getNickname()).Append("&")
                    .Append(NetmeraConstants.Netmera_UserName).Append("=").Append(item.getName()).Append("&")
                    .Append(NetmeraConstants.Netmera_UserSurname).Append("=").Append(item.getSurname()).Append("&")
                    .Append(NetmeraConstants.Netmera_UserEmail).Append("=").Append(item.getEmail());

                sendHttp(strBuild.ToString(), NetmeraConstants.Http_Method_Post, (o, e) =>
                {
                    if (callback != null)
                        callback(o, e);
                });
            }
            else
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_API_KEY_NOT_FOUND, "You didn't set your api key. Please use NetmeraClient.init(apiKey)."));
            }
        }

        public static void twitterRegisterUser(RequestItem item, Action<JObject, Exception> callback)
        {
            String securityToken = NetmeraClient.getSecurityToken();
            if (securityToken != null && securityToken.Trim() != "")
            {
                StringBuilder strBuild = new StringBuilder()
                    .Append(NetmeraConstants.Netmera_Domain_Url)
                    .Append(NetmeraConstants.Netmera_Domain_Rest_Url)
                    .Append(NetmeraConstants.Rest_Twitter_Login_Url)
                    .Append(NetmeraConstants.SecurityToken_Params).Append("=").Append(securityToken).Append("&")
                    .Append(NetmeraConstants.Netmera_UserTwId).Append("=").Append(item.getTwId()).Append("&")
                    .Append(NetmeraConstants.Netmera_UserNickname).Append("=").Append(item.getNickname()).Append("&")
                    .Append(NetmeraConstants.Netmera_UserName).Append("=").Append(item.getName()).Append("&")
                    .Append(NetmeraConstants.Netmera_UserSurname).Append("=").Append(item.getSurname());

                sendHttp(strBuild.ToString(), NetmeraConstants.Http_Method_Post, (o, e) =>
                {
                    if (callback != null)
                        callback(o, e);
                });
            }
            else
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_API_KEY_NOT_FOUND, "You didn't set your api key. Please use NetmeraClient.init(apiKey)."));
            }
        }

        public static void getRichPushContent(String url, Dictionary<string, object> postParameters, Action<JObject, Exception> callback)
        {
            string userAgent = "";
            MultipartFormData.MultipartFormDataPost(url, userAgent, postParameters, (response, ex) =>
            {
                if (response != null)
                {
                    int statusCode = (int)response.StatusCode;

                    if (statusCode == 200)
                    {
                        Stream responseStream = response.GetResponseStream();
                        if (responseStream != null)
                        {
                            StreamReader responseStreamReader = new StreamReader(responseStream);
                            string responseStr = responseStreamReader.ReadToEnd();

                            JObject json = JObject.Parse(responseStr);
                            callback(json, null);
                        }
                    }
                    else
                    {
                        if (callback != null)
                            callback(null, new NetmeraException(Netmera.NetmeraException.ErrorCode.EC_HTTP_PROTOCOL_EXCEPTION, "Http exception occurred while getting push notification content with HTTP Code : " + statusCode));
                    }
                }
                else
                {
                    if (callback != null)
                        callback(null, new NetmeraException(Netmera.NetmeraException.ErrorCode.EC_NULL_EXCEPTION, "Response of unregister push notification method is null"));
                }
            });
        }

        public static void activateUser(RequestItem item, Action<Exception> callback)
        {
            String securityToken = NetmeraClient.getSecurityToken();
            if (securityToken != null && securityToken.Trim() != "")
            {
                StringBuilder strBuildForJson = new StringBuilder()
                   .Append("{\"" + NetmeraConstants.Method_Params + "\":{")
                   .Append("\"" + NetmeraConstants.Netmera_UserEmail + "\" : \"" + item.getEmail() + "\"")
                   .Append(" },\"" + NetmeraConstants.Method_Params + "\":\"" + NetmeraConstants.Netmera_ActivateUser_Method + "\"}");

                StringBuilder strBuildForUrl = new StringBuilder()
                   .Append(NetmeraConstants.Netmera_Domain_Url)
                   .Append(NetmeraConstants.Netmera_Domain_Rpc_Url)
                   .Append(NetmeraConstants.SecurityToken_Params).Append("=").Append(securityToken);

                sendHttpWithJson(strBuildForUrl.ToString(), strBuildForJson.ToString(), NetmeraConstants.Http_Method_Post, (o, ex) =>
                {
                    if (callback != null)
                        callback(ex);
                });
            }
            else
            {
                if (callback != null)
                    callback(new NetmeraException(NetmeraException.ErrorCode.EC_API_KEY_NOT_FOUND, "You didn't set your api key. Please use NetmeraClient.init(apiKey)."));
            }
        }

        public static void deactivateUser(RequestItem item, Action<Exception> callback)
        {
            String securityToken = NetmeraClient.getSecurityToken();
            if (securityToken != null && securityToken.Trim() != "")
            {
                StringBuilder strBuildForJson = new StringBuilder()
                   .Append("{\"" + NetmeraConstants.Params_Params + "\":{")
                   .Append("\"" + NetmeraConstants.Netmera_UserEmail + "\" : \"" + item.getEmail() + "\"")
                   .Append(" },\"" + NetmeraConstants.Method_Params + "\":\"" + NetmeraConstants.Netmera_DeactivateUser_Method + "\"}");

                StringBuilder strBuildForUrl = new StringBuilder()
                   .Append(NetmeraConstants.Netmera_Domain_Url)
                   .Append(NetmeraConstants.Netmera_Domain_Rpc_Url)
                   .Append(NetmeraConstants.SecurityToken_Params).Append("=").Append(securityToken);

                sendHttpWithJson(strBuildForUrl.ToString(), strBuildForJson.ToString(), NetmeraConstants.Http_Method_Post, (o, ex) =>
                {
                    if (callback != null)
                        callback(ex);
                });
            }
            else
            {
                if (callback != null)
                    callback(new NetmeraException(NetmeraException.ErrorCode.EC_API_KEY_NOT_FOUND, "You didn't set your api key. Please use NetmeraClient.init(apiKey)."));
            }
        }

        private static long currentTimeMillis()
        {
            DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (long)((DateTime.UtcNow - Jan1st1970).TotalMilliseconds);
        }

        private static NetmeraPushDetail parsePushResponse(JObject channelJson)
        {
            NetmeraPushDetail channelDetail = new NetmeraPushDetail();
            try
            {
                JObject result = channelJson.Value<JObject>("result");
                channelDetail.setError(result.Value<String>("error"));
                channelDetail.setFailed(result.Value<int>("failed"));
                channelDetail.setSuccessful(result.Value<int>("successful"));
                channelDetail.setMessage(result.Value<String>("message"));
                channelDetail.setStatus(result.Value<String>("status"));
                channelDetail.setPath(result.Value<String>("path"));
            }
            catch (JsonException)
            {

            }
            return channelDetail;
        }
    }
}