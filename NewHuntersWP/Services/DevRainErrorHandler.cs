using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Net.Browser;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Info;
using Newtonsoft.Json;

namespace HuntersWP.Services
{

    

    public class DeviceHelper
    {
        public static string GetAssemblyFileVersion()
        {
            var assembly = Assembly.GetCallingAssembly();
            var versionString = assembly.GetCustomAttributes(false)
                .OfType<AssemblyFileVersionAttribute>()
                .First()
                .Version;

            return versionString;
        }


        public static Version GetAppVersion()
        {
            try
            {
                var data = ApplicationManifestHelper.Read();

                Version version;
                if (Version.TryParse(data.Version, out version))
                {
                    return version;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return default(Version);

            //try
            //{
            //	var doc = XDocument.Load("WMAppManifest.xml");
            //	var xAttribute = doc.Descendants("App").First().Attribute("Version");
            //	if (xAttribute != null)
            //	{
            //		var version = xAttribute.Value;
            //		if (!string.IsNullOrEmpty(version))
            //		{
            //			Version result;
            //			if (Version.TryParse(version, out result))
            //			{
            //				return result;
            //			}
            //		}
            //	}
            //}
            //// ReSharper disable EmptyGeneralCatchClause
            //catch
            //// ReSharper restore EmptyGeneralCatchClause
            //{
            //}
            //return default(Version);
        }

        public static long GetTimestamp(DateTime dateTime)
        {
            long ticks = dateTime.Ticks - new DateTime(1970, 1, 1).Ticks;
            ticks /= 10000000; //Convert windows ticks to seconds
            return ticks;
        }

        public static TimeSpan GetTimeSpan(long timestamp)
        {
            timestamp *= 10000000;

            timestamp += new DateTime(1970, 1, 1).Ticks;


            return TimeSpan.FromTicks(timestamp);
        }

        public static PhoneApplicationFrame GetCurrentApplicationFrame()
        {
            var frame = Application.Current.RootVisual as PhoneApplicationFrame;
            return frame;
        }

        public static PhoneApplicationPage GetCurrentPage()
        {
            var frame = GetCurrentApplicationFrame();
            var startPage = frame.Content as PhoneApplicationPage;

            return startPage;
        }

        public static PageOrientation GetCurrentOrientation()
        {
            return GetCurrentPage().Orientation;
        }

        public static string GetDeviceUniqueID()
        {
            try
            {
                byte[] result = null;
                object uniqueId;
                if (DeviceExtendedProperties.TryGetValue("DeviceUniqueId", out uniqueId))
                    result = (byte[])uniqueId;

                return Convert.ToBase64String(result);
            }
            catch
            {
                return string.Empty;
            }
        }




        public static string GetDeviceManufacturer()
        {
            try
            {
                return DeviceExtendedProperties.GetValue("DeviceManufacturer").ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string GetDeviceType()
        {
            try
            {
                return DeviceExtendedProperties.GetValue("DeviceName").ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string GetOsVersion()
        {
            return string.Format("WP {0}", System.Environment.OSVersion.Version);
        }

        public static bool IsLowLevelWP8Device()
        {
            var t = GetDeviceType();

            if (t == null) return false;

            return t.Contains("520") || t.Contains("525") || t.Contains("620") || t.Contains("625") || t.Contains("720") ||
                   t.Contains("725");
        }


        public static bool IsWP8
        {
            get { return Environment.OSVersion.Version >= WP8TargetedVersion; }
        }

        private static Version WP8TargetedVersion = new Version(8, 0);


        //public static IPAddress GetIpAddress()
        //{
        //	List<string> ipAddresses = new List<string>();

        //	var hostnames = NetworkInformation.GetHostNames();
        //	foreach (var hn in hostnames)
        //	{
        //		if (hn.IPInformation != null)
        //		{
        //			string ipAddress = hn.DisplayName;
        //			ipAddresses.Add(ipAddress);
        //		}
        //	}

        //	IPAddress address = IPAddress.Parse(ipAddresses[0]);
        //	return address;
        //}
    }

    public static class ApplicationManifestHelper
    {
        public static ManifestData Read()
        {
            var data = new ManifestData();
            var manifestXml = XElement.Load("WMAppManifest.xml");
            var appElement = manifestXml.Descendants("App").FirstOrDefault();

            if (appElement != null)
            {
                data.ProductId = (string)appElement.Attribute("ProductID");
                data.Title = (string)appElement.Attribute("Title");
                data.RuntimeType = (string)appElement.Attribute("RuntimeType");
                data.Version = (string)appElement.Attribute("Version");
                data.Genre = (string)appElement.Attribute("Genre");
                data.Author = (string)appElement.Attribute("Author");
                data.Description = (string)appElement.Attribute("Description");
                data.Publisher = (string)appElement.Attribute("Publisher");
            }

            appElement = manifestXml.Descendants("PrimaryToken").FirstOrDefault();

            if (appElement != null)
            {
                data.TokenId = (string)appElement.Attribute("TokenID");
            }

            return data;
        }
    }

    public class ManifestData
    {
        public string TokenId;
        public string Genre;
        public string Author;
        public string Description;
        public string Publisher;
        public string Title;
        public string Version;
        public string RuntimeType;
        public string ProductId;

        public string DisplayVersion
        {
            get
            {
                if (string.IsNullOrEmpty(Version)) return string.Empty;
                return Version.Substring(0, Version.IndexOf(".", StringComparison.Ordinal) + 2);
            }
        }
    }
}
