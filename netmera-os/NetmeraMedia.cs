using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Collections.Specialized;

namespace Netmera
{
    /// <summary>
    /// It is used to create media content. 
    /// Its constructor is used to create different types of media content.
    /// </summary>
    public class NetmeraMedia
    {
        /// <summary>
        ///  Type of the photo size.
        /// </summary>
        public enum PhotoSize
        {
            /// <summary>
            /// This is the original image size.If no photo size is setted then original size is used.
            /// </summary>
            DEFAULT,

            /// <summary>
            /// Image size is defined as 41x41
            /// </summary>
            THUMBNAIL,

            /// <summary>
            /// Image size is defined as 82x82 px
            /// </summary>
            SMALL,

            /// <summary>
            /// Image size is defined as 180x250 px
            /// </summary>
            MEDIUM,

            /// <summary>
            /// Image size is defined as 300x350 px
            /// </summary>
            LARGE
        }

        /// <summary>
        ///  Byte array that holds image data
        /// </summary>
        private byte[] data;

        /// <summary>
        /// Path of the media
        /// </summary>
        private String url;

        /// <summary>
        /// Constructor that takes byte array as a parameter and creates <see cref="NetmeraMedia"/> object
        /// </summary>
        /// <param name="data">The image's data as byte array</param>
        public NetmeraMedia(byte[] data)
        {
            this.data = data;
        }

        /// <summary>
        /// Returns the image data as byte array.
        /// </summary>
        /// <returns>The image data as byte array.</returns>
        public byte[] getData()
        {
            return data;
        }

        internal void setUrl(String url)
        {
            this.url = url;
        }

        /// <summary>
        /// Returns the URL of file with the given size.
        /// </summary>
        /// <param name="size"> <see cref="PhotoSize"/> object</param>
        /// <returns>URL of the photo with the given size</returns>
        public String getUrl(PhotoSize size)
        {
            String url = null;

            if (size == PhotoSize.SMALL)
            {
                url = this.url.Replace("/org", "/small");
            }
            else if (size == PhotoSize.MEDIUM)
            {
                url = this.url.Replace("/org", "/medium");
            }
            else if (size == PhotoSize.LARGE)
            {
                url = this.url.Replace("/org", "/large");
            }
            else if (size == PhotoSize.THUMBNAIL)
            {
                url = this.url.Replace("/org", "/thumbnail");
            }
            else
            {
                url = this.url;
            }

            return url;
        }

        internal void save(String appId, String apiKey, String contentPath, String viewerId, Action<String, Exception> callback)
        {
            try
            {
                Dictionary<String, Object> uploadMap = new Dictionary<String, Object>();

                uploadMap.Add(NetmeraConstants.Upload_Url_Params_Content_Path, contentPath);
                uploadMap.Add(NetmeraConstants.Upload_Url_Params_St, apiKey);
                uploadMap.Add(NetmeraConstants.Upload_Url_Params_Opensocial_Viewer_Id, viewerId);
                uploadMap.Add(NetmeraConstants.Upload_Url_Params_Opensocial_Netmera_Domain, NetmeraConstants.Netmera_Domain_Url);
                uploadMap.Add(NetmeraConstants.Upload_Url_Params_Opensocial_App_Id, appId);

                getData(NetmeraConstants.Upload_Url, uploadMap, responseData =>
                {
                    Dictionary<String, String> resp = setUploadEntryParams(responseData);

                    StringBuilder strBuild = new StringBuilder();
                    strBuild.Append(NetmeraConstants.Swf_Url).Append(NetmeraConstants.Swf_Url_Params_Upload_Type).Append("image_").Append(resp[NetmeraConstants.Site]).Append("_")
                            .Append(resp[NetmeraConstants.Domain].Replace("http://www.", ""));
                    postImageAsData(strBuild.ToString(), data, (respDataToImage, ex) =>
                    {

                        String uploadKey = setUpSwfUploadResponseParams(respDataToImage);
                        strBuild = new StringBuilder();
                        strBuild.Append(NetmeraConstants.Save_Photo_Url).Append(NetmeraConstants.Save_Photo_Url_Params_St).Append(apiKey).Append("&")
                                .Append(NetmeraConstants.Save_Photo_Url_Params_Album).Append(resp[NetmeraConstants.Path]).Append("&")
                                .Append(NetmeraConstants.Save_Photo_Url_Params_Uploaded_Photo_Hash).Append(uploadKey).Append("&").Append(NetmeraConstants.Save_Photo_Url_Params_Cdn_Domain)
                                .Append(NetmeraConstants.Cdn_Domain).Append("&").Append(NetmeraConstants.Save_Photo_Url_Params_Opensocial_App_Id).Append(resp[NetmeraConstants.Site]).Append("&")
                                .Append(NetmeraConstants.Save_Photo_Url_Params_Opensocial_Netmera_Domain).Append(resp[NetmeraConstants.Domain].Replace("http://www.", "")).Append("&")
                                .Append(NetmeraConstants.Save_Photo_Url_Params_Opensocial_Viewer_Id).Append(viewerId).Append("&");

                        getData(strBuild.ToString(), null, result =>
                        {
                            JObject responseObj;
                            try
                            {
                                responseObj = JObject.Parse(result);
                                JObject photoObj = responseObj.Value<JObject>(NetmeraConstants.Netmera_Media_Photo);
                                JObject contentObj = photoObj.Value<JObject>(NetmeraConstants.Netmera_Media_Content);
                                JObject dataObj = contentObj.Value<JObject>(NetmeraConstants.Netmera_Media_Data);

                                if (dataObj[NetmeraConstants.Netmera_Media_Thumbnail_Url] != null)
                                {
                                    this.url = dataObj[NetmeraConstants.Netmera_Media_Thumbnail_Url].ToString() + "/org"; ;
                                }
                            }
                            catch (JsonException e)
                            {
                                Console.WriteLine(e.StackTrace);
                            }

                            if (callback != null)
                                callback(this.url, null);
                        });
                    });
                });
            }
            catch (Exception e)
            {
                if (callback != null)
                    callback(null, e);
            }
        }

