using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using SyslogNet.Client.Serialization;

namespace SyslogNet.Client.Transport
{
	public class SyslogEncryptedTcpSender : ISyslogMessageSender
	{
		private readonly string hostname;
		private readonly int port;

		public SyslogEncryptedTcpSender(string hostname, int port)
		{
			this.hostname = hostname;
			this.port = port;
		}

		public void Send(SyslogMessage message, ISyslogMessageSerializer serializer)
		{
			// TODO: Does this need to be optimized? There is some mention in the MSDN docs about SslStream reusing cached SSL sessions
			// Need to be sure this won't cause a huge overhead of re-authenticating for each log message

			using (var tcpClient = new TcpClient(hostname, port))
			using (var sslStream = new SslStream(tcpClient.GetStream(), false, ValidateServerCertificate))
			{
				sslStream.AuthenticateAsClient(hostname);

				byte[] bytes = serializer.Serialize(message);
				sslStream.Write(bytes, 0, bytes.Length);
				sslStream.Flush();
			}
		}

		// Quick and nasty way to avoid logging framework dependency
		public static Action<string> CertificateErrorHandler = err => { };

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