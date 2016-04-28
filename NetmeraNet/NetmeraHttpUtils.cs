using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace Netmera
{
    internal static class NetmeraHttpUtils
    {
        public static JObject sendHttp(String url, String method)
        {
            try
            {
                return sendHttpWithJson(url, null, method);
            }
            catch (WebException)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_REQUEST, "A general Web exception occurred.");
            }
            catch (NetmeraException)
            {
                throw;
            }
        }

        public static JObject sendHttpWithJson(String url, String jsonCall, String method)
        {
            try
            {
                StringBuilder strBuild = new StringBuilder(url);
                strBuild.Append("&").Append(NetmeraConstants.Netmera_SDK_Params).Append(NetmeraConstants.Netmera_SDK_Value);
                strBuild.Append("&").Append(NetmeraConstants.Netmera_SDKVERSION_Params).Append(NetmeraConstants.Netmera_SDKVERSION_Value);

                WebRequest webRequest = WebRequest.Create(url);
                webRequest.Method = method;
                webRequest.ContentType = NetmeraConstants.Http_Content_Type;

                // it is for json call (add json body to the request. e.g. for user update operation)
                if (!String.IsNullOrEmpty(jsonCall))
                {
                    Stream writerStream = ((HttpWebRequest)webRequest).GetRequestStream();
                    StreamWriter writer = new StreamWriter(writerStream);
                    writer.Write(jsonCall);
                    writer.Flush();
                    writer.Close();
                }

                HttpWebResponse b = (HttpWebResponse)webRequest.GetResponse();

                Stream readerStream = b.GetResponseStream();
                StreamReader uberReader = new StreamReader(readerStream);
                String response = uberReader.ReadToEnd();
                b.Close();

                NetmeraClient.finish();

                return JObject.Parse(response);
            }
            catch (WebException)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_REQUEST, "A general Web exception occurred.");
            }
        }

        public static bool sendHttpwithReturn(String url, String method)
        {
            try
            {
                return sendHttpWithJsonwithReturn(url, null, method);
            }
            catch (WebException)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_REQUEST, "A general Web exception occurred.");
            }
            catch (NetmeraException)
            {
                throw;
            }
        }

        public static bool sendHttpWithJsonwithReturn(String url, String jsonCall, String method)
        {
            bool flag = false;
            try
            {
                StringBuilder strBuild = new StringBuilder(url);
                strBuild.Append("&").Append(NetmeraConstants.Netmera_SDK_Params).Append(NetmeraConstants.Netmera_SDK_Value);
                strBuild.Append("&").Append(NetmeraConstants.Netmera_SDKVERSION_Params).Append(NetmeraConstants.Netmera_SDKVERSION_Value);

                WebRequest webRequest = WebRequest.Create(url);
                webRequest.Method = method;
                webRequest.ContentType = NetmeraConstants.Http_Content_Type;

                // it is for json call (add json body to the request. e.g. for user update operation)
                if (!String.IsNullOrEmpty(jsonCall))
                {
                    Stream writerStream = ((HttpWebRequest)webRequest).GetRequestStream();
                    StreamWriter writer = new StreamWriter(writerStream);
                    writer.Write(jsonCall);
                    writer.Flush();
                    writer.Close();
                }

                HttpWebResponse b = (HttpWebResponse)webRequest.GetResponse();

                Stream readerStream = b.GetResponseStream();
                StreamReader uberReader = new StreamReader(readerStream);
                String response = uberReader.ReadToEnd();
                b.Close();

                NetmeraClient.finish();
                if (response.IndexOf("true") != -1)
                {
                    flag = true;
                }
                return flag;
            }
            catch (WebException)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_REQUEST, "A general Web exception occurred.");
            }
        }

        public static Dictionary<Netmera.BasePush.PushChannel, NetmeraPushDetail> sendPushMessage(String url, Dictionary<string, object> postParameters)
        {
            string userAgent = "Someone";

            StringBuilder strBuild = new StringBuilder(url);
            strBuild.Append("&").Append(NetmeraConstants.Netmera_SDK_Params).Append(NetmeraConstants.Netmera_SDK_Value);
            strBuild.Append("&").Append(NetmeraConstants.Netmera_SDKVERSION_Params).Append(NetmeraConstants.Netmera_SDKVERSION_Value);

            HttpWebResponse response = MultipartFormData.MultipartFormDataPost(url, userAgent, postParameters);
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
                            return result;
                        }
                        else
                        {
                            String error = json.Value<String>("message");
                            throw new NetmeraException(NetmeraException.ErrorCode.EC_PUSH_ERROR, error);
                        }
                    }
                    else
                    {
                        throw new NetmeraException(NetmeraException.ErrorCode.EC_NULL_EXCEPTION, "Response entity is null while sending notification to devices");
                    }
                }
                else
                {
                    throw new NetmeraException(NetmeraException.ErrorCode.EC_HTTP_PROTOCOL_EXCEPTION, "Http exception occurred while sending notification to android devices");
                }
            }
            else
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_NULL_EXCEPTION, "Response is null while sending notification to android devices");
            }
        }

        public static JObject createContent(JObject paramsObject)
        {
            //security token is stored in a temporary variable because createActionToken resets the security token
            //inside NetmeraClien due to the method sendHttp which contains NetmeraClient.finish();
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

                JObject responseHttp = sendHttp(strBuild.ToString(), NetmeraConstants.Http_Method_Post);
                return (JObject)responseHttp[NetmeraConstants.Entry_Params];
            }
            else
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_API_KEY_NOT_FOUND, "You didn't set your api key. Please use NetmeraClient.init(apiKey).");
            }
        }

        public static JObject createBulkContent(JObject paramsObject)
        {
            //security token is stored in a temporary variable because createActionToken resets the security token
            //inside NetmeraClien due to the method sendHttp which contains NetmeraClient.finish();
            String securityToken = NetmeraClient.getSecurityToken();

            if (securityToken != null && securityToken.Trim() != "")
            {
                long contentNameRnd = currentTimeMillis();

                StringBuilder strBuild = new StringBuilder()
                    .Append(NetmeraConstants.Netmera_Domain_Url)
                    .Append(NetmeraConstants.Netmera_Domain_Rpc_Url)
                    //.Append(NetmeraConstants.Netmera_CreateBulkContent_Url)
                    .Append(NetmeraConstants.SecurityToken_Params).Append("=").Append(securityToken);
                        //.Append("&")
                    //.Append(NetmeraConstants.ContentType_Params).Append("=").Append(NetmeraConstants.Default_ContentType).Append("&")
                    //.Append(NetmeraConstants.ContentName_Params).Append("=").Append(contentNameRnd).Append("&")
                    //.Append(NetmeraConstants.ContentPrivacy_Params).Append("=").Append(paramsObject[NetmeraConstants.ContentPrivacy_Params]).Append("&")
                    //.Append(NetmeraConstants.Path_Params).Append("=").Append(NetmeraConstants.Default_ParentPath).Append("&")
                    //.Append(NetmeraConstants.Service_Params).Append("=").Append(NetmeraConstants.Default_Service).Append("&")
                    //.Append(NetmeraConstants.Action_Params).Append("=").Append(NetmeraConstants.Create_Action).Append("&")
                    //.Append(NetmeraConstants.Content_Params);
                //.Append("=").Append(JsonConvert.SerializeObject(paramsObject));
                String strBody = JsonConvert.SerializeObject(paramsObject);

                JObject responseHttp = sendHttpWithJson(strBuild.ToString(), strBody, NetmeraConstants.Http_Method_Post);
                return responseHttp;
            }
            else
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_API_KEY_NOT_FOUND, "You didn't set your api key. Please use NetmeraClient.init(apiKey).");
            }
        }





        public static JObject updateContent(JObject paramsObject, String path)
        {
            //the same as in the createContent method
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

                JObject responseHttp = sendHttp(strBuild.ToString(), NetmeraConstants.Http_Method_Post);
                return (JObject)responseHttp[NetmeraConstants.Entry_Params];
            }
            else
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_API_KEY_NOT_FOUND, "You didn't set your api key. Please use NetmeraClient.init(apiKey).");
            }
        }

        public static bool deleteContent(String path)
        {
            bool flag = false;
            String securityToken = NetmeraClient.getSecurityToken();
            if (securityToken != null && securityToken.Trim() != "")
            {
                StringBuilder strBuild = new StringBuilder()
                    .Append(NetmeraConstants.Netmera_Domain_Url)
                    .Append(NetmeraConstants.Netmera_Domain_Rest_Url)
                    .Append(NetmeraConstants.Netmera_RemoveContent_Url)
                    .Append(NetmeraConstants.SecurityToken_Params).Append("=").Append(securityToken).Append("&")
                    .Append(NetmeraConstants.Path_Params).Append("=").Append(path);

                flag=sendHttpwithReturn(strBuild.ToString(), NetmeraConstants.Http_Method_Post);
            }
            else
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_API_KEY_NOT_FOUND, "You didn't set your api key. Please use NetmeraClient.init(apiKey).");
            }
            return flag;
        }

        public static JObject search(RequestItem item)
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

                return sendHttp(strBuild.ToString(), NetmeraConstants.Http_Method_Post);
            }
            else
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_API_KEY_NOT_FOUND, "You didn't set your api key. Please use NetmeraClient.init(apiKey).");
            }
        }

        public static JObject searchUser(RequestItem item)
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

                return sendHttp(strBuild.ToString(), NetmeraConstants.Http_Method_Post);
            }
            else
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_API_KEY_NOT_FOUND, "You didn't set your api key. Please use NetmeraClient.init(apiKey).");
            }
        }

        public static JObject get(RequestItem item)
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

                return sendHttp(strBuild.ToString(), NetmeraConstants.Http_Method_Get);
            }
            else
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_API_KEY_NOT_FOUND, "You didn't set your api key. Please use NetmeraClient.init(apiKey).");
            }
        }

        public static JObject locationSearch(RequestItem item)
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

                return sendHttp(strBuild.ToString(), NetmeraConstants.Http_Method_Post);
            }
            else
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_API_KEY_NOT_FOUND, "You didn't set your api key. Please use NetmeraClient.init(apiKey).");
            }
        }

        public static JObject registerUser(RequestItem item)
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

                return sendHttp(strBuild.ToString(), NetmeraConstants.Http_Method_Post);
            }
            else
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_API_KEY_NOT_FOUND, "You didn't set your api key. Please use NetmeraClient.init(apiKey).");
            }
        }

        public static JObject profileUpdate(RequestItem item)
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

                return sendHttpWithJson(strBuildForUrl.ToString(), strBuildForJson.ToString(), NetmeraConstants.Http_Method_Post);
            }
            else
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_API_KEY_NOT_FOUND, "You didn't set your api key. Please use NetmeraClient.init(apiKey).");
            }
        }

        public static JObject accountUpdate(RequestItem item)
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

                return sendHttpWithJson(strBuildForUrl.ToString(), strBuildForJson.ToString(), NetmeraConstants.Http_Method_Post);
            }
            else
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_API_KEY_NOT_FOUND, "You didn't set your api key. Please use NetmeraClient.init(apiKey).");
            }
        }

        public static JObject login(RequestItem item)
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

                return sendHttpWithJson(strBuildForUrl.ToString(), strBuildForJson.ToString(), NetmeraConstants.Http_Method_Post);
            }
            else
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_API_KEY_NOT_FOUND, "You didn't set your api key. Please use NetmeraClient.init(apiKey).");
            }
        }

        public static JObject loginAsGuest()
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

                return sendHttpWithJson(strBuildForUrl.ToString(), strBuildForJson.ToString(), NetmeraConstants.Http_Method_Post);
            }
            else
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_API_KEY_NOT_FOUND, "You didn't set your api key. Please use NetmeraClient.init(apiKey).");
            }
        }

        public static void activateUser(RequestItem item)
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

                sendHttpWithJson(strBuildForUrl.ToString(), strBuildForJson.ToString(), NetmeraConstants.Http_Method_Post);
            }
            else
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_API_KEY_NOT_FOUND, "You didn't set your api key. Please use NetmeraClient.init(apiKey).");
            }
        }

        public static void deactivateUser(RequestItem item)
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

                sendHttpWithJson(strBuildForUrl.ToString(), strBuildForJson.ToString(), NetmeraConstants.Http_Method_Post);
            }
            else
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_API_KEY_NOT_FOUND, "You didn't set your api key. Please use NetmeraClient.init(apiKey).");
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