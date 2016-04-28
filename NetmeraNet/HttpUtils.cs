using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

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
        /// <param name="uri">Source media url</param>
        /// <returns>Byte array obtained from the url</returns>
        public static byte[] toByteArray(Uri uri)
        {
            var webClient = new WebClient();
            byte[] imageBytes = webClient.DownloadData(uri);

            return imageBytes;
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
        /// <param name="file">Source media file</param>
        /// <returns>Byte array obtained from a local media file</returns>
        public static byte[] toByteArray(FileInfo file)
        {
            byte[] buffer = null;

            string fileName = file.FullName;

            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);

            BinaryReader br = new BinaryReader(fs);

            long totalBytes = new FileInfo(fileName).Length;

            buffer = br.ReadBytes((Int32)totalBytes);

            fs.Close();
            fs.Dispose();
            br.Close();

            return buffer;
        }
    }
}
