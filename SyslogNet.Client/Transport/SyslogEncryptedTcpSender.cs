using System;
using System.Net.Security;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace SyslogNet.Client.Transport
{
	public class SyslogEncryptedTcpSender : SyslogTcpSender
	{
		public SyslogEncryptedTcpSender(string hostname, int port, int timeout = Timeout.Infinite) : base(hostname, port)
		{
			startTLS(hostname, timeout);
		}

		private void startTLS(String hostname, int timeout)
		{
			transportStream = new SslStream(tcpClient.GetStream(), false, ValidateServerCertificate)
			{
				ReadTimeout = timeout,
				WriteTimeout = timeout
			};

			((SslStream)transportStream).AuthenticateAsClient(hostname);

			if (!((SslStream)transportStream).IsEncrypted)
				throw new SecurityException("Could not establish an encrypted connection");
		}

		private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			if (sslPolicyErrors == SslPolicyErrors.None)
				return true;

			CertificateErrorHandler(String.Format("Certificate error: {0}", sslPolicyErrors));
			return false;
		}

		// Quick and nasty way to avoid logging framework dependency
		public static Action<string> CertificateErrorHandler = err => { };
	}
}
