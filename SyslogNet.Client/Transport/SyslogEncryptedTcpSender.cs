using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using SyslogNet.Client.Serialization;

namespace SyslogNet.Client.Transport
{
	public class SyslogEncryptedTcpSender : ISyslogMessageSender, IDisposable
	{
		private readonly TcpClient tcpClient;
		private readonly SslStream sslStream;

		public SyslogEncryptedTcpSender(string hostname, int port)
		{
			try
			{
				tcpClient = new TcpClient(hostname, port);
				tcpClientStream = tcpClient.GetStream();
				sslStream = new SslStream(tcpClientStream, false, ValidateServerCertificate);

				sslStream.AuthenticateAsClient(hostname);

				if (!sslStream.IsEncrypted)
					throw new SecurityException("Could not establish an encrypted connection");
			}
			catch
			{
				Dispose();
				throw;
			}
		}

		public void Send(SyslogMessage message, ISyslogMessageSerializer serializer)
		{
			byte[] datagramBytes = serializer.Serialize(message);
			sslStream.Write(datagramBytes, 0, datagramBytes.Length);
			sslStream.Flush();
			tcpClientStream.Flush();

			// Note: This doesn't work reliably. Can't seem to find a method which does
			if (!tcpClient.Connected)
				throw new CommunicationsException("Could not send message because client was disconnected");
		}

		// Quick and nasty way to avoid logging framework dependency
		public static Action<string> CertificateErrorHandler = err => { };
		private readonly NetworkStream tcpClientStream;

		public void Dispose()
		{
			if (tcpClient != null)
			{
				tcpClient.Close();
				((IDisposable)tcpClient).Dispose();
			}

			if (tcpClientStream != null)
				tcpClientStream.Dispose();

			if (sslStream != null)
			{
				sslStream.Close();
				sslStream.Dispose();
			}
		}

		private static bool ValidateServerCertificate(
			object sender,
			X509Certificate certificate,
			X509Chain chain,
			SslPolicyErrors sslPolicyErrors)
		{
			if (sslPolicyErrors == SslPolicyErrors.None)
				return true;

			CertificateErrorHandler(String.Format("Certificate error: {0}", sslPolicyErrors));
			return false;
		}
	}
}