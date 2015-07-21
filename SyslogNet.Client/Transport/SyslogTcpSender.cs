using System;
using System.IO;
using System.Net.Sockets;
using SyslogNet.Client.Serialization;

namespace SyslogNet.Client.Transport
{
	public class SyslogTcpSender : ISyslogMessageSender, IDisposable
	{
		protected readonly TcpClient tcpClient;
		protected Stream transportStream;

		public SyslogTcpSender(string hostname, int port)
		{
			try
			{
				tcpClient = new TcpClient(hostname, port);
				transportStream = tcpClient.GetStream();
			}
			catch
			{
				Dispose();
				throw;
			}
		}

		public void Send(SyslogMessage message, ISyslogMessageSerializer serializer)
		{
			Send(message, serializer, true);
		}

		protected void Send(SyslogMessage message, ISyslogMessageSerializer serializer, bool flush = true)
		{
			var datagramBytes = serializer.Serialize(message);
			transportStream.Write(datagramBytes, 0, datagramBytes.Length);

			if (flush && !(transportStream is NetworkStream))
				transportStream.Flush();
		}

		public void Dispose()
		{
			if (transportStream != null)
				transportStream.Close();

			if (tcpClient != null)
				tcpClient.Close();
		}
	}
}