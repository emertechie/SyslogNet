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
			tcpClientStream.Flush();

			// Note: This doesn't work reliably. Can't seem to find a method which does
			if (!tcpClient.Connected)
				throw new CommunicationsException("Could not send message because client was disconnected");
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