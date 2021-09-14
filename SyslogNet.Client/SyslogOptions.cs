
namespace SyslogNet.Client
{


    [System.Flags]
    public enum SslProtocols
    {
        None = 0, // Allows the operating system to choose the best protocol to use
        Ssl2 = 12, // SSL 2.0 
        Ssl3 = 48, // SSL 3.0
        Tls = 192, // TLS 1.0, RFC 2246 
        Default = 240, // Negotiate between SSL 3.0 or TLS 1.0
        Tls11 = 768, // RFC 4346 
        Tls12 = 3072, // RFC 5246 
        Tls13 = 12288 // RFC 8446 
    }


    public enum NetworkProtocols
    {
        UPD,
        TCP,
        TLS
    }


    public enum SyslogVersions
    {
        Rfc5424,
        Rfc3164, // obsolete - for backwards compatiblity 
        Local
    }


    // Package CommandLineParser 
    public class SyslogOptions
    {

        // [CommandLine.Option('h', "hostName", Required = false, HelpText = "The host name. If not set, defaults to the NetBIOS name of the local machine")]
        public string LocalHostName { get; set; }

        // [CommandLine.Option('a', "appName", Required = false, HelpText = "The application name")]
        public string AppName { get; set; }

        // [CommandLine.Option('p', "procId", Required = false, HelpText = "The process identifier")]
        public string ProcId { get; set; }

        // [CommandLine.Option('t', "msgType", Required = false, HelpText = "The message type (called msgId in spec)")]
        public string MsgType { get; set; }

        // [CommandLine.Option('m', "msg", Required = false, HelpText = "The message")]
        public string Message { get; set; }

        // [CommandLine.Option('s', "syslogServer", Required = true, HelpText = "Host name of the syslog server")]
        public string SyslogServerHostname { get; set; }

        // [CommandLine.Option('r', "syslogPort", Required = true, HelpText = "The syslog server port")]
        public int SyslogServerPort { get; set; }

        // [CommandLine.Option('v', "version", Required = false, Default = "5424", HelpText = "The version of syslog protocol to use. Possible values are '3164' and '5424' (from corresponding RFC documents) or 'local' to send messages to a local syslog (only on Linux or OS X). Default is '5424'")]
        public SyslogVersions SyslogVersion { get; set; }

        // [CommandLine.Option('o', "protocol", Required = false, Default = "tcp", HelpText = "The network protocol to use. Possible values are 'tcp' or 'udp' to send to a remote syslog server, or 'local' to send to a local syslog over Unix sockets (only on Linux or OS X). Default is 'tcp'. Note: TCP always uses SSL connection.")]


        protected NetworkProtocols m_networkProtocol;


        public NetworkProtocols NetworkProtocol
        {
            get { return this.m_networkProtocol; }
            set
            {
                this.m_networkProtocol = value;
                this.InferNetworkProtocol();
            }
        }

        public SslProtocols SslProtocol { get; set; }

        public bool UseUtf8 { get; set; }




        // [CommandLine.Option('c', "cert", Required = false, HelpText = "Optional path to a CA certificate used to verify Syslog server certificate when using TCP protocol")]
        public string CACertPath { get; set; }

        public SyslogOptions()
        {
            this.UseUtf8 = true;
            this.AppName = System.AppDomain.CurrentDomain.FriendlyName;
            this.ProcId = System.Diagnostics.Process.GetCurrentProcess().Id.ToString(System.Globalization.CultureInfo.InvariantCulture);
            this.LocalHostName = System.Environment.MachineName;

            this.SyslogVersion = SyslogVersions.Rfc5424;
            this.SyslogServerHostname = "127.0.0.1";

            this.NetworkProtocol = NetworkProtocols.UPD;
            this.InferNetworkProtocol();
            this.InferDefaultPort();
        }


        public void InferNetworkProtocol()
        {
            if (this.NetworkProtocol == NetworkProtocols.TLS)
            {
                if (this.SslProtocol == SslProtocols.None)
                    this.SslProtocol = SyslogNet.Client.SslProtocols.Tls;
            }
            else
            {
                this.SslProtocol = SyslogNet.Client.SslProtocols.None;
            }
        }


        public void InferDefaultPort()
        {
            switch (this.NetworkProtocol)
            {
                case NetworkProtocols.TCP:
                    this.SyslogServerPort = 1468; // TCP 
                    break;
                case NetworkProtocols.TLS:
                    this.SyslogServerPort = 6514; // TLS 
                    break;
                default:
                    this.SyslogServerPort = 514; // UDP
                    break;
            }
        }


        internal SyslogNet.Client.Serialization.ISyslogMessageSerializer Serializer
        {
            get
            {
                switch (this.SyslogVersion)
                {


                    case SyslogVersions.Rfc5424:
                        return new SyslogNet.Client.Serialization.SyslogRfc5424MessageSerializer();
                    case SyslogVersions.Rfc3164:
                        return new SyslogNet.Client.Serialization.SyslogRfc3164MessageSerializer(this.UseUtf8);
                    default:
                        return new SyslogNet.Client.Serialization.SyslogLocalMessageSerializer();
                }

            }
        }


        internal SyslogNet.Client.Transport.ISyslogMessageSender Sender
        {
            get
            {
                switch (this.NetworkProtocol)
                {
                    case NetworkProtocols.UPD:
                        return new SyslogNet.Client.Transport.SyslogUdpSender(this.SyslogServerHostname, this.SyslogServerPort);
                    case NetworkProtocols.TCP:
                        return new SyslogNet.Client.Transport.SyslogTcpSender(this.SyslogServerHostname, this.SyslogServerPort);
                    case NetworkProtocols.TLS:
                        return new SyslogNet.Client.Transport.SyslogEncryptedTcpSender(this.SyslogServerHostname, this.SyslogServerPort, this.SslProtocol, -1, true);
                    default:
                        return new SyslogNet.Client.Transport.SyslogLocalSender();
                }
            }
        }


    }


}
