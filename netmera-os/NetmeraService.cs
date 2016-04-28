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
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.IO;

namespace Netmera
{
    /// <copyright>Copyright 2012 Inomera Research</copyright>
    /// <summary>
    /// It is used to get <see cref="NetmeraContent"/> object by its search() and get() methods. Many query options defined to help finding exact object.
    /// </summary>
    public class NetmeraService
    {
        /// <summary>
        /// Name of the content
        /// </summary>
        private String objectName = null;

        /// <summary>
        /// List to add queries for the search
        /// </summary>
        private List<String> queries = new List<String>();

        /// <summary>
        /// Is used for the conditional/range queries
        /// </summary>
        private Dictionary<String, String> conditionalMap = new Dictionary<String, String>();

        /// <summary>
        /// Text to search
        /// </summary>
        private String searchText;

        /// <summary>
        /// Maximum number of the content to return
        /// </summary>
        private int max;

        /// <summary>
        /// Number of the pages
        /// </summary>
        private int page;

        /// <summary>
        /// Path of the content
        /// </summary>
        private String path;

        /// <summary>
        /// 
        /// </summary>
        private String sortBy;

        /// <summary>
        /// 
        /// </summary>
        private SortOrder sortOrder;

        /// <summary>
        /// Content sort order values
        /// </summary>
        public enum SortOrder
        {
            /// <summary>
            /// Ascending sort order value
            /// </summary>
            ascending,
            /// <summary>
            /// Descending sort order value
            /// </summary>
            descending
        }

        /// <summary>
        /// Static class contatining sort-by constants
        /// </summary>
        public static class SortBy
        {
            /// <summary>
            /// Constant to be used to sort contents by location
            /// </summary>
            public static readonly string NEARBY = "nearyby_netmera_forlocation";
        }

        private NetmeraService() { }

        /// <summary>
        /// Default constructor for the NetmeraService that sets objectName and other default parameters.
        /// Default value for the max = 10 and page = 0. It returns 10 result in each page. It skips page * max in each iteration.
        /// </summary>
        /// <param name="objectName">Name of the content</param>
        public NetmeraService(String objectName)
        {
            this.objectName = objectName;
            this.max = 10;
            this.page = 0;
        }

        /// <summary>
        /// Sets the total number of results to return. If it is less than or equal to 0 then it is setted to 10.
        /// </summary>
        /// <param name="max">Maximum content size</param>
        public void setMax(int max)
        {
            if (max <= 0)
            {
                this.max = 10;
            }
            else
            {
                this.max = max;
            }
        }

        /// <summary>
        /// Sets the total number of pages. If it is less than 0 then it is setted to 0.
        /// </summary>
        /// <param name="page">Page number</param>
        public void setPage(int page)
        {
            if (page < 0)
            {
                this.page = 0;
            }
            else
            {
                this.page = page;
            }
        }

        /// <summary>
        /// Returns the path of the content.
        /// </summary>
        /// <returns>Returns the path of the content</returns>
        public String getPath()
        {
            return path;
        }

        /// <summary>
        /// Sets the path of the content. This is used to find the content to delete and update.
        /// </summary>
        /// <param name="path">Path ofthe content</param>
        public void setPath(String path)
        {
            this.path = path;
        }

        /// <summary>
        /// Sorts the content with the given key.
        /// </summary>
        /// <param name="sortBy">Key to sort content</param>
        public void setSortBy(String sortBy)
        {
            this.sortBy = sortBy;
        }

        /// <summary>
        /// Sorts the content with the given order. If not setted then by default it is sorted by ascending order.
        /// </summary>
        /// <param name="sortOrder">Order of the sort</param>
        public void setSortOrder(SortOrder sortOrder)
        {
            this.sortOrder = sortOrder;
        }

