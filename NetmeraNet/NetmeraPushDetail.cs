using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Netmera
{
    /// <summary>
    /// The NetmeraPushDetail class is used to get the detailed information after a push notification is send.
    /// After sending notifications; returned map contains NetmeraPushDetails which contains the detailed information for each channel.
    /// </summary>
    public class NetmeraPushDetail
    {
        /// <summary>
        ///Path of the PushDetail 
        /// </summary>
        String path;
        /// <summary>
        /// Status of the Push Notification
        /// </summary>
        String status;
        /// <summary>
        /// Error of the Push Notification
        /// </summary>
        String error;
        /// <summary>
        /// Number of successes of the Push Notification 
        /// </summary>
        int successful;
        /// <summary>
        /// Number of failures of the Push Notification
        /// </summary>
        int failed;
        /// <summary>
        /// Message of the Push Notification
        /// </summary>
        String message;

        /// <summary>
        /// getPath
        /// </summary>
        /// <returns>Path of the PushDetail </returns>
        public String getPath()
        {
            return path;
        }
        internal void setPath(String path)
        {
            this.path = path;
        }
        /// <summary>
        /// getStatus
        /// </summary>
        /// <returns>Status of the Push Notification</returns>
        public String getStatus()
        {
            return status;
        }
        internal void setStatus(String status)
        {
            this.status = status;
        }
        /// <summary>
        /// getError
        /// </summary>
        /// <returns>Error of the Push Notification</returns>
        public String getError()
        {
            return error;
        }
        internal void setError(String error)
        {
            this.error = error;
        }
        /// <summary>
        /// getSuccessful
        /// </summary>
        /// <returns>Number of successes of the Push Notification </returns>
        public int getSuccessful()
        {
            return successful;
        }
        internal void setSuccessful(int successful)
        {
            this.successful = successful;
        }
        /// <summary>
        /// getFailed
        /// </summary>
        /// <returns>Number of failures of the Push Notification</returns>
        public int getFailed()
        {
            return failed;
        }
        internal void setFailed(int failed)
        {
            this.failed = failed;
        }
        /// <summary>
        /// getMessage
        /// </summary>
        /// <returns>Message of the Push Notification</returns>
        public String getMessage()
        {
            return message;
        }
        internal void setMessage(String message)
        {
            this.message = message;
        }
    }
}
