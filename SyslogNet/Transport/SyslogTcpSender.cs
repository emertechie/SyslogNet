using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using SyslogNet.Serialization;

namespace SyslogNet.Transport
{
	public class SyslogTcpSender : ISyslogMessageSender, IDisposable
	{
		private readonly TcpClient tcpClient;
		private readonly SslStream sslStream;

		public SyslogTcpSender(string hostname, int port)
		{
			try
			{
				tcpClient = new TcpClient(hostname, port);
				sslStream = new SslStream(tcpClient.GetStream(), false, ValidateServerCertificate);

				sslStream.AuthenticateAsClient(hostname);
			}
			catch
			{
				if (tcpClient != null)
					((IDisposable)tcpClient).Dispose();

				if (sslStream != null)
					sslStream.Dispose();

				throw;
			}
		}

		public void Send(SyslogMessage message, ISyslogMessageSerializer serializer)
		{
			byte[] datagramBytes = serializer.Serialize(message);
			sslStream.Write(datagramBytes, 0, datagramBytes.Length);
			sslStream.Flush();
		}

		// Quick and nasty way to avoid logging framework dependency
		public static Action<string> CertificateErrorHandler = err => { };

		public void Dispose()
		{
			tcpClient.Close();
			sslStream.Close();

			((IDisposable)tcpClient).Dispose();
			sslStream.Dispose();
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