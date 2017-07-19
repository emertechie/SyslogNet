using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SyslogNet.Client.Serialization;

namespace SyslogNet.Client.Transport
{
	public enum MessageTransfer {
		OctetCounting		= 0,
		NonTransparentFraming	= 1
	}

	public class SyslogTcpSender : ISyslogMessageSender
	{
		protected String hostname;
		protected int port;

		protected TcpClient tcpClient = null;
		protected Stream transportStream = null;

		public virtual MessageTransfer messageTransfer { get; set; }
		public byte trailer { get; set; }

		public SyslogTcpSender(string hostname, int port, bool connect = true)
		{
			this.hostname = hostname;
			this.port = port;

			if (connect)
				Connect();

			messageTransfer = MessageTransfer.OctetCounting;
			trailer = 10; // LF
		}

		public void Connect()
		{
			try
			{
				tcpClient = new TcpClient(hostname, port);
				transportStream = tcpClient.GetStream();
			}
			catch
			{
				Disconnect();
				throw;
			}
		}

		public virtual void Reconnect()
		{
			Disconnect();
			Connect();
		}

		public void Disconnect()
		{
			if (transportStream != null)
			{
				transportStream.Close();
				transportStream = null;
			}

			if (tcpClient != null)
			{
				tcpClient.Close();
				tcpClient = null;
			}
		}

		public void Send(SyslogMessage message, ISyslogMessageSerializer serializer)
		{
			SendAsync(message, serializer).Wait();
		}

		public void Send(IEnumerable<SyslogMessage> messages, ISyslogMessageSerializer serializer)
		{
			SendAsync(messages, serializer).Wait();
		}

		public async Task SendAsync(SyslogMessage message, ISyslogMessageSerializer serializer)
		{
			await SendAsync(message, serializer, true);
		}
		
		public async Task SendAsync(SyslogMessage message, ISyslogMessageSerializer serializer, bool flush)
		{
			if(transportStream == null)
			{
				throw new IOException("No transport stream exists");
			}

			using (MemoryStream memoryStream = new MemoryStream())
			{
				var datagramBytes = serializer.Serialize(message);

				if (messageTransfer.Equals(MessageTransfer.OctetCounting))
				{
					byte[] messageLength = Encoding.ASCII.GetBytes(datagramBytes.Length.ToString());
					await memoryStream.WriteAsync(messageLength, 0, messageLength.Length);
					await memoryStream.WriteByteAsync(32); // Space
				}

				await memoryStream.WriteAsync(datagramBytes, 0, datagramBytes.Length);

				if (messageTransfer.Equals(MessageTransfer.NonTransparentFraming))
				{
					await memoryStream.WriteByteAsync(trailer); // LF
				}

				await transportStream.WriteAsync(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
			}

			if (flush && !(transportStream is NetworkStream))
				await transportStream.FlushAsync();
		}

		public async Task SendAsync(IEnumerable<SyslogMessage> messages, ISyslogMessageSerializer serializer)
		{
			foreach (SyslogMessage message in messages)
			{
				await SendAsync(message, serializer, false);
			}

			if (!(transportStream is NetworkStream))
				transportStream.Flush();
		}

		public void Dispose()
		{
			Disconnect();
		}
	}

	static class MemoryStreamExtensions
	{
		private static readonly byte[] WriteByteAsyncBuffer = new byte[1];
		public static Task WriteByteAsync(this MemoryStream stream, byte value)
		{
			WriteByteAsyncBuffer[0] = value;
			return stream.WriteAsync(WriteByteAsyncBuffer, 0, 1);
		}
	}
}