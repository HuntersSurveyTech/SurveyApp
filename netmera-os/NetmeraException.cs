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
    /// <copyright>Copyright 2012 Inomera Research</copyright>
    /// <summary>
    /// Exception handling mechanism for Netmera API. It throws an exception when there is a failure while creating,editing,deleting and searching data. Also it throws and exception when there is an network connection error.
    /// </summary>
    public class NetmeraException : Exception
    {
        /// <summary>
        /// Error codes container for Netmera
        /// </summary>
        public class ErrorCode
        {
            /// <summary>
            /// Error Code
            /// </summary>
            public static ErrorCode EC_INTERNAL_SERVER_ERROR = new ErrorCode(100);
            /// <summary>
            /// Error Code
            /// </summary>
            public static ErrorCode EC_IO_EXCEPTION = new ErrorCode(101);
            /// <summary>
            /// Error Code
            /// </summary>
            public static ErrorCode EC_NULL_EXCEPTION = new ErrorCode(102);
            /// <summary>
            /// Error Code
            /// </summary>
            public static ErrorCode EC_HTTP_PROTOCOL_EXCEPTION = new ErrorCode(103);
            /// <summary>
            /// Error Code
            /// </summary>
            public static ErrorCode EC_INVALID_URL = new ErrorCode(104);
            /// <summary>
            /// Error Code
            /// </summary>
            public static ErrorCode EC_INVALID_JSON = new ErrorCode(105);
            /// <summary>
            /// Error Code
            /// </summary>
            public static ErrorCode EC_INVALID_DATE_FORMAT = new ErrorCode(106);
            /// <summary>
            /// Error Code
            /// </summary>
            public static ErrorCode EC_INVALID_REQUEST = new ErrorCode(107);
            /// <summary>
            /// Error Code
            /// </summary>
            public static ErrorCode EC_INVALID_RESPONSE = new ErrorCode(108);
            /// <summary>
            /// Error Code
            /// </summary>
            public static ErrorCode EC_UNSUPPORTED_ENCODING = new ErrorCode(109);
            /// <summary>
            /// Error Code
            /// </summary>
            public static ErrorCode EC_INVALID_ACTION_TOKEN = new ErrorCode(110);
            /// <summary>
            /// Error Code
            /// </summary>
            public static ErrorCode EC_API_KEY_NOT_FOUND = new ErrorCode(111);

            /// <summary>
            /// Error Code
            /// </summary>
            public static ErrorCode EC_REQUIRED_FIELD = new ErrorCode(131);
            /// <summary>
            /// Error Code
            /// </summary>
            public static ErrorCode EC_INVALID_DATA_TYPE = new ErrorCode(132);
            /// <summary>
            /// Error Code
            /// </summary>
            public static ErrorCode EC_INVALID_KEY = new ErrorCode(133);
            /// <summary>
            /// Error Code
            /// </summary>
            public static ErrorCode EC_INVALID_PATH = new ErrorCode(134);
            /// <summary>
            /// Error Code
            /// </summary>
            public static ErrorCode EC_INVALID_OBJECT_NAME = new ErrorCode(135);

            /// <summary>
            /// Error Code
            /// </summary>
            public static ErrorCode EC_INVALID_EMAIL = new ErrorCode(151);
            /// <summary>
            /// Error Code
            /// </summary>
            public static ErrorCode EC_INVALID_PASSWORD = new ErrorCode(152);
            /// <summary>
            /// Error Code
            /// </summary>
            public static ErrorCode EC_ALREADY_REGISTERED_EMAIL = new ErrorCode(153);
            /// <summary>
            /// Error Code
            /// </summary>
            public static ErrorCode EC_USER_LOGIN_ERROR = new ErrorCode(154);
            /// <summary>
            /// Error Code
            /// </summary>
            public static ErrorCode EC_USER_REGISTER_ERROR = new ErrorCode(155);
            /// <summary>
            /// Error Code
            /// </summary>
            public static ErrorCode EC_USER_UPDATE_ERROR = new ErrorCode(156);

            /// <summary>
            /// ErrorCode related to geo-location
            /// </summary>
            public static ErrorCode EC_INVALID_LATITUDE = new ErrorCode(171);
            /// <summary>
            /// ErrorCode related to geo-location
            /// </summary>
            public static ErrorCode EC_INVALID_LONGITUDE = new ErrorCode(172);

            /// <summary>
            /// ErrorCodes related to push notification
            /// </summary>
            public static ErrorCode EC_PUSH_MESSAGE_EMPTY = new ErrorCode(201);
            /// <summary>
            /// ErrorCode related to push notification
            /// </summary>
            public static ErrorCode EC_PUSH_MESSAGE_LIMIT = new ErrorCode(202);
            /// <summary>
            /// ErrorCodes related to push notification
            /// </summary>
            public static ErrorCode EC_PUSH_ERROR = new ErrorCode(203);
            /// <summary>
            /// ErrorCodes related to push notification
            /// </summary>
            public static ErrorCode EC_PUSH_DEVICE_NOT_REGISTERED = new ErrorCode(204);
            /// <summary>
            /// ErrorCodes related to push notification
            /// </summary>
            public static ErrorCode EC_CHANNEL_QUOTA_EXCEEDED = new ErrorCode(205);

            /// <summary>
            /// ErrorCodes related to push notification
            /// </summary>
            public static ErrorCode EC_RICH_PUSH_ID_EMPTY = new ErrorCode(211);
            /// <summary>
            /// ErrorCodes related to push notification
            /// </summary>
            public static ErrorCode EC_RICH_PUSH_CONTENT_ERROR = new ErrorCode(212);
            /// <summary>
            /// ErrorCodes related to push notification
            /// </summary>
            public static ErrorCode EC_RICH_PUSH_JAVASCRIPT_ERROR = new ErrorCode(212);

            /// <summary>
            /// ErrorCodes related to Facebook
            /// </summary>
            public static ErrorCode EC_FB_ERROR = new ErrorCode(184);

            /// <summary>
            /// ErrorCodes related to Twitter
            /// </summary>
            public static ErrorCode EC_TW_ERROR = new ErrorCode(194);

            private readonly int errorCode;
            ErrorCode(int errorCode)
            {
                this.errorCode = errorCode;
            }

            /// <summary>
            /// get error code
            /// </summary>
            /// <returns>Error code</returns>
            public int getValue()
            {
                return errorCode;
            }
        }

        private readonly ErrorCode errorCode;
        private readonly Object[] exceptionParams;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code">NetmeraException.ErrorCode</param>
        /// <param name="exceptionParams">throw exception message</param>
        public NetmeraException(ErrorCode code, params object[] exceptionParams)
            : base(exceptionParams.Length > 0 ? String.Join(" ", exceptionParams) : "NetmeraException")
        {
            this.errorCode = code;
            this.exceptionParams = exceptionParams;
        }

        /// <summary>
        /// Returns the error code
        /// </summary>
        /// <returns>The error code</returns>
        public int getCode()
        {
            return errorCode.getValue();
        }
    }
}