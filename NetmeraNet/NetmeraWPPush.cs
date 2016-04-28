using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Netmera
{
    /// <summary>
    /// It is used to send push notifications to Windows Phone devices.
    /// </summary>
    public class NetmeraWPPush : BasePush
    {
        /// <summary>
        /// Sends notification to Windows Phone devices.
        /// </summary>
        /// <returns><see cref="BasePush.PushChannel"/>-<see cref="NetmeraPushDetail"/> pairs to show the details of sending notification to devices.</returns>
        public override Dictionary<PushChannel, NetmeraPushDetail> sendNotification()
        {
            List<String> channels = new List<String>();
            channels.Add(NetmeraConstants.Netmera_Push_Type_Wp);
            return base.sendPushMessage(channels);
        }
    }
}