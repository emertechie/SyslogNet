using System.Net.Sockets;
using System.Threading.Tasks;
using SyslogNet.Client.Serialization;

namespace SyslogNet.Client.Transport
{
	public class SyslogUdpSender : ISyslogMessageSender
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

		public async Task SendAsync(SyslogMessage message, ISyslogMessageSerializer serializer)
		{
			byte[] datagramBytes = serializer.Serialize(message);
			await udpClient.SendAsync(datagramBytes, datagramBytes.Length);
		}

		public void Dispose()
		{
			udpClient.Close();
		}
	}
}