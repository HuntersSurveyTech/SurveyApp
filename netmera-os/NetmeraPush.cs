using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace Netmera
{
    /// <summary>
    /// It is used to send push notifications to mobile devices.
    /// </summary>
    public class NetmeraPush : BasePush
    {
        private bool sendToAndroid;
        private bool sendToIos;
        private bool sendToWp;

        /// <summary>
        /// Sends notification to Android, IOS and Windows Phone devices.
        /// </summary>
        /// <param name="callback">The method that will be run just after sending notification.</param>
        public override void sendNotification(Action<Dictionary<PushChannel, NetmeraPushDetail>, Exception> callback)
        {
            bool isPlatformSelected = false;
            List<String> channels = new List<string>();

            if (sendToAndroid)
            {
                channels.Add(NetmeraConstants.Netmera_Push_Type_Android);
                //NetmeraAndroidPush androidPush = new NetmeraAndroidPush();
                //androidPush.setDeviceGroups(this.getDeviceGroups());
                //androidPush.setMessage(this.getMessage());
                //androidPush.sendNotification();
                isPlatformSelected = true;
            }

            if (sendToIos)
            {
                channels.Add(NetmeraConstants.Netmera_Push_Type_Ios);
                //NetmeraIOSPush iosPush = new NetmeraIOSPush();
                //iosPush.setDeviceGroups(this.getDeviceGroups());
                //iosPush.setMessage(this.getMessage());
                //iosPush.sendNotification();
                isPlatformSelected = true;
            }

            if (sendToWp)
            {
                channels.Add(NetmeraConstants.Netmera_Push_Type_Wp);
                isPlatformSelected = true;
            }

            if (channels.Count != 0)
            {
                base.sendPushMessage(channels, callback);
            }
            else if (!isPlatformSelected)
            {
                if (callback != null)
                    callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_REQUIRED_FIELD, "You should set either sendToAndroid or sendToIos or sendToWp to true"));
            }
        }
        /// <summary>
        /// Sets whether the notification will be sent to Android devices or not
        /// </summary>
        /// <param name="sendToAndroid">Whether the notification will be sent to Android devices or not</param>
        public void setSendToAndroid(bool sendToAndroid)
        {
            this.sendToAndroid = sendToAndroid;
        }

        /// <summary>
        /// Sets whether the notification will be sent to IOS devices or not
        /// </summary>
        /// <param name="sendToIos">Whether the notification will be sent to IOS devices or not</param>
        public void setSendToIos(bool sendToIos)
        {
            this.sendToIos = sendToIos;
        }

        /// <summary>
        /// Sets whether the notification will be sent to Windows Phone devices or not
        /// </summary>
        /// <param name="sendToWp">Whether the notification will be sent to Windows Phone devices or not</param>
        public void setSendToWp(bool sendToWp)
        {
            this.sendToWp = sendToWp;
        }
    }
}
