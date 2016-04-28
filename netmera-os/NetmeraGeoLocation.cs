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

namespace Netmera
{
    /// <summary>
    /// It is used to create location with the given latitude and longitude values. It is used to set location into the content and then use it on the search queries.
    /// </summary>
    /// <example>
    /// NetmeraGeoLocation location = new NetmeraGeoLocation(41,29);
    /// NetmeraContent content = new NetmeraContent("SampleContent");
    /// content.add("location",location);
    /// content.create();
    /// </example>
    public class NetmeraGeoLocation
    {
        private double latitude = 0.0D;
        private double longitude = 0.0D;

        /// <summary>
        /// Default location where latitude and longitude are 0.0
        /// </summary>
        public NetmeraGeoLocation() { }

        /// <summary>
        /// Creates location with the given latitude and longitude.
        /// </summary>
        /// <param name="lat">Must be between the range of (-90,90)</param>
        /// <param name="lng">Must be between the range of (-180,180)</param>
        public NetmeraGeoLocation(double lat, double lng)
        {
            setLatitude(lat);
            setLongitude(lng);
        }

        /// <summary>
        /// Set latitude into the location. latitude must be between the range of (-180.0, 180.0).
        /// </summary>
        /// <param name="lat">Location's latitude</param>
        public void setLatitude(double lat)
        {
            if ((lat > 90.0D) || (lat < -90.0D))
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_LATITUDE, "Latitude must be within the range of (-90.0, 90.0)");
            }

            this.latitude = lat;
        }

        /// <summary>
        /// Get latitude.
        /// </summary>
        /// <returns>The latitude of the given location.</returns>
        public double getLatitude()
        {
            return latitude;
        }

        /// <summary>
        /// Set longitude into the location. Longitude must be between the range of (-180.0, 180.0).
        /// </summary>
        /// <param name="lng">Location's longitude</param>
        public void setLongitude(double lng)
        {
            if ((lng > 180.0D) || (lng < -180.0D))
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_LONGITUDE, "Longitude must be within the range (-180.0, 180.0)");
            }

            this.longitude = lng;
        }

        /// <summary>
        /// Get longitude.
        /// </summary>
        /// <returns>the longitude of the given location.</returns>
        public double getLongitude()
        {
            return longitude;
        }
    }
}