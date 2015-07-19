using System;
using System.Net.Sockets;
using System.Text;
using SyslogNet.Client.Serialization;

namespace SyslogNet.Client.Transport
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

		public void Send(SyslogMessage[] messages, ISyslogMessageSerializer serializer)
		{
			foreach(SyslogMessage message in messages)
			{
				Send(message, serializer);
			}
		}

		public void Dispose()
		{
			udpClient.Close();
		}
	}
}