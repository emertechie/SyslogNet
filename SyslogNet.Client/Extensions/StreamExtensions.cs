using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SyslogNet.Client.Extensions
{
    internal static class StreamExtensions
    {
		private static readonly byte[] WriteByteAsyncBuffer = new byte[1];
		public static Task WriteByteAsync(this Stream stream, char value, CancellationToken token)
		{
			WriteByteAsyncBuffer[0] = (byte)value;
			return stream.WriteAsync(WriteByteAsyncBuffer, 0, 1, token);
		}
    }
}