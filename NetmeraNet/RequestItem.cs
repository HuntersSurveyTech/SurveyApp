using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Netmera
{
    internal class RequestItem
    {
        private String service;
        private String action;
        private String actionTokenKey;
        //private String ownerId;
        //private String parentPath;
        //private String contentName;
        private String contentPrivacy;
        //private String property;
        //private String newParentPath;
        //private String fromPath;
        //private String toPath;
        //private String relationType;

       // private Dictionary<String, String> parameters;
       // private String[] categoryPaths;
       // private String[] fields;

        private JObject contentData;

        //private Boolean allChild = false;

        public String path = String.Empty;
        public String contentType = String.Empty;
        public int max = 10;
        public int page = 0;
        public String sortOrder = String.Empty;
        public String sortBy = String.Empty;
        public String customCondition = String.Empty;
        public String searchText = String.Empty;
        public String locationSearchType = String.Empty;
        public String latitudes = String.Empty;
        public String longitudes = String.Empty;
        public String locationSearchField = String.Empty;
        public double distance = 0.0D;

        private String email;
        private String nickname;
        private String password;
        private String name;
        private String surname;
        //private Map<String, String> profileAttributes;

        public FilterOperation filterOperation = FilterOperation.contains;
        public String filterBy;
        public String filterValue;

        public enum FilterOperation
        {
            contains, equals, startsWith, present
        }

        /// <summary>
        /// Return the service
        /// </summary>
        /// <returns>the service</returns>
        public String getService()
        {
            return service;
        }

        /// <summary>
        /// Set the service
        /// </summary>
        /// <param name="service">the service to set</param>
        public void setService(String service)
        {
            this.service = service;
        }

        /// <summary>
        /// Return the action
        /// </summary>
        /// <returns>the action</returns>
        public String getAction()
        {
            return action;
        }

        /// <summary>
        /// Set the action
        /// </summary>
        /// <param name="action">the action to set</param>
        public void setAction(String action)
        {
            this.action = action;
        }

        /// <summary>
        /// Return the path
        /// </summary>
        /// <returns>the path</returns>
        public String getPath()
        {
            return path;
        }

        /// <summary>
        /// Set the path
        /// </summary>
        /// <param name="path">the path to set</param>
        public void setPath(String path)
        {
            this.path = path;
        }

        /// <summary>
        /// Return action token key
        /// </summary>
        /// <returns>the action token key</returns>
        public String getActionTokenKey()
        {
            return actionTokenKey;
        }

        /// <summary>
        /// Set the action token key
        /// </summary>
        /// <param name="actionTokenKey">The action token key to set</param>
        public void setActionTokenKey(String actionTokenKey)
        {
            this.actionTokenKey = actionTokenKey;
        }

        /// <summary>
        /// Return content type
        /// </summary>
        /// <returns>content type</returns>
        public String getContentType()
        {
            return contentType;
        }

        /// <summary>
        /// Set the content type
        /// </summary>
        /// <param name="contentType">the content type</param>
        public void setContentType(String contentType)
        {
            this.contentType = contentType;
        }

        /// <summary>
        /// Return the content data
        /// </summary>
        /// <returns>content data</returns>
        public JObject getContentData()
        {
            return contentData;
        }

        /// <summary>
        /// Set the content data
        /// </summary>
        /// <param name="contentData">Content data to set</param>
        public void setContentData(JObject contentData)
        {
            this.contentData = contentData;
        }

        /// <summary>
        /// Return content privacy
        /// </summary>
        /// <returns>the content privacy</returns>
        public String getContentPrivacy()
        {
            return contentPrivacy;
        }

        /// <summary>
        /// Set content privacy
        /// </summary>
        /// <param name="contentPrivacy">the content privacy</param>
        public void setContentPrivacy(String contentPrivacy)
        {
            this.contentPrivacy = contentPrivacy;
        }

        /// <summary>
        /// Return email
        /// </summary>
        /// <returns>email</returns>
        public String getEmail()
        {
            return email;
        }

        /// <summary>
        /// Set the email
        /// </summary>
        /// <param name="email">email</param>
        public void setEmail(String email)
        {
            this.email = email;
        }

        public String getNickname()
        {
            return nickname;
        }

        public void setNickname(String nickname)
        {
            this.nickname = nickname;
        }

        public String getPassword()
        {
            return password;
        }

        public void setPassword(String password)
        {
            this.password = password;
        }

        public String getName()
        {
            return name;
        }

        public void setName(String name)
        {
            this.name = name;
        }

        public String getSurname()
        {
            return surname;
        }

        public void setSurname(String surname)
        {
            this.surname = surname;
        }
    }
}