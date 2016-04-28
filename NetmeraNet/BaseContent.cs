using System;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.IO;

namespace Netmera
{
    /// <summary>
    /// An abstract class that contains basic functions for <see cref="NetmeraContent"/>
    /// </summary>
    public abstract class BaseContent
    {
        /// <summary>
        /// Object to hold data
        /// </summary>
        public JObject data;

        /// <summary>
        /// Object to hold media data
        /// </summary>
        public JObject mediaData;

        /// <summary>
        /// Adds key,value pairs into the object. If the object contains key, the old value is replaced.
        /// </summary>
        /// <param name="key">Key to identify specified value</param>
        /// <param name="value">Value associates with the specified key</param>
        /// <exception cref="NetmeraException">Throws exception if key is null, if value is null</exception>
        public void add(String key, Object value)
        {
            if (key == null)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_NULL_EXCEPTION, NetmeraConstants.ContentKey);
            }

            if (value == null)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_NULL_EXCEPTION, NetmeraConstants.ContentValue);
            }

            if ((value.GetType() == typeof(JObject)) == false && (value.GetType() == typeof(JArray)) == false && (value.GetType() == typeof(String)) == false &&
                (value.GetType() == typeof(Boolean)) == false && (value.GetType() == typeof(DateTime)) == false && (value.GetType() == typeof(byte[])) == false &&
                (value.GetType() == typeof(double)) == false && (value.GetType() == typeof(Double)) == false && (value.GetType() == typeof(float)) == false &&
                (value.GetType() == typeof(long)) == false && (value.GetType() == typeof(int) == false) && (value.GetType() == typeof(Int16)) == false &&
                (value.GetType() == typeof(Int32)) == false && (value.GetType() == typeof(Int64)) == false && (value.GetType() == typeof(NetmeraMedia)) == false && (value.GetType() == typeof(NetmeraGeoLocation)) == false && (value.GetType() == typeof(NetmeraUser)) == false)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_DATA_TYPE, value.GetType().ToString());
            }

            try
            {
                if (value.GetType() == typeof(NetmeraGeoLocation))
                {
                    double lat = ((NetmeraGeoLocation)value).getLatitude();
                    double lng = ((NetmeraGeoLocation)value).getLongitude();

                    //Replacements are done because ToString() method convert points in a decimal number to comma.
                    String location = lat.ToString().Replace(',', '.') + "," + lng.ToString().Replace(',', '.');

                    JProperty prLocationField = new JProperty(key + NetmeraConstants.LocationField_Suffix, location);

                    //if data already contains a node with name "key + NetmeraConstants.LocationField_Suffix"
                    data.Remove(key + NetmeraConstants.LocationField_Suffix);

                    data.Add(prLocationField);

                    JProperty prLocationLatitudeField = new JProperty(key + NetmeraConstants.LocationLatitude_Suffix, lat);

                    //if data already contains a node with name "key + NetmeraConstants.LocationLatitude_Suffix"
                    data.Remove(key + NetmeraConstants.LocationLatitude_Suffix);

                    data.Add(prLocationLatitudeField);

                    JProperty prLocationLongitudeField = new JProperty(key + NetmeraConstants.LocationLongitude_Suffix, lng);

                    //if data already contains a node with name "key + NetmeraConstants.LocationLongitude_Suffix"
                    data.Remove(key + NetmeraConstants.LocationLongitude_Suffix);

                    data.Add(prLocationLongitudeField);
                }
                else if (value.GetType() == typeof(NetmeraUser))
                {
                    NetmeraClient.setLoggedUserSecurityToken(NetmeraUser.securityToken);
                }
                else if (value.GetType() == typeof(NetmeraMedia))
                {
                    //mediaData.Add(key, value);

                    NetmeraMedia tmpMedia = (NetmeraMedia)value;
                    byte[] tmpMediaData = tmpMedia.getData(); //url is already null, so no need to add to json

                    JProperty prop = new JProperty(key, JsonConvert.SerializeObject(tmpMediaData));

                    //if mediaData already contains a node with name "key"
                    mediaData.Remove(key);

                    mediaData.Add(prop);
                }
                else
                {
                    JProperty prop = new JProperty(key, value);

                    //if data already contains a node with name "key"
                    data.Remove(key);

                    data.Add(prop);
                }
            }
            catch (JsonException e)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_JSON, "Json with key : [" + key + "], value : [" + value + "] pairs is invalid.", e.Message);
            }
        }

        /// <summary>
        /// Gets the object with the specified key.
        /// </summary>
        /// <param name="key">Key to get value</param>
        /// <returns>The object with the specified key. If the key does not exists then it returns null.</returns>
        /// <exception cref="NetmeraException">Throws exception if it cannot get object</exception>
        public Object get(String key)
        {
            if (key == null || data[key] == null)
            {
                return null;
            }

            try
            {
                return data[key];
            }
            catch (JsonException e)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_KEY, "Json with key [" + key + "] is invalid.", e.Message);
            }
        }

        /// <summary>
        /// Gets the String object with the specified key.
        /// </summary>
        /// <param name="key">Key to get value</param>
        /// <returns>Returns The string with the specified key.If the object type is not string or key does not exists then it returns null.</returns>
        public String getString(String key)
        {
            if (key == null || data[key] == null)
            {
                return null;
            }

            String val;
            try
            {
                Object obj = data[key];
                val = Convert.ToString(obj.ToString());
            }
            catch (JsonException e)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_KEY, "Json with key [" + key + "] is invalid.", e.Message);
            }
            catch (InvalidCastException e)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_DATA_TYPE, "Json with key [" + key + "] is invalid.", e.Message);
            }
            return val;
        }

        /// <summary>
        /// Gets the int value with the specified key.
        /// </summary>
        /// <param name="key">Key to get value</param>
        /// <returns>The int value with the specified key.If value is not an integer or key does not exists then it returns 0.</returns>
        public Int16 getInt16(String key)
        {
            if (key == null || data[key] == null)
            {
                return 0;
            }

            Int16 val;
            try
            {
                Object obj = data[key];
                val = Convert.ToInt16(obj.ToString());
            }
            catch (JsonException e)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_KEY, "Json with key [" + key + "] is invalid.", e.Message);
            }
            catch (InvalidCastException e)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_DATA_TYPE, "Json with key [" + key + "] is invalid.", e.Message);
            }
            return val;
        }

        /// <summary>
        /// Gets the int value with the specified key.
        /// </summary>
        /// <param name="key">Key to get value</param>
        /// <returns>The int value with the specified key.If value is not an integer or key does not exists then it returns 0.</returns>
        public Int32 getInt32(String key)
        {
            if (key == null || data[key] == null)
            {
                return 0;
            }

            Int32 val;
            try
            {
                Object obj = data[key];
                val = Convert.ToInt32(obj.ToString());
            }
            catch (JsonException e)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_KEY, "Json with key [" + key + "] is invalid.", e.Message);
            }
            catch (InvalidCastException e)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_DATA_TYPE, "Json with key [" + key + "] is invalid.", e.Message);
            }
            return val;
        }

        /// <summary>
        /// Gets the int value with the specified key.
        /// </summary>
        /// <param name="key">Key to get value</param>
        /// <returns>The int value with the specified key.If value is not an integer or key does not exists then it returns 0.</returns>
        public Int64 getInt64(String key)
        {
            if (key == null || data[key] == null)
            {
                return 0;
            }

            Int64 val;
            try
            {
                Object obj = data[key];
                val = Convert.ToInt64(obj.ToString());
            }
            catch (JsonException e)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_KEY, "Json with key [" + key + "] is invalid.", e.Message);
            }
            catch (InvalidCastException e)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_DATA_TYPE, "Json with key [" + key + "] is invalid.", e.Message);
            }
            return val;
        }

        /// <summary>
        /// Gets the long value with the specified key.
        /// </summary>
        /// <param name="key">Key to get value</param>
        /// <returns>The long value with the specified key.If value is not a long or key does not exists then it returns 0.</returns>
        public long getLong(String key)
        {
            if (key == null || data[key] == null)
            {
                return 0L;
            }

            long val;
            try
            {
                Object obj = data[key];
                val = (long)obj;
            }
            catch (JsonException e)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_KEY, "Json with key [" + key + "] is invalid.", e.Message);
            }
            catch (InvalidCastException e)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_DATA_TYPE, "Json with key [" + key + "] is invalid.", e.Message);
            }
            return val;
        }

        /// <summary>
        /// Gets the boolean value with the specified key.
        /// </summary>
        /// <param name="key">Key to get value</param>
        /// <returns>The boolean value with the specified key.If value is not a boolean or key does not exists then it returns false.</returns>
        public Boolean getBoolean(String key)
        {
            if (key == null || data[key] == null)
            {
                return false;
            }

            Boolean val;
            try
            {
                Object obj = data[key];
                val = Convert.ToBoolean(obj.ToString());
            }
            catch (JsonException e)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_KEY, "Json with key [" + key + "] is invalid.", e.Message);
            }
            catch (InvalidCastException e)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_DATA_TYPE, "Json with key [" + key + "] is invalid.", e.Message);
            }
            return val;
        }

        /// <summary>
        /// Gets the double value with the specified key.
        /// </summary>
        /// <param name="key">Key to get value</param>
        /// <returns>The double value with the specified key.If value is not a double or key does not exists then it returns 0.0.</returns>
        public Double getDouble(String key)
        {
            if (key == null || data[key] == null)
            {
                return 0.0D;
            }

            Double val;
            try
            {
                Object obj = data[key];
                val = Convert.ToDouble(obj.ToString());
            }
            catch (JsonException e)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_KEY, "Json with key [" + key + "] is invalid.", e.Message);
            }
            catch (InvalidCastException e)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_DATA_TYPE, "Json with key [" + key + "] is invalid.", e.Message);
            }
            return val;
        }

        /// <summary>
        /// Gets the <see cref="JObject"/> object with the specified key.
        /// </summary>
        /// <param name="key">Key to get value</param>
        /// <returns><see cref="JObject"/> object with the specified key.If the object type is not an <see cref="JObject"/> or key does not exists then it returns null.</returns>
        public JObject getJObject(String key)
        {
            if (key == null || data[key] == null)
            {
                return null;
            }

            JObject val;
            try
            {
                Object obj = data[key];
                val = (JObject)obj;
            }
            catch (JsonException e)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_KEY, "Json with key [" + key + "] is invalid.", e.Message);
            }
            catch (InvalidCastException e)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_DATA_TYPE, "Json with key [" + key + "] is invalid.", e.Message);
            }
            return val;
        }

        /// <summary>
        /// Gets the <see cref="JArray"/> object with the specified key.
        /// </summary>
        /// <param name="key">Key to get value</param>
        /// <returns><see cref="JArray"/> object with the specified key.If the object type is not an <see cref="JArray"/> or key does not exists then it returns null.</returns>
        public JArray getJArray(String key)
        {
            if (key == null || data[key] == null)
            {
                return null;
            }

            JArray val;
            try
            {
                Object obj = data[key];
                val = (JArray)obj;
            }
            catch (JsonException e)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_KEY, "Json with key [" + key + "] is invalid.", e.Message);
            }
            catch (InvalidCastException e)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_DATA_TYPE, "Json with key [" + key + "] is invalid.", e.Message);
            }
            return val;
        }

        /// <summary>
        /// Gets the <see cref="NetmeraMedia"/> object with the specified key.
        /// </summary>
        /// <param name="key">Key to get value</param>
        /// <returns><see cref="NetmeraMedia"/> object with the specified key.If the object type is not an  <see cref="NetmeraMedia"/> or key does not exists then it returns null.</returns>
        public NetmeraMedia getNetmeraMedia(String key)
        {
            if (string.IsNullOrEmpty(data.Value<String>(key)))
            {
                return null;
            }

            Object obj;
            try
            {
                obj = data.Value<String>(key);
            }
            catch (JsonException)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_KEY, "Json with key [" + key + "] is invalid.");
            }
            if (obj.GetType() != typeof(String))
            {
                return null;
            }

            if (!Regex.IsMatch((String)obj, NetmeraConstants.Url_Pattern))
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_URL, key);
            }

            String url = (String)obj;

            byte[] imageBytes = null;
            try
            {
                imageBytes = HttpUtils.toByteArray(new Uri(url));
            }
            catch (UriFormatException e)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_DATA_TYPE, e.Message);
            }
            catch (IOException e)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_IO_EXCEPTION, e.Message);
            }

            NetmeraMedia nm = new NetmeraMedia(imageBytes);
            nm.setUrl(url);

            return nm;
        }

        /// <summary>
        /// Gets the  <see cref="NetmeraGeoLocation"/> object with the specified key.
        /// </summary>
        /// <param name="key">Key to get value</param>
        /// <returns><see cref="NetmeraGeoLocation"/> object with the specified key. If the object type is not an <see cref="NetmeraGeoLocation"/> or key does not exists then it returns null.</returns>
        public NetmeraGeoLocation getNetmeraGeoLocation(String key)
        {
            String locationKey = key + NetmeraConstants.LocationField_Suffix;
            String latitudeKey = key + NetmeraConstants.LocationLatitude_Suffix;
            String longitudeKey = key + NetmeraConstants.LocationLongitude_Suffix;

            if (data[locationKey] == null)
            {
                return null;
            }

            String latitude, longitude;
            NetmeraGeoLocation geoLocation;
            try
            {
                latitude = Convert.ToString(data[latitudeKey].ToString());
                longitude = Convert.ToString(data[longitudeKey].ToString());

                geoLocation = new NetmeraGeoLocation(Double.Parse(latitude), Double.Parse(longitude));
            }
            catch (JsonException e)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_KEY, "Json with key [" + locationKey + "] is invalid.", e);
            }

            return geoLocation;
        }
    }
}