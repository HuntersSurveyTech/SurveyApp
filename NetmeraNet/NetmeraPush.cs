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
        /// Sends notification to IOS, Android and Windows Phone devices.
        /// </summary>
        /// <returns><see cref="BasePush.PushChannel"/>-<see cref="NetmeraPushDetail"/> pairs to show the details of sending notification to devices.</returns>
        public override Dictionary<PushChannel, NetmeraPushDetail> sendNotification()
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
                //NetmeraIOSPush iosPush = new NetmeraIOSPush();
                //iosPush.setDeviceGroups(this.getDeviceGroups());
                //iosPush.setMessage(this.getMessage());
                //iosPush.sendNotification();
                isPlatformSelected = true;
            }

            if (channels.Count != 0)
            {
                return base.sendPushMessage(channels);
            }
            else if (!isPlatformSelected)
            {
                throw new NetmeraException(NetmeraException.ErrorCode.EC_REQUIRED_FIELD, "You should set either sendToAndroid or sendToIos to true");
            }
            return null;
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
