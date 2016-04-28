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
    /// It is used to send push notifications to Windows Phone devices.
    /// </summary>
    public class NetmeraWPPush : BasePush
    {
        /// <summary>
        /// Sends notification to Windows Phone devices.
        /// </summary>
        /// <param name="callback">The method that will be run just after sending notification.</param>
        public override void sendNotification(Action<Dictionary<PushChannel, NetmeraPushDetail>, Exception> callback)
        {
            List<String> channels = new List<String>();
            channels.Add(NetmeraConstants.Netmera_Push_Type_Wp);
            base.sendPushMessage(channels, callback);
        }
    }
}
