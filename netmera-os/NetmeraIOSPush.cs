using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Netmera
{
    /// <summary>
    /// It is used to send push notifications to IOS devices.
    /// </summary>
    public class NetmeraIOSPush : BasePush
    {
        /// <summary>
        /// Sends notification to IOS devices.
        /// </summary>
        /// <param name="callback">The method that will be run just after sending notification.</param>
        public override void sendNotification(Action<Dictionary<PushChannel, NetmeraPushDetail>, Exception> callback)
        {
            List<String> channels = new List<String>();
            channels.Add(NetmeraConstants.Netmera_Push_Type_Ios);
            base.sendPushMessage(channels, callback);
        }
    }
}
