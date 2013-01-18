using System;
using System.Net.Sockets;
using SyslogNet.Serialization;

namespace SyslogNet.Transport
{
	public class SyslogUdpSender : ISyslogMessageSender, IDisposable
	{
		private readonly UdpClient udpClient;

		public SyslogUdpSender(string hostname, int port)
		{
			udpClient = new UdpClient(hostname, port);
		}

		public void Send(SyslogMessage message, ISyslogMessageSerializer serializer)
		{
			byte[] datagramBytes = serializer.Serialize(message);
			udpClient.Send(datagramBytes, datagramBytes.Length);
		}

		public void Dispose()
		{
			udpClient.Close();
		}
	}
}