        private void getData(String url, Dictionary<String, Object> mapParams, Action<String> callback)
        {
            StringBuilder strBuild = new StringBuilder();
            strBuild.Append(url);
            if (mapParams != null)
            {
                List<String> mapKeys = mapParams.Keys.ToList();

                foreach (String paramName in mapKeys)
                {
                    strBuild.Append(paramName).Append(mapParams[paramName]).Append("&");
                }
            }

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strBuild.ToString());
            request.Method = "GET";

            request.BeginGetResponse(ar =>
            {
                using (var response = (ar.AsyncState as HttpWebRequest).EndGetResponse(ar) as HttpWebResponse)
                using (var streamResponse = response.GetResponseStream())
                using (var streamRead = new StreamReader(streamResponse))
                {
                    string responseString = streamRead.ReadToEnd();

                    if (callback != null)
                        callback(responseString);
                }
            }, request);
        }

        private void postImageAsData(String url, byte[] data, Action<String, Exception> callback)
        {
            // Generate post objects
            Dictionary<string, object> postParameters = new Dictionary<string, object>();
            postParameters.Add("Filename", "file.jpg");
            postParameters.Add("Fileformat", "jpg");
            postParameters.Add("Filedata", new MultipartFormData.FileParameter(data, "file.jpg", "image/jpeg"));

            // Create request and receive response
            string postURL = url;
            string userAgent = "Someone";
            MultipartFormData.MultipartFormDataPost(postURL, userAgent, postParameters, (webResponse, ex) =>
            {
                if (webResponse != null && ex == null)
                {
                    StreamReader responseReader = new StreamReader(webResponse.GetResponseStream());
                    string fullResponse = responseReader.ReadToEnd();
                    if (callback != null)
                        callback(fullResponse, null);
                }
                else
                    if (callback != null)
                        callback(null, ex);
            });
        }

        private Dictionary<String, String> setUploadEntryParams(String JSONString)
        {
            // Parse and handle data which comes from te responce as a JSON

            Dictionary<String, String> resultMap = new Dictionary<String, String>();
            String site = null;
            String domain = null;
            String path = null;

            try
            {
                //JObject obj = new JObject(JSONString);

                JObject obj = JObject.Parse(JSONString);

                if (!string.IsNullOrEmpty(obj.Value<String>(NetmeraConstants.Site)))
                {
                    site = obj.Value<String>(NetmeraConstants.Site);
                    resultMap.Add(NetmeraConstants.Site, site);
                }

                if (!string.IsNullOrEmpty(obj.Value<String>(NetmeraConstants.Domain)))
                {
                    domain = obj.Value<String>(NetmeraConstants.Domain);
                    resultMap.Add(NetmeraConstants.Domain, domain);
                }

                JArray albumList = obj.Value<JArray>(NetmeraConstants.Album_List);

                if (albumList != null && albumList.Count != 0)
                {
                    path = ((albumList.Value<JObject>(0)).Value<JObject>("content")).Value<String>("path");
                    resultMap.Add(NetmeraConstants.Path, path);
                }
            }
            catch (JsonException)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_JSON, "Json contains album list information in media file is invalid.");
            }

            return resultMap;
        }

        private String setUpSwfUploadResponseParams(String JSONString)
        {
            // parse data comes from swf request

            String uploadKey = null;
            try
            {
                JObject obj = JObject.Parse(JSONString);

                if (!string.IsNullOrEmpty(obj.Value<String>(NetmeraConstants.Upload_Key)))
                {
                    uploadKey = obj.Value<String>(NetmeraConstants.Upload_Key);
                }
            }
            catch (JsonException)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_JSON, "Json contains upload key in media file is invalid.");
            }

            return uploadKey;
        }
    }
}
