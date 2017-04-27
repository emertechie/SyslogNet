using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using SyslogNet.Client.Serialization;

namespace SyslogNet.Client.Transport
{
	public class SyslogUdpSender : ISyslogMessageSender, IDisposable
	{
		private readonly UdpClient udpClient;
        private readonly string _hostname;
        private readonly int _port;

        public SyslogUdpSender(string hostname, int port)
		{
#if NET4_0
            udpClient = new UdpClient(hostname, port);
#else
            udpClient = new UdpClient();
#endif
            _hostname = hostname;
            _port = port;
        }

		public void Send(SyslogMessage message, ISyslogMessageSerializer serializer)
		{
			byte[] datagramBytes = serializer.Serialize(message);
#if NET4_0
            udpClient.Send(datagramBytes, datagramBytes.Length);
#else
            udpClient.SendAsync(datagramBytes, datagramBytes.Length, _hostname, _port).Wait();
#endif
        }

		public void Send(IEnumerable<SyslogMessage> messages, ISyslogMessageSerializer serializer)
		{
			foreach(SyslogMessage message in messages)
			{
				Send(message, serializer);
			}
		}

		public void Reconnect() { /* UDP is connectionless */ }

		public void Dispose()
		{
#if NET4_0
            udpClient.Close();
#else
            udpClient.Dispose();
#endif

        }
	}
}