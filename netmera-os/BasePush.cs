using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Netmera
{

    /// <summary>
    /// An abstract class that contains basic functions for <see cref="NetmeraPush"/>, <see cref="NetmeraAndroidPush"/> and <see cref="NetmeraIOSPush"/>,
    /// </summary>
    public abstract class BasePush
    {
        /// <summary>
        /// Push channel types
        /// </summary>
        public enum PushChannel
        {
            /// <summary>
            /// Android
            /// </summary>
            android,
            /// <summary>
            /// IOS
            /// </summary>
            ios,
            /// <summary>
            /// Windows Phone
            /// </summary>
            wp
        }

        /// <summary>
        /// push message
        /// </summary>
        protected String message;
        /// <summary>
        /// device groups
        /// </summary>
        protected List<String> deviceGroups = new List<string>();
        /// <summary>
        /// location type
        /// </summary>
        protected String locationType;
        /// <summary>
        /// distance
        /// </summary>
        protected double distance;
        /// <summary>
        /// first location
        /// </summary>
        protected NetmeraGeoLocation firstLoc;
        /// <summary>
        /// second location
        /// </summary>
        protected NetmeraGeoLocation secondLoc;

        /// <summary>
        /// An abstract method to be overriden sending notification to devices.
        /// </summary>
        /// <param name="callback">The method that will be run just after sending notification.</param>
        public abstract void sendNotification(Action<Dictionary<PushChannel, NetmeraPushDetail>, Exception> callback);

        /// <summary>
        /// Sets the notification message
        /// </summary>
        /// <param name="message">The notification message</param>
        public void setMessage(String message)
        {
            this.message = message;
        }

        /// <summary>
        /// Gets the notification message
        /// </summary>
        /// <returns>The notification message</returns>
        public String getMessage()
        {
            return this.message;
        }

        /// <summary>
        /// Sets the device groups
        /// </summary>
        /// <param name="deviceGroups">Device groups</param>
        public void setDeviceGroups(List<String> deviceGroups)
        {
            this.deviceGroups = deviceGroups;
        }

        /// <summary>
        /// Gets the device groups
        /// </summary>
        /// <returns>Device groups</returns>
        public List<String> getDeviceGroups()
        {
            return this.deviceGroups;
        }

        /// <summary>
        /// Sets one device groups after clearing the previous
        /// </summary>
        /// <param name="deviceGroup">The device group</param>
        public void setDeviceGroup(String deviceGroup)
        {
            deviceGroups.Clear();
            deviceGroups.Add(deviceGroup);
        }

        /// <summary>
        /// Sets the search type of this push notification to box search.
        /// </summary>
        /// <param name="firstLoc">First point of the box</param>
        /// <param name="secondLoc">Second point of the box</param>
        public void setBoxPush(NetmeraGeoLocation firstLoc, NetmeraGeoLocation secondLoc)
        {
            if (firstLoc != null && secondLoc != null)
            {
                this.locationType = NetmeraConstants.Netmera_Push_Type_Box_Location;
                this.firstLoc = firstLoc;
                this.secondLoc = secondLoc;
            }
        }

        /// <summary>
        /// Sets the search type of this push notification to circle search.
        /// </summary>
        /// <param name="centerLoc">Center point of the circle.</param>
        /// <param name="distance">Distance radius of the circle in kilometers.</param>
        public void setCirclePush(NetmeraGeoLocation centerLoc, double distance)
        {
            if (centerLoc != null)
            {
                this.locationType = NetmeraConstants.Netmera_Push_Type_Circle_Location;
                this.distance = distance;
                this.firstLoc = centerLoc;
                this.secondLoc = null;
            }
        }

        /// <summary>
        /// send push message
        /// </summary>
        /// <param name="channelList">channel list</param>
        /// <param name="callback">push channel, netmera push detail</param>
        protected void sendPushMessage(List<String> channelList, Action<Dictionary<PushChannel, NetmeraPushDetail>, Exception> callback)
        {
            if (NetmeraClient.securityToken == null)
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_NULL_EXCEPTION, "Apikey cannot be null, please call NetmeraClient.init() method first!"));
            }
            if (string.IsNullOrEmpty(message))
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_PUSH_MESSAGE_EMPTY, "Message cannot be empty"));
            }
            else if (message.Length > 180)
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_PUSH_MESSAGE_LIMIT, "Message limit cannot exceed 180 characters"));
            }
            else
            {
                try
                {
                    String groupString = null;
                    if (deviceGroups != null && deviceGroups.Count != 0)
                    {
                        JArray jsonArrayForDeviceGroups = JArray.FromObject(deviceGroups);
                        groupString = jsonArrayForDeviceGroups.ToString();
                    }
                    JArray jsonArrayForChannels = JArray.FromObject(channelList);
                    String channels = jsonArrayForChannels.ToString();

                    Dictionary<string, object> postParameters = new Dictionary<string, object>();
                    postParameters.Add(NetmeraConstants.Netmera_Push_Android_Message, message);
                    postParameters.Add(NetmeraConstants.Netmera_Push_Channels, channels);
                    postParameters.Add(NetmeraConstants.Netmera_Push_Apikey, NetmeraClient.securityToken);
                    postParameters.Add(NetmeraConstants.Netmera_Push_Device_Groups, groupString);

                    if (this.locationType == NetmeraConstants.Netmera_Push_Type_Box_Location)
                    {
                        postParameters.Add(NetmeraConstants.Netmera_Push_LocationType_Params, NetmeraConstants.Netmera_Push_Type_Box_Location);
                        postParameters.Add(NetmeraConstants.Netmera_Push_Latitude1_Params, this.firstLoc.getLatitude());
                        postParameters.Add(NetmeraConstants.Netmera_Push_Longitude1_Params, this.firstLoc.getLongitude());
                        postParameters.Add(NetmeraConstants.Netmera_Push_Latitude2_Params, this.secondLoc.getLatitude());
                        postParameters.Add(NetmeraConstants.Netmera_Push_Longitude2_Params, this.secondLoc.getLongitude());
                    }
                    else if (this.locationType == NetmeraConstants.Netmera_Push_Type_Circle_Location)
                    {
                        postParameters.Add(NetmeraConstants.Netmera_Push_LocationType_Params, NetmeraConstants.Netmera_Push_Type_Circle_Location);
                        postParameters.Add(NetmeraConstants.Netmera_Push_Latitude1_Params, this.firstLoc.getLatitude());
                        postParameters.Add(NetmeraConstants.Netmera_Push_Longitude1_Params, this.firstLoc.getLongitude());
                        postParameters.Add(NetmeraConstants.LocationDistance_Params, this.distance);
                    }

                    String url = NetmeraConstants.Netmera_Domain_Url + NetmeraConstants.Netmera_Push_Server_Url + NetmeraConstants.Netmera_Push_Send;

                    NetmeraHttpUtils.sendPushMessage(url, postParameters, callback);
                }
                catch (ProtocolViolationException)
                {
                    if (callback != null)
                        callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_HTTP_PROTOCOL_EXCEPTION, "Protocol exception occurred while sending notification to devices"));
                }
                catch (IOException)
                {
                    if (callback != null)
                        callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_IO_EXCEPTION, "IO Exception occurred while sending notification to devices"));
                }
                catch (JsonException)
                {
                    if (callback != null)
                        callback(null, null);
                }
            }
        }
    }
}