        /// <summary>
        /// Finds the number of <see cref="NetmeraContent"/> objects that matches with the query.
        /// </summary>
        /// <param name="callback">Method called when count operation finishes</param>
        public void count(Action<long, Exception> callback)
        {
            RequestItem item = new RequestItem();

            if (searchText != null)
                item.searchText = searchText;

            item.path = NetmeraConstants.Default_ParentPath;
            item.contentType = NetmeraConstants.Default_ContentType;
            item.max = max;
            item.page = page;
            item.sortBy = sortBy;
            item.sortOrder = sortOrder.ToString();
            item.customCondition = getCustomCondition();

            NetmeraHttpUtils.search(item, (lno, ex) =>
            {
                if (ex != null)
                {
                    if (callback != null)
                        callback(0, ex);
                }
                else if (lno != null)
                {
                    try
                    {
                        long count = getNumberOfTotalResults(new JArray(lno));
                        if (callback != null)
                            callback(count, null);
                    }
                    catch (JsonException e)
                    {
                        if (callback != null)
                            callback(0, new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_JSON, "Json in the response of search method is invalid.", e.Message));
                    }
                    catch (IOException e)
                    {
                        if (callback != null)
                            callback(0, new NetmeraException(NetmeraException.ErrorCode.EC_IO_EXCEPTION, "IO Exception occurred in search method.", e.Message));
                    }
                    catch (Exception e)
                    {
                        if (callback != null)
                            callback(0, new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_REQUEST, "Request exception occurred in search method.", e.Message));
                    }
                    NetmeraClient.finish();
                }
                NetmeraClient.finish();
            });
        }
        
        /// <summary>
        /// Adds the searchText into the query.
        /// </summary>
        /// <param name="searchText">Text to search</param>
        public void addSearchText(String searchText)
        {
            this.searchText = searchText;
        }

        /// <summary>
        /// Adds an options to the query where value that matches with the query is equal to the given value.
        /// </summary>
        /// <param name="key">Key to identify specified value</param>
        /// <param name="value">Value associates with the specified key</param>
        public void whereEqual(String key, Object value)
        {
            StringBuilder strBuild = new StringBuilder();

            strBuild.Append("'").Append(key).Append("'").Append(" : ");

            if (value.GetType() == typeof(String))
                strBuild.Append("'").Append(value).Append("'");
            else
                strBuild.Append(value);

            queries.Add(strBuild.ToString());
        }

        /// <summary>
        /// Adds an options to the query where value that matches with the query is greater than the given value.
        /// </summary>
        /// <param name="key">Key to identify specified value</param>
        /// <param name="value">Value associates with the specified key</param>
        public void whereGreaterThan(String key, Object value)
        {
            StringBuilder strBuild = new StringBuilder();

            strBuild.Append(" $gt : ");

            if (value.GetType() == typeof(String))
                strBuild.Append("'").Append(value).Append("'");
            else
                strBuild.Append(value);

            if (conditionalMap.ContainsKey(key))
            {
                strBuild.Append(",").Append(conditionalMap[key]);
            }

            conditionalMap[key] = strBuild.ToString();
        }

        /// <summary>
        /// Adds an options to the query where value that matches with the query is less than the given value.
        /// </summary>
        /// <param name="key">Key to identify specified value</param>
        /// <param name="value">Value associates with the specified key</param>
        public void whereLessThan(String key, Object value)
        {
            StringBuilder strBuild = new StringBuilder();

            strBuild.Append(" $lt : ");

            if (value.GetType() == typeof(String))
                strBuild.Append("'").Append(value).Append("'");
            else
                strBuild.Append(value);

            if (conditionalMap.ContainsKey(key))
            {
                strBuild.Append(",").Append(conditionalMap[key]);
            }

            conditionalMap[key] = strBuild.ToString();
        }

        /// <summary>
        /// Adds an options to the query where value that matches with the query is not equal to the given value.
        /// </summary>
        /// <param name="key">Key to identify specified value</param>
        /// <param name="value">Value associates with the specified key</param>
        public void whereNotEqual(String key, Object value)
        {
            StringBuilder strBuild = new StringBuilder();

            strBuild.Append("'").Append(key).Append("'").Append(" : {$ne : ");

            if (value.GetType() == typeof(String))
                strBuild.Append("'").Append(value).Append("'");
            else
                strBuild.Append(value);

            strBuild.Append("}");

            queries.Add(strBuild.ToString());
        }

        /// <summary>
        /// Adds an options to the query where value that matches with the query is greater than or equal to the given value.
        /// </summary>
        /// <param name="key">Key to identify specified value</param>
        /// <param name="value">Value associates with the specified key</param>
        public void whereGreaterThanOrEqual(String key, Object value)
        {
            StringBuilder strBuild = new StringBuilder();

            strBuild.Append(" $gte : ");

            if (value.GetType() == typeof(String))
                strBuild.Append("'").Append(value).Append("'");
            else
                strBuild.Append(value);

            if (conditionalMap.ContainsKey(key))
            {
                strBuild.Append(",").Append(conditionalMap[key]);
            }

            conditionalMap[key] = strBuild.ToString();
        }

        /// <summary>
        /// Adds an options to the query where value that matches with the query is less than or equal to the given value.
        /// </summary>
        /// <param name="key">Key to identify specified value</param>
        /// <param name="value">Value associates with the specified key</param>
        public void whereLessThanOrEqual(String key, Object value)
        {
            StringBuilder strBuild = new StringBuilder();

            strBuild.Append(" $lte : ");

            if (value.GetType() == typeof(String))
                strBuild.Append("'").Append(value).Append("'");
            else
                strBuild.Append(value);

            if (conditionalMap.ContainsKey(key))
            {
                strBuild.Append(",").Append(conditionalMap[key]);
            }

            conditionalMap[key] = strBuild.ToString();
        }

        /// <summary>
        /// Adds an options to the query where the given key is exists or not. If value is true then it checks whether key exists, if value is false then it checks whether key not exists.
        /// </summary>
        /// <param name="key">Key to identify specified value</param>
        /// <param name="value">Boolean value to check whether key exists or not</param>
        public void whereExists(String key, Boolean value)
        {
            StringBuilder strBuild = new StringBuilder();
            strBuild.Append("'").Append(key).Append("'").Append(" : {$exists : ")
                .Append(value).Append("}");
            queries.Add(strBuild.ToString());
        }

        /// <summary>
        /// Adds an options to the query where value that returns from the query matches with the given regex.
        /// </summary>
        /// <param name="key">Key to identify specified value</param>
        /// <param name="regex">Value associates with the specified key</param>
        public void whereMatches(String key, String regex)
        {
            StringBuilder strBuild = new StringBuilder();
            strBuild.Append("'").Append(key).Append("'").Append(" : {$regex : ")
                .Append(regex).Append("}");
            queries.Add(strBuild.ToString());
        }

        /// <summary>
        /// Adds an options to the query where value that matches with the query starts with the given prefix.
        /// </summary>
        /// <param name="key">Key to identify specified value</param>
        /// <param name="prefix">Value associates with the specified key</param>
        public void whereStartsWith(String key, String prefix)
        {
            StringBuilder strBuild = new StringBuilder();
            strBuild.Append("'").Append(key).Append("'").Append(" : {$regex : \"^").Append(prefix).Append("\"}");
            queries.Add(strBuild.ToString());
        }

        /// <summary>
        /// Adds an options to the query where value that matches with the query ends with the given suffix.
        /// </summary>
        /// <param name="key">Key to identify specified value</param>
        /// <param name="suffix">Value associates with the specified key</param>
        public void whereEndsWith(String key, String suffix)
        {
            StringBuilder strBuild = new StringBuilder();
            strBuild.Append("'").Append(key).Append("'").Append(" : {$regex : \"").Append(suffix).Append("$\"}");
            queries.Add(strBuild.ToString());
        }

        /// <summary>
        /// Adds an options to the query where value that matches with the query contains any of the values in the given collection.
        /// </summary>
        /// <param name="key">Key to identify specified value</param>
        /// <param name="values">Value associates with the specified key</param>
        public void whereContainedIn<T>(String key, List<T> values)
        {
            StringBuilder strBuild = new StringBuilder();
            JArray jsonArray = new JArray();
            foreach (Object val in values)
            {
                jsonArray.Add(val);
            }

            strBuild.Append("'").Append(key).Append("'").Append(" : {$in : ")
                .Append(jsonArray).Append("}");
            queries.Add(strBuild.ToString());
        }

        /// <summary>
        /// Adds an options to the query where value that matches with the query contains all of the values in the given collection.
        /// </summary>
        /// <param name="key">Key to identify specified value</param>
        /// <param name="values">Value associates with the specified key</param>
        public void whereAllContainedIn<T>(String key, List<T> values)
        {
            StringBuilder strBuild = new StringBuilder();

            JArray jsonArray = new JArray();
            foreach (Object val in values)
            {
                jsonArray.Add(val);
            }

            strBuild.Append("'").Append(key).Append("'").Append(" : {$all : ")
                .Append(jsonArray).Append("}");
            queries.Add(strBuild.ToString());
        }

        /// <summary>
        /// Adds an options to the query where value that matches with the given owner
        /// </summary>
        /// <param name="user">User to be queried</param>
        public void whereOwnerEqual(NetmeraUser user)
        {
            if (user != null)
            {
                if (user.GetType() == typeof(NetmeraUser))
                {
                    NetmeraClient.setLoggedUserSecurityToken(NetmeraUser.securityToken);
                }
                else
                {
                    throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_DATA_TYPE,
                            "You should set the object with the type of NetmeraUser.");
                }
            }
            else
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_NULL_EXCEPTION,
                        "User cannot be null.You should set current logged user.");
            }
        }

        /// <summary>
        /// Retrieves the list of <see cref="NetmeraContent"/> objects that matches with the query.
        /// </summary>
        /// <param name="callback">Method called when search operation finishes</param>
        public void search(Action<List<NetmeraContent>, Exception> callback)
        {
            searchFromNetwork(callback);
        }

        private void searchFromNetwork(Action<List<NetmeraContent>, Exception> callback)
        {
            RequestItem item = new RequestItem();

            item.path = NetmeraConstants.Default_ParentPath;
            item.contentType = NetmeraConstants.Default_ContentType;
            item.max = max;
            item.page = page;
            item.sortBy = sortBy;
            item.sortOrder = sortOrder.ToString();
            item.customCondition = getCustomCondition();

            if (searchText != null)
                item.searchText = searchText;

            NetmeraHttpUtils.search(item, (lno, ex) =>
            {
                if (ex != null)
                {
                    if (callback != null)
                        callback(null, ex);
                }
                else if (lno != null)
                {
                    try
                    {
                        List<NetmeraContent> contentList = convertJsonArrayToNetmeraContent(new JArray(lno));
                        if (callback != null)
                            callback(contentList, null);
                    }
                    catch (JsonException e)
                    {
                        if (callback != null)
                            callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_JSON, "Json in the response of search method is invalid.", e.Message));
                    }
                    catch (IOException e)
                    {
                        if (callback != null)
                            callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_IO_EXCEPTION, "IO Exception occurred in search method.", e.Message));
                    }
                    catch (Exception e)
                    {
                        if (callback != null)
                            callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_REQUEST, "Request exception occurred in search method.", e.Message));
                    }
                    NetmeraClient.finish();
                }
            });
        }

        /// <summary>
        /// Creates box using the given two location (latitude,longitude) data and searches inside that box.
        /// </summary>
        /// <param name="firstPoint"><see cref="NetmeraGeoLocation"/> object</param>
        /// <param name="secondPoint"><see cref="NetmeraGeoLocation"/> object</param>
        /// <param name="locationSearchField">Name of the field that holds location data.</param>      
        /// <param name="callback">Method called when box search operation finishes</param>  
        public void boxSearch(NetmeraGeoLocation firstPoint, NetmeraGeoLocation secondPoint, String locationSearchField, Action<List<NetmeraContent>, Exception> callback)
        {
            RequestItem item = new RequestItem();

            item.path = NetmeraConstants.Default_ParentPath;
            item.contentType = NetmeraConstants.Default_ContentType;
            item.max = max;
            item.page = page;
            item.sortBy = sortBy;
            item.sortOrder = sortOrder.ToString();
            item.customCondition = getCustomCondition();

            if (searchText != null)
                item.searchText = searchText;

            item.locationSearchType = NetmeraConstants.LocationSearchType_Box;

            StringBuilder latitudes = new StringBuilder();
            StringBuilder longitudes = new StringBuilder();

            if (firstPoint.getLatitude() < secondPoint.getLatitude())
                latitudes.Append(firstPoint.getLatitude().ToString()).Append(",").Append(secondPoint.getLatitude().ToString());
            else
                latitudes.Append(secondPoint.getLatitude().ToString()).Append(",").Append(firstPoint.getLatitude().ToString());

            if (firstPoint.getLongitude() < secondPoint.getLongitude())
                longitudes.Append(firstPoint.getLongitude().ToString()).Append(",").Append(secondPoint.getLongitude().ToString());
            else
                longitudes.Append(secondPoint.getLongitude().ToString()).Append(",").Append(firstPoint.getLongitude().ToString());

            item.latitudes = latitudes.ToString();
            item.longitudes = longitudes.ToString();
            item.locationSearchField = locationSearchField;

            NetmeraHttpUtils.locationSearch(item, (lno, ex) =>
            {
                if (ex != null)
                {
                    if (callback != null)
                        callback(null, ex);
                }
                else if (lno != null)
                {
                    try
                    {
                        List<NetmeraContent> contentList = convertJsonArrayToNetmeraContent(new JArray(lno));
                        if (callback != null)
                            callback(contentList, null);
                    }
                    catch (JsonException e)
                    {
                        if (callback != null)
                            callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_JSON, "Json in the response of search method is invalid.", e.Message));
                    }
                    catch (IOException e)
                    {
                        if (callback != null)
                            callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_IO_EXCEPTION, "IO Exception occurred in search method.", e.Message));
                    }
                    catch (Exception e)
                    {
                        if (callback != null)
                            callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_REQUEST, "Request exception occurred in search method.", e.Message));
                    }

                    NetmeraClient.finish();
                }
            });
        }

        /// <summary>
        /// Searches the content by taking given location as a base and retrieves the contents that located given distance far away.
        /// </summary>
        /// <param name="startLocation">Base location to search near it.</param>
        /// <param name="distance">Distance used to create circle by taking the startLocation as a center.</param>
        /// <param name="locationSearchField">Name of the field that holds location data.</param>
        /// <param name="callback">Method called when circle search operation finishes</param>
        public void circleSearch(NetmeraGeoLocation startLocation, double distance, String locationSearchField, Action<List<NetmeraContent>, Exception> callback)
        {
            RequestItem item = new RequestItem();

            item.path = NetmeraConstants.Default_ParentPath;
            item.contentType = NetmeraConstants.Default_ContentType;
            item.max = max;
            item.page = page;
            item.sortBy = sortBy;
            item.sortOrder = sortOrder.ToString();
            item.customCondition = getCustomCondition();

            if (searchText != null)
                item.searchText = searchText;

            item.locationSearchType = NetmeraConstants.LocationSearchType_Circle;
            item.distance = distance;
            item.latitudes = startLocation.getLatitude().ToString();
            item.longitudes = startLocation.getLongitude().ToString();
            item.locationSearchField = locationSearchField;

            NetmeraHttpUtils.locationSearch(item, (lno, ex) =>
            {
                if (ex != null)
                {
                    if (callback != null)
                        callback(null, ex);
                }
                else if (lno != null)
                {
                    try
                    {
                        List<NetmeraContent> contentList = convertJsonArrayToNetmeraContent(new JArray(lno));
                        if (callback != null)
                            callback(contentList, null);
                    }
                    catch (JsonException e)
                    {
                        if (callback != null)
                            callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_JSON, "Json in the response of search method is invalid.", e.Message));
                    }
                    catch (IOException e)
                    {
                        if (callback != null)
                            callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_IO_EXCEPTION, "IO Exception occurred in search method.", e.Message));
                    }
                    catch (Exception e)
                    {
                        if (callback != null)
                            callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_REQUEST, "Request exception occurred in search method.", e.Message));
                    }

                    NetmeraClient.finish();
                }
            });
        }

        /// <summary>
        /// Retrieves the list of <see cref="NetmeraUser"/> objects that matches with the
        /// query.
        /// </summary>
        /// <param name="callback">Method called when user search operation finishes</param>
        public void searchUser(Action<List<NetmeraUser>, Exception> callback)
        {
            RequestItem item = new RequestItem();
            JArray jsonArray = new JArray();

            if (searchText != null)
                item.searchText = searchText;

            item.path = NetmeraConstants.Netmera_People_Url;
            item.max = max;
            item.page = page;
            item.sortBy = sortBy;
            item.sortOrder = sortOrder.ToString();
            item.customCondition = getUserCustomCondition();

            searchUser(item, (users, ex) =>
            {
                if (callback != null)
                    callback(users, ex);
            });
        }

        private void searchUser(RequestItem item, Action<List<NetmeraUser>, Exception> callback)
        {
            NetmeraHttpUtils.searchUser(item, (lno, ex) =>
            {
                if (ex != null)
                {
                    if (callback != null)
                        callback(null, ex);
                }
                else if (lno != null)
                {
                    try
                    {
                        List<NetmeraUser> userList = convertJsonArrayToNetmeraUser(new JArray(lno));
                        if (callback != null)
                            callback(userList, null);
                    }
                    catch (JsonException e)
                    {
                        if (callback != null)
                            callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_JSON, "Json in the response of search method is invalid.", e.Message));
                    }
                    catch (IOException e)
                    {
                        if (callback != null)
                            callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_IO_EXCEPTION, "IO Exception occurred in search method.", e.Message));
                    }
                    catch (Exception e)
                    {
                        if (callback != null)
                            callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_REQUEST, "Request exception occurred in search method.", e.Message));
                    }
                }
            });
        }

        /// <summary>
        /// Creates the <see cref="NetmeraUser"/> service object. It is used to search users.
        /// </summary>
        /// <returns><seealso cref="NetmeraService"/> object</returns>
        public static NetmeraService getNetmeraUserService()
        {
            return new NetmeraService();
        }

        /// <summary>
        /// Gets the unique <see cref="NetmeraContent"/> in a given path.
        /// </summary>
        /// <param name="path">Content path to get</param>
        /// <param name="callback">Method finding the content as the result of get operation and exception if exists.</param>
        public void get(String path, Action<NetmeraContent, Exception> callback)
        {
            getFromNetwork(path, callback);
        }

        /// <summary>
        /// Gets the <see cref="NetmeraContent"/> from the network, not the cache.
        /// </summary>
        /// <param name="path">Content path to get</param>
        /// <param name="callback">Method finding the content as the result of get operation and exception if exists.</param>
        public void getFromNetwork(String path, Action<NetmeraContent, Exception> callback)
        {
            RequestItem item = new RequestItem();

            if (path == null || path.Length == 0)
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_NULL_EXCEPTION, "Path cannot be null or empty"));
            }

            item.path = path;
            item.contentType = NetmeraConstants.Default_ContentType;
            item.customCondition = getCustomCondition();

            NetmeraHttpUtils.get(item, (lno, ex) =>
            {
                if (ex != null)
                {
                    if (callback != null)
                        callback(null, ex);
                }
                else if (lno != null)
                {
                    try
                    {
                        List<NetmeraContent> contentList = convertJsonArrayToNetmeraContent(new JArray(lno));
                        if (callback != null)
                        {
                            if (contentList != null && contentList.Count != 0)
                                callback(contentList.ToArray()[0], null);
                        }
                    }
                    catch (JsonException e)
                    {
                        if (callback != null)
                            callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_JSON, "Json in the response of search method is invalid.", e.Message));
                    }
                    catch (IOException e)
                    {
                        if (callback != null)
                            callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_IO_EXCEPTION, "IO Exception occurred in search method.", e.Message));
                    }
                    catch (Exception e)
                    {
                        if (callback != null)
                            callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_REQUEST, "Request exception occurred in search method.", e.Message));
                    }

                    NetmeraClient.finish();
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonArray"></param>
        /// <returns></returns>
        protected internal List<NetmeraContent> convertJsonArrayToNetmeraContent(JArray jsonArray)
        {
            List<NetmeraContent> contentList = new List<NetmeraContent>();
            JArray entries = new JArray();

            //If the entry is type of JArray, it is already able to be casted to JArray.
            //Else it does not accept JArray cast. Instead, it is added to a JArray as a JObject
            if (jsonArray[0]["entry"].Type == JTokenType.Array)
                entries = (JArray)jsonArray[0]["entry"];
            else
                entries.Add(jsonArray[0]["entry"]);

            for (int i = 0; i < entries.Count; i++)
            {
                try
                {
                    JObject json = (JObject)entries[i];
                    String contentName = (String)json[NetmeraConstants.Content_Params][NetmeraConstants.ContentData_Params][NetmeraConstants.ApiContentType];
                    NetmeraContent content = new NetmeraContent(contentName);
                    content.setContent(json);
                    contentList.Add(content);
                }
                catch (JsonException e)
                {
                    throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_JSON, "Json contains content is invalid.", e.Message);
                }
            }
            return contentList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonArray"></param>
        /// <returns></returns>
        protected internal List<NetmeraUser> convertJsonArrayToNetmeraUser(JArray jsonArray)
        {
            List<NetmeraUser> userList = new List<NetmeraUser>();

            if (jsonArray == null)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_NULL_EXCEPTION,
                        "Json contains user information is null");
            }

            JArray entries = new JArray();

            //If the entry is type of JArray, it is already able to be casted to JArray.
            //Else it does not accept JArray cast. Instead, it is added to a JArray as a JObject
            if (jsonArray[0]["entry"].Type == JTokenType.Array)
                entries = (JArray)jsonArray[0]["entry"];
            else
                entries.Add(jsonArray[0]["entry"]);

            for (int i = 0; i < entries.Count; i++)
            {
                try
                {
                    JObject json = (JObject)entries[i];
                    String email = null;

                    if (json[NetmeraConstants.Netmera_UserEmails] != null)
                    {
                        JObject emailObj = (JObject)json[NetmeraConstants.Netmera_UserEmails].First;

                        if (emailObj[NetmeraConstants.Netmera_UserEmailValue] != null)
                        {
                            email = emailObj[NetmeraConstants.Netmera_UserEmailValue].ToString();
                        }
                    }

                    if (email != "anonymous@anonymous.com")
                    {
                        NetmeraUser user = new NetmeraUser();
                        user.setUser(json);
                        userList.Add(user);
                    }
                }
                catch (JsonException)
                {
                    throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_JSON, "Json contains user information is invalid");
                }
            }
            return userList;
        }

        /// <summary>
        /// Finds the numnber of content according to the result responded by the server using count() method
        /// </summary>
        /// <param name="jsonArray">result responded by the server</param>
        /// <returns>Number of returned contents</returns>
        protected internal long getNumberOfTotalResults(JArray jsonArray)
        {
            
            long totalCount = 0;

            totalCount = (long)jsonArray[0]["totalResults"];

            return totalCount;
            
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected internal String getUserCustomCondition()
        {
            StringBuilder strBuild = new StringBuilder();
            strBuild.Append("{");

            int size = queries.Count;
            int count = 0;
            foreach (String query in queries)
            {
                strBuild.Append(query);
                if (count < size)
                    strBuild.Append(",");
                count++;
            }

            String conditionalQueries = createConditionalQueries();

            if (!string.IsNullOrEmpty(conditionalQueries))
            {
                strBuild.Append(conditionalQueries);
            }

            strBuild.Append("}");
            return strBuild.ToString();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected internal String getCustomCondition()
        {
            StringBuilder strBuild = new StringBuilder();
            strBuild.Append("{");

            if (queries.Count > 0)
            {
                foreach (String query in queries)
                {
                    strBuild.Append(query).Append(",");
                }
            }

            String conditionalQueries = createConditionalQueries();

            if (!string.IsNullOrEmpty(conditionalQueries))
            {
                strBuild.Append(conditionalQueries).Append(",");
            }

            strBuild.Append("'" + NetmeraConstants.ApiContentType + "' : '").Append(objectName).Append("'");
            strBuild.Append("}");
            return strBuild.ToString();
        }

        private String createConditionalQueries()
        {
            StringBuilder strBuild = new StringBuilder();
            int count = 0;
            int keySize = conditionalMap.Keys.Count;

            foreach (String key in conditionalMap.Keys)
            {
                String value = conditionalMap[key];
                strBuild.Append(key).Append(" : { ").Append(value).Append(" } ");

                if (count < (keySize - 1))
                {
                    strBuild.Append(",");
                }

                count++;
            }
            return strBuild.ToString();
        }
    }
}