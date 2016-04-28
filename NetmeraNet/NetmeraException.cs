using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            /// Error Code
            /// </summary>
            public static ErrorCode EC_INVALID_LATITUDE = new ErrorCode(171);
            /// <summary>
            /// Error Code
            /// </summary>
            public static ErrorCode EC_INVALID_LONGITUDE = new ErrorCode(172);

            /// <summary>
            /// ErrorCode related to push notification
            /// </summary>
            public static ErrorCode EC_PUSH_MESSAGE_EMPTY = new ErrorCode(180);
            /// <summary>
            /// ErrorCode related to push notification
            /// </summary>
            public static ErrorCode EC_PUSH_MESSAGE_LIMIT = new ErrorCode(181);
            /// <summary>
            /// ErrorCode related to push notification
            /// </summary>
            public static ErrorCode EC_PUSH_ERROR = new ErrorCode(203);
            /// <summary>
            /// ErrorCode related to push notification
            /// </summary>
            public static ErrorCode EC_PUSH_DEVICE_NOT_REGISTERED = new ErrorCode(204);

            private readonly int errorCode;
            ErrorCode(int errorCode)
            {
                this.errorCode = errorCode;
            }

            /// <summary>
            /// getValue
            /// </summary>
            /// <returns>Error Code</returns>
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
        /// <param name="exceptionParams">throw exception message params</param>
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