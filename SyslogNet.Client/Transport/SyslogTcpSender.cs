using System;
using System.Net.Sockets;
using SyslogNet.Client.Serialization;

namespace SyslogNet.Client.Transport
{
	public class SyslogTcpSender : ISyslogMessageSender, IDisposable
	{
		private readonly TcpClient tcpClient;
		private readonly NetworkStream tcpClientStream;

		public SyslogTcpSender(string hostname, int port)
		{
			try
			{
				tcpClient = new TcpClient(hostname, port);
				tcpClientStream = tcpClient.GetStream();
			}
			catch
			{
				Dispose();
				throw;
			}
		}

		public void Send(SyslogMessage message, ISyslogMessageSerializer serializer)
		{
			var datagramBytes = serializer.Serialize(message);
			tcpClientStream.Write(datagramBytes, 0, datagramBytes.Length);
		}

		public void Dispose()
		{
			if (tcpClient != null)
			{
				tcpClient.Close();
				((IDisposable)tcpClient).Dispose();
			}

			if (tcpClientStream != null)
				tcpClientStream.Dispose();
		}
	}
}