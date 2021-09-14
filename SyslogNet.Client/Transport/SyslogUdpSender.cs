
using SyslogNet.Client.Serialization;


namespace SyslogNet.Client.Transport
{


	public class SyslogUdpSender 
		: ISyslogMessageSender, System.IDisposable
	{

		private readonly System.Net.Sockets.UdpClient udpClient;


		public SyslogUdpSender(string hostname, int port)
		{
			udpClient = new System.Net.Sockets.UdpClient(hostname, port);
		}


		public void Send(SyslogMessage message, ISyslogMessageSerializer serializer)
		{
			byte[] datagramBytes = serializer.Serialize(message);
			udpClient.Send(datagramBytes, datagramBytes.Length);
		}


		public void Send(System.Collections.Generic.IEnumerable<SyslogMessage> messages, ISyslogMessageSerializer serializer)
		{
			foreach(SyslogMessage message in messages)
			{
				Send(message, serializer);
			}
		}


		public void Reconnect() 
		{ 
			// UDP is connectionless 
		}


		public void Dispose()
		{
			udpClient.Close();
		}


	}


}