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

namespace Netmera
{
    /// <summary>
    /// The class used while registering and unregistering devices to specified groups
    /// or getting the detailed information of a registered device.
    /// </summary>
    public class NetmeraDeviceDetail
    {
        /// <summary>
        /// Registration ID (Channel URI) of the device
        /// </summary>
        public String regId;

        /// <summary>
        /// List of the groups of the device
        /// </summary>
        List<String> deviceGroups;

        /// <summary>
        /// Device location to be registered
        /// </summary>
        NetmeraGeoLocation deviceLocation;

        /// <summary>
        /// Class constructor
        /// </summary>
        public NetmeraDeviceDetail()
        {

        }

        /// <summary>
        /// Constructor with register ID (Channel URI)
        /// </summary>
        /// <param name="regId">Register ID (Channel URI)</param>
        public NetmeraDeviceDetail(String regId)
        {
            this.regId = regId;
        }

        /// <summary>
        /// Sets the groups of the device
        /// </summary>
        /// <param name="deviceGroups">Device groups</param>
        public void setDeviceGroups(List<String> deviceGroups)
        {
            this.deviceGroups = deviceGroups;
        }

        /// <summary>
        /// Overrides device groups with the latest one
        /// </summary>
        /// <param name="deviceGroup">The latest device group</param>
        public void setDeviceGroup(String deviceGroup)
        {
            this.deviceGroups = new List<String>();
            this.deviceGroups.Add(deviceGroup);
        }

        /// <summary>
        /// Returns device groups
        /// </summary>
        /// <returns>Device groups</returns>
        public List<String> getDeviceGroups()
        {
            return deviceGroups;
        }

        /// <summary>
        /// Sets device location
        /// </summary>
        /// <param name="location">Device location in type <seealso cref="NetmeraGeoLocation"/></param>
        public void setDeviceLocation(NetmeraGeoLocation location)
        {
            this.deviceLocation = location;
        }

        /// <summary>
        /// Returns device location
        /// </summary>
        /// <returns>Device location in type <seealso cref="NetmeraGeoLocation"/></returns>
        public NetmeraGeoLocation getDeviceLocation()
        {
            return this.deviceLocation;
        }

        internal void setRegId(String regId)
        {
            this.regId = regId;
        }
    }
}
