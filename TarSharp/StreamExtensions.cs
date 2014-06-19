using System;
using System.IO;
using System.Threading.Tasks;

namespace TarSharp
{
    static class StreamExtensions
    {
        public static byte[] ReadBytes(this Stream stream, int count)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");
            byte[] buffer = new byte[count];
            int nRead, totalRead = 0;
            while (totalRead < count && (nRead = stream.Read(buffer, totalRead, count - totalRead)) != 0)
            {
                totalRead += nRead;
            }

            if (count > totalRead)
            {
                Array.Resize(ref buffer, totalRead);
            }
            return buffer;
        }

        public static async Task<byte[]> ReadBytesAsync(this Stream stream, int count)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");
            byte[] buffer = new byte[count];
            int nRead, totalRead = 0;
            while (totalRead < count && (nRead = await stream.ReadAsync(buffer, totalRead, count - totalRead)) != 0)
            {
                totalRead += nRead;
            }

            if (count > totalRead)
            {
                Array.Resize(ref buffer, totalRead);
            }
            return buffer;
        }
    }
}
