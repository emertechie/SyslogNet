using System.IO;
using System.Threading.Tasks;

namespace SyslogNet.Client.Transport
{
    internal static class StreamExtensions
    {
        private static readonly byte[] WriteByteAsyncBuffer = new byte[1];
        public static Task WriteByteAsync(this Stream stream, byte value)
        {
            WriteByteAsyncBuffer[0] = value;
            return stream.WriteAsync(WriteByteAsyncBuffer, 0, 1);
        }
    }
}