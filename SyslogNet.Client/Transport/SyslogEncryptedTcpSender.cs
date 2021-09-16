
namespace SyslogNet.Client.Transport
{


    public class SyslogEncryptedTcpSender
        : SyslogTcpSender
    {

        protected int IOTimeout;
        protected SslProtocols _sslProtocol;

        public bool IgnoreTLSChainErrors { get; private set; }

        protected MessageTransfer _messageTransfer;


        public override MessageTransfer messageTransfer
        {
            get { return _messageTransfer; }
            set
            {
                if (!value.Equals(MessageTransfer.OctetCounting) && transportStream is System.Net.Security.SslStream)
                {
                    throw new SyslogTransportException("Non-Transparent-Framing can not be used with TLS transport");
                }

                _messageTransfer = value;
            }
        }


        public SyslogEncryptedTcpSender(
              string hostname
            , int port
            , SslProtocols sslProtocol
            , int timeout = System.Threading.Timeout.Infinite
            , bool ignoreChainErrors = false)
            : base(hostname, port)
        {
            IOTimeout = timeout;
            IgnoreTLSChainErrors = ignoreChainErrors;
            startTLS();
        }


        public override void Reconnect()
        {
            base.Reconnect();
            startTLS();
        }


        private void startTLS()
        {
            transportStream = new System.Net.Security.SslStream(tcpClient.GetStream(), false
                , new System.Net.Security.RemoteCertificateValidationCallback(ValidateServerCertificate))
            {
                ReadTimeout = IOTimeout,
                WriteTimeout = IOTimeout
            };

            // According to RFC 5425 we MUST support TLS 1.2, but this protocol version only implemented in framework 4.5 and Windows Vista+...
            ((System.Net.Security.SslStream)transportStream).AuthenticateAsClient(
                hostname,
                null,
                (System.Security.Authentication.SslProtocols)_sslProtocol,
                false
            );



            if (!((System.Net.Security.SslStream)transportStream).IsEncrypted)
                throw new System.Security.SecurityException("Could not establish an encrypted connection");

            messageTransfer = MessageTransfer.OctetCounting;
        }


        private bool ValidateServerCertificate(
            object sender, 
            System.Security.Cryptography.X509Certificates.X509Certificate certificate, 
            System.Security.Cryptography.X509Certificates.X509Chain chain, 
            System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == System.Net.Security.SslPolicyErrors.None || 
                (IgnoreTLSChainErrors && sslPolicyErrors == System.Net.Security.SslPolicyErrors.RemoteCertificateChainErrors))
                return true;

            CertificateErrorHandler(string.Format("Certificate error: {0}", sslPolicyErrors));
            return false;
        }

        // Quick and nasty way to avoid logging framework dependency
        public static System.Action<string> CertificateErrorHandler = err => { };


    }


}
