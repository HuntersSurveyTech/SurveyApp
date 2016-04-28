using System;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using System.Net;

namespace Netmera
{
    /// <summary>
    /// It is used to run CRUD operations over the data.
    /// After creating object use add() method to fill data and call create() method to add data into cloud.
    /// </summary>
    /// <copyright>Copyright 2012 Inomera Research</copyright>
    public class NetmeraContent : BaseContent
    {
        /// <summary>
        /// Name of the content
        /// </summary>
        private String objectName;

        /// <summary>
        /// Path to the content
        /// </summary>
        private String path;

        /// <summary>
        /// Create date of the NetmeraContent object
        /// </summary>
        private DateTime createDate;

        /// <summary>
        /// Update date of the NetmeraContent object
        /// </summary>
        private DateTime updateDate;

        /// <summary>
        /// Privacy of the content
        /// </summary>
        private String privacy = NetmeraConstants.Privacy_Public;

        private JObject content;
        private JObject owner;
        private JObject contentType;

        /// <summary>
        /// Default constructor
        /// </summary>
        NetmeraContent() { }

        /// <summary>
        /// Constructor that takes content name as parameter.
        /// </summary>
        /// <param name="objectName">Name of the content</param>
        public NetmeraContent(String objectName)
        {
            data = new JObject();
            mediaData = new JObject();
            this.objectName = objectName;
        }

        /// <summary>
        /// the privacy of the content
        /// </summary>
        /// <returns>privacy</returns>
        public String getPrivacy()
        {
            return privacy;
        }

        /// <summary>
        /// privacy string of the content
        /// </summary>
        /// <param name="privacy">privacy</param>
        protected void setPrivacy(String privacy)
        {
            this.privacy = privacy;
        }

        /// <summary>
        /// allows user to set the privacy of the content
        /// </summary>
        /// <param name="privacy">privacy</param>
        public void setPrivacy(NetmeraPrivacy privacy)
        {
            switch (privacy)
            {
                case NetmeraPrivacy.PRIVATE:
                    this.privacy = NetmeraConstants.Privacy_Private;
                    break;
                case NetmeraPrivacy.PUBLIC:
                    this.privacy = NetmeraConstants.Privacy_Public;
                    break;
                default:
                    this.privacy = NetmeraConstants.Privacy_Public;
                    break;
            }
        }

