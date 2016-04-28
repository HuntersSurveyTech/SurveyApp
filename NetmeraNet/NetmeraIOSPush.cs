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
        /// <returns><see cref="BasePush.PushChannel"/>-<see cref="NetmeraPushDetail"/> pairs to show the details of sending notification to devices.</returns>
        public override Dictionary<PushChannel, NetmeraPushDetail> sendNotification()
        {
            List<String> channels = new List<String>();
            channels.Add(NetmeraConstants.Netmera_Push_Type_Ios);
            return base.sendPushMessage(channels);
        }
    }
}
