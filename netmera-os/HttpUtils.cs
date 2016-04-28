using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Resources;
using System.Windows.Controls;
using System.Threading;

namespace Netmera
{
    /// <summary>
    /// Provides some utilities to obtain byte arrays from some sorts of media data.
    /// </summary>
    public static class HttpUtils
    {
        private static readonly int DEFAULT_BUFFER_SIZE = 1024 * 4;

        /// <summary>
        /// Converts media in a URL to byte array
        /// </summary>
        /// <param name="uri">Remote media URI</param>
        /// <param name="callback">Method to be called just after byte array conversion finished </param>
        public static void toByteArray(Uri uri, Action<byte[], Exception> callback)
        {
            try
            {
                byte[] buffer = null;
                HttpWebRequest WebReq = (HttpWebRequest)WebRequest.Create(uri.AbsoluteUri);

                //This time, our method is GET.
                WebReq.Method = "GET";
                WebReq.BeginGetResponse(ar =>
                {
                    try
                    {
                        using (var response = (ar.AsyncState as HttpWebRequest).EndGetResponse(ar) as HttpWebResponse)
                        using (var streamResponse = response.GetResponseStream())
                        {
                            buffer = new byte[streamResponse.Length];
                            streamResponse.Read(buffer, 0, buffer.Length);
                            streamResponse.Close();

                            if (callback != null)
                                callback(buffer, null);
                        }
                    }
                    catch (WebException ex)
                    {
                        if (callback != null)
                            callback(null, new NetmeraException(NetmeraException.ErrorCode.EC_INVALID_REQUEST, ex));
                    }
                }, WebReq);
            }
            catch (Exception e)
            {
                if (callback != null)
                    callback(null, e);
            }
        }

        /// <summary>
        /// Converts media in a stream reader to byte array
        /// </summary>
        /// <param name="sr">Source media stream reader</param>
        /// <returns>Byte array obtained from stream reader</returns>
        public static byte[] toByteArray(StreamReader sr)
        {
            MemoryStream buffer = new MemoryStream();

            byte[] data = new byte[DEFAULT_BUFFER_SIZE];
            int n = 0;
            long count = 0;
            while (-1 != (n = sr.Read()))
            {
                buffer.Write(data, 0, n);
                count += n;
            }

            buffer.Flush();

            if (count > int.MaxValue)
            {
                return null;
            }

            return buffer.ToArray();
        }

        /// <summary>
        /// Converts media in a local file to byte array
        /// </summary>
        /// <param name="uri">Local media URI</param>
        /// <returns>Byte array obtained from a local media file</returns>
        public static byte[] toByteArray(Uri uri)
        {
            byte[] data = null;
            MemoryStream ms = new MemoryStream();
            StreamResourceInfo sri = null;
            sri = Application.GetResourceStream(uri);
            WriteableBitmap bit = Microsoft.Phone.PictureDecoder.DecodeJpeg(sri.Stream);
            bit.SaveJpeg(ms, bit.PixelWidth, bit.PixelHeight, 0, 100);
            ms.Seek(0, SeekOrigin.Begin);
            data = ms.GetBuffer();

            return data;
        }
    }
}