        /// <summary>
        /// Adds data to the cloud.
        /// </summary>
        /// <exception cref="NetmeraException">Throws exception if invalid request, if cannot connect to the server, if error occurred while adding data.</exception>
        public void create()
        {
            try
            {
                //if data already contains a node with name "NetmeraConstants.ApiContentType"
                data.Remove(NetmeraConstants.ApiContentType);

                data.Add(NetmeraConstants.ApiContentType, objectName);
                data.Add(NetmeraConstants.ContentPrivacy_Params, privacy);
                JObject createContent = create(data);
                setContent(createContent);
            }
            catch (JsonException e)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_OBJECT_NAME, "Objectname is invalid.", e.Message);
            }
            catch (IOException e)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_IO_EXCEPTION, "IO Exception occurred while creating data.", e.Message);
            }
            catch (Exception e)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_REQUEST, "Request exception occurred while creating data.", e.Message);
            }
        }


        public void bulkCreate()
        {
            try
            {
                //if data already contains a node with name "NetmeraConstants.ApiContentType"
                //data.Remove(NetmeraConstants.ApiContentType);

                //data.Add(NetmeraConstants.ApiContentType, objectName);
                //data.Add(NetmeraConstants.ContentPrivacy_Params, privacy);
                JObject createBulkContent = createBulk(data);
                setContent(createBulkContent);
            }
            catch (JsonException e)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_OBJECT_NAME, "Objectname is invalid.", e.Message);
            }
            catch (IOException e)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_IO_EXCEPTION, "IO Exception occurred while creating data.", e.Message);
            }
            catch (Exception e)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_REQUEST, "Request exception occurred while creating data.", e.Message);
            }
        }
        
       

        /// <summary>
        /// Updates data in the cloud.
        /// </summary>
        /// <exception cref="NetmeraException">Throws exception if invalid request, if cannot connect to the server, if error occurred while adding data.</exception>
        public void update()
        {
            try
            {
                //if data already contains a node with name "NetmeraConstants.ApiContentType"
                data.Remove(NetmeraConstants.ApiContentType);

                data.Add(NetmeraConstants.ApiContentType, objectName);
                //data.Add(NetmeraConstants.ContentPrivacy_Params, privacy);
                JObject updateContent = update(data, getPath());
                setContent(updateContent);
            }
            catch (JsonException e)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_OBJECT_NAME, "Objectname is invalid.", e.Message);
            }
            catch (IOException e)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_IO_EXCEPTION, "IO Exception occurred while updating data.", e.Message);
            }
            catch (Exception e)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_REQUEST, "Request exception occurred while updating data.", e.Message);
            }
        }

        /// <summary>
        /// Deletes data from the cloud.
        /// </summary>
        public bool delete()
        {
            bool flag = false;
            flag=NetmeraHttpUtils.deleteContent(getPath());
            clearContent();
            return flag;
        }

        /// <summary>
        /// Returns the path of the content.
        /// </summary>
        /// <returns>The path of the content</returns>
        /// <exception cref="NetmeraException">Throws exception if error occurred while getting data.</exception>
        public String getPath()
        {
            if (path != null)
            {
                return path;
            }
            else
            {
                try
                {
                    return (String)content[NetmeraConstants.Path_Params];
                }
                catch (JsonException e)
                {
                    throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_PATH, "Path is invalid.", e.Message);
                }
            }
        }

        /// <summary>
        /// Sets the path of the content.This is used to find the content to delete and update.
        /// </summary>
        /// <param name="path">Path ofthe content</param>
        public void setPath(String path)
        {
            this.path = path;
        }

        /// <summary>
        /// Sets the owner to the content.
        /// </summary>
        /// <param name="user">Current logged user</param>
        public void setOwner(NetmeraUser user)
        {
            if (user != null)
            {
                if (user.GetType() == typeof(NetmeraUser))
                {
                    NetmeraClient.setLoggedUserSecurityToken(NetmeraUser.securityToken);
                }
                else
                {
                    throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_DATA_TYPE, "You should set the object with the type of NetmeraUser.");
                }
            }
            else
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_NULL_EXCEPTION, "User cannot be null.You should set current logged user.");
            }
        }

        /// <summary>
        /// Returns the name of the content.
        /// </summary>
        /// <returns>The name of the content</returns>
        public String getObjectName()
        {
            return objectName;
        }

        /// <summary>
        /// Gets the createDate of the content.
        /// </summary>
        /// <returns>Create date</returns>
        public DateTime getCreateDate()
        {
            if (createDate != null)
            {
                return createDate;
            }
            else
            {
                try
                {
                    return (DateTime)content[NetmeraConstants.ContentCreateDate_Params];
                }
                catch (JsonException e)
                {
                    throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_DATE_FORMAT, "Create date is invalid.", e.Message);
                }
            }
        }

        /// <summary>
        /// Gets the updateDate of the content.
        /// </summary>
        /// <returns>Update date</returns>
        public DateTime getUpdateDate()
        {
            if (updateDate != null)
            {
                return updateDate;
            }
            else
            {
                try
                {
                    return (DateTime)content[NetmeraConstants.ContentUpdateDate_Params];
                }
                catch (JsonException e)
                {
                    throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_DATE_FORMAT, "Update date is invalid.", e.Message);
                }
            }
        }

        /// <summary>
        /// Sets the content parameters.
        /// </summary>
        protected internal void setContent(JObject json)
        {
            try
            {
                if (json[NetmeraConstants.Content_Params] != null)
                    this.content = (JObject)json[NetmeraConstants.Content_Params];

                if (json[NetmeraConstants.Owner_Params] != null)
                    this.owner = (JObject)json[NetmeraConstants.Owner_Params];

                if (json[NetmeraConstants.Type_Params] != null)
                    this.contentType = (JObject)json[NetmeraConstants.Type_Params];

                if (content != null && content[NetmeraConstants.ContentData_Params] != null)
                    this.data = (JObject)content[NetmeraConstants.ContentData_Params];

                if (data != null && data[NetmeraConstants.ApiContentType] != null)
                    this.objectName = (String)data[NetmeraConstants.ApiContentType];

                if (content != null && content[NetmeraConstants.Path_Params] != null)
                    this.path = (String)content[NetmeraConstants.Path_Params];

                if (content != null && content[NetmeraConstants.ContentCreateDate_Params] != null)
                    this.createDate = (DateTime)content[NetmeraConstants.ContentCreateDate_Params];

                if (content != null && content[NetmeraConstants.ContentUpdateDate_Params] != null)
                    this.updateDate = (DateTime)content[NetmeraConstants.ContentUpdateDate_Params];

                if (content != null && content[NetmeraConstants.ContentPrivacy_Field] != null)
                    this.privacy = (String)content[NetmeraConstants.ContentPrivacy_Field];
            }
            catch (JsonException e)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_JSON, "Json is invalid.", e.Message);
            }
        }


        /// <summary>
        /// Clears the content parameters.
        /// </summary>
        private void clearContent()
        {
            data = new JObject();
            objectName = null;
            path = null;
            createDate = new DateTime();
            updateDate = new DateTime();
            content = new JObject();
            contentType = new JObject();
            owner = new JObject();
        }

        /// <summary>
        /// Returns the  <see cref="JObject"/> which contains detailed informations about content.
        /// </summary>
        /// <returns><see cref="JObject"/></returns>
        protected JObject getContent()
        {
            return content;
        }

        /// <summary>
        /// Returns the  <see cref="JObject"/> which contains detailed informations about
        /// </summary>
        /// <returns><see cref="JObject"/></returns>
        protected JObject getOwner()
        {
            return owner;
        }

        /// <summary>
        /// Creates the specified data.
        /// </summary>
        /// <param name="data">The data</param>
        protected internal JObject create(JObject data)
        {
            JObject json = NetmeraHttpUtils.createContent(data);
            NetmeraContent content = new NetmeraContent();
            content.setContent(json);
            if (mediaData != null && mediaData.Count != 0)
            {
                json = createMedia(content);
            }
            return json;
        }

        protected internal JObject createBulk(JObject data)
        {
            JObject jArray = NetmeraHttpUtils.createBulkContent(data);
            NetmeraContent content = new NetmeraContent();
            //foreach (JObject json in jArray)
            //{

            content.setContent(jArray);
                //if (mediaData != null && mediaData.Count != 0)
                //{
                //    jarray = createMedia(content);
                //}
            //}

            return (JObject)jArray;
        }
        
        /// <summary>
        /// Updates the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="path">The path.</param>
        protected internal JObject update(JObject data, String path)
        {
            JObject json = NetmeraHttpUtils.updateContent(data, path);
            NetmeraContent content = new NetmeraContent();
            content.setContent(json);
            if (mediaData != null && mediaData.Count != 0)
            {
                json = createMedia(content);
            }
            return json;
        }

        private JObject createMedia(NetmeraContent content)
        {
            JObject json = null;
            try
            {
                JObject mediaObject = new JObject();
                bool isMediaFileAdded = false;

                IEnumerable<JProperty> jlist = mediaData.Properties();

                foreach (JProperty token in jlist)
                {
                    String key = token.Name;
                    String tmpMediaDataJson = mediaData.Value<String>(key);

                    byte[] tmpMediaData = JsonConvert.DeserializeObject<byte[]>(tmpMediaDataJson);

                    NetmeraMedia file = new NetmeraMedia(tmpMediaData);

                    isMediaFileAdded = true;

                    String appId = content.getContent().Value<string>(NetmeraConstants.Site);
                    String apiKey = NetmeraClient.getSecurityToken();
                    String contentPath = content.getPath();
                    String viewerId = content.getOwner().Value<String>(NetmeraConstants.Netmera_Owner_Node_Name);
                    String mediaUrl = file.save(appId, apiKey, contentPath, viewerId);

                    mediaObject.Add(key, mediaUrl);
                }
                if (isMediaFileAdded)
                {
                    json = NetmeraHttpUtils.updateContent(mediaObject, content.getPath());
                }
            }
            catch (JsonException)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_JSON, "Json in the response of creating media file is invalid");
            }
            catch (WebException)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_REQUEST, "Request exception occurred while saving media data");
            }
            catch (IOException)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_IO_EXCEPTION, "IO Exception occurred while saving media data");
            }
            return json;
        }
    }
}